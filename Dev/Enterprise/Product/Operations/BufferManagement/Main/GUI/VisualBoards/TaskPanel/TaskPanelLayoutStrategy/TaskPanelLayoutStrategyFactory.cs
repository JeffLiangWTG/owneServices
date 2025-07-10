using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	static class TaskPanelLayoutStrategyFactory
	{
		public static TaskPanelLayoutStrategy GetStrategy(BMBoardSectionViewModel viewModel)
		{
			switch (viewModel.SectionPanelLayoutStyle)
			{
				case PanelLayoutTypeList.Codes.Stacked:
					return new StackedTaskCardLayoutStrategy();

				case PanelLayoutTypeList.Codes.Staggered:
				default:
					return new StaggeredTaskCardLayoutStrategy();
			}
		}
	}
}
