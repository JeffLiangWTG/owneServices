using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GOVCBR5FVMessageData))]
	sealed class GOVCBR5FVMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFormatted()
		{
			var messageData = new GOVCBR5FVMessageData(Factory);
			messageData.TaxInvoiceNumber = "0302010763124";
			messageData.ImportDeclarationNumber = "1234520000045M";
			messageData.NoticeNumber = "0127030112000511999";
			messageData.RefundApprovalNo = "030752021999";
			messageData.ImporterID = "1268110513";
			messageData.ImporterIDType = IdentificationType.BusinessRegNo;

			AssertEquals("10763124", messageData.FormattedTaxInvoiceNumber);
			AssertEquals("12345-20-000045M", messageData.FormattedImportDeclarationNumber);
			AssertEquals("0127-030-11-20-0-051199-9", messageData.FormattedNoticeNumber);
			AssertEquals("030-75-20-21999", messageData.FormattedRefundApprovalNo);
			AssertEquals("126-81-10513", messageData.FormattedImporterID);

			messageData.ImporterID = "9703051567895";
			messageData.ImporterIDType = IdentificationType.KoreanRegNoForResident;

			AssertEquals("970305-1567895", messageData.FormattedImporterID);
		}
		public void TestCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.BusinessNumber, "6018300048");
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.Address, "부산시 중구 중앙동 4-17");

			Factory.Save();

			var messageData = new GOVCBR5FVMessageData(Factory);
			messageData.TaxInvoiceNumber = "0302010763124";
			messageData.PaymentDate = ZDate.Today;

			AssertEquals("601-83-00048", messageData.FormattedCustomsOfficeBusinessNumber);
			AssertEquals("부산세관", messageData.CustomsOffice.ZZD_Description);
			AssertEquals("부산시 중구 중앙동 4-17", messageData.CustomsOfficeAddress);
		}

		public void TestBaseAmountAndVAT()
		{
			var messageData = new GOVCBR5FVMessageData(Factory);
			messageData.BlankCount = "5";
			messageData.CustomsValue = 17605791m;
			messageData.Tax = 1760570m;

			AssertNullOrEmpty(messageData.CustomsValueDigits[0]);
			AssertNullOrEmpty(messageData.CustomsValueDigits[1]);
			AssertNullOrEmpty(messageData.CustomsValueDigits[2]);
			AssertNullOrEmpty(messageData.CustomsValueDigits[3]);
			AssertEquals("1", messageData.CustomsValueDigits[4]);
			AssertEquals("7", messageData.CustomsValueDigits[5]);
			AssertEquals("6", messageData.CustomsValueDigits[6]);
			AssertEquals("0", messageData.CustomsValueDigits[7]);
			AssertEquals("5", messageData.CustomsValueDigits[8]);
			AssertEquals("7", messageData.CustomsValueDigits[9]);
			AssertEquals("9", messageData.CustomsValueDigits[10]);
			AssertEquals("1", messageData.CustomsValueDigits[11]);

			AssertNullOrEmpty(messageData.TaxDigits[0]);
			AssertNullOrEmpty(messageData.TaxDigits[1]);
			AssertNullOrEmpty(messageData.TaxDigits[2]);
			AssertNullOrEmpty(messageData.TaxDigits[3]);
			AssertEquals("1", messageData.TaxDigits[4]);
			AssertEquals("7", messageData.TaxDigits[5]);
			AssertEquals("6", messageData.TaxDigits[6]);
			AssertEquals("0", messageData.TaxDigits[7]);
			AssertEquals("5", messageData.TaxDigits[8]);
			AssertEquals("7", messageData.TaxDigits[9]);
			AssertEquals("0", messageData.TaxDigits[10]);
		}

		public void TestReason()
		{
			var messageData = new GOVCBR5FVMessageData(Factory);
			messageData.TaxInvoiceType = TaxInvoiceTypeList.Codes._02;
			messageData.NoticeNumber = "0127030112000511999";
			messageData.RefundApprovalNo = "030752021999";

			messageData.IssueReasonCode = IssueReasonCodeList.Codes._0;
			AssertEquals("0127-030-11-20-0-051199-9로 납부", messageData.Note);

			messageData.IssueReasonCode = IssueReasonCodeList.Codes.E;
			messageData.RefundType = RefundTransactionNatureCodeList.Codes.A;
			AssertEquals("030-75-20-21999로 과오납환급", messageData.Note);

			messageData.RefundType = RefundTransactionNatureCodeList.Codes.B;
			AssertEquals("030-75-20-21999로 위약환급", messageData.Note);

			messageData.RefundType = RefundTransactionNatureCodeList.Codes.C;
			AssertEquals("030-75-20-21999로 조감법 환급", messageData.Note);

			messageData.RefundType = RefundTransactionNatureCodeList.Codes.E;
			AssertEquals("030-75-20-21999로 특소세등 환급", messageData.Note);

			messageData.IssueReasonCode = IssueReasonCodeList.Codes.F;
			AssertEquals("030-75-20-21999로 환급 0127-030-11-20-0-051199-9로 충당", messageData.Note);

			messageData.IssueReasonCode = IssueReasonCodeList.Codes.G;
			AssertEquals("과세표준 수정분임", messageData.Note);

			messageData.IssueReasonCode = IssueReasonCodeList.Codes.H;
			AssertEquals("0127-030-11-20-0-051199-9로 수납 취소 감액분", messageData.Note);

			messageData.IssueReasonCode = IssueReasonCodeList.Codes.A;
			messageData.AmendDate = ZDate.Today;
			AssertEquals("수입자 정정 감액 (" + ZDate.Today.ToString(DateFormatType.DateSlash) + ")", messageData.Note);

			messageData.IssueReasonCode = IssueReasonCodeList.Codes.B;
			messageData.AmendDate = ZDate.Today.AddDays(-1);
			AssertEquals("수입자 정정 감액 (" + ZDate.Today.AddDays(-1).ToString(DateFormatType.DateSlash) + ")", messageData.Note);

			messageData.IssueReasonCode = IssueReasonCodeList.Codes.C;
			messageData.AmendDate = ZDate.Today.AddDays(1);
			AssertEquals("수입자 정정 감액 (" + ZDate.Today.AddDays(1).ToString(DateFormatType.DateSlash) + ")", messageData.Note);

			messageData.TaxInvoiceType = TaxInvoiceTypeList.Codes._01;
			AssertEquals("부가가치세 면세", messageData.Note);
		}
	}
}
