using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestZG_TransNatureVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;

			using (var form = new JobDeclarationForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				var transactionNatureDropEdit = control.FindSingle<ZDropEdit>("TransactionNatureDropEdit");
				AssertEquals("[24] Tran. Nature should be invisible when JE_ApplicationCode is Delta G.", false, transactionNatureDropEdit.Visible);
			}

			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new JobDeclarationForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				var transactionNatureDropEdit = control.FindSingle<ZDropEdit>("TransactionNatureDropEdit");
				Assert("[24] Tran. Nature should be visible when JE_ApplicationCode is Delta IE.", transactionNatureDropEdit.Visible);
			}
		}

		public void TestTransNatureInGrid()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;

			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				var lineGrid = control.CustomsInvoiceLinesBoundGrid;

				AssertEquals("ZG_TransNature should be invisible in the grid when JE_ApplicationCode is Delta G.", false, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_TransNature).IsVisible);
			}

			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				var lineGrid = control.CustomsInvoiceLinesBoundGrid;

				AssertEquals("ZG_TransNature should be visible in the grid when JE_ApplicationCode is Delta IE.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_TransNature).IsVisible);
			}
		}

		public void TestCountryOfDispatchCodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var countryOfDispatchCodeFindBox = control.FindSingle<ZCodeFindBox>(x => x.Name == "CountryOfDispatchCodeFindBox");
				AssertEquals("countryOfDispatchCodeFindBox should be invisible when JE_ApplicationCode is Delta G", false, countryOfDispatchCodeFindBox.Visible);
			}

			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var countryOfDispatchCodeFindBox = control.FindSingle<ZCodeFindBox>(x => x.Name == "CountryOfDispatchCodeFindBox");
				AssertEquals("countryOfDispatchCodeFindBox should be visible when JE_ApplicationCode is Delta IE", true, countryOfDispatchCodeFindBox.Visible);
			}
		}

		public void TestJI_Calc_OtherTaxesAmountConvertToLocalCurrencyControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var otherTaxesAmountConvertToLocalCurrencyControl = control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JI_Calc_OtherTaxesAmountConvertToLocalCurrencyControl");
				AssertEquals("JI_Calc_OtherTaxesAmountConvertToLocalCurrencyControl should be visible", true, otherTaxesAmountConvertToLocalCurrencyControl.Visible);
			}
		}

		public void TestUniversalTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("IMP", control.GetUniversalTariffType());

				control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				AssertEquals("IMP", control.GetUniversalTariffType());

				declaration.JE_TariffType = "";
				control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 0;
				AssertEquals("IMP", control.GetUniversalTariffType());

				control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				AssertEquals("IMP", control.GetUniversalTariffType());
			}
		}

		public void TestGetPreviousDocumentsUserControlType()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var previousDocumentsUserControlType = typeof(ImportInvoiceLineUserControl).GetMethod("GetPreviousDocumentsUserControlType", BindingFlags.Instance | BindingFlags.NonPublic);
				AssertEquals(typeof(InvoiceLinePreviousDocumentsUserControl), previousDocumentsUserControlType.Invoke(control, Array.Empty<object>()));
			}
		}

		public void TestCountryOfDestinationFindBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var countryOfDestinationCodeFindBox = control.FindSingle<ZCodeFindBox>("CountryOfDestinationCodeFindBox");

				CombineAssertions(() =>
				{
					AssertEquals("CountryOfDestinationCodeFindBox.Visible", false, countryOfDestinationCodeFindBox.Visible);
					AssertEquals("CountryOfDestinationGridColumn.Visible", false, !control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.ZG_CountryOfDestination)).IsUnavailable);
				});

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				control = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				countryOfDestinationCodeFindBox = control.FindSingle<ZCodeFindBox>("CountryOfDestinationCodeFindBox");
				CombineAssertions(() =>
				{
					AssertEquals("CountryOfDestinationCodeFindBox.Visible", true, countryOfDestinationCodeFindBox.Visible);
					AssertEquals("CountryOfDestinationGridColumn.Visible", true, !control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.ZG_CountryOfDestination)).IsUnavailable);
				});
			}
		}

		public void TestSupplyChainActorReferencesGridColumnsSorting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.Invoices.AddNew();

			var control = new EU.GUI.PlugIn.SupplyChainActorReferencesUserControl();

			using (var form = new ZForm(declaration))
			{
				control.SetDataBinding(declaration, "FilteredInvoiceLines.CusSupplyChainActorReferences");
				form.Controls.Add(control);
				form.Show();

				var supplyChainActorReferencesGrid = control.FindSingle<ZGrid>("SupplyChainActorReferencesGrid");
				AssertSequencesEqual("Columns should be ordered depending ColumnNamesInSortOrderCore property",
						new[] { AutoCusReference.Schema.CFR_Code, CommonCusReference.Schema.OwnerOrgPK, AutoCusReference.Schema.CFR_Reference },
						supplyChainActorReferencesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray());
			}
		}

		public void TestOrganizationsTabPageAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var control = new CustomsBrokerageUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaG;
				AssertEquals("Prerequisite : declaration should not be UCC6 and Import.", false, declaration.IsUCC6AndIsImport);

				control.MainTabControl.SelectedTab = control.InvoiceLinesTabPage;
				var organizationsTabPage = (ZTabPage)control.Controls.Find("OrganizationsTabPage", true).FirstOrDefault();
				AssertNull("Organizations tab should only show for Import UCC6 declarations.", organizationsTabPage);

				control.MainTabControl.SelectedTab = control.DeclarationTabPage;
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.FR.Business.DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals("Prerequisite: declaration should be UCC6 and Import.", true, declaration.IsUCC6AndIsImport);

				control.MainTabControl.SelectedTab = control.InvoiceLinesTabPage;
				organizationsTabPage = (ZTabPage)control.Controls.Find("OrganizationsTabPage", true).FirstOrDefault();
				AssertNotNull("Organizations tab should show for Import UCC6 declarations.", organizationsTabPage);
			}
		}

		public void TestVatDetailGuidDropEditVisibility()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var vatDetailGuidDropEdit = control.FindSingle<ZGuidDropEdit>(x => x.Name == "VatDetailGuidDropEdit");
				AssertEquals("VatDetailGuidDropEdit should be visible", true, vatDetailGuidDropEdit.Visible);
			}
		}

		public void TestVatDropDownVisibility()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var vatTypeDropEdit = control.FindSingle<ZDropEdit>(x => x.Name == "VatTypeDropEdit");
				AssertEquals("vatTypeDropEdit should be visible", true, vatTypeDropEdit.Visible);
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.Controls.Find("SupportingDocumentsTabPage", true).First().Show();
				var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(SupportingDocumentsUserControl), supportingDocument.UserControlType);

				var tabPage = (ZTabPage)control.Controls.Find("NationalAdditionalCodeTabPage", true).FirstOrDefault();
				AssertNull(tabPage);

				var invoiceLinesSummaryGroupboxCtrl = control.Controls.Find("InvoiceLinesSummaryGroupBox", true).FirstOrDefault();
				AssertNotNull("InvoiceLinesSummaryGroupBox control found", invoiceLinesSummaryGroupboxCtrl);

				var invoicedDocumentaryAmountCtrl = invoiceLinesSummaryGroupboxCtrl.Controls.Find("JI_Calc_InvoicedDocumentaryAmountControl", true).FirstOrDefault();
				AssertNotNull("InvoicedDocumentaryAmount control found", invoicedDocumentaryAmountCtrl);

				AssertNotNull(control.PreviousEntryNumberTextBox);
				AssertNotNull(control.PreviousEntryLineNumberCalcEdit);
				AssertNotNull(control.BondedWhsQuantityCalcDropEdit);

				var classificationDetailsGroupBox = control.Controls.Find("ClassificationDetailsGroupBox", true).FirstOrDefault();
				AssertNotNull("TariffBypassCode control found", classificationDetailsGroupBox.Controls.Find("tariffBypassCodeControl", false).FirstOrDefault());
				AssertNotNull("CusNumberCodeFindBox control found", classificationDetailsGroupBox.Controls.Find("CusNumberCodeFindBox", false).FirstOrDefault());
			}
		}

		public void TestPreviousEntryNumberIsNotVisibleWhenIntoWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("Y", "N", control =>
			{
				AssertEquals("Previous entry No. is not visible when procedure is into warehouse.", false, control.PreviousEntryNumberTextBox.Visible);
				AssertEquals("Previous entry No. is not visible when procedure is into warehouse.", false, control.PreviousEntryLineNumberCalcEdit.Visible);
			});
		}

		public void TestPreviousEntryNumberIsVisibleWhenOutOfWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("N", "Y", control =>
			{
				AssertEquals("Previous entry No. is visible when procedure is out of warehouse.", true, control.PreviousEntryNumberTextBox.Visible);
				AssertEquals("Previous entry No. is visible when procedure is out of warehouse.", true, control.PreviousEntryLineNumberCalcEdit.Visible);
			});
		}

		public void TestBondedWhsQuantityIsVisibleWhenIntoWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("Y", "N", control =>
			{
				AssertEquals("Bonded warehouse quantity is visible when procedure is into warehouse.", true, control.BondedWhsQuantityCalcDropEdit.Visible);
			});
		}

		public void TestBondedWhsQuantityIsVisibleWhenOutOfWarehouse()
		{
			AssertVisibilityWhenProcedureChanges("N", "Y", control =>
			{
				AssertEquals("Bonded warehouse quantity is visible when procedure is out of warehouse.", true, control.BondedWhsQuantityCalcDropEdit.Visible);
			});
		}

		void AssertVisibilityWhenProcedureChanges(string intoWhs, string outOfWhs, Action<ImportInvoiceLineUserControl> assertion)
		{
			RefDataHelper.AddProcedure(Factory, EU.Business.MessageTypeList.Codes.Import, "12", "34", "567", intoWhs, outOfWhs);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
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

		public void TestGDMLinkTabIndex()
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var gdmLink = control.Controls.Find("GDMLink", true).Single();
				AssertEquals(9, gdmLink.TabIndex);
			}
		}

		public void TestFRGuidedDecisionMakingFormShownWhenGDMLinkButtonClicked()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var gDMLink = control.FindSingle<ZLinkLabel>("GDMLink");
				gDMLink.PerformClick_ForTest();

				var gDMForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("GDM Form should be created and shown", () =>
				{
					AssertNotNull("GDM Form created", gDMForm);
					AssertType<GDM.GuidedDecisionMakingForm>(gDMForm);
				});
			}
		}

		public void TestCusNumberCodeFindBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				var classificationDetailsGroupBox = control.Controls.Find("ClassificationDetailsGroupBox", true).FirstOrDefault();
				var cusNumberCodeFindBox = classificationDetailsGroupBox.FindSingle<ZCodeFindBox>(x => x.Name == "CusNumberCodeFindBox");
				AssertEquals("Default JE_ApplicationCode: DeltaG", false, cusNumberCodeFindBox.Visible);

				declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals("JE_ApplicationCode: DeltaIE", true, cusNumberCodeFindBox.Visible);

				declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaG;
				AssertEquals("JE_ApplicationCode: DeltaG", false, cusNumberCodeFindBox.Visible);
			}
		}

		public void TestGetAdditionalInfosUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			using (var control = new ImportInvoiceLineUserControlWithExposedUserControlType())
			{
				control.JobDeclaration = declaration;

				declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaG;
				AssertEquals(typeof(AdditionalInfosUserControl), control.GetAdditionalInfosUserControlType_Exposed());

				declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertEquals(typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid), control.GetAdditionalInfosUserControlType_Exposed());
			}
		}

		public void TestAdditionalInfosTabPageCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaG;
				var additionalInfoTabPage = control.FindSingle<ZTabPage>("AdditionalInfosTabPage");
				AssertEquals("[44] Special Mentions", additionalInfoTabPage.Text);

				declaration.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
				additionalInfoTabPage = control.FindSingle<ZTabPage>("AdditionalInfosTabPage");
				AssertEquals("[44] Additional Documents", additionalInfoTabPage.Text);
			}
		}

		public void TestGetValuationIndicatorsUserControlType()
		{
			using (var control = new ImportInvoiceLineUserControlWithExposedUserControlType())
			{
				AssertEquals(typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl), control.GetValuationIndicatorsUserControlType_Exposed());
			}
		}
	}

	class ImportInvoiceLineUserControlWithExposedUserControlType : ImportInvoiceLineUserControl
	{
		public Type GetAdditionalInfosUserControlType_Exposed() => GetAdditionalInfosUserControlType();

		public Type GetValuationIndicatorsUserControlType_Exposed() => GetValuationIndicatorsUserControlType();
	}

	public class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
	{
		public new Func<ZString> GetUniversalTariffType => base.GetUniversalTariffType;
	}
}
