using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	abstract class AUImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public abstract void TestInstantiation();

		public void TestJI_CustomTextBlob1ColumnIsNotDefaultColumn()
		{
			var declaration = GetNewDeclaration();
			using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLines = testForm.InvoiceUserControl as AUImportInvoiceLineUserControl;
				var column = invoiceLines.CustomsInvoiceLinesBoundGrid.GetColumnStyle((ZString)JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1);
				AssertEquals("Column should not be visible", false, column.IsVisible);
			}
		}

		public void TestJI_SerialNumber()
		{
			var declaration = GetNewDeclaration();
			using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLines = testForm.InvoiceUserControl as AUImportInvoiceLineUserControl;
				var column = invoiceLines.CustomsInvoiceLinesBoundGrid.GetColumnStyle((ZString)JobComInvoiceLineSchema.Constants.JI_SerialNumber);
				AssertNotNull("Column should exist", column);
				Assert("Column should not be visible", !column.IsVisible);
			}
		}

		public void TestTariffFindBoxVisibility()
		{
			var declaration = GetNewDeclaration();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineUserControl = testForm.InvoiceUserControl as AUImportInvoiceLineUserControl;

					AssertEquals(false, invoiceLineUserControl.tariffFindBox.Visible);
					AssertEquals(true, invoiceLineUserControl.tariffFindBoxAUCClass.Visible);

					var tariffColumn = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_Tariff");
					AssertType<AUCClassColumnStyleInfo>("AUCClass Column should exist", tariffColumn);
				}
			}

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineUserControl = testForm.InvoiceUserControl as AUImportInvoiceLineUserControl;

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
				using (var importInvoiceLineUserControl = new AUImportInvoiceLineUserControl())
				{
					AssertEquals("AU", importInvoiceLineUserControl.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AU", importInvoiceLineUserControl.tariffFindBox.EffectiveDataGrouping);
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				using (var importInvoiceLineUserControl = new AUImportInvoiceLineUserControl())
				{
					AssertEquals("AUT", importInvoiceLineUserControl.tariffFindBox.EffectiveTariffCountry);
					AssertEquals("AUT", importInvoiceLineUserControl.tariffFindBox.EffectiveDataGrouping);
				}
			}
		}

		protected virtual JobDeclaration GetNewDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected sealed class AUCustomsDeclarationFormForTest : ZAUCustomsDeclarationForm
		{
			public AUCustomsDeclarationFormForTest(JobDeclaration jobDeclaration)
				: base(jobDeclaration)
			{
			}

			public AUCustomsDeclarationFormForTest()
				: this(null)
			{
			}

			internal BaseInvoiceLineUserControl InvoiceUserControl => CustomsBrokerageUserControl.InvoiceLinesUserControl;
		}
	}
}
