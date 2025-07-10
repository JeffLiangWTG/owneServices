using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public sealed class ExTariffColumnStyleTest : TestCaseWithFactory
	{
		public void TestPropertiesForPopup()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_TariffCode = "09022000_001";
			tariff.ZZ1_ZZI_NKTariffType = "LEBIT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var control = new ExTariffColumnStyle(new ExTariffColumnStyleInfo()))
			{
				control.TariffCodeProperty = "TariffCode";
				control.TariffTypeProperty = "TariffType";

				AssertEquals("TariffCodeProperty should be", "TariffCode", control.TariffCodeProperty);
				AssertEquals("TariffTypeProperty should be", "TariffType", control.TariffTypeProperty);
			}
		}
	}
}
