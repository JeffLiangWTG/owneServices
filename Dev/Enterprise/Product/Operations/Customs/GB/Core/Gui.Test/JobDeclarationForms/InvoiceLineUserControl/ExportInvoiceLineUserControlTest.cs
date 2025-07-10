using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestProcedureFindBoxAndColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var control = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertType<FormattedProcedureCodeFindBox>(control.Controls.Find("CPCFindBox", true)[0]);

				var control1 = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(control1.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Procedure));
				AssertType<FormattedProcedureCodeFindBoxColumnStyleInfo>("FormattedProcedureCodeFindBoxColumnStyleInfo", control1.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure));
			}

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var control3 = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(control3.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Procedure));
				AssertType<FormattedProcedureCodeFindBox>(control3.Controls.Find("CPCFindBox", true)[0]);

				var control4 = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(control4.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Procedure));
				AssertType<FormattedProcedureCodeFindBoxColumnStyleInfo>("FormattedProcedureCodeFindBoxColumnStyleInfo", control4.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure));
			}
		}

		public void TestTariffFindBoxAndColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;

				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
				AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
				AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", true)[0]);

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control1 = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(control1.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
				AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control1.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
				AssertType<Universal.GUI.TariffFindBox>(control1.Controls.Find("TariffFindBox", true)[0]);
			}
		}

		public void TestControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			{
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				using (var control = new ExportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
					AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", true)[0]);

					var supplementaryCode1DropEdit = control.Controls.Find("SupplementaryCode1DropEdit", true)[0];
					var supplementaryCode2DropEdit = control.Controls.Find("SupplementaryCode2DropEdit", true)[0];
					AssertType<ZDropEdit>(supplementaryCode1DropEdit);
					Assert(supplementaryCode1DropEdit.Visible);
					AssertType<ZDropEdit>(supplementaryCode2DropEdit);
					Assert(supplementaryCode2DropEdit.Visible);

					var supplementaryCode1TextBox = control.Controls.Find("SupplementaryCode1TextBox", true)[0];
					var supplementaryCode2TextBox = control.Controls.Find("SupplementaryCode2TextBox", true)[0];
					AssertType<ZTextBox>(supplementaryCode1TextBox);
					Assert(!supplementaryCode1TextBox.Visible);
					AssertType<ZTextBox>(supplementaryCode2TextBox);
					Assert(!supplementaryCode2TextBox.Visible);
					var gridFindBox = (ZCodeFindBox)control.Controls.Find("CPCFindBox", true)[0];

					AssertNotNull(control.PreviousEntryNumberTextBox);
					AssertNotNull(control.PreviousEntryLineNumberCalcEdit);
					AssertNotNull(control.BondedWhsQuantityCalcDropEdit);
				}

				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				using (var control = new ExportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();
					AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
					AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", true)[0]);

					var supplementaryCode1DropEdit = control.Controls.Find("SupplementaryCode1DropEdit", true)[0];
					var supplementaryCode2DropEdit = control.Controls.Find("SupplementaryCode2DropEdit", true)[0];
					AssertType<ZDropEdit>(supplementaryCode1DropEdit);
					Assert(supplementaryCode1DropEdit.Visible);
					AssertType<ZDropEdit>(supplementaryCode2DropEdit);
					Assert(supplementaryCode2DropEdit.Visible);

					var supplementaryCode1TextBox = control.Controls.Find("SupplementaryCode1TextBox", true)[0];
					var supplementaryCode2TextBox = control.Controls.Find("SupplementaryCode2TextBox", true)[0];
					AssertType<ZTextBox>(supplementaryCode1TextBox);
					Assert(!supplementaryCode1TextBox.Visible);
					AssertType<ZTextBox>(supplementaryCode2TextBox);
					Assert(!supplementaryCode2TextBox.Visible);

					AssertNotNull(control.PreviousEntryNumberTextBox);
					AssertNotNull(control.PreviousEntryLineNumberCalcEdit);
					AssertNotNull(control.BondedWhsQuantityCalcDropEdit);
				}
			}
		}

		public void TestCalculationAndStatisticalValueBoxVisibilityExports()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			dec.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				// Same as EU....
				control.JobDeclaration = dec;
				form.Controls.Add(control);
				form.Show();
				control.Show();

				var statValueBox = control.Controls.Find("StatisticalValueLocalCurrencyControl", true)[0];
				Assert(statValueBox != null && statValueBox.Visible);

				//.... Different from EU
				var statManualValue = control.Controls.Find("StatisticalValueCalcEdit", true)[0];
				Assert(statManualValue != null && statManualValue.Visible);
				var statManualOverrideValue = control.Controls.Find("StatValueManualOverrideCheckBox", true)[0];
				Assert(statManualOverrideValue != null && statManualValue.Visible);
			}
		}

		public void TestCaptionWithApplicationCodeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				CombineAssertions("CDS", () =>
				{
					AssertEquals("[UCC 6/8] Goods Desc.", control.FindSingle<LongTextControl>(x => x.Name == "JI_DescriptionBoundTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/14] Price", control.FindSingle<ZCalcFindBox>(x => x.Name == "JI_LinePriceBoundCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 5/15] Origin", control.FindSingle<ZDropEdit>(x => x.Name == "goodsOriginDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/5] GWT", control.FindSingle<ZCalcDropEdit>(x => x.Name == "JI_WeightCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 1/10 & 1/11] Procedure", control.FindSingle<ZCodeFindBox>(x => x.Name == "CPCFindBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 1/11] Further Additional Procedures", control.FindSingle<ZTextBox>(x => x.Name == "zTextBoxAddtionalProcedureCodeAsString").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/1] Customs Qty", control.FindSingle<ZCalcDropEdit>(x => x.Name == "JI_CustomsQuantityCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/2] Supp. Qty", control.FindSingle<ZCalcDropEdit>(x => x.Name == "CustomsQuantityCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals(false, control.FindSingle<ZDocAddressControl>(x => x.Name == "SupervisingOfficeAddressControl").Visible);
					AssertEquals(true, control.FindSingle<ZCalcDropEdit>(x => x.Name == "ThirdQtyCalcDropEdit").Visible);
					AssertEquals("[UCC 6/10] Packages", control.FindSingle<ZTabPage>(x => x.Name == "PackagesPivotTabPage").Text);
					AssertEquals("[UCC 6/14] Commodity", control.FindSingle<Universal.GUI.TariffFindBox>(x => x.Name == "TariffFindBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/16 && 6/17] Additional Codes", control.FindSingle<ZLabel>(x => x.Name == "zLabel2").Text);
					AssertEquals(" ", control.FindSingle<ZLabel>(x => x.Name == "zLabel4").Text);
					AssertEquals(" ", control.FindSingle<ZLabel>(x => x.Name == "zLabel5").Text);
					AssertEquals("[UCC 2/3 && 8/7] Supporting Documents", control.FindSingle<ZTabPage>(x => x.Name == "SupportingDocumentsTabPage").Text);
					AssertEquals("[UCC 2/1] Previous Documents", control.FindSingle<ZTabPage>(x => x.Name == "PreviousDocumentsTabPage").Text);
					AssertEquals("[UCC 2/2] Additional Info", control.FindSingle<ZTabPage>(x => x.Name == "AdditionalInfosTabPage").Text);
					AssertEquals("[UCC 3/40] Fiscal References", control.FindSingle<ZTabPage>(x => x.Name == "FiscalReferencesTabPage").Text);

					AssertEquals("Third Qty", control.FindSingle<ZCalcDropEdit>(x => x.Name == "ThirdQtyCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				});
			}
		}

		public void TestPreviousEntryNumberIsNotVisibleWhenIntoWarehouse()
		{
			AssertVisibilityWhenProcedureChanges(true, false, control =>
			{
				AssertEquals("Previous entry No. is not visible when procedure is into warehouse.", false, control.PreviousEntryNumberTextBox.Visible);
				AssertEquals("Previous entry No. is not visible when procedure is into warehouse.", false, control.PreviousEntryLineNumberCalcEdit.Visible);
			});
		}

		public void TestPreviousEntryNumberIsVisibleWhenOutOfWarehouse()
		{
			AssertVisibilityWhenProcedureChanges(false, true, control =>
			{
				AssertEquals("Previous entry No. is visible when procedure is out of warehouse.", true, control.PreviousEntryNumberTextBox.Visible);
				AssertEquals("Previous entry No. is visible when procedure is out of warehouse.", true, control.PreviousEntryLineNumberCalcEdit.Visible);
			});
		}

		public void TestBondedWhsQuantityIsVisibleWhenIntoWarehouse()
		{
			AssertVisibilityWhenProcedureChanges(true, false, control =>
			{
				AssertEquals("Bonded warehouse quantity is visible when procedure is into warehouse.", true, control.BondedWhsQuantityCalcDropEdit.Visible);
			});
		}

		public void TestBondedWhsQuantityIsVisibleWhenOutOfWarehouse()
		{
			AssertVisibilityWhenProcedureChanges(false, true, control =>
			{
				AssertEquals("Bonded warehouse quantity is visible when procedure is out of warehouse.", true, control.BondedWhsQuantityCalcDropEdit.Visible);
			});
		}

		public void TestColumnCEI_DisplaySequence()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();

				var columnStyle = control.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid").GetColumnStyle(JobComInvoiceLine.Schema.JI_Calc_InstructionDisplaySequence);
				CombineAssertions(() =>
				{
					AssertEquals("Visible", true, columnStyle.IsVisible);
					AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(180), columnStyle.Width);
				});
			}
		}

		public void TestNoExceptionWhenShowingFormWithoutAnInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				control.Hide();

				var invoice = declaration.Invoices.AddNew();
				AssertNoExceptionThrown("Should not throw exception", () => control.Show());
			}
		}

		public void AssertVisibilityWhenProcedureChanges(bool intoWhs, bool outOfWhs, Action<ExportInvoiceLineUserControl> assertion)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "A", "12", "34", "567", "One", EU.Business.MessageTypeList.Codes.Export, intoWarehouse: intoWhs, outOfWarehouse: outOfWhs);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "1234567";

				using (var form = new ZForm(declaration))
				using (var control = new ExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					control.JobDeclaration = declaration;
					form.Show();

					assertion(control);
				}
			}
		}
	}
}
