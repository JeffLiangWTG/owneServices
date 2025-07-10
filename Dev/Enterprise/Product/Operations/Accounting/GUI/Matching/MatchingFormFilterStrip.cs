using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class MatchingFormFilterStrip : ZFilterStrip
	{
		public MatchingFormFilterStrip()
		{
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is DependentListFilter)
			{
				return new Control[] { new DependentListFilterControl() };
			}
			else if (currentModuleFilter is OrgWithAddressFilter)
			{
				OrgWithAddressFilterControl control = new OrgWithAddressFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				return new Control[] { control };
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}

