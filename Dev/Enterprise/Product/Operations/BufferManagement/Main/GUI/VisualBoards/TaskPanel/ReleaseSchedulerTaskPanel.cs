using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	class ReleaseSchedulerTaskPanel : TaskPanel
	{
		internal ReleaseSchedulerTaskPanel(CellContent cell, BMBoardSectionViewModel viewModel, KContextMenuStrip taskCardMenuStrip, BMComponentControl componentControl = null, BMBoardSection section = null)
			: base(cell, viewModel, taskCardMenuStrip, null, componentControl, section)
		{
		}

		protected override CellTasksControl ConstructCellTasksControl(IEnumerable<ICardContent> cards, BMBoardSectionViewModel viewModel)
		{
			return ReleaseSchedulerCellTasksControl.GetCellTasksControl(cell, cards, viewModel, SharedMenuStrip);
		}
	}
}
