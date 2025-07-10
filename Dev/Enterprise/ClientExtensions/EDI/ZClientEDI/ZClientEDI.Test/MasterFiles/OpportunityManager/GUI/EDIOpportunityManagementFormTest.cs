using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDIOpportunityManagementForm))]
	internal class EDIOpportunityManagementFormTest : ZFormBasherTest
	{
		public void TestPlugInsAdded()
		{
			using (EDIOpportunityManagementForm form = (EDIOpportunityManagementForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ClientControllerRegistration.OpportunityClientOrgLicence));
				AssertNotNull(form.PlugIns.GetPlugIn(ClientControllerRegistration.OpportunityRelatedProjects));
				AssertNotNull(form.PlugIns.GetPlugIn(ClientControllerRegistration.EDIOpportunityRelatedPSQs));
			}
		}

		protected override Form GetFormToBashCore()
		{
			EDIOrgOpportunity opportunity = Factory.New<EDIOrgOpportunity>();
			opportunity.OrgOpportunityEx.HasChanges = false;
			foreach (var obj in opportunity.ValueAnalysisCollection)
			{
				obj.HasChanges = false;
			}

			opportunity.HasChanges = false;
			EDIOpportunityManagementForm form = new EDIOpportunityManagementForm(opportunity);
			form.Size = new System.Drawing.Size(1200, 770);
			form.ControllerID = ControllerIDs.Opportunity;
			return form;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;
				const int MinScreenWidthSupported = 1200;
				const int MinScreenHeightSupported = 768;
				const int TypicalTaskbarHeight = 43;
				int maxSizeWidth = MinScreenWidthSupported;
				int maxSizeHeight = MinScreenHeightSupported - TypicalTaskbarHeight;
				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}
	}
}
