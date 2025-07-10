using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUDrawbackInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceLineGridContext()
		{
			using (var control = new AUDrawbackInvoiceLineUserControl())
			{
				AssertEquals("Context is set", nameof(Customs.GUI.DeclarationType.Drawback), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestJI_SerialNumber()
		{
			using (var testForm = new ZForm())
			{
				var control = new AUDrawbackInvoiceLineUserControl();
				testForm.Controls.Add(control);
				testForm.Show();

				control.InitializeGridLayout();
				var invoiceLines = testForm.FindSingle<AUDrawbackInvoiceLineUserControl>("AUDrawbackInvoiceLineUserControl");
				var column = invoiceLines.CustomsInvoiceLinesBoundGrid.GetColumnStyle((ZString)JobComInvoiceLineSchema.Constants.JI_SerialNumber);
				AssertNotNull("Column should exist", column);
				Assert("Column should be visible", column.IsVisible);
			}
		}

		public void TestTariffFindBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				using (var testForm = new ZAUCustomsDeclarationForm(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as AUDrawbackInvoiceLineUserControl;

					AssertEquals(false, invoiceLineUserControl.tariffFindBox.Visible);
					AssertEquals(true, invoiceLineUserControl.tariffFindBoxAUCClass.Visible);

					var tariffColumn = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_Tariff");
					AssertType<AUCClassColumnStyleInfo>("AUCClass Column should exist", tariffColumn);
				}
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				using (var testForm = new ZAUCustomsDeclarationForm(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as AUDrawbackInvoiceLineUserControl;

					AssertEquals(true, invoiceLineUserControl.tariffFindBox.Visible);
					AssertEquals(false, invoiceLineUserControl.tariffFindBoxAUCClass.Visible);

					var tariffColumn = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_Tariff");
					AssertType<Universal.GUI.TariffColumnStyleInfo>("Universal Tariff Column should exist", tariffColumn);
				}
			}
		}

		public void TestTariffFindBoxEffectiveTariffCountryAndEffectiveDataGrouping()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (var invoiceLineUserControl = new AUDrawbackInvoiceLineUserControl())
				{
					AssertEquals("AU", invoiceLineUserControl.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AU", invoiceLineUserControl.tariffFindBox.EffectiveDataGrouping);
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				using (var invoiceLineUserControl = new AUDrawbackInvoiceLineUserControl())
				{
					AssertEquals("AUT", invoiceLineUserControl.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AUT", invoiceLineUserControl.tariffFindBox.EffectiveDataGrouping);
				}
			}
		}
	}
}
