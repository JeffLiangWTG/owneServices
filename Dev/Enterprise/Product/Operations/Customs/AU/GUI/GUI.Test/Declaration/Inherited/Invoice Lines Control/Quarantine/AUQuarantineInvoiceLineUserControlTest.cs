using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUQuarantineInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestProductPartDropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NEXDOC_HOR, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (var testForm = new ZForm(declaration))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					var invoice = declaration.Invoices.AddNew();
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					control.LineDetailTabControl.SelectedTab = control.RFPDetailsTabPage;
					var dropEdit = control.RFPDetailsUserControl.ProductPartDropEdit;
					CombineAssertions(() =>
					{
						AssertEquals("Drop edit visibility", true, dropEdit.Visible);
						AssertType<ZDropEdit>("Field type", dropEdit);
						AssertEquals("Binding member", "QuarantineExDocLine.QL_ProductPart", dropEdit.GetBindingMember());
					});

					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
					CombineAssertions(() =>
					{
						AssertEquals("Drop edit visibility when is NEXDOC", false, dropEdit.Visible);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					var invoice = declaration.Invoices.AddNew();
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					control.LineDetailTabControl.SelectedTab = control.RFPDetailsTabPage;
					var dropEdit = control.RFPDetailsUserControl.ProductPartDropEdit;
					CombineAssertions(() =>
					{
						AssertEquals("Drop edit visibility when Errata54 is not enabled", false, dropEdit.Visible);
					});
				}
			}
		}

		public void TestProductConditionGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NEXDOC_HOR, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (var testForm = new ZForm(declaration))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					var invoice = declaration.Invoices.AddNew();
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					control.LineDetailTabControl.SelectedTab = control.RFPDetailsTabPage;
					var groupBox = control.RFPDetailsUserControl.ProductConditionGroupBox;
					var grid = control.RFPDetailsUserControl.ProductConditionGrid;
					CombineAssertions(() =>
					{
						AssertEquals("Group box visibility", true, groupBox.Visible);
						AssertEquals("Group box label", "Product Conditions", groupBox.CaptionResourceString.Caption);
						AssertEquals("Grid visibility", true, grid.Visible);
						AssertEquals("Binding member", "QuarantineExDocLine.ProductConditions", grid.GetBindingMember());
						AssertEquals("Code column width", 80, grid.GetColumnStyle(nameof(ProductCondition.CY_Code)).Width);
					});

					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
					CombineAssertions(() =>
					{
						AssertEquals("Group box visibility when is NEXDOC", false, groupBox.Visible);
						AssertEquals("Grid visibility when is NEXDOC", false, grid.Visible);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					var invoice = declaration.Invoices.AddNew();
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					control.LineDetailTabControl.SelectedTab = control.RFPDetailsTabPage;
					var groupBox = control.RFPDetailsUserControl.ProductConditionGroupBox;
					var grid = control.RFPDetailsUserControl.ProductConditionGrid;
					CombineAssertions(() =>
					{
						AssertEquals("Group box visibility when Errata54 is not enabled", false, groupBox.Visible);
						AssertEquals("Grid visibility when Errata54 is not enabled", false, grid.Visible);
					});
				}
			}
		}

		public void TestTreatmentActiveIngredientGrid_Columns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (var testForm = new ZForm(declaration))
			{
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					var processTabPage = control.FindSingle<ZTabPage>("RFPProcessTabPage");
					control.LineDetailTabControl.SelectedTab = processTabPage;
					var grid = processTabPage.FindSingle<ZGrid>("TreatmentActiveIngredientGrid");
					CombineAssertions(() =>
					{
						AssertEquals("Has code column", true, grid.Columns.Contains("CY_Code"));
						AssertEquals("Has description column", true, grid.Columns.Contains("Description"));
					});
					AssertEquals("Is description column readonly", true, grid.GetColumnStyle("Description").IsReadOnly);
				}
			}
		}

		public void TestTreatmentActiveIngredientGrid_GridLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			using (var testForm = new ZForm(declaration))
			{
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					AssertEquals("Treatment Active Ingredients", control.FindSingle<ZGroupBox>("TreatmentActiveIngredientGroupBox").Text);
				}
			}
		}

		public void TestTreatmentActiveIngredientGrid_Visible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			AssertEquals("Pre-Condition", false, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
			using (var testForm = new ZForm(declaration))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					control.LineDetailTabControl.SelectedTab = control.FindSingle<ZTabPage>("RFPProcessTabPage");
					var grid = control.FindSingleOrDefault<ZGrid>("TreatmentActiveIngredientGrid");
					AssertNotNull("EXDOCS_Errata54 false, grid exists", grid);
					AssertEquals("EXDOCS_Errata54 false, grid not visible", false, grid.Visible);
					AssertEquals("EXDOCS_Errata54 false, groupbox not visible", false, grid.Visible);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					control.LineDetailTabControl.SelectedTab = control.FindSingle<ZTabPage>("RFPProcessTabPage");
					var grid = control.FindSingleOrDefault<ZGrid>("TreatmentActiveIngredientGrid");
					AssertEquals("EXDOCS_Errata54 true, grid visible", true, grid.Visible);
				}
			}

			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			using (var testForm = new ZForm(declaration))
			{
				AssertEquals("Pre-Condition", true, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				using (var control = new AUQuarantineInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					testForm.Controls.Add(control);
					testForm.Show();
					control.LineDetailTabControl.SelectedTab = control.FindSingle<ZTabPage>("RFPProcessTabPage");
					var grid = control.FindSingleOrDefault<ZGrid>("TreatmentActiveIngredientGrid");
					AssertEquals("EXDOCS_Errata54 is true but this is a NEXDOC job. The Active Ingredients grid should not be visible for NEXDOCS jobs", false, grid.Visible);
					AssertEquals("Treatment Active Ingredients", false, control.FindSingle<ZGroupBox>("TreatmentActiveIngredientGroupBox").Visible);
				}
			}
		}

		public void TestInvoiceLinesContext()
		{
			using (var control = new AUQuarantineInvoiceLineUserControl())
			{
				AssertEquals("Context is set", nameof(DeclarationType.Quarantine), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestEXDOCCertificateControlsAreHiddenWhenNexdocIsActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				AssertEquals(true, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceLineUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPCertificatesTabPage");
				AssertEquals("QL_CommercialProductDescriptionTextBox", false, quarantineInvoiceLineUserControl.FindSingle<ZTextBox>("QL_CommercialProductDescriptionTextBox").Visible);
				AssertEquals("QL_HealthCertificateDescriptionTextBox", false, quarantineInvoiceLineUserControl.FindSingle<ZTextBox>("QL_HealthCertificateDescriptionTextBox").Visible);
				AssertEquals("QL_ImportAuthorityCodeTextBox", false, quarantineInvoiceLineUserControl.FindSingle<ZTextBox>("QL_ImportAuthorityCodeTextBox").Visible);
				AssertEquals("QL_SendHCDescCheckBox", false, quarantineInvoiceLineUserControl.FindSingle<ZCheckBox>("QL_SendHCDescCheckBox").Visible);
			}
		}

		public void TestUseInvoiceLineDescriptionButton()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				AssertEquals(true, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceLineUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPCertificatesTabPage");
				var buttonControl = quarantineInvoiceLineUserControl.FindSingle<ZButton>("CopyMeatInspectionDescriptionFromLineButton");
				AssertEquals("CopyMeatInspectionDescriptionFromLineButton", true, buttonControl.Visible);
				var textbox = quarantineInvoiceLineUserControl.FindSingle<ZTextBox>("QL_AdditionalProductDescriptionTextBox");
				AssertEquals("QL_AdditionalProductDescriptionTextBox", true, textbox.Visible);
				Assert("Button does not overlap the text box", !buttonControl.Bounds.IntersectsWith(textbox.Bounds));
				invoiceLine.JI_Description = "Testing";
				Assert("QL_MeatInspectionDescription.IsEmpty", invoiceLine.QuarantineExDocLine.QL_MeatInspectionDescription.IsEmpty);
				buttonControl.PerformClick();
				AssertEquals("Button click copies JI_Description to QL_MeatInspectionDescription", invoiceLine.JI_Description, invoiceLine.QuarantineExDocLine.QL_MeatInspectionDescription);
			}
		}

		public void TestMeatInspectionDescriptionTextBoxCaption_RFP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				AssertEquals(false, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceLineUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPCertificatesTabPage");
				AssertEquals("Line Item (Inspection Description)", quarantineInvoiceLineUserControl.FindSingle<ZTextBox>("QL_MeatInspectionDescriptionTextBox").CaptionResourceString.Caption);
			}
		}

		public void TestMeatInspectionDescriptionTextBoxCaption_NEXDOCS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				AssertEquals(true, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceLineUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPCertificatesTabPage");
				AssertEquals("Manual Certificate Product Description", quarantineInvoiceLineUserControl.FindSingle<ZTextBox>("QL_MeatInspectionDescriptionTextBox").CaptionResourceString.Caption);
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				AssertEquals("Line Item (Inspection Description)", quarantineInvoiceLineUserControl.FindSingle<ZTextBox>("QL_MeatInspectionDescriptionTextBox").CaptionResourceString.Caption);
			}
		}

		public void TestNoPermitRequiredCheckBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.InvoiceLines.AddNew();

			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				AssertEquals(false, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				var noPermitRequiredCheckBox = quarantineInvoiceLineUserControl.FindSingle<ZCheckBox>("NoPermitRequiredCheckBox");
				Assert("noPermitRequiredCheckBox is not visible for EXDOC", !noPermitRequiredCheckBox.Visible);
			}

			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				AssertEquals(true, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				var noPermitRequiredCheckBox = quarantineInvoiceLineUserControl.FindSingle<ZCheckBox>("NoPermitRequiredCheckBox");
				Assert("noPermitRequiredCheckBox is visible for NEXDOC", noPermitRequiredCheckBox.Visible);
			}
		}

		public void TestNEXDOCTabText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				AssertEquals(false, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				var tab1 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPPackagesTabPage");
				var tab2 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPDetailsTabPage");
				var tab3 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPProcessTabPage");
				var tab4 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPCertificatesTabPage");
				var tab5 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPAnalysisTabPage");
				var tab6 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPMeatTabPage");
				var tab7 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPStatementsTabPage");
				var tab8 = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPNumbersTabPage");
				AssertEquals("RFP Packages", tab1.Text);
				AssertEquals("RFP Details", tab2.Text);
				AssertEquals("RFP Process", tab3.Text);
				AssertEquals("RFP Certificates", tab4.Text);
				AssertEquals("RFP Analysis", tab5.Text);
				AssertEquals("RFP Meat", tab6.Text);
				AssertEquals("RFP Statements", tab7.Text);
				AssertEquals("RFP Numbers", tab8.Text);

				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				AssertEquals("NEXDOC is active", true, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				AssertTabTextIsNEXDOC(tab1, tab2, tab3, tab4, tab5, tab6, tab7, tab8);

				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
				AssertEquals("NEXDOC is active", true, invoice.QuarantineExDocHeader.IsNEXDOCSActive);
				AssertTabTextIsNEXDOC(tab1, tab2, tab3, tab4, tab5, tab6, tab7, tab8);
			}
		}

		void AssertTabTextIsNEXDOC(ZTabPage tab1, ZTabPage tab2, ZTabPage tab3, ZTabPage tab4, ZTabPage tab5, ZTabPage tab6, ZTabPage tab7, ZTabPage tab8)
		{
			AssertEquals("REX Packages", tab1.Text);
			AssertEquals("REX Details", tab2.Text);
			AssertEquals("REX Process", tab3.Text);
			AssertEquals("REX Certificates", tab4.Text);
			AssertEquals("REX Analysis", tab5.Text);
			AssertEquals("REX Meat", tab6.Text);
			AssertEquals("REX Statements", tab7.Text);
			Assert(!tab8.TabVisible);
		}

		public void TestControlsVisibilityIfQH_ProductTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				var rfpNumbersTabPage = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPNumbersTabPage");
				quarantineInvoiceLineUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPDetailsTabPage");
				AssertEquals(false, quarantineInvoiceLineUserControl.FindSingle<ZCodeFindBox>("QL_CategoryCodeFindBox").Visible);
				AssertEquals(true, quarantineInvoiceLineUserControl.FindSingle<ZCodeFindBox>("QL_SupplimentaryCodeCodeFindBox").Visible);
				AssertEquals(false, quarantineInvoiceLineUserControl.FindSingle<ZDropEdit>("QL_SupplimentaryCodeDropEdit").Visible);
				AssertEquals(false, quarantineInvoiceLineUserControl.FindSingle<QuarantineEUTariffFindBox>("EUTariffFindBox").Visible);
				AssertEquals(true, quarantineInvoiceLineUserControl.FindSingle<ZCheckBox>("FinalConsumerCheckBox").Visible);
				Assert(rfpNumbersTabPage.TabVisible);
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				Assert(!rfpNumbersTabPage.TabVisible);
				quarantineInvoiceLineUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPDetailsTabPage");
				AssertEquals(true, quarantineInvoiceLineUserControl.FindSingle<ZCodeFindBox>("QL_CategoryCodeFindBox").Visible);
				AssertEquals(false, quarantineInvoiceLineUserControl.FindSingle<ZCodeFindBox>("QL_SupplimentaryCodeCodeFindBox").Visible);
				AssertEquals(true, quarantineInvoiceLineUserControl.FindSingle<ZDropEdit>("QL_SupplimentaryCodeDropEdit").Visible);
				AssertEquals(true, quarantineInvoiceLineUserControl.FindSingle<QuarantineEUTariffFindBox>("EUTariffFindBox").Visible);
				AssertEquals(true, quarantineInvoiceLineUserControl.FindSingle<ZCheckBox>("FinalConsumerCheckBox").Visible);
			}
		}

		public void TestEE_EstablishmentIndicatorColumnVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("RFPProcessTabPage");
				var grid = quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab.FindSingle<ZGrid>("ProcessingGrid");
				AssertEquals(false, grid.Columns.Contains("EE_EstablishmentIndicator"));
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
				AssertEquals(true, grid.Columns.Contains("EE_EstablishmentIndicator"));
			}
		}

		public void TestREXProductAttachmentsTabPageVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				var tab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("REXProductAttachmentsTabPage");
				Assert(tab.TabVisible);
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
				Assert(!tab.TabVisible);
			}
		}

		public void TestCusContainerInvoiceLineGridReadOnly()
		{
			var dec = CreateQuarantineDecWithMessage(EDIInterchange.ApplicationCodes.EXDOC);
			var container = dec.CusContainers.AddNew();
			container.CO_ContainerNumber = "MAEU9304711";
			var invoice = dec.Invoices[0];
			var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line1.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container).IsForInvoiceLine = true;
			AssertCusContainerInvoiceLineGridReadOnly(dec, false);

			dec.MakeNonPersistent();
			invoice.JZ_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			// IsNEXDOCSActive is always true when dec is non-persistent. 
			AssertCusContainerInvoiceLineGridReadOnly(dec, true);
		}

		public void TestDeclarationModes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				AssertEquals("Invoice User Control should be QuarantineInvoiceUserControl", typeof(AUQuarantineInvoiceLineUserControl), testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.GetType());
				//GST and Duty are invisible
				var lineUserControl = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("Duty Not visible", false, lineUserControl.DutyConvertToLocalCurrencyControl.Visible);
				AssertEquals("GST Not visible", false, lineUserControl.GSTConvertToLocalCurrencyControl.Visible);
			}
		}

		public void TestLockingShipmentData()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.SetReadOnlyIncludingChildren(true);
			dec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = dec.Invoices.AddNew();
			Assert(invoiceHeader.ReadOnly);
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			Assert(invoiceLine.ReadOnly);

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var testForm = new TestAUCustomsDeclarationForm(dec))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("LineDetailsTabPage");
				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").Visible);
				Assert(invoiceLine.JI_CountryOfOriginInfo.ReadOnly);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<LongTextControl>("JI_DescriptionBoundTextBox").ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_AUStateBoundTextBox").ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_TempImportNumBoundTextBox").ReadOnly);

				AssertEquals("JI_TariffFindBox Visible", true, quarantineInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBoxAHECC Visible", false, quarantineInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBox ReadOnly", true, quarantineInvoiceUserControl.JI_TariffFindBox.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", true, quarantineInvoiceUserControl.AssayCodeBoundButton.ReadOnly);

				dec.SetReadOnlyIncludingChildren(false);
				Assert(!invoiceHeader.ReadOnly);
				Assert(!invoiceLine.ReadOnly);

				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").Visible);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<LongTextControl>("JI_DescriptionBoundTextBox").ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_AUStateBoundTextBox").ReadOnly);
				dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_AUStateBoundTextBox").ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_TempImportNumBoundTextBox").ReadOnly);

				AssertEquals("JI_TariffFindBox Visible", true, quarantineInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBoxAHECC Visible", false, quarantineInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBox ReadOnly", false, quarantineInvoiceUserControl.JI_TariffFindBox.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", false, quarantineInvoiceUserControl.AssayCodeBoundButton.ReadOnly);
			}
		}

		public void TestLockingShipmentData_AHECC()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.SetReadOnlyIncludingChildren(true);
			dec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = dec.Invoices.AddNew();
			Assert(invoiceHeader.ReadOnly);
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			Assert(invoiceLine.ReadOnly);

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var testForm = new TestAUCustomsDeclarationForm(dec))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("LineDetailsTabPage");
				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").Visible);
				Assert(invoiceLine.JI_CountryOfOriginInfo.ReadOnly);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<LongTextControl>("JI_DescriptionBoundTextBox").ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_AUStateBoundTextBox").ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", true, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_TempImportNumBoundTextBox").ReadOnly);

				AssertEquals("JI_TariffFindBox Visible", false, quarantineInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBoxAHECC Visible", true, quarantineInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBoxAHECC ReadOnly", true, quarantineInvoiceUserControl.JI_TariffFindBoxAHECC.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", true, quarantineInvoiceUserControl.AssayCodeBoundButton.ReadOnly);

				dec.SetReadOnlyIncludingChildren(false);
				Assert(!invoiceHeader.ReadOnly);
				Assert(!invoiceLine.ReadOnly);

				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.BalanceConvertToLocalCurrencyControl.Visible);
				Assert("Precondition of test not met (VISIBLE)", quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").Visible);
				AssertEquals("JI_CountryOfOriginBoundFindBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZCodeFindBox>("JI_CountryOfOriginBoundFindBox").ReadOnly);
				AssertEquals("JI_DescriptionBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<LongTextControl>("JI_DescriptionBoundTextBox").ReadOnly);
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_AUStateBoundTextBox").ReadOnly);
				dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				AssertEquals("JI_AUStateBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_AUStateBoundTextBox").ReadOnly);
				AssertEquals("JI_TempImportNumBoundTextBox ReadOnly", false, quarantineInvoiceUserControl.FindSingle<ZTextBox>("JI_TempImportNumBoundTextBox").ReadOnly);

				AssertEquals("JI_TariffFindBox Visible", false, quarantineInvoiceUserControl.JI_TariffFindBox.Visible);
				AssertEquals("JI_TariffFindBoxAHECC Visible", true, quarantineInvoiceUserControl.JI_TariffFindBoxAHECC.Visible);
				AssertEquals("JI_TariffFindBoxAHECC ReadOnly", false, quarantineInvoiceUserControl.JI_TariffFindBoxAHECC.ReadOnly);
				AssertEquals("AssayCodeBoundButton ReadOnly", false, quarantineInvoiceUserControl.AssayCodeBoundButton.ReadOnly);
			}
		}

		public void TestEUTariffFindBoxAndFormatter()
		{
			var dataHelper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var tariffType = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EXP", nomenclatureGroupType: "CN");
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "12345678", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "GROUP 1", "12.34.56.78", "CN");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_CombinedNomenclature = "12345678";
			using (var testForm = new TestAUCustomsDeclarationForm(dec))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("RFPDetailsTabPage");
				var findbox = quarantineInvoiceUserControl.FindSingle<QuarantineEUTariffFindBox>("EUTariffFindBox");
				Assert("EUTariffFindBox is visible", findbox.Visible);
				Assert("EUTariffFindBox is editable", !findbox.ReadOnly);
				AssertEquals("TariffType", "EXP", findbox.TariffType);
				AssertEquals("GetDataGrouping", "EUN", findbox.GetDataGrouping());
				Common.ITariffFormatter findboxFormatter = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					if (dialog is ZForm form)
					{
						form.Activated += (sender, args) =>
						{
							var helper = (TariffSearchHelper)form.BusinessEntity;
							findboxFormatter = helper.TariffFormatter;
						};
					}
				});
				findbox.SelectFromPopupForm();
				AssertSame("EUTariffFindBox is using formatter from quarantine line", quarantineLine.EUTariffFormatter, findboxFormatter);
			}
		}

		public void TestRFPMeatUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_LabelApprovalNumber = "11";

			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.CustomsInvoiceLinesBoundGrid.Select(0);

				var meatTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("RFPMeatTabPage");
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = meatTab;
				Assert(meatTab.TabVisible);

				var meatUserControl = meatTab.FindSingle<RFPMeatUserControl>("RFPMeatUserControl");
				AssertNotNull(meatUserControl);

				var meatGroupBox = meatUserControl.FindSingle<ZGroupBox>("MeatGroupBox");
				AssertEquals("The 'meatGroupBox' label caption is visible and matches the expected text.", "Meat", meatGroupBox.Text);

				var labelApprovalNumberTextBox = meatUserControl.FindSingle<ZTextBox>("QL_LabelApprovalNumberTextBox");
				AssertEquals("11", labelApprovalNumberTextBox.Text);
			}
		}

		public void TestRFPPackagesUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_ShippingMarks = "MARKS";

			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.CustomsInvoiceLinesBoundGrid.Select(0);

				var packagesTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("RFPPackagesTabPage");
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = packagesTab;
				Assert(packagesTab.TabVisible);

				var packagesUserControl = packagesTab.FindSingle<RFPPackagesUserControl>("RFPPackagesUserControl");
				AssertNotNull(packagesUserControl);

				var packagesLabel = packagesUserControl.FindSingle<ZGroupBox>("PackagesGroupBox");
				AssertEquals("The 'PackagesGroupBox' label caption is not visible or does not match the expected text.", "Packages", packagesLabel.Text);

				var textBoxShippingMarks = packagesUserControl.FindSingle<ZTextBox>("TextBoxQL_ShippingMarks");
				AssertEquals("MARKS", textBoxShippingMarks.Text);
			}
		}
		public void TestRFPStatementsUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_StatementText = "THIS IS A STATEMENT.";

			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.CustomsInvoiceLinesBoundGrid.Select(0);

				var statementsTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("RFPStatementsTabPage");
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = statementsTab;
				Assert(statementsTab.TabVisible);

				var statementsUserControl = statementsTab.FindSingle<RFPStatementsUserControl>("RFPStatementsUserControl");
				AssertNotNull(statementsUserControl);

				var statementGroupBox = statementsUserControl.FindSingle<ZGroupBox>("StatementTextGroupBox");
				AssertEquals("The 'statementGroupBox' label caption is visible and matches the expected text.", "Statement Text", statementGroupBox.Text);

				var statementTextTextBox = statementsUserControl.FindSingle<ZTextBox>("QL_StatementTextTextBox");
				AssertEquals("THIS IS A STATEMENT.", statementTextTextBox.Text);
			}
		}

		public void TestRFPCertificatesUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_AddtionalProductDescription = "GOOD MEAT";
			quarantineExDocLine.QL_MeatInspectionDescription = "GOOD MEAT";
			invoiceLine.JI_Description = "BAD MEAT";

			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var quarantineInvoiceUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
				quarantineInvoiceUserControl.CustomsInvoiceLinesBoundGrid.Select(0);

				var certificatesTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("RFPCertificatesTabPage");
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = certificatesTab;
				Assert(certificatesTab.TabVisible);

				var certificatesUserControl = certificatesTab.FindSingle<RFPCertificatesUserControl>("RFPCertificatesUserControl");
				AssertNotNull(certificatesUserControl);

				var certificateGroupBox = certificatesUserControl.FindSingle<ZGroupBox>("CertificateGroupBox");
				AssertEquals("The 'CertificateGroupBox' label caption is not visible or does not match the expected text.", "Certificate", certificateGroupBox.Text);

				var additionalProductDescriptionTextBox = certificatesUserControl.FindSingle<ZTextBox>("QL_AdditionalProductDescriptionTextBox");
				AssertEquals("GOOD MEAT", additionalProductDescriptionTextBox.Text);

				var meatInspectionDescriptionTextBox = certificatesUserControl.FindSingle<ZTextBox>("QL_MeatInspectionDescriptionTextBox");
				var copyMeatInspectionDescriptionFromLineButton = certificatesUserControl.FindSingle<ZButton>("CopyMeatInspectionDescriptionFromLineButton");
				copyMeatInspectionDescriptionFromLineButton.PerformClick();
				AssertEquals("BAD MEAT", meatInspectionDescriptionTextBox.Text);
			}
		}

		public void TestTariffFindBoxEffectiveTariffCountryAndEffectiveDataGrouping()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				using (var quarantineInvoiceUserControl = new AUQuarantineInvoiceLineUserControl())
				{ 
					AssertEquals("AU", quarantineInvoiceUserControl.JI_TariffFindBox.EffectiveTariffCountry);
					AssertEquals("AU", quarantineInvoiceUserControl.JI_TariffFindBox.EffectiveDataGrouping);
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				using (var quarantineInvoiceUserControl = new AUQuarantineInvoiceLineUserControl())
				{ 
					AssertEquals("AUT", quarantineInvoiceUserControl.JI_TariffFindBox.EffectiveTariffCountry);
					AssertEquals("AUT", quarantineInvoiceUserControl.JI_TariffFindBox.EffectiveDataGrouping);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		void AssertCusContainerInvoiceLineGridReadOnly(JobDeclaration dec, bool shouldBeReadonly)
		{
			using (var testForm = new TestAUCustomsDeclarationForm(dec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceUserControl = (AUQuarantineInvoiceLineUserControl)testForm.InvoiceControl;
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("ContainersTabPage");
				var grid = quarantineInvoiceUserControl.FindSingle<ZGrid>("CusContainerInvoiceLineGrid");
				AssertEquals("CusContainerInvoiceLineGrid Visible", true, grid.Visible);
				AssertEquals("CusContainerInvoiceLineGrid RowCount", 1, grid.VisibleRowCount);
				AssertEquals("CusContainerInvoiceLineGrid ReadOnly", shouldBeReadonly, grid.ReadOnly);
			}
		}

		JobDeclaration CreateQuarantineDecWithMessage(string messageApplicationCode)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var inv = dec.Invoices.AddNew();
			var quarantineInv = inv.QuarantineExDocHeader;
			var msg = quarantineInv.Messages.AddNew();
			msg.EM_ApplicationCode = messageApplicationCode;
			return dec;
		}
	}

	internal sealed class TestAUCustomsDeclarationForm : ZAUCustomsDeclarationForm
	{
		public TestAUCustomsDeclarationForm(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		public TestAUCustomsDeclarationForm() : this(null)
		{
		}

		public BaseCustomsDeclarationUserControl DeclarationUserControl => CustomsBrokerageUserControl.DeclarationUserControlForTesting;

		public BaseInvoiceLineUserControl InvoiceControl => CustomsBrokerageUserControl.InvoiceLinesUserControl;
	}
}
