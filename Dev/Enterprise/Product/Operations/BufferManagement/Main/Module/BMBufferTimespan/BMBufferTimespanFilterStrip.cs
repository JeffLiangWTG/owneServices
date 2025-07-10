using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.Module
{
	public class BMBufferTimespanFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is ModuleDurationFilter)
			{
				result = GetModuleDurationFilterControls().ToArray();
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}

		IEnumerable<Control> GetModuleDurationFilterControls()
		{
			var control = new ModuleDurationFilterControl();
			ControlDpiScalingHelper.SetTop(ref control, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(control, FilterDescriptionDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
			FilterControlBindingSource.SetBindingMember(control, ".");

			yield return control;
		}
	}
}
