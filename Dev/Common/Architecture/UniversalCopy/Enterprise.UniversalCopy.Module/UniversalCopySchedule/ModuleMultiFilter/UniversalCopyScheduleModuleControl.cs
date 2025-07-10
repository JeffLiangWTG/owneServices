using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter
{
	public partial class UniversalCopyScheduleModuleControl : ZUserControl
	{
		public UniversalCopyScheduleModuleControl()
		{
			InitializeComponent();

			modulesPairListDropEdit.LastSelectedItemChanged += new EventHandler(ModulesPairListDropEdit_LastSelectedItemChanged);
		}

		void ModulesPairListDropEdit_LastSelectedItemChanged(object sender, EventArgs e)
		{
			if (objectDescirptionEditText.ReadOnly)
			{
				objectDescirptionEditText.Text = ZString.Empty;
			}
		}
	}
}
