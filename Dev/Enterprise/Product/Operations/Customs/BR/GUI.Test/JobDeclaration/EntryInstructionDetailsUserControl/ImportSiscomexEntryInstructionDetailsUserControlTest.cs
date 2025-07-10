using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class ImportSiscomexEntryInstructionDetailsUserControlTest : Customs.GUI.Testing.EntryInstructionDetailsUserControlTest
	{
		public void TestChangeControlsVisibilityImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

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
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportSiscomexEntryInstructionDetailsUserControl>().First();
				CombineAssertions(() =>
				{
					Assert("EntryInstructionAdditionalInformationUserControl should be Visible", instrUserControl.EntryInstructionAdditionalInformationUserControl.Visible);
					Assert("MercosulForeignDeclarationGroupBox should be Visible", instrUserControl.MercosulForeignDeclarationGroupBox.Visible);
					Assert("MercosulForeignDeclarationGrid should be Visible", instrUserControl.MercosulForeignDeclarationGrid.Visible);
					Assert("AFRMMGroupBox should be Visible", instrUserControl.AFRMMDetailsUserControl.AFRMMGroupBox.Visible);
					Assert("IsAFRMMRateOverriddenCheckBox should be Visible", instrUserControl.AFRMMDetailsUserControl.IsAFRMMRateOverriddenCheckBox.Visible);
					Assert("SystemUtilizationFeeOverrideCurrencyTextBox should be Visible", instrUserControl.AFRMMDetailsUserControl.SystemUtilizationFeeOverrideCurrencyTextBox.Visible);
				});
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				Assert("AFRMMGroupBox should NOT be Visible", !instrUserControl.AFRMMDetailsUserControl.AFRMMGroupBox.Visible);
			}
		}

		public void TestGridVisibleColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
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
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportSiscomexEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).FirstOrDefault();
				AssertNotNull("Instruction Grid should NOT be NULL", entryInstructionGrid);
				CombineAssertions(() =>
				{
					AssertEquals("Instruction Grid Columns Number", 4, entryInstructionGrid.Columns.Count(x => x.IsVisible));
					Assert("Should have CEI_Description column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.CEI_Description));
					Assert("Should have AdditionalInformationOptionDescription column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.AdditionalInformationOptionDescription));
					Assert("Should have AdditionalInformationManual column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.AdditionalInformationManual));
					Assert("Should have AdditionalInformation column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.AdditionalInformation));
				});
			}
		}

		public void TestEntryInstructionsGridWithColumnsOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
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
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportSiscomexEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).First();

				entryInstructionGrid.ResetColumns();

				AssertEquals("Instruction Grid Columns Number", 4, entryInstructionGrid.Columns.Count);

				for (var i = 0; i < ExpectedColumnNamesListOnThisOrder.Count; i++)
				{
					var column = entryInstructionGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedColumnNamesListOnThisOrder[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
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
					expectedColumnNamesListOnThisOrder.Add(BR.Business.CusEntryInstruction.Schema.AdditionalInformationOptionDescription);
					expectedColumnNamesListOnThisOrder.Add(BR.Business.CusEntryInstruction.Schema.AdditionalInformationManual);
					expectedColumnNamesListOnThisOrder.Add(BR.Business.CusEntryInstruction.Schema.AdditionalInformation);
				}
				return expectedColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedColumnNamesListOnThisOrder;
	}
}
