using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportLicense.Testing
{
	class TariffDetachProviderTest : TestCaseWithFactory
	{
		public void TestTariffDetachCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "87";
			var tariffDetach = invoiceLine1.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "999";
			var tariffDetachProvider = new TariffDetachProvider(tariffDetach);

			AssertEquals("999", tariffDetachProvider.TariffDetachCode);
		}
	}
}
