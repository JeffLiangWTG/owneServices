using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationCustomsValuationProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationCustomsValuationProvider()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			var nveNumber = invLine.NVECusCodeDataCollection.AddNew();
			nveNumber.CY_Order = 6;
			nveNumber.CY_Data = "XXXX";
			nveNumber.CY_Code = "XX";

			var nveProvider = new DeclarationCustomsValuationProvider(nveNumber);

			AssertEquals("Attribute", "XX", nveProvider.Attribute);
			AssertEquals("Position", "6", nveProvider.Position);
			AssertEquals("Specification", "XXXX", nveProvider.Specification);
		}
	}
}
