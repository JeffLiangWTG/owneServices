using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationDetachProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationDetachProvider()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			var detach = invLine.TariffDetachs.AddNew();
			detach.CY_Code = "999";

			var detachProvider = new DeclarationDetachProvider(detach);

			AssertEquals("Detach Number", "999", detachProvider.ReferenceNumber);
		}
	}
}

