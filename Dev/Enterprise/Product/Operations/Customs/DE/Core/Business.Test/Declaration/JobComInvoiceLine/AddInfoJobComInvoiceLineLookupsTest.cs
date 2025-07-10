using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEconomicConditionsList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_A2055, "Economic Conditions");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_A2055, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_A2055, "02", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var list = lookups.EconomicConditionsList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "01, 02", list.CodesAsString);
				AssertSame("Cached", list, lookups.EconomicConditionsList);
			});
		}

		public void TestCessionFlagList_DeclarationNull()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertNoExceptionThrown(() => _ = invoiceLine.AddInfoLookups.CessionFlagList);
		}

		public void TestCessionFlagList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "40", "00", "0C9", "", "IMP");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "01");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "02");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var addInfoLookups = invoiceLine.AddInfoLookups;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				invoiceLine.JI_FormattedProcedure = "40000C9";
				var list = addInfoLookups.CessionFlagList;
				AssertEquals("Test1: CodesAsString", "01, 02", list.CodesAsString);
				AssertSame("Test1: Cached", list, addInfoLookups.CessionFlagList);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				invoiceLine.JI_FormattedProcedure = "40000C8";
				list = addInfoLookups.CessionFlagList;
				AssertEquals("Test 2: Invalid CPC number", 0, list.Count);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				invoiceLine.JI_FormattedProcedure = "40000C9";
				list = addInfoLookups.CessionFlagList;
				AssertEquals("Test 3: Export Declaration", 0, list.Count);
			});
		}

		public void TestIdentificationMeansTypeList()
		{
			var list = lookups.IdentificationMeansTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "1, 2, 3, 4, 5, 6", list.CodesAsString);
				AssertSame("Cached", list, lookups.IdentificationMeansTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			var invoiceLineAddInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
			lookups = new AddInfoJobComInvoiceLineLookups(invoiceLineAddInfo);
		}
		JobComInvoiceLine invoiceLine;
		AddInfoJobComInvoiceLineLookups lookups;
	}
}
