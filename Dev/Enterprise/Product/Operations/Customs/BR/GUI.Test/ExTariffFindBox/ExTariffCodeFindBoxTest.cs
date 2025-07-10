using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public sealed class ExTariffCodeFindBoxTest : TestCaseWithFactory
	{
		public void TestSettingPropertiesOnPopupSelected()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_TariffCode = "09022000_001";
			tariff.ZZ1_Description = "09022000_001 DESC";
			tariff.ZZ1_ZZI_NKTariffType = "LEBIT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();

			using (var form = new ZForm())
			using (var control = new ExTariffGridFindBox())
			{
				form.Controls.Add(control);
				control.TariffCodeProperty = "TariffCode";
				control.TariffTypeProperty = "TariffType";
				control.SetDataBinding(additionTariff, "ExNumber");
				new ExTariffPopupModuleDecisionProvider(control).SetFindBoxCodeDescription(tariff);

				AssertEquals("TariffCode should be set from selected BO", "09022000_001", additionTariff.TariffCode);
				AssertEquals("TariffType should be set from selected BO", "LEBIT", additionTariff.TariffType);
				AssertEquals("Code should be set from selected BO", "001", (control as IFindBox).Code);
				AssertEquals("Description should be set from selected BO", "", (control as IFindBox).Description);
			}
		}

		public void TestPropertiesForPopup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionTariff = invoiceLine.AdditionalTariffs.AddNew();
			additionTariff.TariffCode = "09022000_001";

			using (var form = new ZForm())
			using (var control = new ExTariffGridFindBox())
			{
				form.Controls.Add(control);
				control.TariffCodeProperty = "TariffCode";
				control.TariffTypeProperty = "TariffType";
				control.SetDataBinding(additionTariff, "ExNumber");
				control.SelectFromPopupForm();

				AssertEquals("CodeForPopup should be", "09022000_001", (control as ICustomizableFindBoxPopup).CodeForPopup);
				AssertEquals("PropertyNameForPopup should be", "ZZ1_TariffCode", (control as ICustomizableFindBoxPopup).PropertyNameForPopup);
			}
		}
	}
}
