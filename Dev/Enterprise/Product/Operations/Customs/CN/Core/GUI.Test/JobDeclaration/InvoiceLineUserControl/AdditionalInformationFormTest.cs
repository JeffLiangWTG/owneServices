using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(AdditionalInformationForm))]
	class AdditionalInformationFormTest : ZFormBasherTest
	{
		public void TestShowDialog()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "99998", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("99998", "规格型号");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "2713200000";
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				((ZForm)dialog).Shown += (sender, e) =>
				{
					var form = sender as AdditionalInformationForm;
					var wrapper = form.BusinessEntity;
					wrapper.AdditionalElementValues["00000"].ElementValue = "X";
					wrapper.AdditionalElementValues["99998"].ElementValue = "Y";
					wrapper.AdditionalElementValues["99999"].ElementValue = "Z";
					form.OKButton.PerformClick();
					form.Close();
				};
			}
			);
			AdditionalInformationForm.ShowDialog(invoiceLine, invoiceLine.JI_NameOfGoodsInfo, invoiceLine.XC_GoodsSpecModelInfo, EnteringOrExiting.Both);
			AssertType<AdditionalInformationForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("JI_NameOfGoods should be set", "X", invoiceLine.JI_NameOfGoods);
			AssertEquals("XC_GoodsSpecModel should be set", "Y|Z", invoiceLine.XC_GoodsSpecModel);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				((ZForm)dialog).Shown += (sender, e) =>
				{
					var form = (AdditionalInformationForm)sender;
					var wrapper = form.BusinessEntity;
					wrapper.AdditionalElementValues["00000"].ElementValue = "A";
					wrapper.AdditionalElementValues["99998"].ElementValue = "B";
					wrapper.AdditionalElementValues["99999"].ElementValue = "C";
					form.CancelButton.PerformClick();
					form.Close();
				};
			});
			AdditionalInformationForm.ShowDialog(invoiceLine, invoiceLine.JI_NameOfGoodsInfo, invoiceLine.XC_GoodsSpecModelInfo, EnteringOrExiting.Both);
			AssertType<AdditionalInformationForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("JI_NameOfGoods should be set", "X", invoiceLine.JI_NameOfGoods);
			AssertEquals("XC_GoodsSpecModel should be set", "Y|Z", invoiceLine.XC_GoodsSpecModel);
		}

		protected override Form GetFormToBashCore()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new AdditionalInformationForm(new AdditionalInformationWrapper(invoiceLine, ZString.Empty, ZString.Empty, EnteringOrExiting.Both));
		}
	}
}
