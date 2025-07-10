using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetailLookups))]
	sealed class CusLineTariffDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffTypeList()
		{
			var helper = new CusLineTariffDetailTestHelper();
			helper.PrepareTestDataForTariffTypeList(Factory, "IMP", "L121041", "E02");
			helper.PrepareTestDataForTariffTypeList(Factory, "EXP", "L121042", "E03");
			lookups = tariffDetail.Lookups;
			var tariffTypeList = lookups.TariffTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, tariffTypeList.Count);
				Assert("Contains code: L", tariffTypeList.ContainsCode("L"));
				Assert("Not Contains code: EXP", !tariffTypeList.ContainsCode("EXP"));
				Assert("Not Contains code: IMP", !tariffTypeList.ContainsCode("IMP"));
			});
		}

		public void TestTariffCodeList()
		{
			tariffDetail.BZ_Type = "L";
			var tariffCodeList = lookups.TariffCollection;

			CombineAssertions(() =>
			{
				AssertSame("Cached", TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Japan, tariffDetail.BZ_Type, ZDateTime.Today), tariffCodeList);
				Assert("Tariff Matches", tariff.MatchesFilter(tariffCodeList.CompleteFilter));
			});
		}

		public void TestExemptionReductionCodeList()
		{
			var exemptionReductionCodeList = lookups.ExemptionReductionCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, exemptionReductionCodeList.Count);
				Assert("Contains code: E01", exemptionReductionCodeList.ContainsCode("E01"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new CusLineTariffDetailTestHelper();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoices = declaration.Invoices.AddNew();
			var invoiceLine = invoices.JobComInvoiceLines.AddNew();
			tariffDetail = invoiceLine.DomesticConsumptionTaxes.AddNew();
			tariff = helper.PrepareTestData(Factory);
			lookups = tariffDetail.Lookups;
		}

		CusLineTariffDetailLookups lookups;
		CusLineTariffDetail tariffDetail;
		TariffView tariff;
	}
}
