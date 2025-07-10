using System;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	public class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestVatDetailGUIDControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			{
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();
					var vatDetailGUIDControl = control.Controls.Find("VatDetailGuidDropEdit", true)[0];
					Assert("CHIEF doesn't need the FR-style additional VAT field, only the old VatTypeDropEdit must show.", !vatDetailGUIDControl.Visible);
				}
			}

			using (var form = new ZForm(declaration))
			{
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();
					var vatDetailGUIDControl = control.Controls.Find("VatDetailGuidDropEdit", true)[0];
					Assert("CDS doesn't need the FR-style additional VAT field, only the old VatTypeDropEdit must show.", !vatDetailGUIDControl.Visible);
				}
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
				declaration.JE_MessageType = MessageTypeList.Codes.Import;

				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
				AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
				AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", true)[0]);

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control1 = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNull(control1.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
				AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control1.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
				AssertType<Universal.GUI.TariffFindBox>(control1.Controls.Find("TariffFindBox", true)[0]);
			}
		}

		public void TestControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			{
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();
					AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
					AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", true)[0]);

					var quotaTextBox = control.Controls.Find("QuotaTextBox", true)[0];
					AssertType<ZTextBox>(quotaTextBox);
					Assert(!quotaTextBox.Visible);

					var quotaDropEdit = control.Controls.Find("QuotaDropEdit", true)[0];
					AssertType<ZDropEdit>(quotaDropEdit);
					Assert(quotaDropEdit.Visible);

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

				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();
					AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
					AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", true)[0]);

					var quotaTextBox = control.Controls.Find("QuotaTextBox", true)[0];
					AssertType<ZTextBox>(quotaTextBox);
					Assert(!quotaTextBox.Visible);

					var quotaDropEdit = control.Controls.Find("QuotaDropEdit", true)[0];
					AssertType<ZDropEdit>(quotaDropEdit);
					Assert(quotaDropEdit.Visible);

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

		public void TestMethodOfPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_SupplementaryCode1 = "1";
			invoiceLine1.JI_SupplementaryCode2 = "2";
			using (var form = new ZForm(declaration))
			{
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					var methodOfPaymentDropEdit = control.FindSingle<ZDropEdit>("MethodOfPaymentDropEdit");
					var customsInvoiceLinesBoundGrid = control.FindSingle<ZGrid>(x => x.Name == "CustomsInvoiceLinesBoundGrid");
					Assert(customsInvoiceLinesBoundGrid.Columns["ZG_MethodOfPayment"].IsVisible);
					Assert(!customsInvoiceLinesBoundGrid.Columns["ZG_MethodOfPayment"].IsUnavailable);
					Assert(methodOfPaymentDropEdit.Visible);
				}

				declaration.JE_ApplicationCode = "CHF";
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					var zdropEditMethodOfPayment = control.FindSingle<ZDropEdit>("MethodOfPaymentDropEdit");
					var customsInvoiceLinesBoundGrid = control.FindSingle<ZGrid>(x => x.Name == "CustomsInvoiceLinesBoundGrid");
					Assert(!customsInvoiceLinesBoundGrid.Columns.Contains("ZG_CountryOfDestination"));
					Assert(!zdropEditMethodOfPayment.Visible);
				}
			}
		}

		public void TestCountryOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_SupplementaryCode1 = "1";
			invoiceLine1.JI_SupplementaryCode2 = "2";
			using (var form = new ZForm(declaration))
			{
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					var customsInvoiceLinesBoundGrid = control.FindSingle<ZGrid>(x => x.Name == "CustomsInvoiceLinesBoundGrid");
					AssertEquals(true, customsInvoiceLinesBoundGrid.Columns.Contains("ZG_CountryOfDestination"));
				}

				declaration.JE_ApplicationCode = "CHF";
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();
					form.Show();

					var customsInvoiceLinesBoundGrid = control.FindSingle<ZGrid>(x => x.Name == "CustomsInvoiceLinesBoundGrid");
					AssertEquals(false, customsInvoiceLinesBoundGrid.Columns.Contains("ZG_CountryOfDestination"));
				}
			}
		}

		public void TestCanAddAdditionalSupplementaryCodesWhenCDS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_SupplementaryCode1 = "1";
			invoiceLine1.JI_SupplementaryCode2 = "2";

			AssertNullOrEmpty("invoiceLine1.JI_AdditionalSupplements", invoiceLine1.JI_AdditionalSupplements);
			AssertEquals("AdditionalSupplementaryCodes.AllowNew should be true", true, invoiceLine1.AdditionalSupplementaryCodes.AllowNew);

			invoiceLine1.AdditionalSupplementaryCodes.AddNew("A");
			invoiceLine1.AdditionalSupplementaryCodes.AddNew("B");

			AssertEquals("JI_AdditionalSupplements", "A,B", invoiceLine1.JI_AdditionalSupplements);

			using (var form = new ZForm(declaration))
			{
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();
					form.Controls.Add(control);
					form.Show();

					AssertEquals("JI_AdditionalSupplementsTextBox", true, control.FindSingle<ZTextBox>(x => x.Name == "JI_AdditionalSupplementsTextBox").Visible);
					AssertEquals("AdditionalSupplementaryCodesEditButton", true, control.FindSingle<ZButton>(x => x.Name == "AdditionalSupplementaryCodesEditButton").Visible);
				}
			}
		}

		public void TestCannotAddAdditionalSupplementaryCodesWhenCHF()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CHF";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_SupplementaryCode1 = "1";
			invoiceLine1.JI_SupplementaryCode2 = "2";

			AssertNullOrEmpty("JI_AdditionalSupplements", invoiceLine1.JI_AdditionalSupplements);
			AssertEquals("AdditionalSupplementaryCodes.AllowNew", false, invoiceLine1.AdditionalSupplementaryCodes.AllowNew);

			using (var form = new ZForm(declaration))
			{
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					AssertEquals("JI_AdditionalSupplementsTextBox should not be visible", false, control.FindSingle<ZTextBox>(x => x.Name == "JI_AdditionalSupplementsTextBox").Visible);
					AssertEquals("AdditionalSupplementaryCodesEditButton should not be visible", false, control.FindSingle<ZButton>(x => x.Name == "AdditionalSupplementaryCodesEditButton").Visible);
				}
			}
		}

		public void TestCaptionWithApplicationCodeChanged()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				CombineAssertions("CDS", () =>
				{
					AssertEquals("[UCC 6/8] Goods Desc.", control.FindSingle<LongTextControl>(x => x.Name == "JI_DescriptionBoundTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/14] Price", control.FindSingle<ZCalcFindBox>(x => x.Name == "JI_LinePriceBoundCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 5/15,16] Country/Region of (Preferential) Origin", control.FindSingle<ZDropEdit>(x => x.Name == "goodsOriginDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/5] GWT", control.FindSingle<ZCalcDropEdit>(x => x.Name == "JI_WeightCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/17] Pref. Code", control.FindSingle<ZDropEdit>(x => x.Name == "PreferenceCodeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 8/1] Quota", control.FindSingle<ZDropEdit>(x => x.Name == "QuotaDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/16] Valuation Method", control.FindSingle<ZDropEdit>(x => x.Name == "ValuationMethodDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					var cpcFindBox = control.FindSingle<ZCodeFindBox>(x => x.Name == "CPCFindBox");
					Assert(cpcFindBox is FormattedProcedureCodeFindBox);
					AssertEquals("[UCC 1/10 & 1/11] Procedure", cpcFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 1/11] Further Additional Procedures", control.FindSingle<ZTextBox>(x => x.Name == "zTextBoxAddtionalProcedureCodeAsString").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/1] Customs Qty", control.FindSingle<ZCalcDropEdit>(x => x.Name == "JI_CustomsQuantityCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/2] Supp. Qty", control.FindSingle<ZCalcDropEdit>(x => x.Name == "CustomsQuantityCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals(false, control.FindSingle<ZDocAddressControl>(x => x.Name == "SupervisingOfficeAddressControl").Visible);
					AssertEquals(false, control.FindSingle<ZDropEdit>(x => x.Name == "ValuationAdjustmentCodeDropEdit").Visible);
					AssertEquals(false, control.FindSingle<ZCalcEdit>(x => x.Name == "ValuationAdjustmentPercentageCalcEdit").Visible);
					AssertEquals(true, control.FindSingle<ZCalcDropEdit>(x => x.Name == "ThirdQtyCalcDropEdit").Visible);
					AssertEquals("[UCC 6/10] Packages", control.FindSingle<ZTabPage>(x => x.Name == "PackagesPivotTabPage").Text);
					AssertEquals("[UCC 6/14 & 6/15] Commodity and TARIC", control.FindSingle<Universal.GUI.TariffFindBox>(x => x.Name == "TariffFindBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 6/16 && 6/17] Additional Codes", control.FindSingle<ZLabel>(x => x.Name == "zLabel2").Text);
					AssertEquals(" ", control.FindSingle<ZLabel>(x => x.Name == "zLabel4").Text);
					AssertEquals(" ", control.FindSingle<ZLabel>(x => x.Name == "zLabel5").Text);
					AssertEquals("[UCC 2/3 && 8/7] Supporting Documents", control.FindSingle<ZTabPage>(x => x.Name == "SupportingDocumentsTabPage").Text);
					AssertEquals("[UCC 2/1] Previous Documents", control.FindSingle<ZTabPage>(x => x.Name == "PreviousDocumentsTabPage").Text);
					AssertEquals("[UCC 2/2] Additional Info", control.FindSingle<ZTabPage>(x => x.Name == "AdditionalInfosTabPage").Text);
					AssertEquals("[UCC 3/40] Fiscal References", control.FindSingle<ZTabPage>(x => x.Name == "FiscalReferencesTabPage").Text);
					AssertEquals("[UCC 6/17] VAT", control.FindSingle<ZDropEdit>(x => x.Name == "VatTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/13] Value Indicators", control.FindSingle<ZTabPage>(x => x.Name == "ValueIndicatorsTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("[UCC 4/8] Method of Payment", control.FindSingle<ZDropEdit>(x => x.Name == "MethodOfPaymentDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);

					var column = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle("ZG_CountryOfDestination");
					AssertNotNull(column);
					AssertEquals("[UCC 5/8] Destination", column.Caption);
					AssertEquals("[UCC 5/15] Origin Override", control.FindSingle<ZCodeFindBox>(x => x.Name == "CountryOfSupplyCodeFindBox").GetExtension<ILabelCaptionRenderer>().Caption);

					column = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure);
					AssertEquals("[UCC 1/10 & 1/11] Procedure", column.Caption);

					column = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationCode);
					AssertEquals("[UCC 4/16] Valuation Method", column.Caption);

					AssertEquals("Third Qty", control.FindSingle<ZCalcDropEdit>(x => x.Name == "ThirdQtyCalcDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				});
			}
		}

		public void TestEntryInstructions()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					declaration.JE_ApplicationCode = "CDS";
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

					var invoiceLineControl = (EUInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					AssertEquals("JI_CEIGuidDropEdit", true, invoiceLineControl.InvoiceDetailsGroupBox.Controls.Find("JI_CEIGuidDropEdit", true)?.FirstOrDefault()?.Visible);
					AssertEquals(true, invoiceLineControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI).IsVisible);
				}
			}
		}

		public void TestZG_ValueAdjustmentCodeVisible()
		{
			var cdsDeclartion = Factory.New<JobDeclaration>();
			cdsDeclartion.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			cdsDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(cdsDeclartion))
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = cdsDeclartion;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.ZG_ValueAdjustmentCode)).IsUnavailable);
				AssertEquals(false, control.FindSingle<ZDropEdit>("ValuationAdjustmentCodeDropEdit").Visible);
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
			using (var control = new ImportInvoiceLineUserControl())
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

		public void TestColumnTariffUsedForNorthernIrelandImport()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();

				var columnStyle = control.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid").GetColumnStyle(nameof(JobComInvoiceLine.TariffUsedForNorthernIrelandImport));
				CombineAssertions(() =>
				{
					AssertEquals("NI Tariff", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.TariffUsedForNorthernIrelandImport)).Caption);
					AssertEquals("ReadOnly", expected: true, columnStyle.IsReadOnly);
					AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), columnStyle.Width);
				});
			}
		}

		public void TestNoExceptionWhenShowingFormWithoutAnInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				control.Hide();

				var invoice = declaration.Invoices.AddNew();
				AssertNoExceptionThrown("Should not throw exception", () => control.Show());
			}
		}

		public void TestFourthQtyCalcDropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				var fourthQtyCalcDropEdit = control.Controls.Find("FourthQtyCalcDropEdit", true)[0] as ZCalcDropEdit;
				AssertNotNull("Control exists", fourthQtyCalcDropEdit);
				Assert("Control is visible", fourthQtyCalcDropEdit.Visible);
			}
		}

		public void TestFifthQtyCalcDropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				var fifthQtyCalcDropEdit = control.Controls.Find("FifthQtyCalcDropEdit", true)[0] as ZCalcDropEdit;
				AssertNotNull("Control exists", fifthQtyCalcDropEdit);
				Assert("Control is visible", fifthQtyCalcDropEdit.Visible);
				AssertEquals("BindToAmount", "FilteredInvoiceLines.JI_CustomsFifthQuantity", fifthQtyCalcDropEdit.BindToAmount);
				AssertEquals("BindToList", "FilteredInvoiceLines.Lookups+CustomsUQList", fifthQtyCalcDropEdit.BindToList);
				AssertEquals("BindToUnit", "FilteredInvoiceLines.JI_CustomsFifthUnitQty", fifthQtyCalcDropEdit.BindToUnit);
			}
		}

		public void AssertVisibilityWhenProcedureChanges(bool intoWhs, bool outOfWhs, Action<ImportInvoiceLineUserControl> assertion)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "A", "12", "34", "567", "One", EU.Business.MessageTypeList.Codes.Import, intoWarehouse: intoWhs, outOfWarehouse: outOfWhs);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "1234567";

				using (var form = new ZForm(declaration))
				using (var control = new ImportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					control.JobDeclaration = declaration;
					form.Show();

					assertion(control);
				}
			}
		}

		public void TestGoodsCategoryDropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				form.Controls.Add(control);
				form.Show();

				var goodsCategoryDropEdit = control.Controls.Find("GoodsCategoryDropEdit", true)[0] as ZDropEdit;
				AssertNotNull("Control exists", goodsCategoryDropEdit);
				AssertEquals("BindTo", "FilteredInvoiceLines.ZG_GoodsCategory", goodsCategoryDropEdit.BindTo);

				declaration.JE_NorthernIrelandMode = ZString.Empty;
				Assert("Control is not visible", !goodsCategoryDropEdit.Visible);

				declaration.JE_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
				Assert("Control is visible", goodsCategoryDropEdit.Visible);
			}
		}

		public void TestDispatchCountryDropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using var form = new ZForm(declaration);
			using var control = new ImportInvoiceLineUserControl();
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var dispatchCountryDropEdit = control.FindSingle<ZDropEdit>("DispatchCountryDropEdit");
			AssertNotNull("Control exists", dispatchCountryDropEdit);
			AssertEquals("BindTo", "FilteredInvoiceLines.JI_RN_NKCountryOfExport", dispatchCountryDropEdit.BindTo);
			Assert("Control is visible", dispatchCountryDropEdit.Visible);
			AssertEquals("Caption", "[UCC 5/14] Dispatch/Export Country/Region", dispatchCountryDropEdit.GetExtension<ILabelCaptionRenderer>().Caption);
		}

		public void TestDispatchCountryGridColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using var form = new ZForm(declaration);
			using var control = new ImportInvoiceLineUserControl();
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			form.Controls.Add(control);
			form.Show();

			var customsInvoiceLinesBoundGrid = control.FindSingle<ZGrid>(x => x.Name == "CustomsInvoiceLinesBoundGrid");
			var dispatchCountryColumnStyleInfo = customsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport) as ZDropEditColumnStyleInfo;
			AssertNotNull("Column exists", dispatchCountryColumnStyleInfo);
			Assert("Column is visible", dispatchCountryColumnStyleInfo.IsVisible);
			AssertEquals("Caption", "[UCC 5/14] Dispatch/Export Country/Region", customsInvoiceLinesBoundGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport));
		}
	}
}
