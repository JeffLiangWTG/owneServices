using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.MarketingManager;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrganisationFilterStrip : OrganisationFilterStrip
	{
		public EDIOrganisationFilterStrip() : base()
		{
			AddCustomFilterControlsBuilder(new LicenceUsageFilterControlBuilder());
			AddCustomFilterControlsBuilder(new ZCurrentDateOffsetFilterControlBuilder());
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is MembershipFilter)
			{
				var filterControl = new MembershipFilterControl();
				PreferredHeight = filterControl.Height + ControlDpiScalingHelper.OnePixel;
				result = new Control[] { filterControl };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}
	}
}

