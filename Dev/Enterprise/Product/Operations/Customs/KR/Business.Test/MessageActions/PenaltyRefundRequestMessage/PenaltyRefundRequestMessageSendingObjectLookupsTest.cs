using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PenaltyRefundRequestMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefundCauseCode()
		{
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13", sendingObject.Lookups.RefundCauseCodeList.CodesAsString);
		}

		public void TestRefundReasonCode()
		{
			AssertEquals("01, 02, 03", sendingObject.Lookups.RefundReasonCodeList.CodesAsString);
		}
		public void TestTaxOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.TaxOffice, "Tax Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.TaxOffice, "100", "서울청", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.TaxOffice, "101", "종로", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.TaxOffice, "104", "남대문", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var taxOfficeList = sendingObject.Lookups.TaxOfficeList;
			taxOfficeList.Load();
			AssertEquals(3, taxOfficeList.Count);

			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "100"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "서울청"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "101"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "종로"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "104"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "남대문"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var refundSessionalData = instruction.RefundSessionalDataCollection.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
		}
		PenaltyRefundRequestMessageSendingObject sendingObject;
	}
}
