using System.Windows.Forms;
using Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.Module.UniversalCopySchedule
{
	class UniversalCopyScheduleFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ZArchitecture.Business.ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is UniversalCopyScheduleModuleFilter)
			{
				var item = new UniversalCopyScheduleModuleControl();

				try
				{
					PreferredHeight = item.Height;

					return new Control[] { item };
				}
				catch
				{
					item.Dispose();
					throw;
				}
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
