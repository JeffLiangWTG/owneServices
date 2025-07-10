using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceLineGridContext()
		{
			using (var control = new AUExportInvoiceLineUserControl())
			{
				AssertEquals("Context is set", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestDeclarationModes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (TestAUCustomsDeclarationForm testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("Invoice User Control should be ExportInvoiceUserControl", typeof(AUExportInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
				//GST and Duty are invisible
				var lineUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Duty Not visible", false, lineUserControl.DutyConvertToLocalCurrencyControl.Visible);
				AssertEquals("GST Not visible", false, lineUserControl.GSTConvertToLocalCurrencyControl.Visible);
			}
		}

		public void TestLockingShipmentData()
		{
			var jobDecBizObj = Factory.New<JobDeclaration>();
			jobDecBizObj.SetReadOnlyIncludingChildren(true);
			var invoiceHeader = jobDecBizObj.Invoices.AddNew();
			Assert(invoiceHeader.ReadOnly);
			var invoiceLine = jobDecBizObj.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			Assert(invoiceLine.ReadOnly);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var testForm = new TestAUCustomsDeclarationForm(jobDecBizObj))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var exportInvoiceUserControl = testForm.InvoiceControl as AUExportInvoiceLineUserControl;
				exportInvoiceUserControl.LineDetailTabControl.SelectedTab = exportInvoiceUserControl.FindSingle<ZTabPage>("LineDetailsTabPage");

				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.Visible);
				Assert("JI_CountryOfOrigin ReadOnly", invoiceLine.JI_CountryOfOriginInfo.ReadOnly);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", true, exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_DescriptionBoundTextBox.ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_AUStateBoundTextBox.ReadOnly);
				AssertEquals("JI_PermitNumberBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_PermitNumberBoundTextBox.ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_TempImportNumBoundTextBox.ReadOnly);

				AssertEquals("JI_TariffFindBoxAHECC Visible", false, exportInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBox Visible", true, exportInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBox ReadOnly", true, exportInvoiceUserControl.JI_TariffFindBox.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", true, exportInvoiceUserControl.AssayCodeBoundButton.ReadOnly);

				jobDecBizObj.SetReadOnlyIncludingChildren(false);
				Assert(!invoiceHeader.ReadOnly);
				Assert(!invoiceLine.ReadOnly);

				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.Visible);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", false, exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_DescriptionBoundTextBox.ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_AUStateBoundTextBox.ReadOnly);
				jobDecBizObj.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_AUStateBoundTextBox.ReadOnly);
				AssertEquals("JI_PermitNumberBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_PermitNumberBoundTextBox.ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_TempImportNumBoundTextBox.ReadOnly);

				AssertEquals("JI_TariffFindBoxAHECC Visible", false, exportInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBox Visible", true, exportInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBox ReadOnly", false, exportInvoiceUserControl.JI_TariffFindBox.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", false, exportInvoiceUserControl.AssayCodeBoundButton.ReadOnly);

				var tariffColumn = exportInvoiceUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_Tariff");
				AssertType<Universal.GUI.TariffColumnStyleInfo>("Universal Tariff Column should exist", tariffColumn);
			}
		}

		public void TestLockingShipmentData_AHECC()
		{
			var jobDecBizObj = Factory.New<JobDeclaration>();
			jobDecBizObj.SetReadOnlyIncludingChildren(true);
			var invoiceHeader = jobDecBizObj.Invoices.AddNew();
			Assert(invoiceHeader.ReadOnly);
			var invoiceLine = jobDecBizObj.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			Assert(invoiceLine.ReadOnly);

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var testForm = new TestAUCustomsDeclarationForm(jobDecBizObj))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var exportInvoiceUserControl = testForm.InvoiceControl as AUExportInvoiceLineUserControl;
				exportInvoiceUserControl.LineDetailTabControl.SelectedTab = exportInvoiceUserControl.FindSingle<ZTabPage>("LineDetailsTabPage");

				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.Visible);
				Assert("JI_CountryOfOrigin ReadOnly", invoiceLine.JI_CountryOfOriginInfo.ReadOnly);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", true, exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_DescriptionBoundTextBox.ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_AUStateBoundTextBox.ReadOnly);
				AssertEquals("JI_PermitNumberBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_PermitNumberBoundTextBox.ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", true, exportInvoiceUserControl.JI_TempImportNumBoundTextBox.ReadOnly);

				AssertEquals("JI_TariffFindBox Visible", false, exportInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBoxAHECC Visible", true, exportInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBoxAHECC ReadOnly", true, exportInvoiceUserControl.JI_TariffFindBoxAHECC.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", true, exportInvoiceUserControl.AssayCodeBoundButton.ReadOnly);

				jobDecBizObj.SetReadOnlyIncludingChildren(false);
				Assert(!invoiceHeader.ReadOnly);
				Assert(!invoiceLine.ReadOnly);

				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.Visible);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", false, exportInvoiceUserControl.JI_CountryOfOriginBoundFindBox.ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_DescriptionBoundTextBox.ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_AUStateBoundTextBox.ReadOnly);
				jobDecBizObj.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_AUStateBoundTextBox.ReadOnly);
				AssertEquals("JI_PermitNumberBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_PermitNumberBoundTextBox.ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", false, exportInvoiceUserControl.JI_TempImportNumBoundTextBox.ReadOnly);

				AssertEquals("JI_TariffFindBox Visible", false, exportInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBoxAHECC Visible", true, exportInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBoxAHECC ReadOnly", false, exportInvoiceUserControl.JI_TariffFindBoxAHECC.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", false, exportInvoiceUserControl.AssayCodeBoundButton.ReadOnly);

				var tariffColumn = exportInvoiceUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle("JI_Tariff");
				AssertType<AHECCTariffColumnStyleInfo>("AHECC Column should exist", tariffColumn);
			}
		}

		public void TestTariffFindBoxEffectiveTariffCountryAndEffectiveDataGrouping()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (var exportInvoiceUserControl = new AUExportInvoiceLineUserControl())
				{
					AssertEquals("AU", exportInvoiceUserControl.JI_TariffFindBox.EffectiveTariffCountry);
					AssertEquals("AU", exportInvoiceUserControl.JI_TariffFindBox.EffectiveDataGrouping);
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				using (var exportInvoiceUserControl = new AUExportInvoiceLineUserControl())
				{
					AssertEquals("AUT", exportInvoiceUserControl.JI_TariffFindBox.EffectiveTariffCountry);
					AssertEquals("AUT", exportInvoiceUserControl.JI_TariffFindBox.EffectiveDataGrouping);
				}
			}
		}

		sealed class TestAUCustomsDeclarationForm : ZAUCustomsDeclarationForm
		{
			public TestAUCustomsDeclarationForm(JobDeclaration jobDeclaration) : base(jobDeclaration)
			{
			}

			public TestAUCustomsDeclarationForm() : this(null)
			{
			}

			internal BaseCustomsDeclarationUserControl DeclarationUserControl => CustomsBrokerageUserControl.DeclarationUserControlForTesting;

			internal BaseInvoiceLineUserControl InvoiceControl => CustomsBrokerageUserControl.InvoiceLinesUserControl;
		}
	}
}
