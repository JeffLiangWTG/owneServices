using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("Reimport Date", control.FindSingleOrDefault<ZDateEdit>("ReimportDateEdit"));
					AssertNotNull("Usual Replacement", control.FindSingleOrDefault<ZCheckBox>("UsualReplacementCheckBox"));
				});
			}
		}

		public void TestGetPreviousDocumentsUserControlType()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertEquals(typeof(ExportInvoiceLinePreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
			}
		}

		public void TestPreviousProceduresTabPageVisible()
		{
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var previousProceduresTabPage = control.FindSingle<ZTabPage>("PreviousProceduresTabPage");
				AssertEquals(true, previousProceduresTabPage.TabVisible);
			}
		}

		public void TestExportTaxTabPageNotVisible()
		{
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				AssertNull(control.Controls.Find("TaxTabPage", true).FirstOrDefault());
			}
		}

		public void TestAdditionalInfosTabPage()
		{
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var additionalInfosTabPage = control.FindSingle<ZTabPage>("AdditionalInfosTabPage");
				AssertEquals("AdditionalInfosTabPage visible", true, additionalInfosTabPage.TabVisible);
				AssertEquals("Caption", "[44] Additional Documents", additionalInfosTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestTabPagesOrder()
		{
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var tabPages = control.LineDetailTabControl.TabPages;
				AssertArrayEqualsByElements(new[] { "NewLineDetailsTabPage", "OrganizationsTabPage", "LineChargesTabPage", "SupportingDocumentsTabPage" , "AdditionalInfosTabPage",
					"PreviousDocumentsTabPage", "PackagesPivotTabPage", "PreviousProceduresTabPage", "AuthorisationsTabPage", "CustomFieldsTabPage" }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
			}
		}

		public void TestColumnJI_CustomsQuantity()
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Show();
				var groupname = "[38] Net Mass Measure";
				CombineAssertions(() =>
				{
					AssertEquals("JI_CustomsQuantity GroupName Caption", groupname, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(EU.Business.Declaration.JobComInvoiceLine.Schema.JI_CustomsQuantity).GroupName.Caption);
					AssertEquals("Column Size", 150, control.CustomsInvoiceLinesBoundGrid.GetColumnWidth(EU.Business.Declaration.JobComInvoiceLine.Schema.JI_CustomsQuantity));
				});
			}
		}

		public void TestColumnJI_CustomsUnitQtyGroupName()
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Show();

				AssertEquals("JI_CustomsUnitQty GroupName Caption", "[38] Net Mass Measure", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(EU.Business.Declaration.JobComInvoiceLine.Schema.JI_CustomsUnitQty).GroupName.Caption);
			}
		}

		public void TestColumnJI_Description()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				var descColumn = control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.JI_Description);
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, descColumn.CharacterCasing);
			}
		}

		public void TestPreviousProceduresCorrectlyLoaded()
		{
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				control.SetDataBinding(declaration, ZString.Empty);
				form.Show();

				var previousProceduresTabPage = control.FindSingle<ZTabPage>("PreviousProceduresTabPage");
				control.LineDetailTabControl.SelectedTab = previousProceduresTabPage;

				var previousDocumentGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
				AssertEquals("Column Count", 5, previousDocumentGrid.Columns.Count);
			}
		}

		public void TestInvoiceLineDetailsPanelLayout()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				var layout = control.GetNewInvoiceLineDetailsPanelLayout_Exposed().Layout;
				var exportLayout = ((IPanelLayoutProvider)new ExportInvoiceLineDetailsLayout()).Layout;
				AssertContainsExactElementsInAnyOrder("IncludedControls", layout.IncludedControls, exportLayout.IncludedControls);
			}
		}

		public void TestDynamicLayoutApplied()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertEquals(true, control.DynamicLayoutApplied_Exposed);
			}
		}

		public void TestHasDifferentPanelLayout()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertEquals(true, control.HasDifferentPanelLayout_Exposed);
			}
		}

		public void TestAdditionalInfosUserControlType()
		{
			using (var frm = new ZForm(declaration))
			using (var userControl = new ExportInvoiceLineUserControl())
			{
				frm.Controls.Add(userControl);
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfosTabPage");
				tabPage.Show();
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1");
				AssertEquals(typeof(AdditionalInfosUserControlWithGrid), foundUserControl.UserControlType);
			}
		}

		public void TestColumnJI_Procedure()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();
				CombineAssertions(() =>
				{
					AssertNotNull("JI_Procedure", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Procedure));
					AssertNull("JI_FormattedProcedure", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure));
				});
			}
		}

		public void TestDgSubstanceUserControl_MoreButtonClick()
		{
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dgSubstanceUserControl = control.FindSingle<DgSubstanceUserControl>();
				dgSubstanceUserControl.MoreButton.PerformClick();
				AssertType<MasterFiles.GUI.UNDGDataItemForm>("Clicking button opens up UNDGDataItemForm", ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumns()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();
				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				CombineAssertions(() =>
				{
					AssertEquals("JI_StateOrRegionOfOrigin", false, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin).IsUnavailable);
				});
			}
		}

		public void TestInvoiceLineChargesUserControlType()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertType<ExportInvoiceLineChargesUserControl>(control.InvoiceLineCharges); 
			}
		}

		public void TestBondedWhsQuantityCalcDropEdit()
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var calcDropEdit = control.FindSingle<ZCalcDropEdit>("BondedWhsQuantityCalcDropEdit");
				AssertEquals(3, calcDropEdit.UnitPreBoundMaxLength);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}

	sealed class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
	{
		public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();

		public ZBool DynamicLayoutApplied_Exposed => base.DynamicLayoutApplied;

		public ZBool HasDifferentPanelLayout_Exposed => base.HasDifferentPanelLayout;

		public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout_Exposed() => base.GetNewInvoiceLineDetailsPanelLayout();
	}
}
