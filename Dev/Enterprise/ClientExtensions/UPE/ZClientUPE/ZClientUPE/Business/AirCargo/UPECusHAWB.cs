using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.CMR;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Client.UPE.Business.GSSi;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	[DependentBusinessObject(null, "")] // Unless some work is done here on strongly typed collections, Factory.
	[Enterprise.Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.NotApplied, "Enterprise.Client.UPE.Metadata.UPECusHAWB, ZClientUPE, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")]
	public partial class UPECusHAWB : CusHAWB, IShipmentData, IUPEDocumentSupportable, ICommercialInvoiceSupportable, IBisiUpload
	{
		#region Schema

		public new class Schema : CusHAWB.Schema
		{
			public const string RequiresConsigneeMatch = "RequiresConsigneeMatch";
			public const string RequiresConsignorMatch = "RequiresConsignorMatch";
			public const string WayBillShort = "WayBillShort";
		}

		#endregion

		public UPECusHAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OrgMatchApprovalLoader = new OrgMatchApproval.Loader(factory);
		}

		#region Business Object Overrides

		protected override void DeleteCore()
		{
			base.DeleteCore();
			UPEPrintBatchItem.CascadeDeleteItems(this);
			if (BISIUploadedShipmentHeader != null)
			{
				BISIUploadedShipmentHeader.Delete();
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new UPECusHAWBFetchStrategy(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Intervention;
			CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease;
			CurrentQueue.HasChanges = false;
		}
		public override Notes Notes => fNotes ?? (fNotes = new UPENotes(this));

		protected override void OnFactorySaving()
		{
			StoreLevel1RecordInNote();
			base.OnFactorySaving();

			if (CurrentQueue.P4_StatusInfo.HasChanges)
			{
				commercialQueueStatusChange = true;
			}

			if (!IsInDatabase || CurrentQueue.P4_CustomsStatusInfo.HasChanges)
			{
				customsQueueStatusChange = true;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			CascadeUpdateAddresses();
		}
		internal bool IsMasterHouseBillDifferent
		{
			get
			{
				bool result = false;
				if (IsInDatabase && MAWB != null)
				{
					ZString oldValue = CS_MasterHouseBillInfo.OriginalValue.IsEmpty ? (ZString)MAWB.CM_MasterHouseBillInfo.OriginalValue : (ZString)CS_MasterHouseBillInfo.OriginalValue;
					ZString newValue = AggregatedCoLoadMaster;
					result = oldValue.ToUpper() != newValue.ToUpper();
				}
				return result;
			}
		}

		protected override Customs.Business.CusHAWBValidation GetNewValidation()
		{
			var result = base.GetNewValidation();

			if (result is CMRCusHAWBValidation)
			{
				result = new UPECMRCusHAWBValidation(this);
			}

			result.Add(UPECusHAWBValidation);
			return result;
		}

		public new UPECusHAWBLookups Lookups
		{
			get { return (UPECusHAWBLookups)base.Lookups; }
		}

		protected override Customs.Business.CusHAWBLookups GetNewLookups()
		{
			return new UPECusHAWBLookups(this);
		}

		protected UPECusHAWBValidation UPECusHAWBValidation
		{
			get
			{
				if (fUPECusHAWBValidation == null)
				{
					fUPECusHAWBValidation = new UPECusHAWBValidation(this);
				}
				return fUPECusHAWBValidation;
			}
		}
		UPECusHAWBValidation fUPECusHAWBValidation;

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();
			if (IsInDatabase && HandleMultipleBusinessObjectsAroundOneRow && ((ZBool)IsHoldForCollectionInfo.OriginalValue != IsHoldForCollection || !CurrentQueue.EIRRaisedLog.IsEmpty) && Logs.AutoCreatedLog != null)
			{
				using (((IUpdateFieldsLock)Logs.AutoCreatedLog).LockForUpdatingKeyFields())
				{
					if ((ZBool)IsHoldForCollectionInfo.OriginalValue != IsHoldForCollection)
					{
						string referenceToAppend = Logs.AutoCreatedLog.SL_Reference.IsEmpty ? string.Empty : "; ";
						referenceToAppend += "HFC:" + IsHoldForCollection.ToString();
						Logs.AutoCreatedLog.SL_Reference += referenceToAppend;
					}

					if (!CurrentQueue.EIRRaisedLog.IsEmpty)
					{
						Logs.AutoCreatedLog.SL_Reference += (Logs.AutoCreatedLog.SL_Reference.IsEmpty ? string.Empty : "; ") + CurrentQueue.EIRRaisedLog;
					}
				}
			}
		}

		internal bool HandleMultipleBusinessObjectsAroundOneRow
		{
			get
			{
				bool isCallout = this is Callout;
				return (Factory.BusinessObjectsInformation.Contains("Enterprise.Client.UPE.Business.Callout") && isCallout) ||
					(!Factory.BusinessObjectsInformation.Contains("Enterprise.Client.UPE.Business.Callout") && !isCallout);
			}
		}

		#endregion

		#region New Properties

		#region Level 1 Record

		public Level1Record Level1Record
		{
			get
			{
				if (fLevel1Record == null)
				{
					ZString level1RecordAsString = Level1RecordNote.Text;
					if (level1RecordAsString.Length > 0)
					{
						fLevel1Record = new Level1Record();
						fLevel1Record.AddRecordLines(level1RecordAsString.Split('\n'));
					}
				}
				return fLevel1Record;
			}
			set
			{
				fLevel1Record = value;
			}
		}
		Level1Record fLevel1Record;

		internal Level1RecordNote Level1RecordNote
		{
			get
			{
				if (fLevel1RecordNote == null)
				{
					fLevel1RecordNote = new Level1RecordNote(this);
				}
				return fLevel1RecordNote;
			}
		}
		Level1RecordNote fLevel1RecordNote;

		void StoreLevel1RecordInNote()
		{
			if (fLevel1Record != null)
			{
				Level1RecordNote.Text = fLevel1Record.ToString();
			}
		}

		#endregion

		#region HasFormalDec

		public ZBool HasFormalDec
		{
			get { return !CS_JE_CustomsFormalEntry.IsEmpty; }
		}

		public ZPropertyInfo HasFormalDecInfo
		{
			get { return GetZPropertyInfo(nameof(HasFormalDec)); }
		}

		#endregion

		#region WayBillShort

		[MaxLength(JobRelatedWayBill.Schema.EB_WaybillShortNumberMaxLength)]
		public ZString WayBillShort
		{
			get
			{
				ZString result;
				if (NonPersistingParentWayBill == null)
				{
					result = (CS_HAWB.Length <= JobRelatedWayBillSchema.EB_WaybillShortNumber.MaxLength) ? CS_HAWB : ZString.Empty;
				}
				else
				{
					result = NonPersistingParentWayBill.EB_WaybillShortNumber;
				}
				return result;
			}
			set
			{
				ParentWayBill.EB_WaybillShortNumber = value;
				WayBillShortInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WayBillShortInfo
		{
			get { return GetZPropertyInfo(nameof(WayBillShort)); }
		}

		JobRelatedWayBill ParentWayBill
		{
			get
			{
				if (fParentWayBill == null)
				{
					fParentWayBill = LoadOrCreateParentWayBill();
					RegisterEditableChildObject(fParentWayBill);
				}
				return fParentWayBill;
			}
		}

		JobRelatedWayBill NonPersistingParentWayBill
		{
			get
			{
				if (fParentWayBill == null)
				{
					fParentWayBill = LoadParentWayBill();
					RegisterEditableChildObject(fParentWayBill);
				}
				return fParentWayBill;
			}
		}

		JobRelatedWayBill LoadOrCreateParentWayBill()
		{
			return LoadParentWayBill() ?? CreateParentWayBill();
		}

		JobRelatedWayBill CreateParentWayBill()
		{
			JobRelatedWayBill result = (JobRelatedWayBill)Factory.New(typeof(JobRelatedWayBill));
			result.EB_ParentID = PK;
			result.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Parent;
			return result;
		}

		JobRelatedWayBill LoadParentWayBill()
		{
			ZQuery filter = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, PK);
			filter.FetchOnlyFromLocalCache = !IsInDatabase;
			filter.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Parent);
			return (JobRelatedWayBill)Factory.LoadTop1(typeof(JobRelatedWayBill), filter);
		}

		JobRelatedWayBill fParentWayBill;

		#endregion

		#region DutyType

		public ZString DutyType
		{
			get { return CurrentQueue.P4_CustomAttrib6; }
			set { CurrentQueue.P4_CustomAttrib6 = value; }
		}

		public virtual ZPropertyInfo DutyTypeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DutyType), x => CurrentQueue.P4_CustomAttrib6Info); }
		}

		protected int DutyType_MaxLength
		{
			get { return 3; }
		}

		#endregion

		#region IsAlternateBrokerSplitShipment

		public ZBool IsAlternateBrokerSplitShipment
		{
			get
			{
				bool result = false;
				if (Declaration != null && Declaration.AlternateBroker != null)
				{
					UPECusHAWB[] relatedCusHAWBs = (UPECusHAWB[])Factory.Load(typeof(UPECusHAWB), new ZQuery(CusHAWBSchema.CS_HAWB, CS_HAWB));
					result = relatedCusHAWBs.Length >= 2;
				}
				return result;
			}
		}

		#endregion

		#region BISI Upload Date

		public static readonly SchemaDateTimeColumn BisiUploadDateProcessQueueColumn = ProcessQueueSchema.P4_CustomDate1;

		ZDateTime IBisiUpload.TransferredDateTime
		{
			get { return BisiUploadDate; }
			set { BisiUploadDate = value; }
		}

		public ZDateTime BisiUploadDate
		{
			get { return CurrentQueue.P4_CustomDate1; }
			set { CurrentQueue.P4_CustomDate1 = value; }
		}

		public ZPropertyInfo BisiUploadDateInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(BisiUploadDateInfo), x => CurrentQueue.P4_CustomDate1Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region DeliveryDate

		public ZDateTime DeliveryDate
		{
			get { return CurrentQueue.P4_CustomDate3; }
			set { CurrentQueue.P4_CustomDate3 = value; }
		}

		public ZPropertyInfo DeliveryDateInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(DeliveryDate), x => CurrentQueue.P4_CustomDate3Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region WeightInKg

		public ZDecimal WeightInKg
		{
			get { return (CS_WeightUQ == Core.Constants.Weight.Kilograms || !Core.Constants.Weight.ContainsCode(CS_WeightUQ)) ? CS_Weight : (ZDecimal)Core.Constants.Weight.Convert(CS_Weight, CS_WeightUQ, Core.Constants.Weight.Kilograms); }
		}

		public ZPropertyInfo WeightInKgInfo
		{
			get { return GetZPropertyInfo(nameof(WeightInKg)); }
		}

		#endregion

		#region InvoiceNumber

		public static readonly SchemaStringColumn InvoiceNumberProcessQueueColumn = ProcessQueueSchema.P4_CustomAttrib1;

		public ZString InvoiceNumber
		{
			get { return CurrentQueue.P4_CustomAttrib1; }
			set { CurrentQueue.P4_CustomAttrib1 = value; }
		}

		public ZPropertyInfo InvoiceNumberInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(InvoiceNumber), x => CurrentQueue.P4_CustomAttrib1Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region Consignor / Consignee AccountNum

		#region ConsignorAccountNum

		public ZString ConsignorAccountNum
		{
			get { return Consignor == null ? Level1RecordConsignorAccountNum : Consignor.AccountNumber; }
		}

		public ZPropertyInfo ConsignorAccountNumInfo
		{
			get { return GetZPropertyInfo(nameof(ConsignorAccountNum)); }
		}

		#endregion

		#region ConsigneeAccountNum

		public ZString ConsigneeAccountNum
		{
			get { return Consignee == null ? Level1RecordConsigneeAccountNum : Consignee.AccountNumber; }
		}

		public ZPropertyInfo ConsigneeAccountNumInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneeAccountNum)); }
		}

		#endregion

		#region Level1RecordConsignorAccountNum

		public ZString Level1RecordConsignorAccountNum
		{
			get { return (Level1Record != null && Level1Record._300000 != null) ? Level1Record._300000.AccountNumber : string.Empty; }
		}

		public ZPropertyInfo Level1RecordConsignorAccountNumInfo
		{
			get { return GetZPropertyInfo(nameof(Level1RecordConsignorAccountNum)); }
		}

		#endregion

		#region Level1RecordConsigneeAccountNum

		public ZString Level1RecordConsigneeAccountNum
		{
			get { return (Level1Record != null && Level1Record._400000 != null) ? Level1Record._400000.AccountNumber : string.Empty; }
		}

		public ZPropertyInfo Level1RecordConsigneeAccountNumInfo
		{
			get { return GetZPropertyInfo(nameof(Level1RecordConsigneeAccountNum)); }
		}

		#endregion

		#endregion

		#region BillToAccountNumber

		public static readonly SchemaStringColumn BillToAccountNumberProcessQueueColumn = ProcessQueueSchema.P4_CustomAttrib2;

		public virtual ZString BillToAccountNumber
		{
			get { return CurrentQueue.P4_CustomAttrib2; }
			set { CurrentQueue.P4_CustomAttrib2 = value; }
		}

		public ZPropertyInfo BillToAccountNumberInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(BillToAccountNumber), x => CurrentQueue.P4_CustomAttrib2Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region BillingTerms

		[MaxLength(3)]
		public ZString BillingTerms
		{
			get { return CurrentQueue.P4_CustomAttrib3; }
			set
			{
				if (CurrentQueue.P4_CustomAttrib3 != value)
				{
					CheckMaximumLength(BillingTermsInfo, value);
					CurrentQueue.P4_CustomAttrib3 = value;
					SetCustomsBillingTerm();
				}

				if (!IsValidationSuspended)
				{
					UPECusHAWBValidation.ValidateBillingTerms();
				}
				BillingTermsInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo BillingTermsInfo
		{
			get { return GetZPropertyInfo(nameof(BillingTerms)); }
		}

		protected internal bool IsBillingTermsFreeDomicile
		{
			get { return BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreeDomicile; }
		}

		void SetCustomsBillingTerm()
		{
			switch (BillingTerms)
			{
				case BillingTermsCodeDescriptionPairList.Codes.CostAndFreight:
				case BillingTermsCodeDescriptionPairList.Codes.FreeDomicile:
				case BillingTermsCodeDescriptionPairList.Codes.SplitDutyAndVat:
				case BillingTermsCodeDescriptionPairList.Codes.Prepaid:
					base.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.PrepaidOnly;
					break;

				case BillingTermsCodeDescriptionPairList.Codes.FreeBorder:
				case BillingTermsCodeDescriptionPairList.Codes.FreightCollect:
					base.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.Collect;
					break;

				case BillingTermsCodeDescriptionPairList.Codes.FreeOnBoard:
					base.CS_FreightPrepaidCollect = CMRMethodsOfPayment.Codes.FobPortOfCall;
					break;
			}
		}

		#endregion

		#region ShipmentType

		public ZString ShipmentType
		{
			get { return CurrentQueue.P4_CustomAttrib4; }
			set
			{
				bool isDiff = CurrentQueue.P4_CustomAttrib4 != value;
				CurrentQueue.P4_CustomAttrib4 = value;
				if (isDiff)
				{
					SetCustomsShipmentType();
				}
			}
		}

		public virtual ZPropertyInfo ShipmentTypeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ShipmentType), x => CurrentQueue.P4_CustomAttrib4Info); }
		}

		protected int ShipmentType_MaxLength
		{
			get { return 3; }
		}

		void SetCustomsShipmentType()
		{
			if (ShipmentType == ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments)
			{
				CS_ShipmentType = "STD";
			}
			else
			{
				CS_ShipmentType = "DOC";
			}
		}

		#endregion

		#region Rebill Flags

		#region IsFreeDomicile

		public ZBool IsFreeDomicile
		{
			get { return RebillFlag == RebillFlags.IsChangedToFreeDomicile; }
			set
			{
				if (value)
				{
					RebillFlag = RebillFlags.IsChangedToFreeDomicile;
				}
				else
				{
					RebillFlag = RebillFlags.Unflagged;
				}

				IsFreeDomicileInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsFreeDomicileInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsFreeDomicile));
				((IZPropertyInfoObsolete)result).ReadOnly = IsFreeDomicileReadOnly;
				return result;
			}
		}

		protected virtual bool IsFreeDomicileReadOnly
		{
			get { return !IsFreeDomicile && !IsUnflagged; }
		}

		#endregion

		#region IsTranshipment

		public ZBool IsTranshipment
		{
			get { return RebillFlag == RebillFlags.IsTranshipment; }
			set
			{
				if (value)
				{
					RebillFlag = RebillFlags.IsTranshipment;
				}
				else
				{
					RebillFlag = RebillFlags.Unflagged;
				}

				IsTranshipmentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsTranshipmentInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsTranshipment));
				((IZPropertyInfoObsolete)result).ReadOnly = IsTranshipmentReadOnly;
				return result;
			}
		}

		protected virtual bool IsTranshipmentReadOnly
		{
			get { return !IsTranshipment && !IsUnflagged; }
		}

		#endregion

		#region IsAbandoned

		public ZBool IsAbandoned
		{
			get { return RebillFlag == RebillFlags.IsAbandoned; }
			set
			{
				if (value)
				{
					RebillFlag = RebillFlags.IsAbandoned;
				}
				else
				{
					RebillFlag = RebillFlags.Unflagged;
				}

				IsAbandonedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsAbandonedInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsAbandoned));
				((IZPropertyInfoObsolete)result).ReadOnly = IsAbandonedReadOnly;
				return result;
			}
		}

		protected virtual bool IsAbandonedReadOnly
		{
			get { return !IsAbandoned && !IsUnflagged; }
		}

		#endregion

		#region IsRTS

		public ZBool IsRTS
		{
			get { return RebillFlag == RebillFlags.IsRTS; }
			set
			{
				if (value)
				{
					RebillFlag = RebillFlags.IsRTS;
				}
				else
				{
					RebillFlag = RebillFlags.Unflagged;
				}

				IsRTSInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsRTSInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(IsRTS));
				((IZPropertyInfoObsolete)result).ReadOnly = IsRTSReadOnly;
				return result;
			}
		}

		protected virtual bool IsRTSReadOnly
		{
			get { return !IsRTS && !IsUnflagged; }
		}

		#endregion

		public RebillFlags RebillFlag
		{
			get { return (RebillFlags)((int)CurrentQueue.P4_CustomDecimal1.ToZInt()); }
			set
			{
				ZDecimal value1 = (int)value;
				if (CurrentQueue.P4_CustomDecimal1 != value1)
				{
					RebillFlagChangingEventArgs eventArgs = new RebillFlagChangingEventArgs(value);
					OnRebillFlagChanging(eventArgs);
					if (!eventArgs.Cancel)
					{
						CurrentQueue.P4_CustomDecimal1 = value1;
						OnRebillFlagChanged();
					}
					RefreshBinding();
				}
			}
		}

		public event RebillFlagChangingEventHandler RebillFlagChanging;

		void OnRebillFlagChanging(RebillFlagChangingEventArgs eventArgs)
		{
			if (RebillFlagChanging != null)
			{
				RebillFlagChanging(eventArgs);
			}
		}

		void OnRebillFlagChanged()
		{
			BillingTerms = BillingTermsCodeDescriptionPairList.Codes.FreeDomicile;
			if (IsAbandoned)
			{
				MoveDeclarationToQueue(
					true,
					DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding,
					ZString.Empty,
					ZString.Empty,
					"REBILL FLAG CHANGED: IS ABANDONED");
			}
			else if (IsFreeDomicile)
			{
				MoveDeclarationToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.Classification,
					ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration,
					ZString.Empty,
					"REBILL FLAG CHANGED: IS FREE DOM");
			}
		}

		bool IsUnflagged
		{
			get { return RebillFlag == RebillFlags.Unflagged; }
		}

		#endregion

		#region Status Flags

		public bool DoesStatusIndicateTranshipment
		{
			get
			{
				return CS_CustomsStatus == CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus
					|| CS_CustomsStatus == CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			}
		}

		#endregion

		#region IsHoldForCollection
		[ReadOnlyMember(nameof(IsHoldForCollection_ReadOnly))]
		public ZBool IsHoldForCollection
		{
			get { return CurrentQueue.P4_CustomFlag5; }
			set
			{
				CurrentQueue.P4_CustomFlag5 = value;
				if (!IsHoldForCollection)
				{
					HoldForCollectDepot = string.Empty;
					HFCContactName = string.Empty;
					HFCContactPhoneNumber = string.Empty;
				}
				HoldForCollectDepotInfo.RefreshBinding();
				MarkAsNeedingValidation();
			}
		}

		public ZPropertyInfo IsHoldForCollectionInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(IsHoldForCollection), x => CurrentQueue.P4_CustomFlag5Info);
				return result;
			}
		}

		public ZBool IsHoldForCollection_ReadOnly
		{
			get { return HasAlternateBroker && !Declaration.AlternateBroker.IsDeliveryHandledByUPSForThisAlternateBroker; }
		}
		#endregion

		#region HoldForCollectDepot
		[ReadOnlyMember(nameof(IsHoldForCollectDepot_ReadOnly))]
		[BusinessObjectTestExclude()]
		public ZString HoldForCollectDepot
		{
			get
			{
				switch (CurrentQueue.P4_CustomDecimal3.ToZInt())
				{
					case 1: return HFCDepotCodeDescriptionPairList.Codes.Sydney;
					case 2: return HFCDepotCodeDescriptionPairList.Codes.Melbourne;
					case 3: return HFCDepotCodeDescriptionPairList.Codes.Brisbane;
					case 4: return HFCDepotCodeDescriptionPairList.Codes.Perth;
					case 5: return HFCDepotCodeDescriptionPairList.Codes.Adelaide;
					case 6: return HFCDepotCodeDescriptionPairList.Codes.Tasmania;
					default: return string.Empty;
				}
			}
			set
			{
				ZDecimal newValue = ZDecimal.Zero;
				switch (value)
				{
					case HFCDepotCodeDescriptionPairList.Codes.Sydney:
						newValue = 1m;
						break;

					case HFCDepotCodeDescriptionPairList.Codes.Melbourne:
						newValue = 2m;
						break;

					case HFCDepotCodeDescriptionPairList.Codes.Brisbane:
						newValue = 3m;
						break;

					case HFCDepotCodeDescriptionPairList.Codes.Perth:
						newValue = 4m;
						break;

					case HFCDepotCodeDescriptionPairList.Codes.Adelaide:
						newValue = 5m;
						break;

					case HFCDepotCodeDescriptionPairList.Codes.Tasmania:
						newValue = 6m;
						break;

					default:
						newValue = 0m;
						break;
				}
				CurrentQueue.P4_CustomDecimal3 = newValue;
				HoldForCollectDepotInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HoldForCollectDepotInfo
		{
			get
			{
				((IZPropertyInfoObsolete)CurrentQueue.P4_CustomDecimal3Info).ReadOnly = !IsHoldForCollection;
				return GetWrappedZPropertyInfo(nameof(HoldForCollectDepot), x => CurrentQueue.P4_CustomDecimal3Info);
			}
		}

		public bool IsHoldForCollectDepot_ReadOnly
		{
			get { return !IsHoldForCollection; }
		}
		#endregion

		#region HFCContactName

		public ZString HFCContactName
		{
			get { return CurrentQueue.P4_CustomAttrib10; }
			set { CurrentQueue.P4_CustomAttrib10 = value; }
		}

		public ZPropertyInfo HFCContactNameInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(HFCContactName), x => CurrentQueue.P4_CustomAttrib10Info);
				((IZPropertyInfoObsolete)result).ReadOnly = !IsHoldForCollection;
				((IZPropertyInfoObsolete)CurrentQueue.P4_CustomAttrib10Info).ReadOnly = !IsHoldForCollection;
				return result;
			}
		}

		#endregion

		#region HFCContactPhoneNumber

		public ZString HFCContactPhoneNumber
		{
			get { return CurrentQueue.P4_CustomAttrib9; }
			set { CurrentQueue.P4_CustomAttrib9 = value; }
		}

		public ZPropertyInfo HFCContactPhoneNumberInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(HFCContactPhoneNumber), x => CurrentQueue.P4_CustomAttrib9Info);
				((IZPropertyInfoObsolete)result).ReadOnly = !IsHoldForCollection;
				((IZPropertyInfoObsolete)CurrentQueue.P4_CustomAttrib9Info).ReadOnly = !IsHoldForCollection;
				return result;
			}
		}

		#endregion

		#region COD and Brown Post Code

		public bool IsConsigneeOrDeliveryAddressPostcodeCOD
		{
			get { return IsConsigneeOrDeliveryAddressWithinZone(CODPostcodeTransportProvider); }
		}

		public bool IsConsigneeOrDeliveryAddressPostcodeBrown
		{
			get { return IsConsigneeOrDeliveryAddressWithinZone(BrownPostcodeTransportProvider); }
		}

		public bool IsCOD
		{
			get { return PaymentMethod == UPECargoPaymentMethod.Cheque || IsConsigneeOrDeliveryAddressPostcodeBrown; }
		}

		bool IsConsigneeOrDeliveryAddressWithinZone(UPERateTransportProvider transportProvider)
		{
			if (transportProvider != null && !transportProvider.IsDeleted)
			{
				var postCode = IsRedirected ? DeliveryAddressOverride.P3_PostCode : CS_ConsigneePostcode;
				var zoneItem = RateTransportZoneHelper.GetZoneItemForPostCode(transportProvider, postCode);

				return zoneItem != null;
			}

			return false;
		}

		UPERateTransportProvider CODPostcodeTransportProvider
		{
			get { return codPostcodeTransportProvider ?? (codPostcodeTransportProvider = UPERateTransportProvider.LoadCODPostcodeTransportProvider(Factory)); }
		}
		UPERateTransportProvider codPostcodeTransportProvider;

		UPERateTransportProvider BrownPostcodeTransportProvider
		{
			get { return brownPostcodeTransportProvider ?? (brownPostcodeTransportProvider = UPERateTransportProvider.LoadBrownPostcodeTransportProvider(Factory)); }
		}
		UPERateTransportProvider brownPostcodeTransportProvider;

		#endregion

		#region Payment Method

		public UPECargoPaymentMethod PaymentMethod
		{
			get { return (UPECargoPaymentMethod)((int)CurrentQueue.P4_CustomDecimal2.ToZInt()); }
			set { CurrentQueue.P4_CustomDecimal2 = (int)value; }
		}

		public UPECargoPaymentMethod PaymentMethodOriginalValue
		{
			get { return (UPECargoPaymentMethod)((int)((ZDecimal)CurrentQueue.P4_CustomDecimal2Info.OriginalValue).ToZInt()); }
		}

		#endregion

		#region IsSubsequentSplitShipment

		public ZBool IsSubsequentSplitShipment
		{
			get
			{
				return CurrentQueue.P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Completed &&
					CurrentQueue.P4_Status == ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			}
			set
			{
				if (value)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Completed,
						ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment,
						ZString.Empty,
						"SET TO SUBSEQUENT SPLIT SHIPMENT");
				}
			}
		}

		#endregion

		#region SplitShipments

		public bool IsSplitShipment
		{
			get
			{
				ZQuery stmALogFilter = new ZQuery(StmALogSchema.SL_Parent, CurrentQueue.PK);
				stmALogFilter.AddToFilter(StmALogSchema.SL_Reference, "CUS\"\",\"DN\",\"\",\"\",\"\"");
				return Factory.GetDatabaseCount(typeof(StmALog), stmALogFilter) == 1;
			}
		}

		public void SetSplitShipment()
		{
			CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.DN_SplitShipment, "", "", "");
		}

		#endregion

		#region DeclarationQueueStatus

		public ZString DeclarationQueueStatus
		{
			get
			{
				ZString result = "NO DEC";
				if (Declaration != null)
				{
					DeclarationQueueCodeDescriptionPairList list = new DeclarationQueueCodeDescriptionPairList();
					result = list.GetDescriptionFromCode(Declaration.CurrentQueue.P4_CustomsQueue);
				}
				return result;
			}
		}

		public ZPropertyInfo DeclarationQueueStatusInfo
		{
			get { return GetZPropertyInfo(nameof(DeclarationQueueStatus)); }
		}

		#endregion

		#endregion

		#region Property Overrides

		public new UPECargoReportQueue CurrentQueue
		{
			get
			{
				UPECargoReportQueue currentQueue = (UPECargoReportQueue)base.CurrentQueue;
				if (cachedCurrentQueue != currentQueue)
				{
					UnRegisterListChangedCalledRefreshBinding(cachedCurrentQueue);
					cachedCurrentQueue = currentQueue;
					RegisterListChangedCalledRefreshBinding(cachedCurrentQueue);
				}
				return cachedCurrentQueue;
			}
		}
		UPECargoReportQueue cachedCurrentQueue;

		public new UPEOrgHeader Consignee
		{
			get
			{
				UPEOrgHeader result = (UPEOrgHeader)base.Consignee;

				if (result == null && !Level1RecordConsigneeAccountNum.IsEmpty)
				{
					result = (UPEOrgHeader)UPEOrgHeader.FindByOrgCusCode(Factory, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, Level1RecordConsigneeAccountNum);
				}

				return result;
			}
		}

		public override ZString CS_ConsigneePostcode
		{
			get
			{
				return base.CS_ConsigneePostcode;
			}
			set
			{
				// We need a workaround as the base class is designed so that CS_ConsigneePostcode is not saved to the db when Consignee is not null.
				var tempValue = value.Length >= CS_ConsigneePostcodeInfo.MaxLength
					? value
					: new ZString(value + " ");
				CheckMaximumLength(CS_ConsigneePostcodeInfo, tempValue);
				SetPropertyValue(CS_ConsigneePostcodeInfo, tempValue);
				base.CS_ConsigneePostcode = value.Trim();
			}
		}

		public new UPEOrgHeader Consignor
		{
			get { return (UPEOrgHeader)base.Consignor; }
		}

		protected override void ReloadCore()
		{
			base.ReloadCore();
			RunPreSaveValidation();
		}

		protected virtual bool ShouldBeReadonly
		{
			get { return IsInDatabase && !GlbStaff.CurrentUser.GS_IsController; }
		}

		public override ZBool CS_IsResponsePending
		{
			get { return base.CS_IsResponsePending; }
			set
			{
				base.CS_IsResponsePending = value;
				if (value)
				{
					MoveCustomsQueueTo(
						CargoReportQueueCodeDescriptionPairList.Codes.Pending,
						ZString.Empty,
						ZString.Empty,
						"SET TO RESPONSE PENDING");
				}
			}
		}

		public override ZPropertyInfo CS_HAWBInfo
		{
			get
			{
				ZPropertyInfo result = base.CS_HAWBInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo CS_MasterHouseBillInfo
		{
			get
			{
				ZPropertyInfo result = base.CS_MasterHouseBillInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo CS_WarehouseLocationInfo
		{
			get
			{
				ZPropertyInfo result = base.CS_WarehouseLocationInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo CS_RS_NK_ServiceLevelInfo
		{
			get
			{
				ZPropertyInfo result = base.CS_RS_NK_ServiceLevelInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo CS_PiecesManifestedInfo
		{
			get
			{
				ZPropertyInfo result = base.CS_PiecesManifestedInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		public override ZPropertyInfo CS_PiecesLandedInfo
		{
			get
			{
				ZPropertyInfo result = base.CS_PiecesLandedInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = ShouldBeReadonly;
				return result;
			}
		}

		protected override ZPropertyInfo InnerPrepaidCollectInfo
		{
			get { return BillingTermsInfo; }
		}

		protected override ZPropertyInfo InnerShipmentTypeInfo
		{
			get { return ShipmentTypeInfo; }
		}

		protected override Type TypeOfProcessQueue
		{
			get { return typeof(UPECargoReportQueue); }
		}

		#endregion

		#region Overridden Captions

		public override ZString CS_PaymentTypeCaption
		{
			get { return "Billing Terms"; }
		}

		public override ZString WarehouseLocationCaption
		{
			get { return "Bond Location:"; }
		}

		public override ZString ChargableWeightCaption
		{
			get { return "Dimensional Weight:"; }
		}

		#endregion

		#region Related Business Objects

		#region CusDecImporter

		public ZGuid CusDecImporterPK
		{
			get { return (CusDecImporter != null) ? CusDecImporter.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo CusDecImporterPKInfo
		{
			get { return GetZPropertyInfo(nameof(CusDecImporterPK)); }
		}

		public UPEOrgHeader CusDecImporter
		{
			get
			{
				UPEOrgHeader result = (Declaration != null) ? Declaration.Importer : null;
				if (result != null)
				{
					result.SetReadOnlyIncludingChildren(true);
				}
				return result;
			}
		}

		#endregion

		#region Declaration

		virtual public new UPEJobDeclaration Declaration
		{
			get { return (UPEJobDeclaration)base.Declaration; }
		}

		#endregion

		#region ConsigneePostcodeZone

		public RateTransportZone ConsigneePostcodeZone
		{
			get
			{
				var filter = new ZQuery(RateTransportProviderSchema.TP_OH_RelatedParty, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var transportProviders = Factory.Load<RateTransportProvider>(filter);

				foreach (RateTransportProvider transportProvider in transportProviders)
				{
					var zoneItem = RateTransportZoneHelper.GetZoneItemForPostCode(transportProvider, CS_ConsigneePostcode);
					if (zoneItem != null)
					{
						return zoneItem.Zone;
					}
				}

				return null;
			}
		}

		#endregion

		#region BISIUploadedShipmentHeader

		public ClientBISIShipmentHeader BISIUploadedShipmentHeader
		{
			get
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(ClientBISIShipmentHeaderSchema.T8_CS, PK);
				return Factory.LoadTop1<ClientBISIShipmentHeader>(query);
			}
		}

		#endregion

		#region UPEImporterAddress / ImporterMatchApproval

		public OrgPatternMatchAddress UPEImporterAddress
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(OrgPatternMatchAddressSchema.P3_AddressType, OrgPatternMatchAddress.Constants.AddressType.AirCargoImporter);
				filter.AddToFilter(OrgPatternMatchAddressSchema.P3_ParentID, PK);
				return Factory.LoadTop1<OrgPatternMatchAddress>(filter);
			}
		}

		[BusinessObjectTestExclude]
		public ZBool RequiresImporterMatch
		{
			get { return ImporterMatchApproval != null; }
			set
			{
				if (value)
				{
					if (UPEImporterAddress == null)
					{
						ErrorReporter.ReportOnce("ImporterMustBeAvailableToMatchOnIt", "Org match approving was requested on an air cargo record that doesn't have an importer");
					}
					else
					{
						fImporterMatchApproval = (UPECusHAWBImporterMatchApproval)OrgMatchApprovalLoader.LoadOrCreate(UPEImporterAddress, OrgMatchApprovalType.AirCargoImporter);
					}
				}
				else if (ImporterMatchApproval != null)
				{
					ImporterMatchApproval.Delete();
					fImporterMatchApproval = null;
				}
				RequiresImporterMatchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RequiresImporterMatchInfo
		{
			get { return GetZPropertyInfo(nameof(RequiresImporterMatch)); }
		}

		public UPECusHAWBImporterMatchApproval ImporterMatchApproval
		{
			get
			{
				if (fImporterMatchApproval == null)
				{
					fImporterMatchApproval = (UPECusHAWBImporterMatchApproval)OrgMatchApprovalLoader.Load(PK, OrgMatchApprovalType.AirCargoImporter);
				}
				return fImporterMatchApproval;
			}
		}

		UPECusHAWBImporterMatchApproval fImporterMatchApproval;

		#endregion

		#region UPEConsigneeAddress / ConsigneeMatchApproval

		public OrgPatternMatchAddress UPEConsigneeAddress
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(OrgPatternMatchAddressSchema.P3_AddressType, OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignee);
				filter.AddToFilter(JoinCondition.And, OrgPatternMatchAddressSchema.P3_ParentID, SQLComparisonOperator.Equal, PK);
				return (OrgPatternMatchAddress)Factory.LoadTop1(typeof(OrgPatternMatchAddress), filter);
			}
		}

		OrgPatternMatchAddress LoadOrCreateUPEConsigneeAddress()
		{
			OrgPatternMatchAddress result = UPEConsigneeAddress;
			if (result == null)
			{
				result = (OrgPatternMatchAddress)Factory.New(typeof(OrgPatternMatchAddress));
				result.P3_ParentID = PK;
				result.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignee;
			}
			return result;
		}

		public ZBool RequiresConsigneeMatch
		{
			get { return ConsigneeMatchApproval != null; }
			set
			{
				if (value)
				{
					fConsigneeMatchApproval = (UPECusHAWBConsigneeMatchApproval)OrgMatchApprovalLoader.LoadOrCreate(PK, OrgMatchApprovalType.AirCargoConsignee);
				}
				else if (ConsigneeMatchApproval != null)
				{
					ConsigneeMatchApproval.AddressToBeMatched.Delete();
					ConsigneeMatchApproval.Delete();
					fConsigneeMatchApproval = null;
				}
				RequiresConsigneeMatchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RequiresConsigneeMatchInfo
		{
			get { return GetZPropertyInfo(nameof(RequiresConsigneeMatch)); }
		}

		public UPECusHAWBConsigneeMatchApproval ConsigneeMatchApproval
		{
			get
			{
				if (fConsigneeMatchApproval == null)
				{
					fConsigneeMatchApproval = (UPECusHAWBConsigneeMatchApproval)OrgMatchApprovalLoader.Load(PK, OrgMatchApprovalType.AirCargoConsignee);
				}
				return fConsigneeMatchApproval;
			}
		}

		UPECusHAWBConsigneeMatchApproval fConsigneeMatchApproval;

		#endregion

		#region UPEConsignorAddress / ConsignorMatchApproval

		public OrgPatternMatchAddress UPEConsignorAddress
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(OrgPatternMatchAddressSchema.P3_AddressType, OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignor);
				filter.AddToFilter(JoinCondition.And, OrgPatternMatchAddressSchema.P3_ParentID, SQLComparisonOperator.Equal, PK);
				return (OrgPatternMatchAddress)Factory.LoadTop1(typeof(OrgPatternMatchAddress), filter);
			}
		}

		OrgPatternMatchAddress LoadOrCreateUPEConsignorAddress()
		{
			OrgPatternMatchAddress result = UPEConsignorAddress;
			if (result == null)
			{
				result = (OrgPatternMatchAddress)Factory.New(typeof(OrgPatternMatchAddress));
				result.P3_ParentID = PK;
				result.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignor;
			}
			return result;
		}

		public ZBool RequiresConsignorMatch
		{
			get { return ConsignorMatchApproval != null; }
			set
			{
				if (value)
				{
					fConsignorMatchApproval = (UPECusHAWBConsignorMatchApproval)OrgMatchApprovalLoader.LoadOrCreate(PK, OrgMatchApprovalType.AirCargoConsignor);
				}
				else if (ConsignorMatchApproval != null)
				{
					ConsignorMatchApproval.AddressToBeMatched.Delete();
					ConsignorMatchApproval.Delete();
					fConsignorMatchApproval = null;
				}
				RequiresConsignorMatchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RequiresConsignorMatchInfo
		{
			get { return GetZPropertyInfo(nameof(RequiresConsignorMatch)); }
		}

		public UPECusHAWBConsignorMatchApproval ConsignorMatchApproval
		{
			get
			{
				if (fConsignorMatchApproval == null)
				{
					fConsignorMatchApproval = (UPECusHAWBConsignorMatchApproval)OrgMatchApprovalLoader.Load(PK, OrgMatchApprovalType.AirCargoConsignor);
				}
				return fConsignorMatchApproval;
			}
		}

		UPECusHAWBConsignorMatchApproval fConsignorMatchApproval;

		#endregion

		#region ImporterOrConsigneeMatchApproval

		public ZBool RequiresImporterOrConsigneeMatchApproval
		{
			get { return RequiresConsigneeMatch || RequiresImporterMatch; }
			set
			{
				if (value)
				{
					if (UPEConsigneeAddress != null || UPEImporterAddress == null)
					{
						RequiresConsigneeMatch = true;
					}
					else
					{
						RequiresImporterMatch = true;
					}
				}
				else
				{
					RequiresImporterMatch = false;
					RequiresConsigneeMatch = false;
				}
				RequiresImporterOrConsigneeMatchApprovalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RequiresImporterOrConsigneeMatchApprovalInfo
		{
			get { return GetZPropertyInfo(nameof(RequiresImporterOrConsigneeMatchApproval)); }
		}

		public CusHAWBConsigneeConsignorMatchApproval ImporterOrConsigneeMatchApproval
		{
			get { return ImporterMatchApproval ?? (CusHAWBConsigneeConsignorMatchApproval)ConsigneeMatchApproval; }
		}

		#endregion

		#region ImporterOrConsigneeMatchedOrgPK

		public ZGuid ImporterOrConsigneeMatchedOrgPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (UPEConsigneeAddress != null)
				{
					result = UPEConsigneeAddress.P3_OH_MatchOrg;
				}
				else if (UPEImporterAddress != null)
				{
					result = UPEImporterAddress.P3_OH_MatchOrg;
				}
				return result;
			}
		}

		#endregion

		#region ChildRelatedWayBills

		[ChildEditable(true)]
		public WayBillChildPackageCollection ChildRelatedWayBills
		{
			get
			{
				if (fChildRelatedWayBills == null)
				{
					fChildRelatedWayBills = new WayBillChildPackageCollection(this);
					RegisterEditableChildObject(fChildRelatedWayBills);
					if (IsChildRelatedWayBillsReadOnly)
					{
						fChildRelatedWayBills.SetReadOnlyIncludingChildren(true);
					}
					fChildRelatedWayBills.Load();
				}
				return fChildRelatedWayBills;
			}
		}

		protected virtual bool IsChildRelatedWayBillsReadOnly
		{
			get { return false; }
		}

		WayBillChildPackageCollection fChildRelatedWayBills;

		#endregion

		#region Delivery Address Redirection

		public ZBool IsRedirected
		{
			get { return (DeliveryAddressOverride != null); }
			set
			{
				IsRedirectedChangingEventArgs eventArgs = new IsRedirectedChangingEventArgs(value);
				OnIsRedirectedChanging(eventArgs);
				if (!eventArgs.Cancel)
				{
					if (value)
					{
						CreateAndCopyDeliveryAddressFromConsigneeDetails();
					}
					else
					{
						DeleteDeliveryAddressOverrideIfExist();
					}
				}
				IsRedirectedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsRedirectedInfo
		{
			get { return GetZPropertyInfo(nameof(IsRedirected)); }
		}

		public OrgPatternMatchAddress DeliveryAddressOverride
		{
			get
			{
				if (fDeliveryAddressOverride == null)
				{
					ZQuery filter = new ZQuery(OrgPatternMatchAddressSchema.P3_ParentID, PK);
					filter.AddToFilter(OrgPatternMatchAddressSchema.P3_AddressType, OrgPatternMatchAddress.Constants.AddressType.AirCargoConsigneeOverride);
					fDeliveryAddressOverride = (OrgPatternMatchAddress)Factory.LoadTop1(typeof(OrgPatternMatchAddress), filter);
					if (fDeliveryAddressOverride != null)
					{
						RegisterEditableChildObject(fDeliveryAddressOverride);
					}
				}
				return fDeliveryAddressOverride;
			}
		}

		public event IsRedirectedChangingEventHandler IsRedirectedChanging;

		void OnIsRedirectedChanging(IsRedirectedChangingEventArgs eventArgs)
		{
			if (IsRedirectedChanging != null)
			{
				IsRedirectedChanging(eventArgs);
			}
		}

		void CreateAndCopyDeliveryAddressFromConsigneeDetails()
		{
			CreateDeliveryAddressOverrideIfNotExist();

			DeliveryAddressOverride.P3_CompanyName = CS_ConsigneeName;
			DeliveryAddressOverride.P3_ContactName = CS_ConsigneeContactName;
			DeliveryAddressOverride.P3_Address1 = CS_ConsigneeStreet;
			DeliveryAddressOverride.P3_Address2 = CS_ConsigneeStreet2;
			DeliveryAddressOverride.P3_City = CS_ConsigneeCity;
			DeliveryAddressOverride.P3_State = CS_ConsigneeState;
			DeliveryAddressOverride.P3_PostCode = CS_ConsigneePostcode;
			DeliveryAddressOverride.P3_Phone = CS_ConsigneePhone;
		}

		internal void CreateDeliveryAddressOverrideIfNotExist()
		{
			if (DeliveryAddressOverride == null)
			{
				fDeliveryAddressOverride = (OrgPatternMatchAddress)Factory.New(typeof(OrgPatternMatchAddress));
				fDeliveryAddressOverride.P3_ParentID = PK;
				fDeliveryAddressOverride.P3_ParentTableCode = TablePrefix;
				fDeliveryAddressOverride.P3_AddressType = OrgPatternMatchAddress.Constants.AddressType.AirCargoConsigneeOverride;
				RegisterEditableChildObject(fDeliveryAddressOverride);
			}
		}

		void DeleteDeliveryAddressOverrideIfExist()
		{
			if (DeliveryAddressOverride != null)
			{
				DeliveryAddressOverride.Delete();
				fDeliveryAddressOverride = null;
			}
		}

		OrgPatternMatchAddress fDeliveryAddressOverride;

		#endregion

		readonly OrgMatchApproval.Loader OrgMatchApprovalLoader;

		#endregion

		#region Alerts

		public StringCollection AlertsList
		{
			get
			{
				if (fAlertsList == null)
				{
					fAlertsList = new StringCollection();
					GetDefaultAlertsList(fAlertsList);
				}
				return fAlertsList;
			}
		}
		protected StringCollection fAlertsList;

		protected virtual void GetDefaultAlertsList(StringCollection list)
		{
			if (DocManagerInfo.HasUnreadRelatedDocuments)
			{
				list.Add("There are unread eDocs attached to this job");
			}
		}

		public bool HasAlerts
		{
			get { return AlertsList != null && AlertsList.Count > 0; }
		}

		public void ResetAlertsList()
		{
			fAlertsList = null;
		}

		#endregion

		#region Document Properties

		public virtual ZString BusinessObjectType
		{
			get { return "UPECusHAWB"; }
		}

		public virtual ZBool IsInReBillQueue
		{
			get { return false; }
		}

		#endregion

		#region Declaration Queue Summary

		public ZString DeclarationQueueHeldOrCompleted
		{
			get
			{
				ZString result;

				if (Declaration != null)
				{
					result = Declaration.CurrentQueue.CustomsQueueHeldOrCompleted;
				}
				else
				{
					result = (!IsFormalDecRequired) ? UPEProcessQueue.QueueCompletedText : UPEProcessQueue.QueueHeldText;
				}

				return result;
			}
		}

		public ZPropertyInfo DeclarationQueueHeldOrCompletedInfo
		{
			get { return GetZPropertyInfo(nameof(DeclarationQueueHeldOrCompleted)); }
		}

		public ZString DeclarationQueueSummary
		{
			get
			{
				ZString result;

				if (Declaration != null)
				{
					result = Declaration.CurrentQueue.CustomsQueueSummary;
				}
				else
				{
					result = (!IsFormalDecRequired) ? "NOT REQUIRED" : string.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo DeclarationQueueSummaryInfo
		{
			get { return GetZPropertyInfo(nameof(DeclarationQueueSummary)); }
		}

		public ZDateTime DeclarationQueuedDate
		{
			get
			{
				return (Declaration != null && IsFormalDecRequired) ? Declaration.CurrentQueue.CustomsQueuedDate : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo DeclarationQueuedDateInfo
		{
			get { return GetZPropertyInfo(nameof(DeclarationQueuedDate)); }
		}

		#endregion

		#region Queue Moving

		public bool HasAlternateBroker
		{
			get { return CS_JE_CustomsFormalEntry.IsValid && Declaration.HasAlternateBroker; }
		}

		#region Customs Queue Moving

		public override ZString CS_CustomsStatus
		{
			get { return base.CS_CustomsStatus; }
			set
			{
				base.CS_CustomsStatus = value;
				MoveCustomsQueueDependingOnCustomsStatus();
			}
		}

		public override ZString CS_MsgStatus
		{
			get { return base.CS_MsgStatus; }
			set
			{
				if (base.CS_MsgStatus != value)
				{
					base.CS_MsgStatus = value;
					MoveCustomsQueueDependingOnCMRMessageStatus();
				}
			}
		}

		public override ZGuid CS_JE_CustomsFormalEntry
		{
			get { return base.CS_JE_CustomsFormalEntry; }
			set
			{
				if (base.CS_JE_CustomsFormalEntry != value)
				{
					base.CS_JE_CustomsFormalEntry = value;
					if (!CS_JE_CustomsFormalEntry.IsEmpty)
					{
						MoveDeclarationQueueDependingOnCMRCustomsStatus();
					}
				}
			}
		}

		void MoveCustomsQueueDependingOnCustomsStatus()
		{
			if (Env.CurrentUser.IsBatchProcessor && CS_CustomsStatus != CMRBaseStatuses.Codes.NotSent)
			{
				MoveCustomsQueueDependingOnCMRCustomsStatus();
			}
		}

		bool IsCustomsStatus(params string[] statuses)
		{
			return ((IList)statuses).Contains((string)CS_CustomsStatus);
		}

		bool IsMessageStatus(params string[] statuses)
		{
			return ((IList)statuses).Contains((string)CS_MsgStatus);
		}

		public void MoveCustomsQueueTo(ZString queueName, ZString status, ZString subStatus, ZString reason)
		{
			UPETools.Instance.PerformActionInCorrectBranch(MAWB?.Branch, () => UPETools.Instance.LogQueueMovement(Logs, queueName, reason));

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

		internal void MoveDeclarationToQueue(ZString queueName, ZString status, ZString subStatus, ZString reason)
		{
			MoveDeclarationToQueue(false, queueName, status, subStatus, reason);
		}

		void MoveDeclarationToQueue(bool forceMove, ZString queueName, ZString status, ZString subStatus, ZString reason)
		{
			if (!HasFormalDec || forceMove)
			{
				CreateFormalDecAndMatchIfRequired();

				if (Declaration != null)
				{
					Declaration.MoveToQueue(queueName, status, subStatus, reason);
				}
			}
		}

		#region CMR

		void MoveDeclarationQueueDependingOnCMRCustomsStatus()
		{
			if (IsCustomsStatus(UPECMRCodes.CMREntryStatusCodesForCRCompleted))
			{
				if (Declaration != null && Declaration.CustomsEntryHeaders.Count == 0)
				{
					MoveDeclarationToQueue(
						true,
						DeclarationQueueCodeDescriptionPairList.Codes.Completed,
						ZString.Empty,
						ZString.Empty,
						"CMR: CR CPL WITH NO ENTRY HEADERS");
				}
			}
			else if (IsCustomsStatus(UPECMRCodes.CMREntryStatusCodesForCRCompletedDecCustomsBonding))
			{
				MoveDeclarationToQueue(
					true,
					DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding,
					ZString.Empty,
					ZString.Empty,
					"CMR: CPL DEC CUSTOMS BONDING");
			}
			else if (IsCustomsStatus(UPECMRCodes.CMREntryStatusCodesForCRCompletedDecQuarantineHold))
			{
				MoveDeclarationToQueue(
					true,
					DeclarationQueueCodeDescriptionPairList.Codes.BCA,
					ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold,
					ZString.Empty,
					"CMR: CPL DEC QUARANTINE HOLD");
			}
		}

		void MoveCustomsQueueDependingOnCMRCustomsStatus()
		{
			bool isQueueMoved = true;

			if (IsCustomsStatus(UPECMRCodes.CMREntryStatusCodesForCRCompleted))
			{
				MoveCustomsQueueTo(
					CargoReportQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"CMR: CR COMPLETED");

				if (Declaration != null && Declaration.CustomsEntryHeaders.Count == 0)
				{
					MoveDeclarationToQueue(
						true,
						DeclarationQueueCodeDescriptionPairList.Codes.Completed,
						ZString.Empty,
						ZString.Empty,
						"CMR: CR CPL WITH NO ENTRY HEADERS");
				}
			}
			else if (IsCustomsStatus(UPECMRCodes.CMREntryStatusCodesForCRCompletedDecCustomsBonding))
			{
				MoveCustomsQueueTo(
					CargoReportQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"CMR: CPL DEC CUSTOMS BONDING");

				MoveDeclarationToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding,
					ZString.Empty,
					ZString.Empty,
					"CMR: CPL DEC CUSTOMS BONDING");
			}
			else if (IsCustomsStatus(UPECMRCodes.CMREntryStatusCodesForCRCompletedDecQuarantineHold))
			{
				MoveCustomsQueueTo(
					CargoReportQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"CMR: CPL DEC QUARANTINE HOLD");

				MoveDeclarationToQueue(
					DeclarationQueueCodeDescriptionPairList.Codes.BCA,
					ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold,
					ZString.Empty,
					"CMR: CPL DEC QUARANTINE HOLD");
			}
			else if (IsCustomsStatus(UPECMRCodes.CMREntryStatusCodesWithAdditionalInfo))
			{
				isQueueMoved = TryMoveCustomsQueueBasedOnLastCMRCARSTMessage();
			}
			else
			{
				isQueueMoved = false;
			}

			if (!isQueueMoved)
			{
				if (HasFormalDec)
				{
					MoveCustomsQueueTo(
						CargoReportQueueCodeDescriptionPairList.Codes.AwaitingDeclaration,
						ZString.Empty,
						ZString.Empty,
						"CMR: UNABLE TO MOVE Q WITH LAST MSG");
				}
				else
				{
					MoveCustomsQueueTo(
						CargoReportQueueCodeDescriptionPairList.Codes.Unknown,
						ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease,
						ZString.Empty,
						"CMR: NO DECLARATION");
				}
			}

			if (CurrentQueue.IsCustomsQueueCompleted && HasAlternateBroker)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"CMR: CUSTOMS Q CPL HAS ALT BROKER");
			}
		}

		void MoveCustomsQueueDependingOnCMRMessageStatus()
		{
			if (CurrentQueue.P4_CustomsQueue != CargoReportQueueCodeDescriptionPairList.Codes.Completed)
			{
				if (IsMessageStatus(UPECMRCodes.CMRMessageStatusCodesForCRPending))
				{
					MoveCustomsQueueTo(
						CargoReportQueueCodeDescriptionPairList.Codes.Pending,
						ZString.Empty,
						ZString.Empty,
						"CMR: CR PENDING");
				}
				else if (IsMessageStatus(UPECMRCodes.CMRMessageStatusCodesForCRIntervention))
				{
					MoveCustomsQueueTo(
						CargoReportQueueCodeDescriptionPairList.Codes.Intervention,
						ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease,
						ZString.Empty,
						"CMR: CR INTERVENTION");
				}
			}
		}

		bool TryMoveCustomsQueueBasedOnLastCMRCARSTMessage()
		{
			ClearSegmentDictionary();
			CMRCARSTMessage lastCARSTMessage = GetLastCMRCARSTMessage();
			if (lastCARSTMessage != null)
			{
				foreach (CusHAWBAutoQueueMovement movement in UPEDataRegistry.Instance.SortedCusHAWBAutoQueueMovements)
				{
					if (FreeTextSegmentContains(lastCARSTMessage, movement.FreeTextSegmentName, movement.FreeTextSegmentValue))
					{
						if (movement.Queue.QueueName == CargoReportQueueCodeDescriptionPairList.Codes.Unknown && HasFormalDec)
						{
							MoveCustomsQueueTo(
								CargoReportQueueCodeDescriptionPairList.Codes.AwaitingDeclaration,
								ZString.Empty,
								ZString.Empty,
								"CMR: FREE TXT HAS MOVE & HAS DEC");
						}
						else
						{
							MoveCustomsQueueTo(
								movement.Queue.QueueName,
								movement.Queue.Status,
								movement.Queue.SubStatus,
								"CMR: FREE TXT SEGMENT MOVEMENT");
						}
						return true;
					}
				}
			}
			return false;
		}

		bool FreeTextSegmentContains(CMRCARSTMessage message, ZString segmentName, ZString segmentValue)
		{
			ZString segmentInfos;
			if (!SegmentDictionary.Contains(segmentName))
			{
				segmentInfos = ZString.Join(delimiter, message.GetFTXSegmentInfos(segmentName));
				SegmentDictionary.Add(segmentName, segmentInfos);
			}
			else
			{
				segmentInfos = (ZString)SegmentDictionary[segmentName];
			}
			return segmentInfos.Contains(segmentValue);
		}
		const string delimiter = "~";

		void ClearSegmentDictionary()
		{
			fSegmentDictionary = null;
		}

		CMRCARSTMessage GetLastCMRCARSTMessage()
		{
			ZQuery cARSTFilter = new ZQuery();
			cARSTFilter.AddToFilter(EDIMessageSchema.EM_MessageType, CMRMessage.CMRMessageTypes.CARST);

			EDIMessage[] cARSTMessages = (EDIMessage[])Messages.Find(cARSTFilter);
			Array.Sort(cARSTMessages, new EDIMessageComparer(ListSortDirection.Descending));
			return (cARSTMessages.Length > 0) ? cARSTMessages[0] as CMRCARSTMessage : null;
		}

		HybridDictionary SegmentDictionary
		{
			get { return fSegmentDictionary ?? (fSegmentDictionary = new HybridDictionary()); }
		}
		HybridDictionary fSegmentDictionary;

		#endregion

		#endregion

		#region CommercialQueueMoving

		public void MoveToQueue(ZString queueName, ZString status, ZString subStatus, ZString reason)
		{
			UPETools.Instance.PerformActionInCorrectBranch(MAWB?.Branch, () => UPETools.Instance.LogQueueMovement(Logs, queueName, reason));

			if (CurrentQueue.P4_QueueName != queueName)
			{
				CurrentQueue.P4_QueueName = queueName;
			}

			if (CurrentQueue.P4_Status != status)
			{
				CurrentQueue.P4_Status = status;
			}

			if (CurrentQueue.P4_SubStatus != subStatus)
			{
				CurrentQueue.P4_SubStatus = subStatus;
			}
		}

		protected virtual bool IsHeldForFinanceQueuePayment
		{
			get
			{
				ZBool result = BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
				if (Consignee != null)
				{
					result &= UPEUtility.HoldFinanceQueueDebtorGroups.Contains(Consignee.AccountClass);
				}
				return result;
			}
		}

		protected virtual bool IsHeldForARQueuePayment
		{
			get
			{
				ZBool result = BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
				if (Consignee != null)
				{
					result &= UPEUtility.HoldARQueueDebtorGroups.Contains(Consignee.AccountClass);
				}
				return result;
			}
		}

		void IBisiUpload.OnBeforeBisiUpload()
		{
			ZDecimal cachedTotalLocalCharges = TotalLocalCharges;

			if (CurrentQueue.IsCustomsQueueCompleted && HasAlternateBroker)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"UPLOAD: CUS CPL && HAS ALTERNATE BROKER");
			}
			else if (HasAlternateBroker)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.AlternateBroker,
					ZString.Empty,
					ZString.Empty,
					"UPLOAD: HAS ALTERNATE BROKER");
			}
			else if (cachedTotalLocalCharges.IsEmpty)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"UPLOAD: NO LOCAL CHARGES");
			}
			else if (IsBillingTermsFreeDomicile)
			{
				if (IsAbandoned)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Rebill,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: BILL TERMS = F/D: IS ABANDONED");
				}
				else if (IsFreeDomicile)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Rebill,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: BILL TERMS = F/D: IS FREE DOM");
				}
				else if (IsRTS)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Rebill,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: BILL TERMS = F/D: IS RTS");
				}
				else if (IsTranshipment)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Rebill,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: BILL TERMS = F/D: IS TRANSHIP");
				}
				else
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Completed,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: BILL TERMS = F/D: DEFAULT");
				}
			}
			else if (ThirdPartyIndicator == "5" || ThirdPartyIndicator == "8")
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"UPLOAD: THIRD PARTY INDICATOR = 5 OR 8");
			}
			else if (IsPreferredAccountImporter)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"UPLOAD: IS PREFERRED ACC IMPORTER");
			}
			else if (cachedTotalLocalCharges < UPEDataRegistry.Instance.CODConfirmPaymentThreshold)
			{
				if (IsStandardAccountImporter)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Completed,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: CHG < COD - STD ACC IMPORTER");
				}
				else if (IsCreditCardOrOnFileCODAccountImporter)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.OnFile,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: CHG < COD - CC OR ON FILE ACC");
				}
				else if (cachedTotalLocalCharges <= UPEDataRegistry.Instance.CODAutoReleaseAtUploadThreshold)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Completed,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: CHARGES <= COD REL UPLD THRESH");
				}
				else if (cachedTotalLocalCharges <= UPEDataRegistry.Instance.CODAutoReleaseAndChaseThreshold)
				{
					MoveToQueue(
						CommercialQueueCodeDescriptionPairList.Codes.Chase,
						ZString.Empty,
						ZString.Empty,
						"UPLOAD: CHARGES <= COD REL CHASE THRESH");
				}
				else if (!IsConsigneeOrDeliveryAddressPostcodeBrown && !IsAROrStandardLegalAccountImporter)
				{
					CurrentQueue.CommercialQueueLogs.AddNew(
							string.Empty,
							ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment,
							string.Empty,
							"auto upload Local Charges below COD Threshold",
							string.Empty);
				}
			}
			else
			{
				CurrentQueue.CommercialQueueLogs.AddNew(
					string.Empty,
					ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment,
					string.Empty,
					string.Empty,
					string.Empty);
			}
		}

		bool IsPreferredAccountImporter
		{
			get { return Declaration != null && Declaration.Importer != null && Declaration.Importer.IsPreferredAccount; }
		}

		bool IsStandardAccountImporter
		{
			get { return Declaration != null && Declaration.Importer != null && Declaration.Importer.IsStandardAccount; }
		}

		bool IsCreditCardOrOnFileCODAccountImporter
		{
			get { return Declaration != null && Declaration.Importer != null && (Declaration.Importer.IsCreditCardAccount || Declaration.Importer.IsOnFileCODAccount); }
		}

		bool IsAROrStandardLegalAccountImporter
		{
			get { return Declaration != null && Declaration.Importer != null && (Declaration.Importer.IsARAccount || Declaration.Importer.IsStandardLegalAccount); }
		}

		#endregion

		#endregion

		#region Delivery Instructions Note

		public StmNote DeliveryInstructionsNote
		{
			get
			{
				StmNote[] deliveryInstructionsNotes = Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
				return deliveryInstructionsNotes.Length > 0 ? deliveryInstructionsNotes[0] : null;
			}
		}

		#endregion

		#region Is Formal Dec Required

		public bool IsFormalDecRequired
		{
			get
			{
				var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);
				return CS_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Australia && GoodsValueInAUD > deminimus;
			}
		}

		public ZDecimal GoodsValueInAUD
		{
			get
			{
				ZDecimal result = 0;
				Money moneyInAUD = CurrencyConverter.ConvertRounded(new Money(CS_GoodsValue, GoodsCurrency), AUD);
				if (moneyInAUD != null)
				{
					result = moneyInAUD.Amount;
				}
				return result;
			}
		}

		internal CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = CurrencyConverter.New(Factory, ZDateTime.Now, ZArchitecture.Core.ExchangeRateType.Customs, 10);
				}
				return fCurrencyConverter;
			}
		}

		internal RefCurrency AUD
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia); }
		}

		CurrencyConverter fCurrencyConverter;

		#endregion

		#region Cascade Updating Consignee/Consignor Address Record

		void CascadeUpdateAddresses()
		{
			CascadeUpdateConsigneeAddress();
			CascadeUpdateConsignorAddress();
		}

		void CascadeUpdateConsigneeAddress()
		{
			if ((!IsInDatabase && CS_OA_ConsigneeAddress.IsValid) || CS_OA_ConsigneeAddressInfo.HasChanges)
			{
				OrgPatternMatchAddress consigneeAddress = CS_OA_ConsigneeAddress.IsEmpty ? UPEConsigneeAddress : LoadOrCreateUPEConsigneeAddress();
				if (consigneeAddress != null)
				{
					consigneeAddress.P3_OH_MatchOrg = base.Consignee?.PK ?? ZGuid.Empty;
				}
				if (Consignee != null)
				{
					CS_ConsigneePostcode = ConsigneeAddressForPortOfDestination.PostCode;
				}
			}
		}

		void CascadeUpdateConsignorAddress()
		{
			if ((!IsInDatabase && CS_OA_ConsignorAddress.IsValid) || CS_OA_ConsignorAddressInfo.HasChanges)
			{
				OrgPatternMatchAddress consignorAddress = CS_OA_ConsignorAddress.IsEmpty ? UPEConsignorAddress : LoadOrCreateUPEConsignorAddress();
				if (consignorAddress != null)
				{
					consignorAddress.P3_OH_MatchOrg = base.Consignor?.PK ?? ZGuid.Empty;
				}
			}
		}

		#endregion

		#region CreateFormalDecAndMatch / PutOnOrgMatchingQueueOrCreateFormalDec

		public void CreateFormalDecAndMatchIfRequired()
		{
			// todo - suggest rename to CreateOrAttachFormalDeclaration().
			if (!HasFormalDec)
			{
				if (IsFormalDecRequired)
				{
					CreateFormalDecAndMatch();
				}
				else if (CS_FreightPrepaidCollect == Core.Constants.PaymentType.Collect)
				{
					InsertOPSCANHoldAndReleaseQueueEvent(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, string.Empty);
				}
			}
		}

		public void CreateFormalDecAndMatch()
		{
			TryMatchAutomatically();
			PutOnOrgMatchingQueueOrCreateFormalDec();
			InsertOPSCANHoldAndReleaseQueueEvent(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, StatusCodeDescriptionPairList.Codes.Y1_DocumentsToCustomsAQIS);
		}

		internal void PutOnOrgMatchingQueueOrCreateFormalDec()
		{
			if (ImporterOrConsigneeMatched && ConsignorMatched)
			{
				new UPECusHAWBOrgMatchApprovedDirector(this).RunMatchApprovedActivities();
			}
			else
			{
				PutOnOrgMatchingQueueIfRequired();
			}
		}

		public void PutOnOrgMatchingQueueIfRequired()
		{
			if (!ImporterOrConsigneeMatched)
			{
				RequiresImporterOrConsigneeMatchApproval = true;
			}
			if (!ConsignorMatched)
			{
				RequiresConsignorMatch = true;
			}
		}

		public bool IsOnOrgMatchingQueueOrMatchingCompleted
		{
			get
			{
				bool consigneeOrImporterQueuedOrCompleted = RequiresImporterOrConsigneeMatchApproval || ImporterOrConsigneeMatched;
				bool consignorQueuedOrCompleted = RequiresConsignorMatch || ConsignorMatched;
				return consigneeOrImporterQueuedOrCompleted && consignorQueuedOrCompleted;
			}
		}

		bool ImporterOrConsigneeMatched
		{
			get { return !ImporterOrConsigneeMatchedOrgPK.IsEmpty; }
		}

		bool ConsignorMatched
		{
			get { return !CS_OA_ConsignorAddress.IsEmpty; }
		}

		internal void TryMatchAutomatically()
		{
			if (UPEConsigneeAddress == null || UPEConsigneeAddress.P3_OH_MatchOrg.IsEmpty)
			{
				TryMatchConsignee();
			}
			else if (UPEImporterAddress != null)
			{
				if (UPEImporterAddress.P3_OH_MatchOrg.IsEmpty)
				{
					TryMatchImporter();
				}
			}

			if (CS_OA_ConsignorAddress.IsEmpty)
			{
				TryMatchConsignor();
			}
		}

		internal void TryMatchConsignee()
		{
			var fax = Level1Record?._400000?.Fax ?? ZString.Empty;
			var matchingOrg = UPEOrganisationMatching.TryMatchOrganisation(
				CS_ConsigneeName,
				CS_RL_NKDestination,
				CS_ConsigneeCity,
				CS_ConsigneePhone,
				fax,
				CS_ConsigneePostcode,
				CS_ConsigneeState,
				CS_ConsigneeStreet,
				CS_ConsigneeStreet2,
				CS_RN_NKConsigneeCountry,
				Level1Record?._400000?.AccountNumber ?? ZString.Empty);
			UpdatePhoneAndFax(matchingOrg, fax);

			if (matchingOrg != null)
			{
				OrgPatternMatchAddress consigneeAddress = LoadOrCreateUPEConsigneeAddress();
				consigneeAddress.P3_OH_MatchOrg = matchingOrg.PK;
				LogMatchEvent(true, "Consignee: " + matchingOrg.OH_Code);
			}
		}

		internal void TryMatchConsignor()
		{
			var fax = Level1Record?._300000?.Fax ?? ZString.Empty;
			var matchingOrg = UPEOrganisationMatching.TryMatchOrganisation(
				CS_ConsignorName,
				CS_RL_NKOrigin.IsEmpty && MAWB != null ? MAWB.CM_RL_NKLoadPort : CS_RL_NKOrigin,
				CS_ConsignorCity,
				CS_ConsignorPhone,
				fax,
				CS_ConsignorPostcode,
				CS_ConsignorState,
				CS_ConsignorStreet,
				CS_ConsignorStreet2,
				CS_RN_NKConsignorCountry,
				Level1Record?._300000?.AccountNumber ?? ZString.Empty);
			UpdatePhoneAndFax(matchingOrg, fax);

			if (matchingOrg != null)
			{
				var addressDecider = new Customs.Business.PortBasedOrgAddressDecider(matchingOrg, Customs.Business.CargoAddressType.Pickup, delegate
				{
					return CS_RL_NKOrigin;
				});
				var consignorAddress = addressDecider.OrgAddress;
				if (consignorAddress != null)
				{
					CS_OA_ConsignorAddress = consignorAddress.PK;
					LogMatchEvent(true, "Consignor: " + matchingOrg.OH_Code);
				}
			}
		}

		internal void TryMatchImporter()
		{
			var fax = Level1Record?._401000?.Fax ?? ZString.Empty;
			var matchingOrg = UPEOrganisationMatching.TryMatchOrganisation(
				UPEImporterAddress.P3_CompanyName,
				CS_RL_NKDestination,
				UPEImporterAddress.P3_City,
				UPEImporterAddress.P3_Phone,
				fax,
				UPEImporterAddress.P3_PostCode,
				UPEImporterAddress.P3_State,
				UPEImporterAddress.P3_Address1,
				UPEImporterAddress.P3_Address2,
				CS_RN_NKConsigneeCountry,
				Level1Record?._401000?.AccountNumber ?? ZString.Empty);
			UpdatePhoneAndFax(matchingOrg, fax);

			if (matchingOrg != null)
			{
				UPEImporterAddress.P3_OH_MatchOrg = matchingOrg.PK;
				LogMatchEvent(true, "Importer: " + matchingOrg.OH_Code);
			}
		}

		void UpdatePhoneAndFax(OrgHeader org, ZString fax)
		{
			if (org != null && !org.OH_Code.IsEmpty)
			{
				var mainAddress = org.MainAddress;
				if (!CS_ConsigneePhone.IsEmpty && mainAddress.OA_Phone.IsEmpty)
				{
					mainAddress.OA_Phone = CS_ConsigneePhone;
				}
				if (!fax.IsEmpty && mainAddress.OA_Fax.IsEmpty)
				{
					mainAddress.OA_Fax = fax;
				}
			}
		}

		public void LogMatchEvent(bool isAutoMatch, string reference)
		{
			Event @event = isAutoMatch ? Events.AutoMatchDone : Events.ManualMatchDone;
			Logs.AddNew(@event, reference);
		}

		public int TotalAutoMatches
		{
			get
			{
				ZQuery autoMatchFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code);
				return Logs.GetAllLogs().Find(autoMatchFilter).Length;
			}
		}

		public int TotalManualMatches
		{
			get
			{
				ZQuery manualMatchFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ManualMatchDone.Code);
				return Logs.GetAllLogs().Find(manualMatchFilter).Length;
			}
		}

		UPEOrganisationMatching UPEOrganisationMatching
		{
			get { return fUPEOrganisationMatching ?? (fUPEOrganisationMatching = new UPEOrganisationMatching(Factory, true)); }
		}
		UPEOrganisationMatching fUPEOrganisationMatching;

		void InsertOPSCANHoldAndReleaseQueueEvent(ZString reasonCode, ZString statusCode)
		{
			/* todo: 
			 * - confirm this actually works.  It doesn't make sense to me. 
			 * - rename to AddUniqueReasonAndStatusToCustomsQueueLog().*/
			bool oPSCANHoldAndReleaseReasonCodeHasBeenAdded = CurrentQueue.CustomsQueueLogs.ContainsQueue(string.Empty);
			if (!oPSCANHoldAndReleaseReasonCodeHasBeenAdded)
			{
				CurrentQueue.CustomsQueueLogs.AddNew(string.Empty, reasonCode, statusCode, string.Empty, string.Empty);
			}
		}

		#endregion

		#region Sending Air Cargo Messages

		public void SendAirCargoMessage()
		{
			if (!IsIdentifiedForScreening && !IsIdentifiedForQuarantine)
			{
				RunPreSaveValidation();
				if (!HasMessageErrors && !HasErrors)
				{
					LandingToSendMessageOn = this;
					IMessageBuilder builder = this.GetCargoReportBuilder();
					builder.PopulateMessages();
					CS_IsResponsePending = true;
					ReadOnly = true;
					MAWB.ReadOnly = true;
				}
			}
		}

		#endregion

		#region Related Notes and Events

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				ArrayList result = new ArrayList(base.BusinessObjectsWithRelatedNotes);
				if (Declaration != null)
				{
					result.Add(Declaration);
				}
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList result = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				if (Declaration != null)
				{
					result.Add(Declaration);
				}
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Shipment Held Letter

		public event QueryShipmentHeldLetterDetailsEventHandler QueryShipmentHeldLetterDetails;

		internal bool QueryForShipmentHeldLetterDetails(ShipmentHeldLetterRecipient recipient)
		{
			QueryShipmentHeldLetterDetailsEventArgs e = new QueryShipmentHeldLetterDetailsEventArgs(ShipmentHeldLetterDetails, recipient);

			if (QueryShipmentHeldLetterDetails != null)
			{
				QueryShipmentHeldLetterDetails(this, e);
			}
			else
			{
				e.Cancel = true;
			}
			return e.Cancel;
		}

		public ShipmentHeldLetterBusinessObject ShipmentHeldLetterDetails
		{
			get
			{
				if (fShipmentHeldLetterDetails == null)
				{
					fShipmentHeldLetterDetails = ShipmentHeldLetterDetailsWithoutLoad;
					fShipmentHeldLetterDetails.LoadFromNote();
				}
				return fShipmentHeldLetterDetails;
			}
		}
		ShipmentHeldLetterBusinessObject fShipmentHeldLetterDetails;

		ShipmentHeldLetterBusinessObject ShipmentHeldLetterDetailsWithoutLoad
		{
			get
			{
				if (fShipmentHeldLetterDetailsWithoutLoad == null)
				{
					fShipmentHeldLetterDetailsWithoutLoad = new ShipmentHeldLetterBusinessObject(this);
				}
				return fShipmentHeldLetterDetailsWithoutLoad;
			}
		}
		ShipmentHeldLetterBusinessObject fShipmentHeldLetterDetailsWithoutLoad;

		#endregion

		#region Shipment Held Letter Auto-delivery

		bool QueueMovementRequiresAutoDeliveryOnFactorySaved;

		virtual internal void OnCusHAWBOrDeclarationProcessQueueSaving()
		{
			QueueMovementRequiresAutoDeliveryOnFactorySaved = ShipmentHeldLetterDetails.QueueMovementRequiresAutoDelivery();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (QueueMovementRequiresAutoDeliveryOnFactorySaved)
				{
					QueueMovementRequiresAutoDeliveryOnFactorySaved = false;
					if (Env.CurrentUser.IsBatchProcessor)
					{
						ShipmentHeldLetterDetails.SetDefaultsForAutoDelivery();
					}
					DeliverShipmentHeldLetter(ShipmentHeldLetterRecipient.Consignee);
					DeliverShipmentHeldLetter(ShipmentHeldLetterRecipient.Consignor);
				}

				ProcessGSSiMessaging();
			}
		}

		void DeliverShipmentHeldLetter(ShipmentHeldLetterRecipient recipient)
		{
			UPEShipmentHeldLetterAutoDelivery delivery = new UPEShipmentHeldLetterAutoDelivery(this, recipient);
			delivery.Deliver();
		}

		#endregion

		#region Profiling and Screening

		public ZBool IsValidForSAC
		{
			get { return new UPESACDecider(this).IsValidForSAC; }
		}

		public ZBool IsIdentifiedForProfiling
		{
			get { return new SACDecider(Factory, GoodsValueInAUD, CS_GoodsDescription).StopPhrasesFoundInGoodsDescription.Any(); }
		}

		public ZPropertyInfo IsIdentifiedForProfilingInfo
		{
			get { return GetZPropertyInfo(nameof(IsIdentifiedForProfiling)); }
		}

		public ZBool IsIdentifiedForScreening
		{
			get { return Screening.IsIdentifiedForScreening; }
		}

		public ZPropertyInfo IsIdentifiedForScreeningInfo
		{
			get { return GetZPropertyInfo(nameof(IsIdentifiedForScreening)); }
		}

		internal ZBool IsIdentifiedForQuarantine
		{
			get { return Screening.IsIdentifiedForQuarantine; }
		}

		public ZString IdentifiedForAction
		{
			get
			{
				if (fIdentifiedForAction == null)
				{
					fIdentifiedForAction = new CachedProperty<ZString>(Factory, delegate
					{
						ZString result = ZString.Empty;
						int actionFlag = (Convert.ToInt32(IsIdentifiedForProfiling) * (int)IndentifiedActionFlag.Profiling) +
							(Convert.ToInt32(IsIdentifiedForScreening) * (int)IndentifiedActionFlag.Screening) +
							(Convert.ToInt32(IsIdentifiedForQuarantine) * (int)IndentifiedActionFlag.Quarantine);

						if (actionFlag.Equals((int)IndentifiedActionFlag.Profiling))
						{
							result = "Profiling";
						}
						else if (actionFlag.Equals((int)IndentifiedActionFlag.Screening))
						{
							result = "Screening";
						}
						else if (actionFlag.Equals((int)IndentifiedActionFlag.Quarantine))
						{
							result = "Quarantine";
						}
						else if (!actionFlag.Equals((int)IndentifiedActionFlag.None))
						{
							result = "Any";
						}
						return result;
					});
				}
				return fIdentifiedForAction.Value;
			}
		}
		internal CachedProperty<ZString> fIdentifiedForAction;

		public ZPropertyInfo IdentifiedForActionInfo
		{
			get { return GetZPropertyInfo(nameof(IdentifiedForAction)); }
		}

		public void RunScreeningValidation()
		{
			UPECusHAWBValidation.ValidateCS_GoodsDescription();
			UPECusHAWBValidation.ValidateCS_GoodsValue();

			UPECusHAWBValidation.ValidateCS_ConsignorName();
			UPECusHAWBValidation.ValidateCS_ConsignorStreet();
			UPECusHAWBValidation.ValidateCS_ConsignorStreet2();
			UPECusHAWBValidation.ValidateLevel1RecordConsignorAccountNum();

			UPECusHAWBValidation.ValidateCS_ConsigneeName();
			UPECusHAWBValidation.ValidateCS_ConsigneeStreet();
			UPECusHAWBValidation.ValidateCS_ConsigneeStreet2();
			UPECusHAWBValidation.ValidateLevel1RecordConsigneeAccountNum();
		}

		public void SetRemarksIfThisIsANewHAWBAndInInterventionQueue()
		{
			if (!this.IsInDatabase && CurrentQueue.P4_CustomsQueue == CargoReportQueueCodeDescriptionPairList.Codes.Intervention)
			{
				CurrentQueue.P4_CustomsReason = Screening.IdentifiedReason;
			}
		}

		UPEScreening Screening
		{
			get
			{
				if (fScreening == null)
				{
					fScreening = new UPEScreening(this);
				}
				return fScreening;
			}
		}
		UPEScreening fScreening;

		[Flags]
		[WTG.StaticAnalysis.Annotation.CodeAlive("enum")]
		enum IndentifiedActionFlag
		{
			None = 0,
			Profiling = 2,
			Screening = 3,
			Quarantine = 4
		}

		#endregion

		#region GSSI Processing

		bool customsQueueStatusChange;
		bool commercialQueueStatusChange;

		void ProcessGSSiMessaging()
		{
			if (customsQueueStatusChange || commercialQueueStatusChange)
			{
				var innerFactory = new BusinessObjectFactory();
				var cusHawbInInnerFactory = innerFactory.Load<UPECusHAWB>(PK);

				if (customsQueueStatusChange)
				{
					CreateGSSIHoldMessage(ProcessQueueType.Customs, cusHawbInInnerFactory);
					customsQueueStatusChange = false;
				}

				if (commercialQueueStatusChange)
				{
					CreateGSSIHoldMessage(ProcessQueueType.Commercial, cusHawbInInnerFactory);
					commercialQueueStatusChange = false;
				}

				innerFactory.Save();
			}
		}

		void CreateGSSIHoldMessage(string processQueueType, UPECusHAWB cusHAWB)
		{
			var currentQueueLog = GSSiManager.Instance.GetLatestProcessLogByQueueType(processQueueType, Factory, CurrentQueue.PK);

			if (currentQueueLog != null && currentQueueLog.ShipmentStatus == "03" && currentQueueLog.HoldReasonCode.Length == 2)
			{
				List<UPECusHAWB> uPECusHAWBs = new List<UPECusHAWB>() { cusHAWB };

				if (!WayBillShort.IsEmpty)
				{
					uPECusHAWBs.AddRange(GetSplitShipments(WayBillShort, PK, cusHAWB.Factory));
				}

				var relatedBills = uPECusHAWBs.Select(r => new BillPair() { BillNumber = r.CS_HAWB, cusHawb = r });
				var childBills = uPECusHAWBs.SelectMany(p => p.ChildJobRelatedWayBills.Select(c => new BillPair() { BillNumber = c.EB_WaybillNumber, cusHawb = p }));
				var dict = relatedBills.Union(childBills).GroupBy(r => r.BillNumber).ToDictionary(grp => grp.Key, grp => grp.ToList());

				foreach (var pair in dict)
				{
					var parentInTheList = pair.Value.Where(a => a.cusHawb.PK == PK).Count();
					var parent = (pair.Value.Count == 1 || parentInTheList == 0) ? pair.Value.First().cusHawb : cusHAWB;

					GSSiManager.Instance.ProcessGSSIHOLDMessage(
						parent,
						currentQueueLog,
						() => new UniqueList<ZString>() { pair.Key },
						Destination,
						parent == cusHAWB ? "" : PK.ToString());
				}
			}
		}

		UPECusHAWB[] GetSplitShipments(ZString shortTrackingNumber, ZGuid cusHawbPK, BusinessObjectFactory factory)
		{
			var query = UPEUtility.GetDuplicateShipmentQuery(shortTrackingNumber);
			query.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, cusHawbPK);

			return factory.Load<UPECusHAWB>(query);
		}
		#endregion
		#region Job Related Way Bills - Note: this includes either Merge Clearance, Split Shipments or Virtual Shipments!

		/// <summary>
		/// Get a list of either Mergec Clearance, Split Shipment or Virtual Job Related Waybill children.
		/// </summary>
		/// <remarks>This does assume this house bill is a parent.</remarks>
		public ReadOnlyCollection<UPEJobRelatedWayBill> ChildJobRelatedWayBills
		{
			get
			{
				ZQuery query = new ZQuery(JobRelatedWayBillSchema.EB_ParentID, PK);
				query.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, SQLComparisonOperator.NotEqual, UPEJobRelatedWayBill.Constants.RelatedWayBillType.Parent);
				return new ReadOnlyCollection<UPEJobRelatedWayBill>(Factory.Load<UPEJobRelatedWayBill>(query));
			}
		}

		#endregion

		#region IUPEDocumentSupporter

		public event PrintBatchItemQueuedEventHandler PrintBatchItemQueued;

		void IUPEDocumentSupportable.OnPrintBatchItemQueued(PrintBatchItemQueuedEventArgs e)
		{
			if (PrintBatchItemQueued != null)
			{
				PrintBatchItemQueued(this, e);
			}
		}

		internal CusHAWBDocumentSupporter GetDocumentSupporter()
		{
			return DocumentSupporter;
		}

		protected override CusHAWBDocumentSupporter DocumentSupporter
		{
			get { return new UPECusHAWBDocumentSupporter(this); }
		}

		#endregion

		#region IShipmentData Members

		bool IShipmentData.IsAlreadyUploaded
		{
			get
			{
				return !((IShipmentData)this).BisiDeclarationUploadDate.IsEmpty;
			}
		}

		bool IShipmentData.ShouldBeUploaded
		{
			get
			{
				// beautiful code heh?

				bool shouldBeUploadedBasedOn3rdParty = UPEUtility.UploadableDebtorGroups.Contains(ThirdPartyIndicator);

				bool result = !TotalLocalCharges.IsEmpty
					|| BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect
					|| DutyType == DutyTypeCodeDescriptionPairList.Codes.GCC
					|| shouldBeUploadedBasedOn3rdParty;

				return result;
			}
		}

		bool IShipmentData.ShouldBeDownloaded
		{
			get
			{
				bool result = ((IShipmentData)this).ShouldBeUploaded;
				result = result && (BillingTerms != BillingTermsCodeDescriptionPairList.Codes.FreeDomicile);
				return result;
			}
		}

		ZString ILineKey.ShipmentRef
		{
			get { return WayBillShort; }
		}

		ZString ILineKey.FlightNo
		{
			get { return CS_FlightNo; }
		}

		ZDateTime ILineKey.ImportDate
		{
			get { return CS_ArrivalDate; }
		}

		ZString ILineKey.ConsigneePostCode
		{
			get { return Consignee != null ? Consignee.MainAddress.OA_PostCode : CS_ConsigneePostcode; }
		}

		ZString IShipmentData.ImporterAccountNumber
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.DutyType
		{
			get { return DutyType; }
		}

		ZString IShipmentData.MasterBillNumber
		{
			get { return (MAWB != null) ? MAWB.CM_MAWB : ZString.Empty; }
		}

		ZString IShipmentData.DischargePort
		{
			get { return (MAWB != null) ? MAWB.CM_RL_NKDischargePort : ZString.Empty; }
		}

		ZDecimal IShipmentData.CustomsValue
		{
			get { return (Level1Record != null && Level1Record._200000 != null) ? Level1Record._200000.DeclaredValue : ZDecimal.Zero; }
		}

		ZString IShipmentData.DVCCurrencyCode
		{
			get { return (Level1Record != null && Level1Record._200000 != null) ? Level1Record._200000.CurrencyCodeForDeclaredValue : ZString.Empty; }
		}

		ZDecimal IShipmentData.CustomsExchangeRate
		{
			get { return 0; }
		}

		ZString IShipmentData.BISICustomsEntryStatus
		{
			get { return !TotalLocalCharges.IsEmpty ? "02" : string.Empty; }
		}

		ZString IShipmentData.CustomsStatus
		{
			get { return CS_CustomsStatus; }
		}

		ZString IShipmentData.CustomsEntryNumber
		{
			get { return (Declaration != null) ? new ZString(Declaration.JobNumber) : ZString.Empty; }
		}

		ZDateTime IShipmentData.CustomsEntryDate
		{
			get { return (Declaration != null && !Declaration.CustomsEntryDate.IsEmpty) ? Declaration.CustomsEntryDate : ZDateTime.Now; }
		}

		ZString IShipmentData.ThirdPartyIndicator
		{
			get { return ThirdPartyIndicator; }
		}

		protected ZString ThirdPartyIndicator
		{
			get { return (Level1Record != null && Level1Record._200000 != null) ? (ZString)Level1Record._200000.ThirdPartyIndicator : ZString.Empty; }
		}

		ZDateTime IShipmentData.BisiDeclarationUploadDate
		{
			get { return (Declaration != null) ? Declaration.BisiUploadDate : BisiUploadDate; }
			set
			{
				if (Declaration != null)
				{
					Declaration.BisiUploadDate = value;
				}
			}
		}

		public ZDecimal TotalLocalCharges
		{
			get
			{
				ZDecimal result = 0m;
				var allCharges = ((IShipmentData)this).ChargesData;

				foreach (ShipmentChargeData shipmentChargeData in allCharges)
				{
					result += shipmentChargeData.GrossAmount;
				}
				return result;
			}
		}

		IReadOnlyList<CommodityDetailData> IShipmentData.CommoditiesData
		{
			get { return (Declaration != null) ? Declaration.CommoditiesData : Array.Empty<CommodityDetailData>(); }
		}

		IReadOnlyList<ShipmentChargeData> IShipmentData.ChargesData
		{
			get
			{
				ShipmentChargeDataList list = new ShipmentChargeDataList();

				if (Declaration != null)
				{
					list.AddCharge(Declaration.ChargesData.ToArray());
				}
				AddSecurityFeeIfApplicable(list);

				return list.ToArray();
			}
		}

		IReadOnlyList<ShipmentReceiptData> IShipmentData.ReceiptsData
		{
			get { return Array.Empty<ShipmentReceiptData>(); }
		}

		void IShipmentData.MarkShipmentAsSplitShipmentIfApplicable()
		{
			if (Declaration != null && !Declaration.BisiUploadDate.IsEmpty && BisiUploadDate.IsEmpty)
			{
				IsSubsequentSplitShipment = true;
			}
		}

		void AddSecurityFeeIfApplicable(ShipmentChargeDataList charges)
		{
			if (UPEDataRegistry.Instance.SecurityFeeAmount.Value != 0m)
			{
				if (BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect && ShipmentType == ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments)
				{
					charges.AddCharge(ShipmentChargeTypeCode.Security, UPEDataRegistry.Instance.SecurityFeeAmount.Value, Core.Constants.CurrencyCodes.Australia);
				}
				else if (Declaration != null && ServiceLocator.GetService<ICustomsCharges>(Declaration).GetCustomsCharges(null).Length > 0
	&& !Declaration.UploadCustomsCharges && Declaration.IsCustomsEFTActive &&
	BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect)
				{
					charges.AddCharge(ShipmentChargeTypeCode.Security, UPEDataRegistry.Instance.SecurityFeeAmount.Value, Core.Constants.CurrencyCodes.Australia);
				}
				else if (charges.Count > 0 && !IsExemptFromSecurityFee)
				{
					charges.AddCharge(ShipmentChargeTypeCode.Security, UPEDataRegistry.Instance.SecurityFeeAmount.Value, Core.Constants.CurrencyCodes.Australia);
				}
			}
		}

		bool IsExemptFromSecurityFee
		{
			get
			{
				UPEOrgHeader importerOrConsignee = Factory.Load<UPEOrgHeader>(ImporterOrConsigneeMatchedOrgPK);
				return
					(importerOrConsignee == null || !importerOrConsignee.IsITFChargableForThisImporter) ||
					ShipmentType != ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments ||
					IsBillingTermsFreeDomicile ||
					ThirdPartyIndicator == "2" ||
					ThirdPartyIndicator == "5" ||
					ThirdPartyIndicator == "8";
			}
		}

		#region Not Required

		ZDecimal IShipmentData.StatisticalValue
		{
			get { return 0; }
		}

		ZString IShipmentData.EntryType
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.CustomsOfficeNumber
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.VATNumber
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.ImporterVATDefermentNumber
		{
			get { return string.Empty; }
		}

		ZString IShipmentData.SplitDutyDefermentNumber
		{
			get { return string.Empty; }
		}

		#endregion

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

		internal new DocManagerInfo DocManagerInfo
		{
			get { return base.DocManagerInfo; }
		}

		#endregion

		public override string ToString()
		{
			return ToString(spaceDelimiter);
		}
		const string spaceDelimiter = " ";

		public virtual string ToString(ZString delimiter)
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("PK=" + PK.ToString());
			result.Append("HouseBill=" + CS_HAWB);
			result.Append("MasterBill=" + CS_MasterHouseBill);
			result.Append("ShortTrackingNumber=" + WayBillShort);
			result.Append("IsSplitShipment=" + IsSplitShipment);
			result.Append("IsCOD=" + IsCOD);
			result.Append("BillingTerms=" + BillingTerms);
			result.Append("DebtorGroup=" + ThirdPartyIndicator);
			result.Append("ShipmentType=" + ShipmentType);
			return result.ToStringWithDelimiterBetweenAppends(delimiter);
		}

		struct BillPair
		{
			public ZString BillNumber;
			public UPECusHAWB cusHawb;
		}
	}
}
