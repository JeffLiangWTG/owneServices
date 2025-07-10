using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISCommodityCodeValidationTest : AQISSingleValueValidationTest
	{
		public override void TestCodeAgainstLookupList()
		{
			var commodityCode = new AQISCommodityCode(Factory);
			AssertEquals("No Message Error", false, commodityCode.CodeInfo.HasMessageErrors());

			commodityCode.Code = "AAA";
			AssertEquals("Message Error", true, commodityCode.CodeInfo.HasMessageErrors());
		}

		public override void TestNumberOfCodesEntered()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISCommodityCode commodityCode1 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode1.Code = "1";
			AQISCommodityCode commodityCode2 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode2.Code = "2";
			AQISCommodityCode commodityCode3 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode3.Code = "3";
			AQISCommodityCode commodityCode4 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode4.Code = "4";
			AQISCommodityCode commodityCode5 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode5.Code = "5";
			AQISCommodityCode commodityCode6 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode6.Code = "6";
			AQISCommodityCode commodityCode7 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode7.Code = "7";
			AQISCommodityCode commodityCode8 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode8.Code = "8";
			AQISCommodityCode commodityCode9 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode9.Code = "9";
			AQISCommodityCode commodityCode10 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode10.Code = "10";
			commodityCode10.Validation.ValidateAll();
			AssertNoErrors("Commodity Code 1", commodityCode1.CodeInfo);
			AssertNoErrors("Commodity Code 2", commodityCode2.CodeInfo);
			AssertNoErrors("Commodity Code 3", commodityCode3.CodeInfo);
			AssertNoErrors("Commodity Code 4", commodityCode4.CodeInfo);
			AssertNoErrors("Commodity Code 5", commodityCode5.CodeInfo);
			AssertNoErrors("Commodity Code 6", commodityCode6.CodeInfo);
			AssertNoErrors("Commodity Code 7", commodityCode7.CodeInfo);
			AssertNoErrors("Commodity Code 8", commodityCode8.CodeInfo);
			AssertNoErrors("Commodity Code 9", commodityCode9.CodeInfo);
			AssertNoErrors("Commodity Code 10", commodityCode10.CodeInfo);

			AQISCommodityCode commodityCode11 = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode11.Code = "11";
			AssertNoErrors("Commodity Code 1", commodityCode1.CodeInfo);
			AssertNoErrors("Commodity Code 2", commodityCode2.CodeInfo);
			AssertNoErrors("Commodity Code 3", commodityCode3.CodeInfo);
			AssertNoErrors("Commodity Code 4", commodityCode4.CodeInfo);
			AssertNoErrors("Commodity Code 5", commodityCode5.CodeInfo);
			AssertNoErrors("Commodity Code 6", commodityCode6.CodeInfo);
			AssertNoErrors("Commodity Code 7", commodityCode7.CodeInfo);
			AssertNoErrors("Commodity Code 8", commodityCode8.CodeInfo);
			AssertNoErrors("Commodity Code 9", commodityCode9.CodeInfo);
			AssertNoErrors("Commodity Code 10", commodityCode10.CodeInfo);
			AssertHasErrors("Commodity Code 11", commodityCode11.CodeInfo);

			invoiceLine.AQISCommodityCodes.RemoveAndDelete(commodityCode2);
			commodityCode11.Validation.ValidateCode();
			AssertNoErrors("Commodity Code 1", commodityCode1.CodeInfo);
			AssertNoErrors("Commodity Code 3", commodityCode3.CodeInfo);
			AssertNoErrors("Commodity Code 4", commodityCode4.CodeInfo);
			AssertNoErrors("Commodity Code 5", commodityCode5.CodeInfo);
			AssertNoErrors("Commodity Code 6", commodityCode6.CodeInfo);
			AssertNoErrors("Commodity Code 7", commodityCode7.CodeInfo);
			AssertNoErrors("Commodity Code 8", commodityCode8.CodeInfo);
			AssertNoErrors("Commodity Code 9", commodityCode9.CodeInfo);
			AssertNoErrors("Commodity Code 10", commodityCode10.CodeInfo);
			AssertNoErrors("Commodity Code 11", commodityCode11.CodeInfo);
		}

		AQISCommodityCode aqisCommodityCode;
		public override AQISSingleValueBusinessObject BizObjToTest => aqisCommodityCode ?? (aqisCommodityCode = new AQISCommodityCode(Factory));
	}
}
