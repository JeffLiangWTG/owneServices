using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI.Testing;

public abstract class BaseInvoiceLineTemplateTest : TestCaseWithFactory
{
	public void TestTariffFindBox()
	{
		var factory = new BusinessObjectFactory();
		var jobDeclartion = factory.NewWithValidTestData<JobDeclaration>();
		jobDeclartion.JE_MessageType = DeclarationMessageType;
		var header = jobDeclartion.Invoices.AddNew();
		var invoiceLine = header.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "1234567890";

		using var testForm = new JobDeclarationForm(jobDeclartion);
		testForm.Show();
		var brokerageUserControl = testForm.CustomsBrokerageUserControl;
		brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
		var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl;

		var tariffFindBox = invoiceLinesUserControl.FindSingle<TariffFindBox>("TariffFindBox");
		CombineAssertions(() =>
		{
			AssertEquals("NeedLoadParentDataGroup", false, tariffFindBox.NeedLoadParentDataGroup);
			AssertEquals("TariffType", DeclarationMessageType, tariffFindBox.GetTariffType.Invoke());
			AssertEquals("DataGrouping", Core.Constants.CountryCodes.Japan, tariffFindBox.GetDataGrouping.Invoke());
			AssertEquals("CountryCode", Core.Constants.CountryCodes.Japan, tariffFindBox.GetCountryCode.Invoke());
			AssertEquals("EffectiveDate", ZDateTime.Today, tariffFindBox.GetEffectiveDate.Invoke());
			Assert("NeedLoadNomenclatureWhenTariffNotFound", tariffFindBox.NeedLoadNomenclatureWhenTariffNotFound);
		});
	}

	protected abstract ZString DeclarationMessageType { get; }

	protected abstract ZUserControl GetControlForTest();
}
