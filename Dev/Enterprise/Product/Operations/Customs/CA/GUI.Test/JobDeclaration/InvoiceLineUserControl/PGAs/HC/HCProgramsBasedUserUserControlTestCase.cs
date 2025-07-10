using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	abstract class HCProgramsBasedUserUserControlTestCase : TestCaseWithFactory
	{
		public void TestControlsVisibility()
		{
			SetUp();
			using (var control = GetUserControl())
			{
				AssertControlsVisibility(control);
			}
		}

		public void TestCompoentGroupBoxSupported()
		{
			SetUp();

			using (var control = GetUserControl())
			{
				var componentGroupBoxSupported = control.ComponentGroupBoxSupported;
				AssertEquals(componentGroupBoxSupported, control.ComponentGroupBox.Visible);
				AssertEquals(componentGroupBoxSupported ? DockStyle.Left : DockStyle.Fill, control.LPCOGroupBox.Dock);
			}
		}

		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = GetUserControl())
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		protected virtual void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
		}

		protected abstract HCProgramsBasedUserUserControl GetUserControl();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			invoiceline = invoice.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		protected JobComInvoiceLine invoiceline;
	}
}
