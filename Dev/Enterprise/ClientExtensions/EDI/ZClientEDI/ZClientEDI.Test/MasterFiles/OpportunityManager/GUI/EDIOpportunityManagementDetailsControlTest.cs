using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDIOpportunityManagementDetailsControlForm))]
	public class EDIOpportunityManagementDetailsControlTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			Assert(opportunity.ValueAnalysisCollection.Count > 0);
			Assert(opportunity.OrgOpportunityEx.EOM_GlobalPotential == 0);
			Factory.Save();
			ZForm form = new EDIOpportunityManagementDetailsControlForm(opportunity);
			form.ControllerID = ControllerIDs.Opportunity;
			return form;
		}

		class EDIOpportunityManagementDetailsControlForm : ZForm
		{
			public EDIOpportunityManagementDetailsControlForm(EDIOrgOpportunity opportunity) : base(opportunity)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Size = ControlDpiScalingHelper.NewScaledSize(1200, 770, true);
				DetailsControl.Dock = DockStyle.Fill;
				Controls.Add(DetailsControl);
				BindingSource.SetBindingMember(DetailsControl, ".");
				CaptionRenderingEnabled = true;
			}

			readonly EDIOpportunityManagementDetailsControl DetailsControl = new EDIOpportunityManagementDetailsControl();
		}
		#endregion Implementation
	}
}
