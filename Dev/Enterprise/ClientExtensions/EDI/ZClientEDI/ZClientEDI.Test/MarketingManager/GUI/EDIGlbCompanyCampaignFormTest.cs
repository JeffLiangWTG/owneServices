using System.Windows.Forms;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	[TestedType(typeof(EDIGlbCompanyCampaignForm))]
	public class EDIGlbCompanyCampaignFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			EDIGlbCompanyCampaign campaign = Factory.New<EDIGlbCompanyCampaign>();
			EDIGlbCompanyCampaignForm form = new EDIGlbCompanyCampaignForm(campaign);
			form.ControllerID = ControllerIDs.GlbCompanyCampaign;
			return form;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;
				const int MinScreenWidthSupported = 1440;
				const int MinScreenHeightSupported = 811;
				const int TypicalTaskbarHeight = 43;
				int maxSizeWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported);
				int maxSizeHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported - TypicalTaskbarHeight);
				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}
	}
}
