using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationAmendmentMessageSendingObject : JobDeclarationMiscMessageSendingObjectCore, IDHSAmendmentDetails, IImport5UASessionDetails
	{
		public JobDeclarationAmendmentMessageSendingObject(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
			if (entry?.EntryInstruction != null)
			{
				if (AmendmentSessionalData == null)
				{
					amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
					amendmentSessionalData.CSI_LineNo = entry.CH_VersionID + 1;
				}
				AmendmentSessionalData.CSI_Code = AmendmentType.SubstringSafe(0, 1);
			}
			SetDefaultRefundAmount();
		}

		/// <summary>
		/// Please do not use this option as it is used only for the binding purpose.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="messageType"></param>
		public JobDeclarationAmendmentMessageSendingObject(BusinessObjectFactory factory, ZString messageType) : base(null, messageType)
		{
		}

		public ZString OriginalMessageType => AmendmentManager.OriginalMessageType;
		public override ZShort AmendmentVersion
		{
			get
			{
				var result = base.AmendmentVersion;
				if (ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(MessageType))
				{
					var entryNum = Header.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == OriginalMessageType);
					var version = entryNum?.CE_EntryLineReference ?? ZString.Empty;
					result = ZShort.ParseSafe(version, 0);
				}
				else if (MessageType == ElectronicDocumentTypeList.Codes._5FE)
				{
					result = VersionNumberExtensionMethods.CalculateNextCustoms5FEVersionNumber(Header);
				}
				return result;
			}
		}
		public override ZString AmendmentType => AmendmentManager.AmendmentType;
		public override ZString AmendmentTypeDescription
		{
			get
			{
				var result = ZString.Empty;
				switch (MessageType)
				{
					case ElectronicDocumentTypeList.Codes._5BB:
						result = Factory.GetCachedValue<_5BBAmendmentType>().GetDescriptionFromCode(AmendmentType);
						break;
					case ElectronicDocumentTypeList.Codes._5AS:
						result = Factory.GetCachedValue<_5ASAmendmentType>().GetDescriptionFromCode(AmendmentType);
						break;
					case ElectronicDocumentTypeList.Codes._5DS:
					case ElectronicDocumentTypeList.Codes._5DR:
						result = Factory.GetCachedValue<LocalExportAmendmentTypeList>().GetDescriptionFromCode(AmendmentType);
						break;
					case ElectronicDocumentTypeList.Codes._105:
					case ElectronicDocumentTypeList.Codes._DHS:
						result = Factory.GetCachedValue<FTAAmendmentType>().GetDescriptionFromCode(AmendmentType);
						break;
					case ElectronicDocumentTypeList.Codes._5FE:
						var strBuilder = new ZStringBuilder();
						strBuilder.Append(GetFormattedDescriptionAndCode(Factory.GetCachedValue<DutyTaxCorrectionCodeList>(), AmendmentType.SubstringSafe(0, 1)));
						strBuilder.Append(GetFormattedDescriptionAndCode(Factory.GetCachedValue<DeclarationCorrectionCodeList>(), AmendmentType.SubstringSafe(1, 1)));
						result = strBuilder.ToStringWithDelimiterBetweenAppends(", ");
						break;
				}
				return result;
			}
		}
		ZString GetFormattedDescriptionAndCode(CodeDescriptionPairList list, ZString amendmentType)
		{
			var result = ZString.Empty;
			result = ZString.Format("{0}({1})", list.GetDescriptionFromCode(amendmentType), amendmentType);
			return result;
		}

		public new JobDeclarationAmendmentMessageSendingObjectLookups Lookups => new JobDeclarationAmendmentMessageSendingObjectLookups(this);
		public new JobDeclarationAmendmentMessageSendingObjectValidation Validation => new JobDeclarationAmendmentMessageSendingObjectValidation(this);
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new JobDeclarationAmendmentMessageSendingObjectValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}
		public AmendmentDetailsManager AmendmentManager => amendmentManager ?? (amendmentManager = AmendmentDetailsManager.New(Header, MessageType));
		AmendmentDetailsManager amendmentManager;
		public AmendedItemCollection AmendedItems
		{
			get
			{
				if (amendedItems == null)
				{
					amendedItems = new AmendedItemCollection(AmendmentManager.AmendedItems, AmendmentManager.DataItemIDList);
					amendedItems.SetReadOnlyIncludingChildren(true);
				}
				return amendedItems;
			}
		}
		AmendedItemCollection amendedItems;

		public AmendedItemCollection AmendedDutyTaxItems
		{
			get
			{
				if (amendedDutyTaxItems == null)
				{
					var items = AmendmentManager.AmendedItems.Where(item => ImportAmendmentDataItemIDList.IsDutyTaxItemField(item.DataItemID));
					amendedDutyTaxItems = new AmendedItemCollection(items);
					amendedDutyTaxItems.SetReadOnlyIncludingChildren(true);

					var list = Factory.GetCachedValue<EntryTaxTypeList>();
					foreach (AmendedItem element in amendedDutyTaxItems)
					{
						element.DutyTaxType = DataItemIDProvider.GetCustomsFeeID(typeof(IImportEntryHeader), element?.DataItemID ?? ZString.Empty);
						element.DutyTaxTypeDescription = list.GetDescriptionFromCode(element.DutyTaxType);
					}
				}
				return amendedDutyTaxItems;
			}
		}
		AmendedItemCollection amendedDutyTaxItems;

		IImportEntryHeader LatestImportEntryHeader
		{
			get
			{
				if (latestImportEntryHeader == null)
				{
					var snapshot = Header.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
					if (snapshot != null)
					{
						using (var textReader = snapshot.GetCES_SnapshotXmlReader())
						{
							latestImportEntryHeader = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
						}
					}
				}
				return latestImportEntryHeader;
			}
		}
		IImportEntryHeader latestImportEntryHeader;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|BeforeTotalDutyTaxAmount", Caption = "Before Total Duty Tax Amount")]
		public ZDecimal BeforeTotalDutyTaxAmount => LatestImportEntryHeader?.TotalPayableAmount ?? ZDecimal.Zero;
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|AfterTotalDutyTaxAmount", Caption = "After Total Duty Tax Amount")]

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal AfterTotalDutyTaxAmount => Header.TotalAmountPayable;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|DutyTaxDifference", Caption = "Duty Tax Difference")]
		public ZDecimal DutyTaxDifference => AfterTotalDutyTaxAmount - BeforeTotalDutyTaxAmount;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|BeforeCustomsValue", Caption = "Before Customs Value")]
		public ZDecimal BeforeCustomsValue => LatestImportEntryHeader?.TotalCustomsValueKRW ?? ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|AfterCustomsValue", Caption = "After Customs Value")]
		public ZDecimal AfterCustomsValue => Header.CustomsValue;

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|CustomsValueDifference", Caption = "Customs Value Difference")]
		public ZDecimal CustomsValueDifference => AfterCustomsValue - BeforeCustomsValue;

		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|AmendmentTypeForInvoiceLine", Caption = "C/O Amendment Type")]
		public ZString AmendmentTypeForInvoiceLine
		{
			get
			{
				if (MessageType == ElectronicDocumentTypeList.Codes._DHS)
				{
					return ((GOVCBRDHSAmendmentDetailsManager)AmendmentManager).AmendmentTypeForInvoiceLine;
				}
				return ZString.Empty;
			}
		}

		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|LawCodeDescription", Caption = "FTA Law Code")]
		public ZString LawCodeDescription
		{
			get
			{
				if (MessageType == ElectronicDocumentTypeList.Codes._105)
				{
					return ((GOVCBR105AmendmentDetailsManager)AmendmentManager).LawCodeDescription;
				}
				else if (MessageType == ElectronicDocumentTypeList.Codes._DHS)
				{
					return ((GOVCBRDHSAmendmentDetailsManager)AmendmentManager).LawCodeDescription;
				}
				return ZString.Empty;
			}
		}

		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|CustomsDisbursementBillNumber", Caption = "Customs Disbursement Bill #")]
		public ZString CustomsDisbursementBillNumber => Header.EntryInstruction?.CEI_StatementNumber5WN ?? ZString.Empty;

		PenaltyExemptionSessionalData PenaltyExemptionSessionalData => AmendmentSessionalData?.PenaltyExemptionSessionalData;

		[MaxLength(3)]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|PenaltyExemptionReqSequence", Caption = "Penalty Exempt Seq.")]
		public ZInt PenaltyExemptionReqSequence => PenaltyExemptionSessionalData?.PenaltyExemptionRequestVersionNo ?? ZInt.Zero;
		public ZPropertyInfo PenaltyExemptionReqSequenceInfo => GetZPropertyInfo(nameof(PenaltyExemptionReqSequence));

		public ZBool IsIncluding5UAIn5FE => MessageType == ElectronicDocumentTypeList.Codes._5FE
			&& PenaltyExemptionIndicator == Constants.YesNo.Yes
			&& PenaltyExemptionReasonCodeList.IsLegalReasonCode(PenaltyExemptionReasonCode);

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.DutyPenaltyExemptionCodeList))]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|PenaltyExemptionIndicator", Caption = "Penalty Exempt Req.")]
		[ReadOnlyMember(nameof(IsPenaltyExemptionIrrelevant))]
		public ZString PenaltyExemptionIndicator
		{
			get => PenaltyExemptionSessionalData?.PenaltyExemptionCode ?? DutyPenaltyExemptionCodeList.Codes.X;
			set
			{
				if (PenaltyExemptionSessionalData != null)
				{
					PenaltyExemptionSessionalData.PenaltyExemptionCode = value;
					if (PenaltyExemptionIndicator == DutyPenaltyExemptionCodeList.Codes.N)
					{
						PenaltyExemptionReasonCode = ZString.Empty;
						PenaltyExemptionReason = ZString.Empty;
					}
					Apply5UASequenceLogic();
					if (!IsValidationSuspended)
					{
						Validation.ValidatePenaltyExemptionIndicator();
					}
					PenaltyExemptionReqSequenceInfo.RefreshBinding();
					PenaltyExemptionIndicatorInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo PenaltyExemptionIndicatorInfo => GetZPropertyInfo(nameof(PenaltyExemptionIndicator));

		public ZBool IsPenaltyExemptionIrrelevant => PenaltyExemptionSessionalData == null;

		[MaxLength(2)]
		[ReadOnlyMember(nameof(IsPenaltyExemptionNotRequested))]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.AdditiveTaxExemptionReasonCodeList))]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|PenaltyExemptionReasonCode", Caption = "Penalty Exempt Reason Code")]
		public ZString PenaltyExemptionReasonCode
		{
			get => penaltyExemptionReasonCode;
			set
			{
				SetNonPersistentPropertyValue(PenaltyExemptionReasonCodeInfo, ref penaltyExemptionReasonCode, value);
				Apply5UASequenceLogic();

				if (!IsValidationSuspended)
				{
					Validation.ValidatePenaltyExemptionReasonCode();
				}
				PenaltyExemptionReqSequenceInfo.RefreshBinding();
			}
		}
		ZString penaltyExemptionReasonCode;
		public ZPropertyInfo PenaltyExemptionReasonCodeInfo => GetZPropertyInfo(nameof(PenaltyExemptionReasonCode));

		public bool IsPenaltyExemptionRequested => PenaltyExemptionIndicator == YesNoList.Codes.Yes;
		bool IsPenaltyExemptionNotRequested => !IsPenaltyExemptionRequested;

		[MaxLength(250)]
		[ReadOnlyMember(nameof(IsPenaltyExemptionNotRequested))]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|PenaltyExemptionReason", Caption = "Penalty Exempt Reason")]
		public ZString PenaltyExemptionReason
		{
			get => penaltyExemptionReason;
			set
			{
				SetNonPersistentPropertyValue(PenaltyExemptionReasonInfo, ref penaltyExemptionReason, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePenaltyExemptionReason();
				}
				PenaltyExemptionReasonInfo.RefreshBinding();
			}
		}
		ZString penaltyExemptionReason;
		public ZPropertyInfo PenaltyExemptionReasonInfo => GetZPropertyInfo(nameof(PenaltyExemptionReason));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|PenaltyExemptionAmount", Caption = "Penalty Exempt Amount")]
		public ZDecimal PenaltyExemptionAmount => PenaltyExemptionSessionalData?.PenaltyExemptionAmount ?? ZDecimal.Zero;
		public ZPropertyInfo PenaltyExemptionAmountInfo => GetZPropertyInfo(nameof(PenaltyExemptionAmount));

		public AmendmentSessionalData AmendmentSessionalData => amendmentSessionalData ??= Header.EntryInstruction?.AmendmentSessionalDataCollection.Where(x => x.AmendmentCW1VersionNo == (ZInt)Header.CH_VersionID + 1).FirstOrDefault();
		AmendmentSessionalData amendmentSessionalData;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.DomesticTaxPenaltyTypeCodeList))]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|DomesticTaxPenaltyType", Caption = "Domestic Tax Penalty Type")]
		public ZString TaxPenaltyCause
		{
			get => AmendmentSessionalData?.TaxPenaltyCause ?? ZString.Empty;
			set
			{
				if (AmendmentSessionalData != null)
				{
					AmendmentSessionalData.TaxPenaltyCause = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTaxPenaltyCause();
					}
					TaxPenaltyCauseInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo TaxPenaltyCauseInfo => GetZPropertyInfo(nameof(TaxPenaltyCause));

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.DutyPenaltyTypeCodeList))]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|DutyPenaltyType", Caption = "DTY Penalty Type")]
		public ZString DutyPenaltyCause
		{
			get => AmendmentSessionalData?.DutyPenaltyCause ?? ZString.Empty;
			set
			{
				if (AmendmentSessionalData != null)
				{
					AmendmentSessionalData.DutyPenaltyCause = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateDutyPenaltyCause();
					}
					DutyPenaltyCauseInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo DutyPenaltyCauseInfo => GetZPropertyInfo(nameof(DutyPenaltyCause));

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.DutyPenaltyReducedYNCodeList))]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|DutyPenaltyReducedYN", Caption = "DTY Penalty Reduced Y/N")]
		public ZString ApplyDutyPenaltyReduction
		{
			get
			{
				return PenaltyExemptionSessionalData?.ApplyDutyPenaltyReduction ?? ZString.Empty;
			}
			set
			{
				if (PenaltyExemptionSessionalData != null)
				{
					PenaltyExemptionSessionalData.ApplyDutyPenaltyReduction = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateApplyDutyPenaltyReduction();
					}
					ApplyDutyPenaltyReductionInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo ApplyDutyPenaltyReductionInfo => GetZPropertyInfo(nameof(ApplyDutyPenaltyReduction));

		public CusEntryNumber EntryNumber5ULInTransaction => Header.EntryNumbers.Cast<CusEntryNumber>().OrderByDescending(x => x.CE_SystemCreateTimeUtc).FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL && x.CE_EntryLineReference == CustomsDisbursementBill && x.CE_EntryStatus != CustomsMessageStatusTypeList.Codes.OriginalAccepted);
		public ZString RefundRequestNumber
		{
			get
			{
				var result = ZString.Empty;
				if (RefundRequestSubmissionYN == Constants.YesNo.Yes)
				{
					result = EntryNumber5ULInTransaction?.CE_EntryNum ?? EDIMessage.RefundEntryNumberPlaceHolder;
				}
				return result;
			}
		}

		[MaxLength(1)]
		[ReadOnlyMember(nameof(IsRefundRequestIrrelevant))]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.RefundRequestSubmissionYNCodeList))]
		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|RefundRequestSubmissionYN", Caption = "Refund Request Y/N")]
		public ZString RefundRequestSubmissionYN
		{
			get => refundRequestSubmissionYN;
			set
			{
				SetNonPersistentPropertyValue(RefundRequestSubmissionYNInfo, ref refundRequestSubmissionYN, value);
				if (value == YesNoList.Codes.Yes && (Header.EntryInstruction?.CEI_RefundType.IsEmpty ?? false))
				{
					Header.EntryInstruction.CEI_RefundType = RefundTypeList.Codes.A;
					RefundTypeInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundRequestSubmissionYN();
				}
			}
		}
		ZString refundRequestSubmissionYN;
		public ZPropertyInfo RefundRequestSubmissionYNInfo => GetZPropertyInfo(nameof(RefundRequestSubmissionYN));
		bool IsRefundRequestIrrelevant => AmendmentType.Left(1) != DutyTaxCorrectionCodeList.Codes.C;

		[ResourceStringData("5986FC06-7A5F-4AC8-8B9E-C8DF910DAE67", Caption = "5UL sent with 5FE")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.RefundRequestSubmissionYNCodeList))]
		public ZString Is5ULSentWith5FE => MessageType == ElectronicDocumentTypeList.Codes._5FE ? RefundRequestSubmissionYN : YesNoList.Codes.No;

		[MaxLength(6)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.DetectionPatternCodeList))]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|PenaltyPaymentReasonCode", Caption = "Additional Payment Reason Code")]
		[ReadOnlyMember(nameof(IsPenaltyExemptionNotRequested))]
		public ZString PenaltyPaymentReasonCode
		{
			get => penaltyPaymentReasonCode;
			set
			{
				CheckMaximumLength(PenaltyPaymentReasonCodeInfo, value);
				SetNonPersistentPropertyValue(PenaltyPaymentReasonCodeInfo, ref penaltyPaymentReasonCode, value);
			}
		}
		ZString penaltyPaymentReasonCode;
		public ZPropertyInfo PenaltyPaymentReasonCodeInfo => GetZPropertyInfo(nameof(PenaltyPaymentReasonCode));

		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|SubmissionDate", Caption = "Submission Date")]
		public ZDateTime SubmissionDate => ZDateTime.Today;

		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|TotalAmendedItemsCount", Caption = "Total Amended Items Count")]
		public ZInt TotalAmendedItemsCount => AmendedItems.Count;

		[ResourceStringData("JobDeclarationAmendmentMessageSendingObject|TotalAmendedTaxCount", Caption = "Total Amended Tax Count")]
		public ZInt TotalAmendedTaxCount => AmendedItems.Where(x => ImportAmendmentDataItemIDList.IsDutyTaxItemField(x.DataItemID)).Count();

		[ResourceStringData("8621DC32-0BFB-458F-9594-C68E45E09869", Caption = "Declarant Type")]
		public ZString DeclarantType => Header.Declaration.IsSelfDeclaringOwner ? Constants.DeclarantType.GOVCBR5FE.Importer : Constants.DeclarantType.GOVCBR5FE.Broker;
		ZDate IAmendmentDetails.DateOfFinalPrice => DateOfFinalPrice;
		ZString IDHSAmendmentDetails.AmendmentTypeForInvoiceLine => AmendmentTypeForInvoiceLine;
		ZString IImport5UASessionDetails.PenaltyExemptionIndicator => PenaltyExemptionIndicator;
		ZString IImport5UASessionDetails.PenaltyExemptionReasonCode => PenaltyExemptionReasonCode;
		ZString IImport5UASessionDetails.PenaltyExemptionReason => PenaltyExemptionReason;
		ZInt IImport5UASessionDetails.DutyPenaltyExemption5UASequenceNumber => PenaltyExemptionReqSequence;
		ZDecimal IImport5UASessionDetails.PenaltyExemptionAmount => PenaltyExemptionAmount;

		[ResourceStringData("807E8187-639D-49E3-AC3F-8483680DA83D", Caption = "Refund Type")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.RefundTypeList))]
		public ZString RefundType => Header.EntryInstruction?.CEI_RefundType ?? ZString.Empty;
		ZPropertyInfo RefundTypeInfo => GetZPropertyInfo(nameof(RefundType));

		[ResourceStringData("F7475127-E3AF-4F24-9FA0-2A6E52B4CBBF", Caption = "Refund Cause")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.RefundCauseCodeList))]
		public ZString RefundCause => Header.EntryInstruction?.CEI_RefundCauseCode ?? ZString.Empty;

		[ResourceStringData("8E1C3C48-516F-4C47-8051-626B6CB32608", Caption = "Refund Reason")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.RefundReasonCodeList))]
		public ZString RefundReason => Header.EntryInstruction?.CEI_RefundReasonCode ?? ZString.Empty;

		[MaxLength(3)]
		[ResourceStringData("30B7C1A1-1C2E-4821-8A0D-F52ADC381B50", Caption = "Tax Office")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationAmendmentMessageSendingObjectLookups.TaxOfficeList))]
		public ZString TaxOffice
		{
			get => Header.Declaration.JE_TaxOffice;
			set => Header.Declaration.JE_TaxOffice = value;
		}

		public ZString CustomsDisbursementBill => Header.StatementLine929?.IndividualCustomsDisbursementBillNo ?? ZString.Empty;
		[ResourceStringData("7A974A6D-29F6-47CC-BBEA-94A2094A1E9F", Caption = "Customs Disbursement Bill #")]
		public ZString FormattedCustomsDisbursementBill => Header.StatementLine929?.FormattedCustomsDisbursementBill ?? ZString.Empty;

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfDutyAmount
		{
			get => refundAmountOfDutyAmount;
			set => SetNonPersistentPropertyValue(RefundAmountOfDutyAmountInfo, ref refundAmountOfDutyAmount, value);
		}
		ZDecimal refundAmountOfDutyAmount;
		public ZPropertyInfo RefundAmountOfDutyAmountInfo => this.GetZPropertyInfo(nameof(RefundAmountOfDutyAmount));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfLiquorTax
		{
			get => refundAmountOfLiquorTax;
			set => SetNonPersistentPropertyValue(RefundAmountOfLiquorTaxInfo, ref refundAmountOfLiquorTax, value);
		}
		ZDecimal refundAmountOfLiquorTax;
		public ZPropertyInfo RefundAmountOfLiquorTaxInfo => this.GetZPropertyInfo(nameof(RefundAmountOfLiquorTax));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfSpecialConsumptionTax
		{
			get => refundAmountOfSpecialConsumptionTax;
			set => SetNonPersistentPropertyValue(RefundAmountOfSpecialConsumptionTaxInfo, ref refundAmountOfSpecialConsumptionTax, value);
		}
		ZDecimal refundAmountOfSpecialConsumptionTax;
		public ZPropertyInfo RefundAmountOfSpecialConsumptionTaxInfo => this.GetZPropertyInfo(nameof(RefundAmountOfSpecialConsumptionTax));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfTransportationTax
		{
			get => refundAmountOfTransportationTax;
			set => SetNonPersistentPropertyValue(RefundAmountOfTransportationTaxInfo, ref refundAmountOfTransportationTax, value);
		}
		ZDecimal refundAmountOfTransportationTax;
		public ZPropertyInfo RefundAmountOfTransportationTaxInfo => this.GetZPropertyInfo(nameof(RefundAmountOfTransportationTax));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfEducationTax
		{
			get => refundAmountOfEducationTax;
			set => SetNonPersistentPropertyValue(RefundAmountOfEducationTaxInfo, ref refundAmountOfEducationTax, value);
		}
		ZDecimal refundAmountOfEducationTax;
		public ZPropertyInfo RefundAmountOfEducationTaxInfo => this.GetZPropertyInfo(nameof(RefundAmountOfEducationTax));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfAgricultureTax
		{
			get => refundAmountOfAgricultureTax;
			set => SetNonPersistentPropertyValue(RefundAmountOfAgricultureTaxInfo, ref refundAmountOfAgricultureTax, value);
		}
		ZDecimal refundAmountOfAgricultureTax;
		public ZPropertyInfo RefundAmountOfAgricultureTaxInfo => this.GetZPropertyInfo(nameof(RefundAmountOfAgricultureTax));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfVAT
		{
			get => refundAmountOfVAT;
			set => SetNonPersistentPropertyValue(RefundAmountOfVATInfo, ref refundAmountOfVAT, value);
		}
		ZDecimal refundAmountOfVAT;
		public ZPropertyInfo RefundAmountOfVATInfo => this.GetZPropertyInfo(nameof(RefundAmountOfVAT));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		[ResourceStringData("A023B72E-247C-4EE0-B146-870A61105DA8", Caption = "Value For VAT")]
		public ZDecimal RefundAmountOfValueForVAT => RefundAmountOfVAT * 10;

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		[ResourceStringData("DC0B6BD6-9A8D-4FDA-9DFC-B5C3BB8EB69B", Caption = "VAT Exemption Value")]
		public ZDecimal RefundAmountOfVATExemptionValue => ZDecimal.Zero;

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfPenaltyForLateDeclaration
		{
			get => refundAmountOfPenaltyForLateDeclaration;
			set => SetNonPersistentPropertyValue(RefundAmountOfPenaltyForLateDeclarationInfo, ref refundAmountOfPenaltyForLateDeclaration, value);
		}
		ZDecimal refundAmountOfPenaltyForLateDeclaration;
		public ZPropertyInfo RefundAmountOfPenaltyForLateDeclarationInfo => this.GetZPropertyInfo(nameof(RefundAmountOfPenaltyForLateDeclaration));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfPenaltyForMissedDeclaration
		{
			get => refundAmountOfPenaltyForMissedDeclaration;
			set => SetNonPersistentPropertyValue(RefundAmountOfPenaltyForMissedDeclarationInfo, ref refundAmountOfPenaltyForMissedDeclaration, value);
		}
		ZDecimal refundAmountOfPenaltyForMissedDeclaration;
		public ZPropertyInfo RefundAmountOfPenaltyForMissedDeclarationInfo => this.GetZPropertyInfo(nameof(RefundAmountOfPenaltyForMissedDeclaration));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfPenaltyForLatePayment
		{
			get => refundAmountOfPenaltyForLatePayment;
			set => SetNonPersistentPropertyValue(RefundAmountOfPenaltyForLatePaymentInfo, ref refundAmountOfPenaltyForLatePayment, value);
		}
		ZDecimal refundAmountOfPenaltyForLatePayment;
		public ZPropertyInfo RefundAmountOfPenaltyForLatePaymentInfo => this.GetZPropertyInfo(nameof(RefundAmountOfPenaltyForLatePayment));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal RefundAmountOfNonDutyTaxRevenue
		{
			get => refundAmountOfNonDutyTaxRevenue;
			set => SetNonPersistentPropertyValue(RefundAmountOfNonDutyTaxRevenueInfo, ref refundAmountOfNonDutyTaxRevenue, value);
		}
		ZDecimal refundAmountOfNonDutyTaxRevenue;
		public ZPropertyInfo RefundAmountOfNonDutyTaxRevenueInfo => this.GetZPropertyInfo(nameof(RefundAmountOfNonDutyTaxRevenue));

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalLateRefundAmount => RefundAmountOfPenaltyForLateDeclaration + RefundAmountOfPenaltyForMissedDeclaration
											   + RefundAmountOfPenaltyForLatePayment + RefundAmountOfNonDutyTaxRevenue;

		void Apply5UASequenceLogic()
		{
			if (PenaltyExemptionSessionalData == null)
			{
				return;
			}
			if (IsIncluding5UAIn5FE)
			{
				var max5UAEntryNum = Header.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UA);
				var max5UAMaxVersionNumber = ZInt.ParseSafe(max5UAEntryNum?.CE_EntryLineReference ?? ZString.Empty, 0);
				PenaltyExemptionSessionalData.CSI_LineNo = max5UAMaxVersionNumber + 1;
			}
			else
			{
				PenaltyExemptionSessionalData.CSI_LineNo = 0;
			}
		}

		void SetDefaultRefundAmount()
		{
			RefundAmountOfDutyAmount = GetDutyOrTaxAmount(ChargeTypeList.Codes.Duty);
			RefundAmountOfLiquorTax = GetDutyOrTaxAmount(ChargeTypeList.Codes.LiquorTax);
			RefundAmountOfSpecialConsumptionTax = GetDutyOrTaxAmount(ChargeTypeList.Codes.SpecialConsumptionTax);
			RefundAmountOfTransportationTax = GetDutyOrTaxAmount(ChargeTypeList.Codes.TransportationTax);
			RefundAmountOfEducationTax = GetDutyOrTaxAmount(ChargeTypeList.Codes.EducationTax);
			RefundAmountOfAgricultureTax = GetDutyOrTaxAmount(ChargeTypeList.Codes.AgricultureTax);
			RefundAmountOfVAT = GetDutyOrTaxAmount(ChargeTypeList.Codes.VAT);
			RefundAmountOfPenaltyForLateDeclaration = GetPositiveRefundAmount(Header.Charges.GetAmount(ChargeTypeList.Codes.PenaltyForLateDeclaration));
			RefundAmountOfPenaltyForMissedDeclaration = GetPositiveRefundAmount(Header.Charges.GetAmount(ChargeTypeList.Codes.PenaltyForMissedDeclaration));
			RefundAmountOfPenaltyForLatePayment = ZDecimal.Zero;
			RefundAmountOfNonDutyTaxRevenue = ZDecimal.Zero;
		}

		ZDecimal GetDutyOrTaxAmount(ZString chargeType)
		{
			return GetPositiveRefundAmount(Header.GetAdditionalDutyOrTaxAmount(chargeType));
		}

		ZDecimal GetPositiveRefundAmount(ZDecimal amount)
		{
			amount = amount * -1;
			return amount < 0 ? ZDecimal.Zero : amount;
		}

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalAmountOfRefundAmount => RefundAmountOfDutyAmount + RefundAmountOfLiquorTax + RefundAmountOfSpecialConsumptionTax + RefundAmountOfTransportationTax + RefundAmountOfEducationTax + RefundAmountOfAgricultureTax + RefundAmountOfVAT
			+ RefundAmountOfPenaltyForLateDeclaration + RefundAmountOfPenaltyForMissedDeclaration + RefundAmountOfPenaltyForLatePayment + RefundAmountOfNonDutyTaxRevenue;

		[ResourceStringData("2D0542C5-A685-40B0-A6BF-B04160CC256F", Caption = "5WN Version No.")]
		public ZShort AmendSeqNo5WN => ZShort.Zero;
	}
}
