using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.MY.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MY.GUI.Testing
{
	[TestedType(typeof(MYInvoiceLineUserControl))]
	class InvoiceLineUserControlTestBaseOnlyTest : TestCaseWithFactory
	{
		public void TestTariffFindBoxAndRelatedGridColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var tariffControl = control.FindSingle<TariffFindBox>();
				AssertNotNull(tariffControl);
				AssertEquals("Tariff box country code", Enterprise.Core.Constants.CountryCodes.Malaysia, tariffControl.GetCountryCode());
				AssertEquals("Tariff box data grouping", string.Empty, tariffControl.GetDataGrouping());
				AssertEquals("Tariff box tariff type", Universal.Constants.TariffTypes.HarmonizedSystem, tariffControl.TariffType);
				AssertEquals("Min desc length: ", 3, tariffControl.PartialDescriptionMinLengthForSearch);
				var columnStyle = (TariffColumnStyleInfo)control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Tariff);
				AssertNotNull("Should have column JI_Tariff", columnStyle);
				AssertEquals("Tariff box country code", Core.Constants.CountryCodes.Malaysia, columnStyle.GetCountryCode());
				AssertEquals("Tariff box data grouping", string.Empty, columnStyle.GetDataGrouping());
				AssertEquals("Tariff box tariff type", Universal.Constants.TariffTypes.HarmonizedSystem, columnStyle.GetTariffType());
				AssertEquals("Min desc length: ", 3, columnStyle.PartialDescriptionMinLengthForSearch);
			}
		}

		public void TestTariffFindBoxPopup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var previousValue = Env.Registry.ExternalBorderComplianceTool;
			var previousUmpApiBaseAddress = Env.Registry.BorderWiseUmpApiBaseAddress;
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient = false;

			void AssertTariffFindBox<T>(string message, string externalBorderComplianceTool)
			{
				using (new DisposableAction(() =>
				{
					Env.Registry.ExternalBorderComplianceTool = externalBorderComplianceTool;
					Env.Registry.BorderWiseUmpApiBaseAddress = string.Empty;
				}, () =>
				{
					Env.Registry.ExternalBorderComplianceTool = previousValue;
					Env.Registry.BorderWiseUmpApiBaseAddress = previousUmpApiBaseAddress;
				}))
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var control = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					var tariffGridFindBox = control.FindSingle<TariffFindBox>();
					tariffGridFindBox.PopupButton.PerformClick();
					AssertType<T>(message, ((IFindBox)tariffGridFindBox).PopupForm);
				}
			}

			AssertTariffFindBox<EmbeddedModulePopup>("Use default popup", ExternalBorderComplianceToolList.Codes.None);
			AssertTariffFindBox<EmbeddedModulePopup>("Use default popup because BorderWise does not support MY", ExternalBorderComplianceToolList.Codes.BorderWiseWeb);
		}
	}
}
