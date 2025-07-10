using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;

using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Business.GSSi;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using AU = Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business
{
	[Enterprise.Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.NotApplied, "Enterprise.Client.UPE.Metadata.UPEJobDeclaration, ZClientUPE, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")]
	public class UPEJobDeclaration : JobDeclaration, ILineKey, IUPEDocumentSupportable, IRefundEnquiry, ICommercialInvoiceSupportable
	{
		public UPEJobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Compiling;
			CurrentQueue.HasChanges = false;
		}

		public override void Delete()
		{
			UPEOrgRematch[] rematches = Factory.Load<UPEOrgRematch>(new ZQuery(ClientOrgRematchSchema.T5_JE, PK));
			foreach (UPEOrgRematch rematch in rematches)
			{
				rematch.Delete();
			}
			RefundManager.DeleteRefund();
			base.Delete();
		}

		#region EIR Raising

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();

			if (IsInDatabase && !CurrentQueue.EIRRaisedLog.IsEmpty)
			{
				using (((IUpdateFieldsLock)Logs.AutoCreatedLog).LockForUpdatingKeyFields())
				{
					Logs.AutoCreatedLog.SL_Reference += (Logs.AutoCreatedLog.SL_Reference.IsEmpty ? "" : "; ") + CurrentQueue.EIRRaisedLog;
				}
			}
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (CurrentQueue.P4_CustomsStatusInfo.HasChanges)
			{
				fStatusChange = true;
			}

			if (!CurrentQueue.HasCustomsQueueBeenCompleted && CurrentQueue.IsCustomsQueueCompleted)
			{
				CustomsEntryDate = ZDateTime.Now;
			}

			if (DoManualBillActivitiesOnSave)
			{
				ManualBillNotification.DoManualBillActivities();
				DoManualBillActivitiesOnSave = false;
			}
		}
		bool DoManualBillActivitiesOnSave;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			ProcessGSSiMessaging();

			base.OnFactorySaved(saveSucceeded);
			IsIdentifiedAsSplitShipment = false;
		}

		void ProcessGSSiMessaging()
		{
			if (fStatusChange)
			{
				fStatusChange = false;

				IShipmentStatusData currentQueueLog = GSSiManager.Instance.GetLatestProcessLogByQueueType(CurrentQueue.ReferenceCode, Factory, CurrentQueue.PK);

				if (currentQueueLog.ShipmentStatus == "03" && currentQueueLog.HoldReasonCode.Length == 2)
				{
					var innerFactory = new BusinessObjectFactory();
					var jobDecInInnerFactory = innerFactory.Load<JobDeclaration>(PK);

					GSSiManager.Instance.ProcessGSSIHOLDMessage(
					jobDecInInnerFactory,
					currentQueueLog,
				() =>
				{
					var trackingIDs = new UniqueList<ZString>();
					if (FirstCusHAWB != null)
					{
						trackingIDs.Add(FirstCusHAWB.CS_HAWB);
					}

					var relatedcusHawb = from cushawb in GetAssociatedHAWBs()
										 select cushawb.CS_HAWB;

					var childPacks = from cushawb in GetAssociatedHAWBs()
									 from childBill in cushawb.ChildJobRelatedWayBills
									 select childBill.EB_WaybillNumber;

					trackingIDs.AddRange(relatedcusHawb);
					trackingIDs.AddRange(childPacks);

					return trackingIDs;
				},
				PortOfFirstArrival
				);
					// evil but no worse than file based system was...
					innerFactory.Save();
				}
			}
		}

		ZBool fStatusChange;

		#endregion

		#region Related Business Objects

		public UPEOrgHeader AlternateBroker
		{
			get { return (!JE_OH_Importer.IsValid || Importer == null) ? null : (UPEOrgHeader)Importer.DeliveryAirCustomsBroker; }
		}

		public new UPEOrgHeader Importer
		{
			get { return (UPEOrgHeader)base.Importer; }
		}

		public Callout Callout
		{
			get
			{
				if (callout == null)
				{
					var related = Factory.Load<Callout>(CusHAWBFilter);
					var cusHAWBRelated = related.Where(x => x.CurrentQueue.P4_CustomsReason != ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment && x.CurrentQueue.P4_QueueName != ZString.Empty);
					callout = new Lazy<Callout>(() => cusHAWBRelated.OrderByDescending(x => x.BisiDownloadDate).FirstOrDefault());
				}

				return callout.Value;
			}
		}
		Lazy<Callout> callout;

		protected ManualBillNotification ManualBillNotification
		{
			get
			{
				if (fManualBillNotification == null)
				{
					fManualBillNotification = NewManualBillNotification();
				}
				return fManualBillNotification;
			}
		}
		ManualBillNotification fManualBillNotification;

		protected virtual ManualBillNotification NewManualBillNotification()
		{
			return new ManualBillNotification(this);
		}

		#endregion

		#region Commodities Data

		public IReadOnlyList<CommodityDetailData> CommoditiesData
		{
			get
			{
				ArrayList result = new ArrayList();

				if (Globals.IsTest)
				{
					InvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, ListSortDirection.Ascending);
				}
				foreach (JobComInvoiceLine invLine in InvoiceLines)
				{
					string countryOfOrigin = (!invLine.JI_CountryOfOrigin.IsEmpty) ? invLine.JI_CountryOfOrigin : invLine.InvoiceHeader.ZA_ORG;
					CommodityDetailData commodityData = new CommodityDetailData(invLine.JI_Description, invLine.JI_Tariff, countryOfOrigin, invLine.JI_LinePrice);
					result.Add(commodityData);
				}
				return (CommodityDetailData[])result.ToArray(typeof(CommodityDetailData));
			}
		}

		#endregion

		#region Customs Charges

		public IReadOnlyList<ShipmentChargeData> ChargesData
		{
			get
			{
				var result = new ShipmentChargeDataList();

				if (UploadCustomsCharges)
				{
					AddChargesFromCustomsCharges(result);
				}
				AddUPSQuarantineFee(result);
				AddUPSQuarantineProcessingFee(result);
				AddTerminalFeeIfHandledByAlternateBroker(result);
				AddPreReleaseFeeIfApplicable(result);
				return result.ToArray();
			}
		}

		public bool UploadCustomsCharges
		{
			get { return !IsCustomsEFTActive || IsFirstCusHAWBFreeDomicile; }
		}

		public bool IsCustomsEFTActive
		{
			get
			{
				return (Importer != null && Importer.MiscServ != null
					&& !Importer.MiscServ.OM_IMEFTBankBSB.IsEmpty
					&& !Importer.MiscServ.OM_IMEFTBankAccount.IsEmpty
					&& Importer.MiscServ.OM_IMEftCustomsFromImport);
			}
		}

		bool IsFirstCusHAWBFreeDomicile
		{
			get { return (FirstCusHAWB != null && FirstCusHAWB.BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreeDomicile); }
		}

		void AddPreReleaseFeeIfApplicable(ShipmentChargeDataList charges)
		{
			if (IsEntryCompleted && UPEDataRegistry.Instance.PreReleaseChargeEnabled.Value && Importer != null)
			{
				var baseCharge = UPEDataRegistry.Instance.EntryLineChargeBaseAmount.Value;
				var totalCharge = baseCharge;

				var perLineChargeAmount = UPEDataRegistry.Instance.PerLineChargeAmount.Value;
				var linesWhereChargePerLineApplicable = CustomsEntryHeaders.Cast<AU.CusEntryHeader>().SelectMany(h => (h.MergedLines.Cast<AU.CusEntryLine>())).Count() - UPEDataRegistry.Instance.LinesExemptedFromLineCharge.Value;
				if (perLineChargeAmount > 0 && linesWhereChargePerLineApplicable > 0)
				{
					var cappedChargeAmount = UPEDataRegistry.Instance.EntryLineChargeCappedAmount.Value;
					var estimatedChargePerLineAmount = baseCharge + perLineChargeAmount * linesWhereChargePerLineApplicable;

					totalCharge = (estimatedChargePerLineAmount > cappedChargeAmount) ? cappedChargeAmount : estimatedChargePerLineAmount;
				}

				if (totalCharge > 0m)
				{
					charges.AddCharge(ShipmentChargeTypeCode.ChargePerLine, totalCharge, Core.Constants.CurrencyCodes.Australia);
				}

				if (Importer.IsPreReleaseContactFeeApplicable && UPEDataRegistry.Instance.ContactFeeAmount.Value > 0m)
				{
					charges.AddCharge(ShipmentChargeTypeCode.ContactFee, UPEDataRegistry.Instance.ContactFeeAmount.Value, Core.Constants.CurrencyCodes.Australia);
				}
			}
		}

		void AddChargesFromCustomsCharges(ShipmentChargeDataList charges)
		{
			if (IsEntryCompleted)
			{
				foreach (CustomsCharge customsCharge in ServiceLocator.GetService<ICustomsCharges>(this).GetCustomsCharges(null))
				{
					charges.AddCharge(customsCharge.Description, customsCharge.Amount);
				}
			}
		}

		void AddUPSQuarantineFee(ShipmentChargeDataList charges)
		{
			if (!QuarantineFee.IsEmpty)
			{
				charges.AddCharge(ShipmentChargeTypeCode.Quarantine, QuarantineFee, Core.Constants.CurrencyCodes.Australia);
			}
		}

		void AddUPSQuarantineProcessingFee(ShipmentChargeDataList charges)
		{
			if (!QuarantineProcessingFee.IsEmpty)
			{
				charges.AddCharge(ShipmentChargeTypeCode.QuarantinePermit, QuarantineProcessingFee, Core.Constants.CurrencyCodes.Australia);
			}
		}

		void AddTerminalFeeIfHandledByAlternateBroker(ShipmentChargeDataList charges)
		{
			if (HasAlternateBroker && Importer.IsITFChargableForThisImporter && UPEDataRegistry.Instance.TerminalFeeAmount.Value != 0m)
			{
				charges.AddCharge(ShipmentChargeTypeCode.Terminal, UPEDataRegistry.Instance.TerminalFeeAmount.Value, Core.Constants.CurrencyCodes.Australia);
			}
		}

		#endregion

		#region New Properties

		#region BisiUploadDate

		public ZDateTime BisiUploadDate
		{
			get { return CurrentQueue.P4_CustomDate1; }
			set { CurrentQueue.P4_CustomDate1 = value; }
		}

		public ZPropertyInfo BisiUploadDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BisiUploadDate), x => CurrentQueue.P4_CustomDate1Info); }
		}

		#endregion

		#region CustomsEntryDate

		public ZDateTime CustomsEntryDate
		{
			get { return CurrentQueue.P4_CustomDate2; }
			set { CurrentQueue.P4_CustomDate2 = value; }
		}

		public ZPropertyInfo CustomsEntryDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CustomsEntryDate), x => CurrentQueue.P4_CustomDate2Info); }
		}

		protected ZBool IsLOAAlreadyPrinted
		{
			get
			{
				ZQuery query = new ZQuery(StmALogSchema.SL_Parent, PK);
				query.AddToFilter(StmALogSchema.SL_Reference, LOALogReferenceText);
				return Logs.Find(query).Length > 0;
			}
		}

		protected ZString LOALogReferenceText
		{
			get { return "Letter of Authority was printed"; }
		}

		internal ZString AutoSendingLetterOfAuthorityErrorMessage
		{
			get { return autoSendingLetterOfAuthorityErrorMessage; }
		}

		internal event AutoSendingLetterOfAuthority ErrorAutoSendingLetterOfAuthority;

		internal delegate void AutoSendingLetterOfAuthority(ZString errorMessage);

		protected virtual void OnErrorAutoSendingLetterOfAuthority()
		{
			if (ErrorAutoSendingLetterOfAuthority != null)
			{
				ErrorAutoSendingLetterOfAuthority(AutoSendingLetterOfAuthorityErrorMessage);
			}
		}

		ZString autoSendingLetterOfAuthorityErrorMessage = ZString.Empty;

		void LetterOfAuthorityProcessing()
		{
			if (Importer != null)
			{
				if (!IsLOAAlreadyPrinted &&
					(Importer.DateLOAReceivedAuthorisingUPStoClearGoods.IsEmpty || Importer.DateLOAReceivedAuthorisingUPStoClearGoodsIsEarlier7Days) &&
					!Importer.IsLOAReceivedAuthorisingUPStoClearGoods &&
					AlternateBroker == null)
				{
					UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.UPSLetterOfAuthority);
					try
					{
						PrintLetterOfAuthority(currentPrintBatch, new UPEDocumentMenuItemLoader(Factory).LoadLetterOfAuthority().PK);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						autoSendingLetterOfAuthorityErrorMessage = "Unable to Autosend the Lettter of Authority to the printer" + System.Environment.NewLine + System.Environment.NewLine + "Error:" + e.Message;
						OnErrorAutoSendingLetterOfAuthority();
					}
				}
			}
		}

		protected virtual void PrintLetterOfAuthority(UPEPrintBatch currentPrintBatch, ZGuid menuItemPK)
		{
			currentPrintBatch.QueueForBatchPrintAndSave(Importer, menuItemPK, true);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, LOALogReferenceText);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			LogsFactory.Save();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				LetterOfAuthorityProcessing();
			}
		}

		#endregion

		#region Quarantine Fee

		public ZDecimal QuarantineFee
		{
			get { return CurrentQueue.P4_CustomDecimal1; }
			set { CurrentQueue.P4_CustomDecimal1 = value; }
		}

		public ZPropertyInfo QuarantineFeeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(QuarantineFee), x => CurrentQueue.P4_CustomDecimal1Info); }
		}

		#endregion

		#region Quarantine Processing Fee

		public ZDecimal QuarantineProcessingFee
		{
			get { return CurrentQueue.P4_CustomDecimal2; }
			set { CurrentQueue.P4_CustomDecimal2 = value; }
		}

		public ZPropertyInfo QuarantineProcessingFeeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(QuarantineProcessingFee), x => CurrentQueue.P4_CustomDecimal2Info); }
		}

		#endregion

		#region TotalQuarantineCharges

		public ZDecimal TotalQuarantineCharges
		{
			get { return QuarantineFee + QuarantineProcessingFee; }
		}

		public ZPropertyInfo TotalQuarantineChargesInfo
		{
			get { return GetZPropertyInfo(nameof(TotalQuarantineCharges)); }
		}

		#endregion

		#region AlternateBrokerStorageFeeStartDate

		public ZDateTime AlternateBrokerStorageFeeStartDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (JE_DateOfArrival.IsValid)
				{
					result = JE_DateOfArrival;
					result = IncrementDayNotIncludingWeekendsOrHolidays(result);
					result = IncrementDayNotIncludingWeekendsOrHolidays(result);
					if (JE_RS_NKServiceLevel != "1")
					{
						result = IncrementDayNotIncludingWeekendsOrHolidays(result);
					}
				}
				return result;
			}
		}

		public ZDecimal AlternateBrokerStorageFeeIncludingGST
		{
			get { return AlternateBrokerStorageDays * UPEDataRegistry.Instance.AlternateBrokerStorageFeePerDay.Value * 1.1m; }
		}

		ZInt AlternateBrokerStorageDays
		{
			get
			{
				int result = 0;
				ZDateTime current = AlternateBrokerStorageFeeStartDate.Date;
				while (current.IsValid && (ZDate)current < ZDate.Today)
				{
					current = IncrementDayNotIncludingWeekendsOrHolidays(current);
					result++;
				}
				return result;
			}
		}

		ZDateTime IncrementDayNotIncludingWeekendsOrHolidays(ZDateTime date)
		{
			ZDateTime result = date;
			do
			{
				result = result.AddDays(1);
			}
			while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday || GlbBranch.CurrentBranch.GlbHolidays.Contains(result.Date));
			return result;
		}

		#endregion

		#region IsEntryPrintToBISIPending

		public static readonly SchemaBoolColumn IsEntryPrintToBISIPendingProcessQueueColumn = ProcessQueueSchema.P4_CustomFlag1;

		public ZBool IsEntryPrintToBISIPending
		{
			get { return CurrentQueue.P4_CustomFlag1; }
			set
			{
				CurrentQueue.P4_CustomFlag1 = value;
				IsEntryPrintToBISIPendingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsEntryPrintToBISIPendingInfo
		{
			get { return GetZPropertyInfo(nameof(IsEntryPrintToBISIPending)); }
		}

		#endregion

		#region IsAudit

		public static readonly SchemaBoolColumn IsAuditColumn = ProcessQueueSchema.P4_CustomFlag2;

		public ZBool IsAudit
		{
			get { return CurrentQueue.P4_CustomFlag2; }
			set
			{
				CurrentQueue.P4_CustomFlag2 = value;
				IsAuditInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsAuditInfo
		{
			get { return GetZPropertyInfo(nameof(IsAudit)); }
		}

		#endregion

		#region CommercialInvoiceImage

		public ZBool HasCommercialInvoice
		{
			get { return CommercialInvoiceDocManager.HasCommercialInvoice(DocManagerInfo); }
		}

		public Image CommercialInvoiceImage
		{
			get { return CommercialInvoiceDocManager.CommercialInvoiceImage(DocManagerInfo); }
		}

		#endregion

		#region TotalJobsImportedForImporter

		public ZInt TotalJobsImportedForImporter
		{
			get
			{
				if (!TotalJobsImportedForImporterCalculated)
				{
					TotalJobsImportedForImporterCalculated = true;
					if (JE_OH_Importer.IsValid)
					{
						ZQuery query = new ZQuery(JobDeclarationSchema.JE_OH_Importer, JE_OH_Importer);
						fTotalJobsImportedForImporter = Factory.GetDatabaseCount(typeof(JobDeclaration), query);
					}
				}
				return fTotalJobsImportedForImporter;
			}
		}
		ZInt fTotalJobsImportedForImporter;
		bool TotalJobsImportedForImporterCalculated;

		public ZPropertyInfo TotalJobsImportedForImporterInfo
		{
			get { return GetZPropertyInfo(nameof(TotalJobsImportedForImporter)); }
		}

		#endregion

		#endregion

		#region Property Overrides

		#region Overrides

		#region Importer

		public override ZGuid JE_OH_Supplier
		{
			get { return base.JE_OH_Supplier; }
			set
			{
				if (base.JE_OH_Supplier != value)
				{
					base.JE_OH_Supplier = value;
					foreach (JobComInvoiceHeader invHead in Invoices.ToArray())
					{
						invHead.JZ_OH_Supplier = value;
					}
					TryAllocateClassifier();
					UPEOrgRematch.LogRematch(this, JE_OH_SupplierInfo, UPEOrgRematch.OrgTypes.Supplier);
				}
			}
		}

		protected override void DefaultOriginFromSupplier()
		{
			//do nothing
		}

		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				if (base.JE_OH_Importer != value)
				{
					base.JE_OH_Importer = value;
					TotalJobsImportedForImporterCalculated = false;

					TryMoveToAlternateBrokerQueue();
					TryAllocateClassifier();
					UPEOrgRematch.LogRematch(this, JE_OH_ImporterInfo, UPEOrgRematch.OrgTypes.Importer);
				}
			}
		}

		protected override void DefaultFinalDestinationPortFromImporter()
		{
			//do nothing
		}

		bool IsInUnallocatedQueue
		{
			get { return CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.Compiling; }
		}

		#region Alternate Broker Queue Movement

		public bool HasAlternateBroker
		{
			get { return AlternateBroker != null; }
		}

		void TryMoveToAlternateBrokerQueue()
		{
			if (IsInUnallocatedQueue && HasAlternateBroker)
			{
				MoveToAlternateBrokerQueue();
			}
		}

		void MoveToAlternateBrokerQueue()
		{
			MoveToQueue(
				DeclarationQueueCodeDescriptionPairList.Codes.BCA,
				ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker,
				ZString.Empty,
				"Has Alternate Broker");

			if (Callout != null && HasAlternateBroker && !AlternateBroker.IsDeliveryHandledByUPSForThisAlternateBroker)
			{
				Callout.IsHoldForCollection = true;
			}
		}

		#endregion

		#region Classifier Allocation

		void TryAllocateClassifier()
		{
			if (IsInUnallocatedQueue)
			{
				TryAssignClassifierRoleToImporter();
				if (Classifier != null)
				{
					CurrentQueue.P4_GS_NKCustomsTaskAssignedTo = Classifier.GS_Code;
				}
			}
		}

		GlbStaff Classifier
		{
			get
			{
				if (fClassifier == null)
				{
					if (Importer != null)
					{
						ZString classifierNK = Importer.StaffAssignments.GetStaffAssignment(UPEStaffRoles.Codes.Classifier, OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Air);
						if (!classifierNK.IsEmpty)
						{
							fClassifier = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, classifierNK);
						}
					}
				}

				return fClassifier;
			}
		}
		GlbStaff fClassifier;

		#region Allocation of Classifier to Importer

		void TryAssignClassifierRoleToImporter()
		{
			if (Importer != null && Supplier != null && Classifier == null)
			{
				AllocateClassifierToImporter();
			}
		}

		void AllocateClassifierToImporter()
		{
			GlbStaff importersNewClassifier = SupplierRVRoleClassifier ?? GetClassifierWithLeastNumberOfAllocatedJobs();
			if (importersNewClassifier != null)
			{
				OrgStaffAssignments classifierAssignment = Importer.StaffAssignments.AddNew();
				classifierAssignment.O8_Role = UPEStaffRoles.Codes.Classifier;
				classifierAssignment.O8_GS_NKPersonResponsible = importersNewClassifier.GS_Code;
			}
		}

		GlbStaff SupplierRVRoleClassifier
		{
			get
			{
				GlbStaff result = null;
				if (Supplier != null)
				{
					ZString rVStaffAssignmentNK = Supplier.StaffAssignments.GetStaffAssignment(UPEStaffRoles.Codes.RV, OrgStaffAssignmentsCollection.Direction.Import, OrgStaffAssignmentsCollection.AirSea.Air);
					if (!rVStaffAssignmentNK.IsEmpty)
					{
						result = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, rVStaffAssignmentNK);
					}
				}
				return result;
			}
		}

		GlbStaff GetClassifierWithLeastNumberOfAllocatedJobs()
		{
			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			var jobDecQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			jobDecQuery.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);

			string sQL =
				GlbStaffSchema.GS_Code.Name + " IN (" +
				"SELECT top 1 " + ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo.Name +
				" FROM " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName + " " +
				" INNER JOIN " + ProcessQueueSchema.Constants.SqlSchemaName + "." + ProcessQueueSchema.Constants.TableName + " ON " + JobDeclarationSchema.PK.Name + " = " + ProcessQueueSchema.P4_ParentID.Name +
				" INNER JOIN " + GlbStaffSchema.Constants.SqlSchemaName + "." + GlbStaffSchema.Constants.TableName + " on " + GlbStaffSchema.GS_Code.Name + " = " + ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo.Name +
				" INNER JOIN " + GlbGroupLinkSchema.Constants.SqlSchemaName + "." + GlbGroupLinkSchema.Constants.TableName + " ON " + GlbStaffSchema.PK.Name + " = " + GlbGroupLinkSchema.GK_GS.Name +
				" INNER JOIN " + GlbGroupSchema.Constants.SqlSchemaName + "." + GlbGroupSchema.Constants.TableName + " ON " + GlbGroupLinkSchema.GK_GG.Name + " = " + GlbGroupSchema.PK.Name +
				" WHERE " + JobDeclarationSchema.JE_IsCancelled.Name + " = 0" +
				" AND (" + ProcessQueueSchema.P4_CustomsQueue.Name + " = '" + DeclarationQueueCodeDescriptionPairList.Codes.Compiling + "' OR " + ProcessQueueSchema.P4_CustomsQueue.Name + " = '" + DeclarationQueueCodeDescriptionPairList.Codes.Classification + "')" +
				" AND " + GlbGroupSchema.GG_Code.Name + " = @ClassifierGroupCode" +
				" AND " + jobDecQuery.LiteralTextSqlFormatted +
				" GROUP BY " + ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo.Name +
				" ORDER BY count(*))";

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbStaff));
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@ClassifierGroupCode", UPEDataRegistry.Instance.ClassifierStaffGroupCode.Value, GlbStaffSchema.GS_Code);
			query.AddFilterAndZSQLParameterCollection(sQL, @params);

			GlbStaff result = (GlbStaff)Factory.LoadTop1(typeof(GlbStaff), query);
			return result;
		}

		#endregion

		#endregion

		#endregion

		public new UPEDeclarationQueue CurrentQueue
		{
			get
			{
				UPEDeclarationQueue currentQueue = (UPEDeclarationQueue)base.CurrentQueue;
				if (cachedCurrentQueue == null || cachedCurrentQueue.GetHashCode() != currentQueue.GetHashCode())
				{
					UnRegisterListChangedCalledRefreshBinding(cachedCurrentQueue);
					cachedCurrentQueue = currentQueue;
					RegisterListChangedCalledRefreshBinding(cachedCurrentQueue);
				}
				return cachedCurrentQueue;
			}
		}
		UPEDeclarationQueue cachedCurrentQueue;

		public override ZString JE_EntryStatus
		{
			get { return base.JE_EntryStatus; }
			set
			{
				if (JE_EntryStatus != value)
				{
					base.JE_EntryStatus = value;

					if (!IsIdentifiedAsSplitShipment)
					{
						IsIdentifiedAsSplitShipment = TryProcessSplitShipments();
					}

					if (IsEntryCompleted)
					{
						IsEntryPrintToBISIPending = true;
					}

					MoveQueue();
				}
			}
		}

		public override ZString JE_MessageStatus
		{
			get { return base.JE_MessageStatus; }
			set
			{
				if (base.JE_MessageStatus != value)
				{
					base.JE_MessageStatus = value;

					if (!IsIdentifiedAsSplitShipment)
					{
						IsIdentifiedAsSplitShipment = TryProcessSplitShipments();
					}

					MoveQueue();
				}
			}
		}
		bool IsIdentifiedAsSplitShipment;

		public override ZString JE_HouseBill
		{
			get { return base.JE_HouseBill; }
			set
			{
				if (base.JE_HouseBill != value)
				{
					base.JE_HouseBill = value;
					JE_AgentsReference = value;
				}
			}
		}

		protected override Type TypeOfProcessQueue
		{
			get { return typeof(UPEDeclarationQueue); }
		}

		#endregion

		#region Override for Property ReadOnly

		public override ZPropertyInfo JE_MasterBillInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_MasterBillInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_FolioInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_FolioInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_RL_NKPortOfLoadingInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_RL_NKPortOfLoadingInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_RL_NKFinalDestinationInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_RL_NKFinalDestinationInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_RL_NKPortOfArrivalInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_RL_NKPortOfArrivalInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_DateOfArrivalInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_DateOfArrivalInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_DateOfFirstArrivalInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_DateOfFirstArrivalInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_HouseBillInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_HouseBillInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_RL_NKOriginInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_RL_NKOriginInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_RL_NKPortOfFirstArrivalInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_RL_NKPortOfFirstArrivalInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_DateAtOriginInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_DateAtOriginInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_DateAtFinalDestinationInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_DateAtFinalDestinationInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_GoodsDescriptionInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_GoodsDescriptionInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override bool JE_OwnerRef_ReadOnly
		{
			get { return ShouldBeReadonly; }
		}

		public override ZPropertyInfo JE_TotalWeightInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_TotalWeightInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_TotalWeightUnitInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_TotalWeightUnitInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_TotalNoOfPacksInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_TotalNoOfPacksInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_TotalNoOfPacksPackTypeInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_TotalNoOfPacksPackTypeInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_ShipmentIncoTermInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_ShipmentIncoTermInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo JE_AgentsReferenceInfo
		{
			get
			{
				ZPropertyInfo result = base.JE_AgentsReferenceInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override bool ReadOnly
		{
			get { return (!Env.CurrentUser.IsBatchProcessor && base.ReadOnly); }
			set { base.ReadOnly = value; }
		}

		bool ShouldBeReadonly
		{
			get { return IsInDatabase && !GlbStaff.CurrentUser.GS_IsController || base.ReadOnly; }
		}

		#endregion

		#endregion

		#region Queue Moving

		public bool IsInCustomsBondingQueue
		{
			get { return CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding; }
		}

		public bool IsInQuarantineHoldQueue
		{
			get { return CurrentQueue.P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.BCA && CurrentQueue.P4_CustomsStatus == ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold; }
		}

		void MoveQueue()
		{
			if (IsIdentifiedAsSplitShipment && IsEntryCompleted)
			{
				if (!HasAlternateBroker)
				{
					MoveToQueue(
						DeclarationQueueCodeDescriptionPairList.Codes.Lodgement,
						ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration,
						ZString.Empty,
						"Is identified as Split Shipment & Entry is Completed with No Alternate Broker");
				}
			}
			else if (IsMessageStatus(CMRMessageCodesForSubmitted))
			{
				MoveToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.Submitted,
					ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration,
					StatusCodeDescriptionPairList.EmptyStatus,
					"Message Status is Submitted");
			}
			else if (IsMessageStatus(CMRMessageCodesForLodgement))
			{
				MoveToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.Lodgement,
					ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration,
					ZString.Empty,
					"Message Status is Lodgement");
			}
			else if (IsEntryStatus(CMRImportEntryAdvice.Processing.Code))
			{
				MoveToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.Pending,
					ZString.Empty,
					ZString.Empty,
					"Entry Status matches with Processing Code");
			}
			else if (IsEntryCompleted)
			{
				MoveToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"Entry is Completed");

				DoManualBillActivitiesOnSave = ManualBillNotification.ShouldDoManualBillActivitiesOnSave;
			}
			else if (IsEntryStatus(CMREntryCodesForUnknown))
			{
				// HELD, WITHDRAWN, REJECTED, MULTI-STATUS
				MoveToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.Unknown,
					ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease,
					ZString.Empty,
					"Unknown Reason; possible Held, Withdraw, Rejected or Multi-status");
			}
		}

		string[] CMREntryCodesForCompleted
		{
			get
			{
				if (fCMREntryCodesForCompleted == null)
				{
					fCMREntryCodesForCompleted = new string[]
					{
						CMRImportEntryAdvice.Finalised.Code,
						CMRImportEntryAdvice.Clear.Code,
						CMRImportEntryAdvice.ATDReceived.Code,
					};
				}
				return fCMREntryCodesForCompleted;
			}
		}

		string[] CMREntryCodesForUnknown
		{
			get
			{
				if (fCMREntryCodesForUnknown == null)
				{
					fCMREntryCodesForUnknown = new string[]
					{
						CMRImportEntryAdvice.Held.Code,
						CMRImportEntryAdvice.Rejected.Code,
						CMRImportEntryAdvice.Withdrawn.Code,
						CMRImportEntryAdvice.MultiStatus.Code,
					};
				}
				return fCMREntryCodesForUnknown;
			}
		}

		string[] CMRMessageCodesForSubmitted
		{
			get
			{
				if (fCMRMessageCodesForSubmitted == null)
				{
					fCMRMessageCodesForSubmitted = new string[]
						{
							CustomsEntryStatus.AwaitingFormalLodge.Code,
							CustomsEntryStatus.AwaitingPayment.Code,
							CustomsEntryStatus.AwaitingSAC.Code,
							CustomsEntryStatus.AwaitingAmendment.Code
						};
				}
				return fCMRMessageCodesForSubmitted;
			}
		}

		string[] CMRMessageCodesForLodgement
		{
			get
			{
				if (fCMRMessageCodesForLodgement == null)
				{
					fCMRMessageCodesForLodgement = new string[]
						{
							CustomsEntryStatus.FailFormalLodge.Code,
							CustomsEntryStatus.FailPayment.Code,
							CustomsEntryStatus.FailSAC.Code,
							CustomsEntryStatus.FailAmendment.Code
						};
				}
				return fCMRMessageCodesForLodgement;
			}
		}

		string[] fCMREntryCodesForCompleted;
		string[] fCMREntryCodesForUnknown;
		string[] fCMRMessageCodesForSubmitted;
		string[] fCMRMessageCodesForLodgement;

		bool IsEntryCompleted
		{
			get { return IsEntryStatus(CMREntryCodesForCompleted); }
		}

		bool IsEntryStatus(params string[] statuses)
		{
			return ((IList)statuses).Contains((string)JE_EntryStatus);
		}

		bool IsMessageStatus(params string[] statuses)
		{
			return ((IList)statuses).Contains((string)JE_MessageStatus);
		}

		public void MoveToQueue(ZString queueName, ZString status, ZString subStatus, ZString reason)
		{
			UPETools.Instance.PerformActionInCorrectBranch(Branch, () => UPETools.Instance.LogQueueMovement(Logs, queueName, reason));

			if (CurrentQueue.P4_CustomsQueue != queueName)
			{
				CurrentQueue.P4_CustomsQueue = queueName;
			}

			if (CurrentQueue.P4_CustomsStatus != status)
			{
				CurrentQueue.P4_CustomsStatus = status;
			}

			if (CurrentQueue.P4_CustomsSubStatus != subStatus)
			{
				CurrentQueue.P4_CustomsSubStatus = subStatus;
			}
		}

		#endregion

		#region Split Shipments

		const string MasterBillAdded = "Masterbill Added =";

		public void TryMoveSplitShipmentToLodgmentQueue()
		{
			if (IsEntryCompleted && !HasAlternateBroker)
			{
				MoveToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.Lodgement,
					ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration,
					ZString.Empty,
					"Split Shipment with Entry Completed and No Alternate Broker");
			}
		}

		public bool TryProcessSplitShipments()
		{
			bool result = false;

			if (IsCandidateForSplitShipmentChecking)
			{
				int billsCount = Bills.Count;

				UPECusHAWB[] associatedHAWBs = GetAssociatedHAWBs();
				if (associatedHAWBs.Length > 1)
				{
					foreach (UPECusHAWB uPECusHAWB in associatedHAWBs)
					{
						string masterbillNumber = uPECusHAWB.MAWB != null ? uPECusHAWB.MAWB.CM_MAWB : ZString.Empty;
						TryAddMasterBillForSplitShipment(masterbillNumber);
					}
				}
				result = billsCount < Bills.Count;
			}

			return result;
		}

		bool IsCandidateForSplitShipmentChecking
		{
			get { return !CMRImportMessageStatusList.IsAwaitingResponse(JE_MessageStatus); }
		}

		public bool TryAddMasterBillForSplitShipment(string masterbillNumber)
		{
			bool result = false;

			if (!string.IsNullOrEmpty(masterbillNumber) && Bills.FindByBillNumberAndType(masterbillNumber, BillTypeList.Codes.MasterBill) == null)
			{
				Customs.Business.Bill bill = Bills.FindByBillNumberAndType(ZString.Empty, BillTypeList.Codes.MasterBill);
				if (bill == null)
				{
					bill = Bills.AddNew();
					bill.CU_BillType = BillTypeList.Codes.MasterBill;
				}
				bill.CU_BillNum = masterbillNumber;
				result = true;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, MasterBillAdded + " " + masterbillNumber);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			return result;
		}

		bool RequiresSplitShipmentAlert
		{
			get
			{
				bool result = false;

				UPECusHAWB[] uPECusHAWBs = GetAssociatedHAWBs();

				if (uPECusHAWBs.Length > 1)
				{
					ZQuery masterBillAddedFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
					masterBillAddedFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, MasterBillAdded);
					StmALog[] masterBillAddedLogs = (StmALog[])Logs.GetAllLogs().Find(masterBillAddedFilter);
					result = masterBillAddedLogs.Length > 0;
				}
				return result;
			}
		}

		#endregion

		#region IsShipperMatchError

		public void ShipperMatchingError()
		{
			if (!CurrentQueue.P4_CustomFlag3)
			{
				CurrentQueue.P4_CustomFlag3 = true;
				CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.EN_ShipperMatchingError, "", "", "");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, "EN Ticked");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region Refund Processing

		#region IRefundEnquiry Members

		public virtual ClientRefund Refund
		{
			get
			{
				if (refund == null)
				{
					refund = RefundManager.Refund;

					if (refund == null)
					{
						ZString controlNumberFromNotes = RefundManager.GetControlNumberFromNotes(Notes);
						if (!controlNumberFromNotes.IsEmpty)
						{
							refund = RefundManager.CreateClientRefund();
							refund.T10_ControlNumber = controlNumberFromNotes;
							refund.Factory.Save();
						}
					}
				}

				return refund;
			}
		}
		ClientRefund refund;

		public IRefundEnquiry RelatedOwner
		{
			get { return Callout; }
		}

		public void AddRefundNote()
		{
			ZString noteText = ZString.Format(
@"CONTROL NUMBER        : {0}
USER NAME             : {1}
DATE                  : {2}
REMARKS               : {3}",
							Refund.T10_ControlNumber,
							GlbStaff.CurrentUser.GS_FullName,
							ZDateTime.Now.ToLongTimeString(),
							IsRefundEnquiry ? "TICKED" : "UN-TICKED");
			Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.RefundNote.Description, noteText);
		}

		void IRefundEnquiry.SendNotificationEmail()
		{
			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, CurrentQueue.P4_GS_NKCustomsTaskAssignedTo));
			if (staff != null && Callout != null)
			{
				Refund.SendNotificationEmail(staff.GS_EmailAddress, Callout.CS_HAWB, Callout.InvoiceNumber, Callout.TotalAmountDue);
			}
		}

		SchemaGuidColumn IRefundEnquiry.OwnerColumn
		{
			get { return ClientRefundSchema.T10_JE; }
		}

		#region IsRefundEnquiry

		public static readonly SchemaBoolColumn IsRefundEnquiryColumn = ProcessQueueSchema.P4_CustomFlag4;

		public ZBool IsRefundEnquiry
		{
			get { return CurrentQueue.P4_CustomFlag4; }
			set
			{
				CurrentQueue.P4_CustomFlag4 = value;
				IsRefundEnquiryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsRefundEnquiryInfo
		{
			get { return GetZPropertyInfo(nameof(IsRefundEnquiry)); }
		}

		[BusinessObjectTestExclude]
		public ZBool IsRefundEnquiryTemp
		{
			get { return IsRefundEnquiry; }
			set
			{
				if (value)
				{
					IsRefundEnquiryTempInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsRefundEnquiryTempInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsRefundEnquiryTemp));
				((IZPropertyInfoObsolete)result).ReadOnly = IsRefundEnquiry || IsRefundProcessed;
				return result;
			}
		}

		#endregion

		#region IsRefundProcessed

		public static readonly SchemaBoolColumn IsRefundProcessedColumn = ProcessQueueSchema.P4_CustomFlag5;

		public ZBool IsRefundProcessed
		{
			get { return CurrentQueue.P4_CustomFlag5; }
			set
			{
				CurrentQueue.P4_CustomFlag5 = value;
				IsRefundProcessedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsRefundProcessedInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsRefundProcessed));
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#endregion

		public RefundManager RefundManager
		{
			get
			{
				if (refundManager == null)
				{
					refundManager = new RefundManager<UPEJobDeclaration>(this);
					refundManager.OnRefundCreated += delegate(ClientRefund refund)
						{
							refund.T10_GS_NKCreatedUser = !CurrentQueue.P4_GS_NKCustomsTaskAssignedTo.IsEmpty ? CurrentQueue.P4_GS_NKCustomsTaskAssignedTo : GlbStaff.CurrentUser.GS_Code;
						};
				}
				return refundManager;
			}
		}
		RefundManager<UPEJobDeclaration> refundManager;

		public ClientRefundWrapper RefundWrapper
		{
			get { return refundWrapper ?? (refundWrapper = new ClientRefundWrapper(Factory) { RefundOwner = this }); }
		}
		ClientRefundWrapper refundWrapper;

		#endregion

		#region Alerts

		public StringCollection AlertsList
		{
			get
			{
				if (fAlertsList == null)
				{
					fAlertsList = new StringCollection();
					if (RequiresSplitShipmentAlert)
					{
						fAlertsList.Add("Split Shipment");
					}
					if (RequiresFreeDomicileAlert)
					{
						fAlertsList.Add("Free Domicile");
					}
					if (RequiresLocalChargesAboveThresholdAlert)
					{
						fAlertsList.Add("Local Charges are above $3000");
					}
					if (RequiresPreferredCustomerAccountAlert)
					{
						fAlertsList.Add("Importer is a Preferred Customer");
					}
					if (Importer != null)
					{
						if (Importer.IsHighClaimer)
						{ fAlertsList.Add("Importer is a High Claimer"); }
						if (Importer.LetterOfAuthorityDaysToExpiration <= 30)
						{ fAlertsList.AddRange(LetterOfAuthorityAlerts); }
						if (Importer.IsPreReleaseContactFeeApplicable)
						{ fAlertsList.Add("Pre-Release Notification"); }
					}
					if (DocManagerInfo.HasUnreadRelatedDocuments)
					{
						fAlertsList.Add("There are unread eDocs attached to this job");
					}
				}
				return fAlertsList;
			}
		}
		StringCollection fAlertsList;

		string[] LetterOfAuthorityAlerts
		{
			get
			{
				ArrayList result = new ArrayList();
				if (Importer.LetterOfAuthorityExpirationDate.IsEmpty)
				{
					result.Add("No Letter of Authority on file");
				}
				else if (Importer.LetterOfAuthorityDaysToExpiration == 0)
				{
					result.Add("Letter of Authority expired today");
				}
				else if (Importer.LetterOfAuthorityDaysToExpiration <= 0)
				{
					result.Add("Letter of Authority expired " + -Importer.LetterOfAuthorityDaysToExpiration + " days ago");
				}
				else
				{
					result.Add("Letter of Authority will expire in " + Importer.LetterOfAuthorityDaysToExpiration + " days");
				}
				ZDateTime letterOfAuthorityDocumentDelivered = Importer.LetterOfAuthorityDocumentDelivered;
				if (Importer.LetterOfAuthorityDocumentDelivered.IsValid)
				{
					result.Add("Letter of Authority document was last delivered " + Importer.LetterOfAuthorityDocumentDelivered.ToShortDateString());
				}
				return (string[])result.ToArray(typeof(string));
			}
		}

		public bool HasAlerts
		{
			get { return AlertsList.Count > 0; }
		}

		public void ResetAlertsList()
		{
			fAlertsList = null;
		}

		bool RequiresFreeDomicileAlert
		{
			get
			{
				foreach (UPECusHAWB uPECusHAWB in RelatedCusHAWBs)
				{
					if (uPECusHAWB.BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreeDomicile)
					{
						return true;
					}
				}

				return false;
			}
		}

		bool RequiresLocalChargesAboveThresholdAlert
		{
			get
			{
				foreach (UPECusHAWB uPECusHAWB in RelatedCusHAWBs)
				{
					if (uPECusHAWB.TotalLocalCharges >= 3000)
					{
						return true;
					}
				}
				return false;
			}
		}

		bool RequiresPreferredCustomerAccountAlert
		{
			get { return Importer != null && Importer.AccountClass == "2"; }
		}

		#endregion

		#region Related CusHAWBs

		public UPECusHAWB FirstCusHAWB
		{
			get
			{
				if (fFirstCusHAWB == null || fFirstCusHAWB.IsDeleted)
				{
					ZQuery filter = new ZQuery();
					filter.MaximumRows = 1;

					UPECusHAWB[] hAWBs = null;
					if (fRelatedCusHAWBs != null)
					{
						hAWBs = (UPECusHAWB[])RelatedCusHAWBs.Find(filter);
					}

					if (hAWBs == null || hAWBs.Length == 0)
					{
						hAWBs = GetAssociatedHAWBs(filter);
					}

					fFirstCusHAWB = (hAWBs.Length > 0) ? hAWBs[0] : null;
				}

				return fFirstCusHAWB;
			}
		}
		UPECusHAWB fFirstCusHAWB;

		[ChildEditable(true)]
		public UPECusHAWBCollection RelatedCusHAWBs
		{
			get
			{
				if (fRelatedCusHAWBs == null)
				{
					fRelatedCusHAWBs = new UPECusHAWBCollection(Factory);
					fRelatedCusHAWBs.Load(CusHAWBFilter);
					RegisterEditableChildObject(fRelatedCusHAWBs);
				}

				return fRelatedCusHAWBs;
			}
		}
		UPECusHAWBCollection fRelatedCusHAWBs;

		public void ResetRelatedCusHAWBs()
		{
			fRelatedCusHAWBs = null;
		}

		public UPECusHAWB[] GetAssociatedHAWBs()
		{
			return GetAssociatedHAWBs(null);
		}

		UPECusHAWB[] GetAssociatedHAWBs(ZQuery additionalFilter)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(CusHAWBFilter);
			if (additionalFilter != null)
			{
				filter.AddToFilter(additionalFilter);
			}
			return Factory.Load<UPECusHAWB>(filter);
		}

		ZQuery CusHAWBFilter
		{
			get { return fCusHAWBFilter ?? (fCusHAWBFilter = new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, PK)); }
		}
		ZQuery fCusHAWBFilter;

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				ArrayList list = new ArrayList();
				foreach (BusinessObject bizObj in base.BusinessObjectsWithRelatedNotes)
				{
					if (bizObj != null)
					{
						list.Add(bizObj);
					}
				}
				list.AddRange(RelatedCusHAWBs);
				return (BusinessObject[])list.ToArray(typeof(BusinessObject));
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList list = new ArrayList();
				foreach (BusinessObject bizObj in base.BusinessObjectsWithRelatedNotes)
				{
					if (bizObj != null)
					{
						list.Add(bizObj);
					}
				}
				list.AddRange(RelatedCusHAWBs);
				return (BusinessObject[])list.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region ILineKey Members

		ZString ILineKey.ShipmentRef
		{
			get { return JE_AgentsReference; }
		}

		ZString ILineKey.FlightNo
		{
			get { return JE_VoyageFlightNo; }
		}

		ZDateTime ILineKey.ImportDate
		{
			get { return JE_DateOfFirstArrival; }
		}

		ZString ILineKey.ConsigneePostCode
		{
			get
			{
				ZString result = "";
				if (FirstCusHAWB != null)
				{
					result = FirstCusHAWB.Consignee != null ? FirstCusHAWB.Consignee.MainAddress.OA_PostCode : FirstCusHAWB.CS_ConsigneePostcode;
				}
				return result;
			}
		}

		#endregion

		protected override RatingAdaptersProvider GetRatingAdaptersProviderCore()
		{
			return new UPEJobDeclarationRatingAdaptersProvider(this);
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new UPEJobDeclarationRatingAdapter<UPEJobDeclaration>(this);
		}

		#region IUPEDocumentSupportable Members

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new UPEJobDeclarationDocumentSupporter(this);
		}

		void IUPEDocumentSupportable.OnPrintBatchItemQueued(PrintBatchItemQueuedEventArgs e)
		{
			if (FirstCusHAWB != null)
			{
				((IUPEDocumentSupportable)FirstCusHAWB).OnPrintBatchItemQueued(e);
			}
		}

		#endregion
	}
}
