using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class EntryInstructionDetailsUserControlTest : Customs.GUI.Testing.EntryInstructionDetailsUserControlTest
	{
		public void TestGridVisibleColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryInstruction = Factory.New<Business.CusEntryInstruction>();
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
				var userControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)userControl.Controls.Find("EntryInstructionsGrid", true).FirstOrDefault();
				Assert(entryInstructionGrid != null);
				Assert("Should have CEI_Description column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(MX.Business.CusEntryInstruction.Schema.CEI_Description));
			}
		}

		public void TestEntryInstructionIdentifierCollumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entryInstruction = Factory.New<Business.CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "Description";

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;

				var userControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
				var identifiersGrid = (ZGrid)userControl.Controls.Find("IdentifiersGrid", true).FirstOrDefault();

				Assert(identifiersGrid != null);
				Assert("Should have CSI_Code column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Code));
				Assert("Should have CSI_ReferenceNumber column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber));
				Assert("Should have CSI_ReferenceNumber2 column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber2));
				Assert("Should have CSI_Description column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Description));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				userControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().First();
				identifiersGrid = (ZGrid)userControl.Controls.Find("IdentifiersGrid", true).FirstOrDefault();

				Assert(identifiersGrid != null);
				Assert("Should have CSI_Code column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Code));
				Assert("Should have CSI_ReferenceNumber column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber));
				Assert("Should have CSI_ReferenceNumber2 column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_ReferenceNumber2));
				Assert("Should have CSI_Description column in IdentifiersGrid", identifiersGrid.Columns.Contains(CusSupportingInfo.Schema.CSI_Description));
			}
		}
	}
}
