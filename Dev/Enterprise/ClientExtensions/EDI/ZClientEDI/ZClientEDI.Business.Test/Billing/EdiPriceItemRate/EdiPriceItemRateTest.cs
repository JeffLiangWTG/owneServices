using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceItemRate))]
	internal class EdiPriceItemRateTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateRate(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateRate(Factory);
		}

		static EdiPriceItemRate CreateRate(BusinessObjectFactory factory)
		{
			var org = factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var header = org.LicCompany.PriceHeaders.AddNew();
			var item = header.Items.AddNew();
			var bizo = item.CurrencyRates.AddNew();
			bizo.PIR_RX_NKCurrency = "AUD";
			return bizo;
		}
	}
}
