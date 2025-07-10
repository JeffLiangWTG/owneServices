using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5FVMessageData
	{
		ZString TaxInvoiceNumber { get; }
		ZString ImportDeclarationNumber { get; }
		ZString NoticeNumber { get; }
		ZString RefundApprovalNo { get; }
		ZString ImporterID { get; }
		ZString ImporterCompanyName { get; }
		ZString ImporterRepresentativeName { get; }
		ZString ImporterAddressLine { get; }
		ZDate PaymentDate { get; }
		ZString BlankCount { get; }
		ZDecimal CustomsValue { get; }
		ZDecimal Tax { get; }
		ZDate AmendDate { get; }
		ZString IssueReasonCode { get; }
		ZString TaxInvoiceType { get; }
		ZString RefundType { get; }
		ZString ReIssueYN { get; }
	}

	public class GOVCBR5FVMessageData : NonPersistentBusinessObject, IGOVCBR5FVMessageData
	{
		public GOVCBR5FVMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZString TaxInvoiceNumber { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString NoticeNumber { get; set; }
		public ZString RefundApprovalNo { get; set; }
		public ZString ImporterID { get; set; }
		public ZString ImporterIDType { get; set; }
		public ZString ImporterCompanyName { get; set; }
		public ZString ImporterRepresentativeName { get; set; }
		public ZString ImporterAddressLine { get; set; }
		public ZDate PaymentDate { get; set; }
		public ZString BlankCount { get; set; }
		public ZDecimal CustomsValue { get; set; }
		public ZDecimal Tax { get; set; }
		public ZDate AmendDate { get; set; }
		public ZString IssueReasonCode { get; set; }
		public ZString TaxInvoiceType { get; set; }
		public ZString RefundType { get; set; }
		public ZString ReIssueYN { get; set; }

		public ZString FormattedTaxInvoiceNumber => TaxInvoiceNumber.SubstringSafe(5);
		public ZString FormattedCustomsOfficeBusinessNumber => MessageFunctions.GetFormattedNumber(CustomsOffice?.GetAttribute(Constants.ZZ.CodeListAttributeNames.BusinessNumber) ?? ZString.Empty, new int[] { 0, 3, 5 });
		public ZString FormattedImportDeclarationNumber => MessageFunctions.DeclarationNumberFormat(ImportDeclarationNumber);
		public ZString FormattedNoticeNumber => MessageFunctions.GetFormattedNumber(NoticeNumber, new int[] { 0, 4, 7, 9, 11, 12, 18 });
		public ZString FormattedRefundApprovalNo => MessageFunctions.GetFormattedNumber(RefundApprovalNo, new int[] { 0, 3, 5, 7 });
		public ZString FormattedImporterID => ImporterIDType == IdentificationType.KoreanRegNoForResident
														? MessageFunctions.GetFormattedNumber(ImporterID, new int[] { 0, 6 })
														: MessageFunctions.GetFormattedNumber(ImporterID, new int[] { 0, 3, 5 });

		public ZZRefCusCodeListCombined CustomsOffice => customs ?? (customs = MessageFunctions.GetRefCusCodeList(Factory, TaxInvoiceNumber.SubstringSafe(0, 3), Constants.ZZ.NKCodeType.CustomsOffice));
		ZZRefCusCodeListCombined customs;
		public ZString CustomsOfficeAddress => CustomsOffice?.GetAttribute(Constants.ZZ.CodeListAttributeNames.Address) ?? ZString.Empty;

		internal const int BaseAmountMaxLength = 12;
		internal const int VATMaxLength = 11;

		public ZString[] CustomsValueDigits => customsValueDigits ?? (customsValueDigits = MessageFunctions.GetIndividualDigits(CustomsValue.ToString(), BaseAmountMaxLength - (CustomsValue.IsEmpty ? 0 : CustomsValue.ToString().Length)));
		ZString[] customsValueDigits;

		public ZString[] TaxDigits => taxDigits ?? (taxDigits = MessageFunctions.GetIndividualDigits(Tax.ToString(), VATMaxLength - (Tax.IsEmpty ? 0 : Tax.ToString().Length)));
		ZString[] taxDigits;

		public ZString Note
		{
			get
			{
				#region SuppressResourceStringsCheckRegion if multiple lines require the suppression.
				if (TaxInvoiceType == TaxInvoiceTypeList.Codes._01)
				{
					return "부가가치세 면세";
				}
				else if (TaxInvoiceType == TaxInvoiceTypeList.Codes._02)
				{
					switch (IssueReasonCode)
					{
						case IssueReasonCodeList.Codes._0:
							return FormattedNoticeNumber + "로 납부";
						case IssueReasonCodeList.Codes.E:
							switch (RefundType)
							{
								case RefundTransactionNatureCodeList.Codes.A:
									return FormattedRefundApprovalNo + "로 과오납환급";
								case RefundTransactionNatureCodeList.Codes.B:
									return FormattedRefundApprovalNo + "로 위약환급";
								case RefundTransactionNatureCodeList.Codes.C:
									return FormattedRefundApprovalNo + "로 조감법 환급";
								default:
									return FormattedRefundApprovalNo + "로 특소세등 환급";
							}
						case IssueReasonCodeList.Codes.F:
							return FormattedRefundApprovalNo + "로 환급 " + FormattedNoticeNumber + "로 충당";
						case IssueReasonCodeList.Codes.G:
							return "과세표준 수정분임";
						case IssueReasonCodeList.Codes.H:
							return FormattedNoticeNumber + "로 수납 취소 감액분";
						default:
							return "수입자 정정 감액" + (AmendDate.IsEmpty ? "" : (" (" + AmendDate.ToString(DateFormatType.DateSlash) + ")"));
					}
				}

				return "";
				#endregion
			}
		}
	}
}
