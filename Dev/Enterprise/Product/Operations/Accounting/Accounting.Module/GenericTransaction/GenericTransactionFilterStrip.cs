using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class GenericTransactionFilterStrip : ZFilterStrip
	{
		public GenericTransactionFilterStrip()
		{
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is DependentListFilter)
			{
				return new Control[] { new DependentListFilterControl() };
			}
			else if (currentModuleFilter is AccGLHeaderRangeFilter)
			{
				return new Control[] { new AccGLHeaderRangeFilterControl() };
			}
			else if (currentModuleFilter is SubAccountFilter)
			{
				var control = new SubAccountFilterControl();
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
