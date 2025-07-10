using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class ExportEntryInstructionDetailsUserControlTest : Customs.GUI.Testing.EntryInstructionDetailsUserControlTest
	{
		public void TestChangeControlsVisibilityExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "OUT DESC";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ExportEntryInstructionDetailsUserControl>().First();
				var detailsGroup = instrUserControl.Controls.Find("UCRDetailsGroupBox", true)[0];
				AssertEquals(true, detailsGroup.Visible);
			}
		}

		public void TestGridVisibleColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "OUT DESC";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ExportEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).FirstOrDefault();
				Assert(entryInstructionGrid != null);
				Assert("Should have CEI_Description column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.CEI_Description));
				Assert("Should have CEI_SpecialCustomsClearance column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.CEI_SpecialCustomsClearance));
				Assert("Should have CEI_LegalDocument column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.CEI_LegalDocument));
				Assert("Should have CEI_IsConsortedExport column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.CEI_IsConsortedExport));
			}
		}

		public void TestEntryInstructionsAdditionalDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "OUT DESC";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ExportEntryInstructionDetailsUserControl>().First();
				var additionalDetaisTabControl = instrUserControl.Controls.Find("AdditionalDetaisTabControl", true)[0];
				AssertEquals(true, additionalDetaisTabControl.Visible);
			}
		}

		public void TestEntryInstructionsGridWithColumnsOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "OUT DESC";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ExportEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).First();

				entryInstructionGrid.ResetColumns();

				Assert("EntryInstructionGrid grid count must have at least " + ExpectedColumnNamesListOnThisOrder.Count.ToString(),
					ExpectedColumnNamesListOnThisOrder.Count <= entryInstructionGrid.Columns.Count);

				for (int i = 0; i < ExpectedColumnNamesListOnThisOrder.Count; i++)
				{
					var column = entryInstructionGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedColumnNamesListOnThisOrder[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals(expectedColumnName + " should be IsVisible", true, column.IsVisible);
				}
			}
		}

		public void TestJustificationGroupBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "OUT DESC";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ExportEntryInstructionDetailsUserControl>().First();

				AssertEquals(false, instrUserControl.JustificationGroupBox.Visible);

				entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
				entryInstruction.CEI_DetailWithoutLegalDoc = DetailWithoutLegalDocList.Codes._3020;
				AssertEquals(true, instrUserControl.JustificationGroupBox.Visible);
			}
		}

		List<ZString> ExpectedColumnNamesListOnThisOrder
		{
			get
			{
				if (expectedColumnNamesListOnThisOrder == null)
				{
					expectedColumnNamesListOnThisOrder = new List<ZString>();
					expectedColumnNamesListOnThisOrder.Add(BR.Business.CusEntryInstruction.Schema.CEI_Description);
					expectedColumnNamesListOnThisOrder.Add(BR.Business.CusEntryInstruction.Schema.CEI_SpecialCustomsClearance);
					expectedColumnNamesListOnThisOrder.Add(BR.Business.CusEntryInstruction.Schema.CEI_LegalDocument);
				}
				return expectedColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedColumnNamesListOnThisOrder;
	}
}
