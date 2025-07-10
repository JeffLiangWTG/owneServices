using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingOptionSelection;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection.Testing
{
	[TestedType(typeof(AgentPostingOptionSelectionForm))]
	public class AgentPostingOptionSelectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ZGuid agent = creator.AALSHI.PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(agent);
			IReceivablesPostingChargeCollection charges = new IReceivablesPostingChargeCollection();
			charges.Key = new PostingChargeKey(agent, "", ZGuid.Empty, ZGuid.Empty, 0);
			AgentPostingOptionSelector selector = new AgentPostingOptionSelector(Factory, consol, charges);
			return new AgentPostingOptionSelectionForm(selector);
		}

		public void TestContinueAndCancelButton()
		{
			using (var form = (AgentPostingOptionSelectionForm)GetFormToBash())
			{
				AssertEquals("ContinueButton_ForTestOnly should not have a DialogResult", DialogResult.None, form.ContinueButton_ForTestOnly.DialogResult);
				AssertEquals("CancelButton should have a DialogResult", DialogResult.Cancel, form.CancelPostingButton_ForTestOnly.DialogResult);

				form.Show();
				((AgentPostingOptionSelector)form.BusinessEntity).Currency = "123";
				form.ContinueButton_ForTestOnly.PerformClick();
				AssertEquals("ContinueButton DialogResult when there are errors", DialogResult.None, form.DialogResult);

				((AgentPostingOptionSelector)form.BusinessEntity).Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				form.ContinueButton_ForTestOnly.PerformClick();
				AssertEquals("ContinueButton DialogResult when there are no errors", DialogResult.OK, form.DialogResult);
			}
		}
	}
}
