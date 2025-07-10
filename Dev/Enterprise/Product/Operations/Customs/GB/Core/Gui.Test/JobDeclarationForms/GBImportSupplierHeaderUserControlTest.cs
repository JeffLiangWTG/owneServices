using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using static System.FormattableString;

namespace Enterprise.Customs.GB.GUI.Testing
{
	class GBImportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestChargesVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_ManualCalc = true;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				var invoicesUserControl = brokerageControl.SupplierHeaderUserControl;
				var chargesTabControl = GetControlHeirarchey("Splitter->Splitter.Panel2->InvoiceTabControl->ComInvoiceDetailsTabPage->InvDetailRightPanel->ChargesGroupsSplitterContainer->ChargesGroupsSplitterContainer.Panel1->ChargesGroupBox->ChargesTabControl", invoicesUserControl);
				var invoiceChargesTabPage = GetControlHeirarchey("InvoiceChargesTabPage", chargesTabControl);
				var apportionedTabPage = GetControlHeirarchey("ApportionedTabPage", chargesTabControl);

				Assert("Precondition", invoicesUserControl.Visible);
				AssertNotNull("Manual calc - charges controls are visible", invoiceChargesTabPage);
				Assert("Manual calc - charges controls are hidden", !apportionedTabPage.Visible);

				declaration.ZG_ManualCalc = false;
				invoicesUserControl = brokerageControl.SupplierHeaderUserControl;
				chargesTabControl = GetControlHeirarchey("Splitter->Splitter.Panel2->InvoiceTabControl->ComInvoiceDetailsTabPage->InvDetailRightPanel->ChargesGroupsSplitterContainer->ChargesGroupsSplitterContainer.Panel1->ChargesGroupBox->ChargesTabControl", invoicesUserControl);
				invoiceChargesTabPage = GetControlHeirarchey("InvoiceChargesTabPage", chargesTabControl);
				apportionedTabPage = GetControlHeirarchey("ApportionedTabPage", chargesTabControl);
				var apportionedTabPage2 = (ZTabPage)GetControlHeirarchey("ApportionedTabPage", chargesTabControl);
				AssertNotNull("Standard calc - apportioned charges control visible", apportionedTabPage);
				Assert("Standard calc - apportioned charges control visible", !apportionedTabPage.Visible);
				Assert("Standard calc - apportioned charges control visible", apportionedTabPage2.TabVisible);
				AssertNotNull("Standard calc - invoice charges control is still hidden / null because no charges apply direct to invoices (only group invoice)", invoiceChargesTabPage);
			}
		}

		Control GetControlHeirarchey(string periodDelimitedPathToControl, Control parent)
		{
			var control = parent;
			var path = periodDelimitedPathToControl.Split(new string[] { "->" }, StringSplitOptions.None);
			foreach (var nextPath in path)
			{
				if (control != null)
				{
					if (control is KSplitContainer splitContainer)
					{
						control = nextPath.Contains("Panel1") ? splitContainer.Panel1 : splitContainer.Panel2;
					}
					else
					{
						control = control.Controls.Cast<Control>().FirstOrDefault(x => x.Name == nextPath);
					}
				}
				else
				{
					throw new Exception(Invariant($"When trying to get part {nextPath} from {periodDelimitedPathToControl} for top control {parent.Name} we found null on the previous try. It's possible that the names of the base controls, or their heirarchy, has changed. "));
				}
			}
			return control;
		}
	}
}
