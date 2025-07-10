using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIEJobComInvoiceHeaderValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestValueSet()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoice = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var valueSetStrategy = new DeltaIEJobComInvoiceHeaderValueSetStrategy(invoice);

			invoice.ShouldClearIncoTermPlacesIfNeeded = true;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			valueSetStrategy.ValueSet(invoice.JZ_IncoTermInfo, invoice.JZ_IncoTerm);
			AssertEquals("ShouldClearIncoTermPlacesIfNeeded should be false", false, invoice.ShouldClearIncoTermPlacesIfNeeded);

			invoice.JZ_AdditionalTerms = "AAA";

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			valueSetStrategy.ValueSet(invoice.JZ_IncoTermInfo, invoice.JZ_IncoTerm);
			AssertEquals("JZ_AdditionalTerms should be cleared", ZString.Empty, invoice.JZ_AdditionalTerms);

			invoice.ZG_AgreedPlaceCode = "BBB";
			invoice.JZ_IncoTermPlace = "DDD";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			valueSetStrategy.ValueSet(invoice.JZ_IncoTermInfo, invoice.JZ_IncoTerm);
			AssertEquals("ZG_AgreedPlaceCode should be cleared", ZString.Empty, invoice.ZG_AgreedPlaceCode);
			AssertEquals("JZ_IncoTermPlace should be cleared", ZString.Empty, invoice.JZ_IncoTermPlace);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.ZG_AgreedPlaceCode = "BBB";
			invoice.JZ_IncoTermPlace = "CCC";
			valueSetStrategy.ValueSet(invoice.JZ_IncoTermPlaceInfo, invoice.JZ_IncoTermPlace);
			AssertEquals("ZG_AgreedPlaceCode should be cleared", ZString.Empty, invoice.ZG_AgreedPlaceCode);

			invoice.JZ_IncoTermPlace = ZString.Empty;
			invoice.ZG_AgreedPlaceCode = "BBB";
			invoice.ZG_IncotermCountry = "FR";
			valueSetStrategy.ValueSet(invoice.ZG_IncotermCountryInfo, invoice.JZ_IncoTermPlace);
			AssertEquals("ZG_AgreedPlaceCode should be cleared", ZString.Empty, invoice.ZG_AgreedPlaceCode);

			AssertEquals("ZG_IncotermCountry remained with old value", "FR", invoice.ZG_IncotermCountry);
			invoice.ZG_AgreedPlaceCode = "BBB";
			valueSetStrategy.ValueSet(invoice.ZG_AgreedPlaceCodeInfo, invoice.ZG_AgreedPlaceCode);
			AssertEquals("ZG_IncotermCountry should be cleared", ZString.Empty, invoice.ZG_IncotermCountry);
		}
	}
}
