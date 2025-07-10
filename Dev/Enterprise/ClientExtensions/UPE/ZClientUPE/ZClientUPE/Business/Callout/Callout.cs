using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public partial class Callout : UPECusHAWB, IRefundEnquiry, IBisiDownload
	{
		public const string ForcedToFinanceQueueRemarks = "Forced To Finance";

		public Callout(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Documents

		protected override CusHAWBDocumentSupporter DocumentSupporter
		{
			get { return new CalloutDocumentSupporter(this); }
		}

		public override ZString BusinessObjectType
		{
			get { return "UPECallout"; }
		}

		public override ZBool IsInReBillQueue
		{
			get { return CurrentQueue.P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill; }
		}

		#endregion

		#region Property Overrides

		protected override Type TypeOfProcessQueue
		{
			get { return typeof(UPECalloutQueue); }
		}

		public override bool ReadOnly
		{
			get { return false; }
			set { base.ReadOnly = value; }
		}

		public override ZPropertyInfo ShipmentTypeInfo
		{
			get
			{
				ZPropertyInfo result = base.ShipmentTypeInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		public override ZPropertyInfo DutyTypeInfo
		{
			get
			{
				ZPropertyInfo result = base.DutyTypeInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		public override ZPropertyInfo BillingTermsInfo
		{
			get
			{
				ZPropertyInfo result = base.BillingTermsInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region Business Object Overrides

		public new CalloutLookups Lookups
		{
			get { return (CalloutLookups)base.Lookups; }
		}

		protected override Customs.Business.CusHAWBLookups GetNewLookups()
		{
			return new CalloutLookups(this);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetHAWBPropertiesReadOnly();
			((IZPropertyInfoObsolete)CS_ConsigneeContactNameInfo).ReadOnly = false;
			((IZPropertyInfoObsolete)IsExcludedFromBISIWarningInfo).ReadOnly = false;
		}

		#endregion

		#region Related Business Objects

		#region Bill To

		public ZGuid BillToPK
		{
			get { return (BillTo != null) ? BillTo.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo BillToPKInfo
		{
			get { return GetZPropertyInfo(nameof(BillToPK)); }
		}

		public UPEOrgHeader BillTo
		{
			get
			{
				if (fBillTo == null && !BillToAccountNumber.IsEmpty)
				{
					fBillTo = (UPEOrgHeader)OrgHeader.FindByOrgCusCode(Factory, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, BillToAccountNumber);
					if (fBillTo != null)
					{
						fBillTo.SetReadOnlyIncludingChildren(true);
					}
				}
				return fBillTo;
			}
		}

		void AssignOwnerCodeToCusDecImporterIfRequired()
		{
			bool billToCanBeMatched = BillTo != null;
			if (!billToCanBeMatched)
			{
				bool ownerCodeShouldBeAssigned = (CusDecImporter != null && CusDecImporter.AccountNumber.IsEmpty);
				if (ownerCodeShouldBeAssigned)
				{
					CusDecImporter.AccountNumber = BillToAccountNumber.Left(CusDecImporter.AccountNumberInfo.MaxLength);
				}
			}
		}

		UPEOrgHeader fBillTo;

		#endregion

		#region JobHeader

		public CalloutJobHeader JobHeader
		{
			get
			{
				if (fJobHeader == null)
				{
					ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, PK);
					fJobHeader = (CalloutJobHeader)Factory.LoadTop1(typeof(CalloutJobHeader), filter);
				}
				return fJobHeader;
			}
		}

		public void EnsureJobHeaderExists()
		{
			ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, PK);
			fJobHeader = Factory.LoadTop1<CalloutJobHeader>(filter);

			if (fJobHeader == null)
			{
				fJobHeader = (CalloutJobHeader)Factory.New(typeof(CalloutJobHeader));
				fJobHeader.JH_ParentID = PK;
			}
		}

		CalloutJobHeader fJobHeader;

		#endregion

		#region Callout Payment

		public CalloutPayment Payment
		{
			get
			{
				if (fPayment == null)
				{
					fPayment = new CalloutPayment(this);
				}
				return fPayment;
			}
		}

		CalloutPayment fPayment;

		#endregion

		#endregion

		#region Alerts

		protected override void GetDefaultAlertsList(StringCollection list)
		{
			base.GetDefaultAlertsList(list);
			if (Declaration != null && Declaration.IsCustomsEFTActive)
			{
				list.Add("Direct Debit is enabled for the Declaration on this Shipment.");
			}
		}

		#endregion

		#region Properties

		#region BillingTermsReadOnly

		public ZString BillingTermsReadOnly
		{
			get { return BillingTerms; }
		}

		public ZPropertyInfo BillingTermsReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(BillingTermsReadOnly)); }
		}

		#endregion

		#region ShipmentTypeReadOnly

		public ZString ShipmentTypeReadOnly
		{
			get { return ShipmentType; }
		}

		public ZPropertyInfo ShipmentTypeReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(ShipmentTypeReadOnly)); }
		}

		#endregion

		#region TotalAmountDue

		public static readonly SchemaDecimalColumn TotalAmountDueColumn = ProcessQueueSchema.P4_CustomDecimal4;

		public ZDecimal TotalAmountDue
		{
			get { return CurrentQueue.P4_CustomDecimal4; }
		}

		public ZPropertyInfo TotalAmountDueInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(TotalAmountDue), x => CurrentQueue.P4_CustomDecimal4Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		public void CalculateTotalAmountDue()
		{
			ZDecimal total = 0;
			if (JobHeader != null)
			{
				foreach (CalloutCharge charge in JobHeader.Charges)
				{
					total += charge.NettAmount + charge.GSTAmount;
				}
			}
			CurrentQueue.P4_CustomDecimal4 = total;
		}

		#endregion

		#region PaymentCollectionType

		public ZString PaymentCollectionType
		{
			get { return CurrentQueue.P4_CustomAttrib7; }
			set { CurrentQueue.P4_CustomAttrib7 = value; }
		}

		#endregion

		#region PaymentDate

		public ZDateTime PaymentDate
		{
			get { return CurrentQueue.P4_CustomDate5; }
			set { CurrentQueue.P4_CustomDate5 = value; }
		}

		#endregion

		#region FinanceFreightChargeIncludingGST

		public ZDecimal FinanceFreightChargeIncludingGST
		{
			get
			{
				if (JobHeader != null)
				{
					foreach (CalloutCharge charge in JobHeader.Charges)
					{
						if (charge.JR_Desc == ShipmentChargeDescription.Freight)
						{
							return charge.Amount + charge.GSTAmount;
						}
					}
				}
				return 0m;
			}
		}

		#endregion

		#region FinanceSecurityFeeIncludingGST

		public ZDecimal FinanceSecurityFeeIncludingGST
		{
			get
			{
				if (JobHeader != null)
				{
					foreach (CalloutCharge charge in JobHeader.Charges)
					{
						if (charge.JR_Desc == ShipmentChargeDescription.SecurityFee)
						{
							return charge.Amount + charge.GSTAmount;
						}
					}
				}
				return 0m;
			}
		}

		#endregion

		#region FinanceTerminalFeeAmountIncludingGST

		public ZDecimal FinanceTerminalFeeAmountIncludingGST
		{
			get
			{
				if (JobHeader != null)
				{
					foreach (CalloutCharge charge in JobHeader.Charges)
					{
						if (charge.JR_Desc == ShipmentChargeDescription.ITFCharges)
						{
							return charge.Amount + charge.GSTAmount;
						}
					}
				}
				return 0m;
			}
		}

		#endregion

		#endregion

		#region Read Only

		void SetHAWBPropertiesReadOnly()
		{
			foreach (ZPropertyInfo propertyInfo in this.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				if (propertyInfo.HasSetter &&
					propertyInfo != IsHoldForCollectionInfo &&
					propertyInfo != IsRedirectedInfo &&
					propertyInfo != CS_ConsigneeContactNameInfo &&
					propertyInfo != CS_ConsigneePhoneInfo)
				{
					((IZPropertyInfoObsolete)propertyInfo).ReadOnly = true;
				}
			}
			((IZPropertyInfoObsolete)CS_ShipmentTypeForBindingInfo).ReadOnly = true;
			((IZPropertyInfoObsolete)CS_FreightPrepaidCollectForBindingInfo).ReadOnly = true;
		}

		protected override bool IsCS_ConsigneePhoneReadonly
		{
			get { return false; }
		}

		protected override bool ShouldBeReadonly
		{
			get { return true; }
		}

		protected override bool IsConsigneeDetailsReadOnly
		{
			get { return true; }
		}

		protected override bool IsConsigneeDetailsReadOnlyUnlessUnmatched
		{
			get { return true; }
		}

		protected override bool IsConsignorDetailsReadOnly
		{
			get { return true; }
		}

		protected override bool IsConsignorDetailsReadOnlyUnlessUnmatched
		{
			get { return true; }
		}

		protected override bool IsChildRelatedWayBillsReadOnly
		{
			get { return true; }
		}

		protected override bool IsAbandonedReadOnly
		{
			get { return true; }
		}

		protected override bool IsRTSReadOnly
		{
			get { return true; }
		}

		protected override bool IsFreeDomicileReadOnly
		{
			get { return true; }
		}

		protected override bool IsTranshipmentReadOnly
		{
			get { return true; }
		}

		protected override bool IsPrealertHeldByUserReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo PaymentCollectionTypeInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(PaymentCollectionType), x => CurrentQueue.P4_CustomAttrib7Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		public override CusPartShipCollection PartShips
		{
			get
			{
				CusPartShipCollection result = base.PartShips;
				result.SetReadOnlyIncludingChildren(true);
				return result;
			}
		}

		#endregion

		#region Queue Moving

		public override ZString BillToAccountNumber
		{
			get { return base.BillToAccountNumber; }
			set
			{
				base.BillToAccountNumber = value.Left(BillToAccountNumberInfo.MaxLength);
				AssignOwnerCodeToCusDecImporterIfRequired();
			}
		}

		protected override bool IsHeldForFinanceQueuePayment
		{
			get
			{
				ZBool result = BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
				if (BillTo != null)
				{
					result &= UPEUtility.HoldFinanceQueueDebtorGroups.Contains(BillTo.AccountClass);
				}
				return result;
			}
		}

		protected override bool IsHeldForARQueuePayment
		{
			get
			{
				ZBool result = BillingTerms == BillingTermsCodeDescriptionPairList.Codes.FreightCollect;
				if (BillTo != null)
				{
					result &= UPEUtility.HoldARQueueDebtorGroups.Contains(BillTo.AccountClass);
				}
				return result;
			}
		}

		void IBisiDownload.OnAfterBisiDownload()
		{
			ZDecimal cachedTotalLocalCharges = TotalLocalCharges;

			if (CurrentQueue.IsCustomsQueueCompleted && HasAlternateBroker)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: CUS CPL && HAS ALTERNATE BROKER");
			}
			else if (HasAlternateBroker)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.AlternateBroker,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: HAS ALTERNATE BROKER");
			}
			else if (cachedTotalLocalCharges.IsEmpty)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: NO LOCAL CHARGES");
			}
			else if (IsBillingTermsFreeDomicile)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: BILLING TERMS FREE DOMICILE");
			}
			else if (ThirdPartyIndicator == "5" || ThirdPartyIndicator == "8")
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: THIRD PARTY INDICATOR = 5 OR 8");
			}
			else if (IsPreferredBillTo)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: IS PREFERRED BILL TO");
			}
			else if (cachedTotalLocalCharges >= UPEDataRegistry.Instance.CODConfirmPaymentThreshold)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Finance,
					ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment,
					ZString.Empty,
					"DOWNLOAD: CHARGES >= COD CONFIRM THRESH");
			}
			else if (IsStandardBillTo)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: IS STANDARD BILL TO");
			}
			else if (IsCreditCardOrOnFileCODAccount)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.OnFile,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: IS CREDIT OR ON FILE COD ACC");
			}
			else if (cachedTotalLocalCharges <= UPEDataRegistry.Instance.CODAutoReleaseAtUploadThreshold)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: CHARGES <= COD REL UPLD THRESH");
			}
			else if (IsValidToMoveToChaseQueue(cachedTotalLocalCharges))
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Chase,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: Is valid to move to Chase Queue");
			}
			else if (cachedTotalLocalCharges < UPEDataRegistry.Instance.CODAutoReleaseAndChaseThreshold)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Chase,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: CHARGES <= COD REL CHASE THRESH");
			}
			else if (IsConsigneeOrDeliveryAddressPostcodeBrown)
			{
				Payment.IsCheque = true;
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Completed,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: BROWN POSTCODE");
			}
			else if (IsAROrStandardLegalAccountBillTo)
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.AR,
					ZString.Empty,
					ZString.Empty,
					"DOWNLOAD: AR OR STD LEGAL ACC BILL TO");
			}
			else
			{
				MoveToQueue(
					CommercialQueueCodeDescriptionPairList.Codes.Finance,
					ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment,
					ZString.Empty,
					"DEFAULT CONDITION: FINANCE WITH OQ");
			}
		}

		bool IsValidToMoveToChaseQueue(ZDecimal cachedTotalLocalCharges)
		{
			return IsValidDownloadTimeAccordingToChaseValidationRegistry
				&& IsValidServiceLevelAccordingToChaseValidationRegistry
				&& cachedTotalLocalCharges <= UPEDataRegistry.Instance.ChaseQueueValidationTotalLocalChargesThreshold;
		}

		bool IsValidDownloadTimeAccordingToChaseValidationRegistry
		{
			get
			{
				// check that the current time falls within the time set in the registry
				bool result = false;
				ZDateTime now = ZDateTime.Now;
				string dayOfTheWeekNow = Enum.GetName(typeof(DayOfWeek), now.DayOfWeek);

				foreach (ChaseQueueValidation chaseQueueValiation in UPEDataRegistry.Instance.ChaseQueueValidationRegistryItem.Value)
				{
					if (chaseQueueValiation.DayOfTheWeek == dayOfTheWeekNow.ToUpper()
						&& chaseQueueValiation.TimeFrom.TimeOfDay <= now.TimeOfDay
						&& (chaseQueueValiation.TimeTo.IsEmpty || chaseQueueValiation.TimeTo.TimeOfDay >= now.TimeOfDay))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		bool IsValidServiceLevelAccordingToChaseValidationRegistry
		{
			get
			{
				// check that the shipment's service level is valid based on the registry setting.
				bool result = false;

				foreach (ZString serviceLevel in UPEDataRegistry.Instance.ChaseQueueValidationServiceLevelRegistryItem.Value.ToIListZString)
				{
					if (CS_RS_NK_ServiceLevel == serviceLevel)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		bool IsPreferredBillTo
		{
			get { return BillTo != null && BillTo.IsPreferredAccount; }
		}

		bool IsStandardBillTo
		{
			get { return BillTo != null && BillTo.IsStandardAccount; }
		}

		bool IsCreditCardOrOnFileCODAccount
		{
			get { return BillTo != null && (BillTo.IsCreditCardAccount || BillTo.IsOnFileCODAccount); }
		}

		bool IsAROrStandardLegalAccountBillTo
		{
			get { return BillTo != null && (BillTo.IsARAccount || BillTo.IsStandardLegalAccount); }
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				if (fBusinessObjectsWithRelatedNotes == null)
				{
					ArrayList list = new ArrayList();
					if (base.BusinessObjectsWithRelatedNotes != null)
					{
						list.AddRange(base.BusinessObjectsWithRelatedNotes);
					}

					if (BillTo != null)
					{
						list.Add(BillTo);
					}
					fBusinessObjectsWithRelatedNotes = (BusinessObject[])list.ToArray(typeof(BusinessObject));
				}

				return fBusinessObjectsWithRelatedNotes;
			}
		}
		BusinessObject[] fBusinessObjectsWithRelatedNotes;

		public StmNote GetInvoicingPreferencesNote()
		{
			StmNote result = null;
			if (BillTo != null)
			{
				StmNote[] notes = BillTo.Notes.FindByDescription(PredefinedNoteTypes.Instance.InvoicingPreferences.Description);
				if (notes.Length > 0)
				{
					result = notes[0];
				}
			}

			return result;
		}

		#endregion

		#region CalloutPartPayment

		public CalloutPartPayment CalloutPartPayment
		{
			get
			{
				if (fCalloutPartPayment == null)
				{
					fCalloutPartPayment = new CalloutPartPayment(Factory);
				}
				return fCalloutPartPayment;
			}
		}
		CalloutPartPayment fCalloutPartPayment;

		public ZDecimal PartPaymentAmountToCollect
		{
			get { return CurrentQueue.P4_CustomDecimal5; }
			set { CurrentQueue.P4_CustomDecimal5 = value; }
		}

		public ZBool PartPaymentUsed
		{
			get { return CurrentQueue.P4_CustomFlag2; }
			set { CurrentQueue.P4_CustomFlag2 = value; }
		}

		long PartPaymentControlNumber;
		bool PartPaymentExecutedOnFactorySaving;

		protected override void OnFactorySaving()
		{
			PartPaymentControlNumber = 0;
			PartPaymentExecutedOnFactorySaving = false;
			if (fCalloutPartPayment != null)
			{
				string noteText = "";
				if (CalloutPartPayment.MustAddNote)
				{
					if (CalloutPartPayment.IsControlNumberRequired)
					{
						PartPaymentControlNumber = UPENumberFountains.Instance.PartPaymentControlNumberFountain.GetNext(Factory);
						noteText = "Control Number    : " + ZDateTime.Now.Year.ToString().Substring(2) + PartPaymentControlNumber.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + "\r\n";
					}
					if (CalloutPartPayment.IsRefundRequired && CS_JE_CustomsFormalEntry.IsValid)
					{
						Declaration.IsRefundEnquiry = true;
					}

					PartPaymentAmountToCollect = CalloutPartPayment.AmountToCollect;
					PartPaymentUsed = true;
					noteText += CalloutPartPayment.NoteText;
					Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.PartPaymentNote.Description, noteText);
					PartPaymentExecutedOnFactorySaving = true;
				}
			}
			base.OnFactorySaving();
		}

		#endregion

		#region BISI

		#region BisiDownloadDate

		public static readonly SchemaDateTimeColumn BisiDownloadDateProcessQueueColumn = ProcessQueueSchema.P4_CustomDate2;

		ZDateTime IBisiDownload.TransferredDateTime
		{
			get { return BisiDownloadDate; }
			set { BisiDownloadDate = value; }
		}

		// Note: Does it really need to be on some obscure table???  Why not put it on a new field like BISIUploadedShipmentHeader.T8_BisiDownloadDate?
		public ZDateTime BisiDownloadDate
		{
			get { return CurrentQueue.P4_CustomDate2; }
			set { CurrentQueue.P4_CustomDate2 = value; }
		}

		public ZPropertyInfo BisiDownloadDateInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(BisiDownloadDateInfo), x => CurrentQueue.P4_CustomDate2Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region IsExcludedFromBISIWarning

		public static readonly SchemaBoolColumn IsExcludedFromBISIWarningProcessQueueColumn = ProcessQueueSchema.P4_CustomFlag1;

		public ZBool IsExcludedFromBISIWarning
		{
			get { return CurrentQueue.P4_CustomFlag1; }
			set
			{
				CurrentQueue.P4_CustomFlag1 = value;
				IsExcludedFromBISIWarningInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsExcludedFromBISIWarningInfo
		{
			get { return GetZPropertyInfo(nameof(IsExcludedFromBISIWarning)); }
		}

		#endregion

		#region BISIDownloadNotRequiredOrForcedCaption

		public ZString BISIDownloadNotRequiredOrForcedCaption
		{
			get
			{
				ZString result = ZString.Empty;
				if (!((IShipmentData)this).ShouldBeDownloaded)
				{
					result = "NOT REQUIRED";
				}
				else if (WasForcedToFinance)
				{
					result = "FORCED";
				}
				return result;
			}
		}

		public ZPropertyInfo BISIDownloadNotRequiredOrForcedCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(BISIDownloadNotRequiredOrForcedCaption)); }
		}

		bool WasForcedToFinance
		{
			get
			{
				if (!fWasForcedToFinancePopulated)
				{
					foreach (ProcessQueueLog log in CurrentQueue.CommercialQueueLogs)
					{
						if (log.Reason.ToLower().IndexOf(ForcedToFinanceQueueRemarks.ToLower()) != -1)
						{
							fWasForcedToFinance = true;
						}
					}
					fWasForcedToFinancePopulated = true;
				}
				return fWasForcedToFinance;
			}
		}
		bool fWasForcedToFinance;
		bool fWasForcedToFinancePopulated;

		#endregion

		#endregion

		#region Edit log for IsExcludedFromBISIWarning change

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();
			if (IsInDatabase &&
				IsExcludedFromBISIWarning != (ZBool)CurrentQueue.ZPropertyInfoHash[IsExcludedFromBISIWarningProcessQueueColumn.Name].OriginalValue &&
				Logs.AutoCreatedLog.SL_Reference.IndexOf("BISI Warning Report") == -1)
			{
				ZString referenceToAppend = Logs.AutoCreatedLog.SL_Reference.IsEmpty ? "" : "; ";
				referenceToAppend += IsExcludedFromBISIWarning ? "Excluded from BISI Warning Report" : "Included in BISI Warning Report";

				using (((IUpdateFieldsLock)Logs.AutoCreatedLog).LockForUpdatingKeyFields())
				{
					Logs.AutoCreatedLog.SL_Reference += referenceToAppend;
				}
			}
		}

		#endregion

		#region Auto Delivery of Documents on BISI Download

		bool AutoDeliverDocumentsRequiredOnFactorySaved;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			AutoDeliverDocumentsRequiredOnFactorySaved =
				IsInDatabase &&
				!BisiDownloadDate.IsEmpty &&
				(ZDateTime)BisiDownloadDateInfo.OriginalValue != BisiDownloadDate;
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && AutoDeliverDocumentsRequiredOnFactorySaved)
			{
				AutoDeliverDocumentsOnBISIDownload();
			}
			AutoDeliverDocumentsRequiredOnFactorySaved = false;

			if (saveSucceeded)
			{
				if (PartPaymentExecutedOnFactorySaving)
				{
					try
					{
						ZString partPaymentControlNumberAsString = PartPaymentControlNumber > 0 ? PartPaymentControlNumber.ToString(CultureInfo.InvariantCulture) : "";
						CalloutPartPayment.SendNotificationEmail(CS_HAWB, InvoiceNumber, TotalAmountDue, partPaymentControlNumberAsString);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ZString key = PartPaymentExecutedOnFactorySaving ? "CalloutPartPayment.SendNotificationEmail Failed" : "RefundEnquiry.SendNotificationEmail Failed";
						ErrorReporter.ReportOnce(key, "Notification Email sending failed for shipment" + CS_HAWB, ex);
					}
					fCalloutPartPayment = null;
					PartPaymentExecutedOnFactorySaving = false;
				}
			}
		}

		void AutoDeliverDocumentsOnBISIDownload()
		{
			if (RequiresAutoDeliverTaxInvoice)
			{
				GetTaxInvoiceAutoDelivery().Deliver();
			}
			if (RequiresAutoDeliverAlternateBrokerDocumentPack)
			{
				GetAlternateBrokerDocumentPackAutoDelivery(Declaration).Deliver();
			}
		}

		bool RequiresAutoDeliverTaxInvoice
		{
			get
			{
				bool suppressAutoDelivery = TotalAmountDue == 0 && (ThirdPartyIndicator == "1" || ThirdPartyIndicator == "2" || ThirdPartyIndicator == "4" || ThirdPartyIndicator == "7");
				return !suppressAutoDelivery;
			}
		}

		bool RequiresAutoDeliverAlternateBrokerDocumentPack
		{
			get
			{
				return Declaration != null
					&& Declaration.AlternateBroker != null
					&& Declaration.Importer != null
					&& (Declaration.Importer.IsITFChargableForThisImporter || FinanceFreightChargeIncludingGST != 0m);
			}
		}

		internal virtual UPETaxInvoiceAutoDelivery GetTaxInvoiceAutoDelivery()
		{
			return ShouldCommercialInvoiceBeDelivered ?
				new UPETaxInvoiceAndCommercialInvoiceAutoDelivery(this) :
				new UPETaxInvoiceAutoDelivery(this);
		}

		bool ShouldCommercialInvoiceBeDelivered
		{
			get
			{
				return (BillTo != null && BillTo.ShouldReceiveCommercialInvoice &&
						AccountNumberStartsWith_A_E());
			}
		}

		bool AccountNumberStartsWith_A_E()
		{
			return BillToAccountNumber.StartsWith("A") || BillToAccountNumber.StartsWith("E");
		}

		protected virtual UPEAlternateBrokerDocumentPackAutoDelivery GetAlternateBrokerDocumentPackAutoDelivery(UPEJobDeclaration declaration)
		{
			return new UPEAlternateBrokerDocumentPackAutoDelivery(declaration);
		}

		#endregion

		#region IRefundEnquiry

		public ClientRefund Refund
		{
			get
			{
				if (refund == null)
				{
					if (RelatedOwner != null)
					{
						if (RelatedOwner.Refund != null)
						{
							refund = RelatedOwner.Refund;
							refund.T10_CS = PK;
						}
					}

					if (refund == null)
					{
						refund = Factory.LoadTop1<ClientRefund>(new ZQuery(ClientRefundSchema.T10_CS, PK));
						if (refund != null && Declaration != null)
						{
							refund.T10_JE = Declaration.PK;
						}
					}

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

		protected ClientRefund refund;

		SchemaGuidColumn IRefundEnquiry.OwnerColumn
		{
			get { return ClientRefundSchema.T10_CS; }
		}

		public IRefundEnquiry RelatedOwner
		{
			get { return Declaration; }
		}

		void IRefundEnquiry.AddRefundNote()
		{
			if (Refund != null)
			{
				Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.FinanceNote.Description, Refund.NoteText);
			}
		}

		void IRefundEnquiry.SendNotificationEmail() { }

		public ZBool IsRefundEnquiry
		{
			get { return RelatedOwner == null ? ZBool.False : RelatedOwner.IsRefundEnquiry; }
			set
			{
				if (RelatedOwner != null)
				{
					RelatedOwner.IsRefundEnquiry = value;
				}
			}
		}

		public ZBool IsRefundProcessed
		{
			get { return RelatedOwner == null ? ZBool.False : RelatedOwner.IsRefundProcessed; }
			set
			{
				if (RelatedOwner != null)
				{
					RelatedOwner.IsRefundProcessed = value;
				}
			}
		}

		public RefundManager RefundManager
		{
			get { return RelatedOwner == null ? null : RelatedOwner.RefundManager; }
		}

		public ClientRefundWrapper RefundWrapper
		{
			get { return refundWrapper ?? (refundWrapper = new ClientRefundWrapper(Factory) { RefundOwner = this }); }
		}
		ClientRefundWrapper refundWrapper;

		#endregion

		public override string ToString(ZString delimiter)
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append(base.ToString(delimiter));
			result.Append("TotalAmountDue=" + TotalAmountDue);
			return result.ToStringWithDelimiterBetweenAppends(delimiter);
		}

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CurrentQueue.P4_QueueName = "ANY";
		}

#endif
		#endregion
	}
}
