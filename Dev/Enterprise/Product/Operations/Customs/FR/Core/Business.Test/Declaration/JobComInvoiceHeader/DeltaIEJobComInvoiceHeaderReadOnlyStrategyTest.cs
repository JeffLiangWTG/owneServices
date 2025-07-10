using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIEJobComInvoiceHeaderReadOnlyStrategyTest : TestCaseWithFactory
	{
		public void TestIsReadonly()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoice = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var readonlyStrategy = new DeltaIEJobComInvoiceHeaderReadOnlyStrategy(invoice);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			Assert("JZ_IncoTermPlace should be readonly", readonlyStrategy.IsReadonly(invoice.JZ_IncoTermPlaceInfo));
			Assert("ZG_IncotermCountry should be readonly", readonlyStrategy.IsReadonly(invoice.ZG_IncotermCountryInfo));
			Assert("ZG_AgreedPlaceCode should be readonly", readonlyStrategy.IsReadonly(invoice.ZG_AgreedPlaceCodeInfo));
			Assert("JZ_AdditionalTerms should not be readonly", !readonlyStrategy.IsReadonly(invoice.JZ_AdditionalTermsInfo));

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			Assert("JZ_AdditionalTerms should be readonly", readonlyStrategy.IsReadonly(invoice.JZ_AdditionalTermsInfo));
			Assert("ZG_AgreedPlaceCode should not be readonly", !readonlyStrategy.IsReadonly(invoice.ZG_AgreedPlaceCodeInfo));

			invoice.ZG_AgreedPlaceCode = "AAA";
			Assert("JZ_IncoTermPlace should be readonly", readonlyStrategy.IsReadonly(invoice.JZ_IncoTermPlaceInfo));
			Assert("ZG_IncotermCountry should be readonly", readonlyStrategy.IsReadonly(invoice.ZG_IncotermCountryInfo));

			invoice.ZG_AgreedPlaceCode = ZString.Empty;
			Assert("JZ_IncoTermPlace should not be readonly", !readonlyStrategy.IsReadonly(invoice.JZ_IncoTermPlaceInfo));
			Assert("ZG_IncotermCountry should not be readonly", !readonlyStrategy.IsReadonly(invoice.ZG_IncotermCountryInfo));

			invoice.JZ_IncoTermPlace = "AAA";
			Assert("ZG_AgreedPlaceCode should be readonly", readonlyStrategy.IsReadonly(invoice.ZG_AgreedPlaceCodeInfo));

			invoice.JZ_IncoTermPlace = ZString.Empty;
			invoice.ZG_IncotermCountry = "FR";
			Assert("ZG_AgreedPlaceCode should be readonly", readonlyStrategy.IsReadonly(invoice.ZG_AgreedPlaceCodeInfo));
		}
	}
}
