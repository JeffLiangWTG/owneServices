using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class RefundSessionalData : CusSupportingInfo
	{
		public RefundSessionalData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public CusEntryHeader Entry => Parent?.EntryHeader;

		ZBool IsRefundIrrelevant => AmendmentSessionalData != null && !DutyTaxCorrectionCodeList.Is5ULRelevant(AmendmentSessionalData.CSI_Code);
		CusEntryNumber EntryNum5UL => Entry?.GetMatchingRefundCusEntryNum(CSI_ReferenceNumber, CustomsDisbursementBill);
		ZBool Has5ULSentOrAccepted => !CustomsMessageStatusTypeList.IsOriginalMessageAllowed(EntryNum5UL?.CE_EntryStatus ?? ZString.Empty);
		ZBool Is5ULFieldReadOnly => IsRefundIrrelevant || Has5ULSentOrAccepted;
		ZBool Has5FESentOrAccepted => CustomsMessageStatusTypeList.IsAmendmentMessageSentOrAccepted(AmendmentSessionalData?.MessageStatus ?? ZString.Empty);
		ZBool RefundRequestYNReadonly => IsRefundIrrelevant || Has5FESentOrAccepted || AmendmentSessionalData == null;

		[MaxLength(1)]
		[ReadOnlyMember(nameof(RefundRequestYNReadonly))]
		[List(nameof(Lookups) + "." + nameof(RefundSessionalDataLookups.YesNoList))]
		[ResourceStringData("D36B7499-5403-4DD3-86C5-5DFFD7A0AC1F", Caption = "5UL Sent with 5FE Y/N")]
		public ZString RefundRequestYN
		{
			get => CSI_Code;
			set
			{
				CheckMaximumLength(RefundRequestYNInfo, value);
				CSI_Code = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundRequestYN();
				}
				RefundRequestYNInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo RefundRequestYNInfo => GetZPropertyInfo(nameof(RefundRequestYN));

		[ResourceStringData("7BD59C9D-ACE7-4E46-B5A1-729CE213BDD5", Caption = "Refund Request Number")]
		public ZString RefundRequestNumber => CSI_ReferenceNumber;

		[MaxLength(2)]
		[ReadOnlyMember(nameof(Is5ULFieldReadOnly))]
		[List(nameof(Lookups) + "." + nameof(RefundSessionalDataLookups.RefundCauseCodeList))]
		[ResourceStringData("69A44904-CB4B-4661-9E2B-98B56093E80F", Caption = "Refund Cause Code")]
		public ZString RefundCauseCode
		{
			get => CSI_Procedure;
			set
			{
				CheckMaximumLength(RefundCauseCodeInfo, value);
				CSI_Procedure = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundCauseCode();
				}
				RefundCauseCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo RefundCauseCodeInfo => GetZPropertyInfo(nameof(RefundCauseCode));

		[MaxLength(2)]
		[ReadOnlyMember(nameof(Is5ULFieldReadOnly))]
		[List(nameof(Lookups) + "." + nameof(RefundSessionalDataLookups.RefundReasonCodeList))]
		[ResourceStringData("44826685-0D84-4B50-972F-51E3B6F52D5D", Caption = "Refund Reason Code")]
		public ZString RefundReasonCode
		{
			get => CSI_IssuerType;
			set
			{
				CheckMaximumLength(RefundReasonCodeInfo, value);
				CSI_IssuerType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundReasonCode();
				}
				RefundReasonCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo RefundReasonCodeInfo => GetZPropertyInfo(nameof(RefundReasonCode));

		[MaxLength(1)]
		[ReadOnlyMember(nameof(Is5ULFieldReadOnly))]
		[List(nameof(Lookups) + "." + nameof(RefundSessionalDataLookups.RefundTypeList))]
		[ResourceStringData("FFB9DEC9-E3D9-43DD-AAF1-2CAA1A02B1BF", Caption = "Refund Type")]
		public ZString RefundType
		{
			get => CSI_SubType;
			set
			{
				CheckMaximumLength(RefundTypeInfo, value);
				CSI_SubType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundType();
				}
				RefundTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo RefundTypeInfo => GetZPropertyInfo(nameof(RefundType));

		[ResourceStringData("F2D39973-1CA7-4CE0-8FEB-8E2B07A9BA92", Caption = "5UL Accepted Date")]
		public ZDateTime AcceptanceDate5UL => CSI_DateOfIssue;

		[ResourceStringData("7EB0E6E1-34B2-435B-98EC-E2896CD44D51", Caption = "5UL Message Status")]
		public ZString EntryStatus5UL => CSI_Status;
		public ZString EntryStatus5ULDescription => Factory.GetCachedValue<CustomsEntryStatusTypeList>().GetDescriptionFromCode(EntryStatus5UL);

		[MaxLength(19)]
		[List(nameof(Lookups) + "." + nameof(RefundSessionalDataLookups.CustomsDisbursementBillNumberList))]
		[ReadOnlyMember(nameof(Is5ULFieldReadOnly))]
		[ResourceStringData("DDE2575B-4F0D-406B-893C-FA332D9A267E", Caption = "Customs Disbursement Bill #")]
		public ZString CustomsDisbursementBill
		{
			get => CSI_ReferenceNumber2;
			set
			{
				CheckMaximumLength(CustomsDisbursementBillInfo, value);
				CSI_ReferenceNumber2 = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsDisbursementBill();
				}
				CustomsDisbursementBillInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CustomsDisbursementBillInfo => GetZPropertyInfo(nameof(CustomsDisbursementBill));

		public ZString FormattedCustomsDisbursementBill => MessageFunctions.GetFormattedNumberByStatementType(StatementHeaderTypeList.Codes.CustomsDisbursementBill, CustomsDisbursementBill);

		[ResourceStringData("9161F8CE-61AF-4185-A6E6-D2EE43034C7E", Caption = "Refund Approval Date")]
		public ZDateTime RefundApprovalDate => CSI_DateOfExpiry;

		[ResourceStringData("BB35C282-010A-4A8F-9D83-8283D1F9DC9A", MediumCaption = "Refund Approval No", Caption = "Refund Approval Number")]
		public ZString RefundApprovalNumber => CSI_Tariff;

		[DecimalPlaces(DecimalPlacesConstants.TotalRefundAmount)]
		[ResourceStringData("50E5DE15-9948-45C6-9E8B-059D214B79E5", Caption = "Refund Amount")]
		public ZDecimal RefundAmount => CSI_Value;

		public KREntryCustomsBillsViewCollection CustomsDisbursementBills
		{
			get
			{
				if (Entry != null && customsDisbursementBills == null)
				{
					var query = new KREntryCustomsBillsView.Loader(Factory).GetCustomsDisbursementBills(Entry.EntryNumber);
					customsDisbursementBills = new KREntryCustomsBillsViewCollection(Factory, query, Entry.RegistryCompanyPK);
				}
				return customsDisbursementBills;
			}
		}
		KREntryCustomsBillsViewCollection customsDisbursementBills;

		public AmendmentSessionalData AmendmentSessionalData => Parent.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.PK == CSI_CSI_SupportingInfo);

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;
		public new RefundSessionalDataLookups Lookups => (RefundSessionalDataLookups)base.Lookups;
		public new RefundSessionalDataValidation Validation => (RefundSessionalDataValidation)base.Validation;
		protected override CusSupportingInfoLookups GetNewLookups() => new RefundSessionalDataLookups(this);
		protected override CusSupportingInfoValidation GetNewValidation() => new RefundSessionalDataValidation(this);
	}
}
