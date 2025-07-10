using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CommodityCodeWrapperTest : DataProviderTestCase<CommodityCodeWrapper>
	{
		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("CombinedNomenclatureCode should equal 7th and 8th chars from JI_Tariff.", "22", Provider.CombinedNomenclatureCode);
		}

		public void TestHarmonizedSystemSubheadingCode()
		{
			AssertEquals("HarmonizedSystemSubheadingCode should equal first 6 chars from JI_Tariff.", "111111", Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestNationalAdditionalCode()
		{
			AssertEquals("NationalAdditionalCode has only 1 element.", 1, Provider.NationalAdditionalCode.Count);
			AssertEquals("CcQualifier should equal CountryCodes.France.", CountryCodes.France, Provider.NationalAdditionalCode.First().CcQualifier);
			AssertEquals("NationalAdditionalCode should equal line.JI_SupplementaryCode2.", "V905", Provider.NationalAdditionalCode.First().NationalAdditionalCode);
		}

		public void TestTaricAdditionalCode()
		{
			AssertEquals("TaricAdditionalCode has only 1 element.", 1, Provider.TaricAdditionalCode.Count);
			AssertEquals("TaricAdditionalCode should equal line.JI_SupplementaryCode1.", "YYY", Provider.TaricAdditionalCode.First().TaricAdditionalCode);
		}

		public void TestWhenSupplementaryCodesAreNationalOrTaricAdditionalCodes()
		{
			line.JI_SupplementaryCode1 = "V901";
			line.JI_SupplementaryCode2 = "V902";
			var provider = GetUpdatedProvider();
			AssertContainsExactElementsInAnyOrder("National additional code list should contain V901 & V902.", new[] { "V901", "V902" }, provider.NationalAdditionalCode.Select(x => x.NationalAdditionalCode));

			line.JI_SupplementaryCode1 = "A001";
			line.JI_SupplementaryCode2 = "B001";
			provider = GetUpdatedProvider();
			AssertContainsExactElementsInAnyOrder("Taric additional code list should contain A001 & B001.", new[] { "A001", "B001" }, provider.TaricAdditionalCode.Select(x => x.TaricAdditionalCode));
		}

		public void TestTaricCode()
		{
			AssertEquals("TaricCode should equal 9th and 10th digits of JI_Tariff.", "33", Provider.TaricCode);
		}

		protected override CommodityCodeWrapper GetProvider()
		{
			line.JI_Tariff = "1111112233";
			line.JI_SupplementaryCode2 = "V905";
			line.JI_SupplementaryCode1 = "YYY";

			return CommodityCodeWrapper.New(line);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			line = invoiceHeader.InvoiceLines.AddNew();
		}

		CommodityCodeWrapper GetUpdatedProvider()
		{
			return CommodityCodeWrapper.New(line);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine line;
	}
}
