using System;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public static class TaskCardRendererFactory
	{
		public static ITaskCardRenderer GetRenderer(BMBoardSectionViewModel viewModel, bool isReleaseScheduler = false, Func<ITaskCardRenderer, ITaskCardRenderer> cachingRendererBuilder = null)
		{
#if DEBUG
			if (taskCardRenderer_Override.IsOverriden)
			{
				return taskCardRenderer_Override.Value;
			}
#endif

			if (isReleaseScheduler)
			{
				return new ReleaseSchedulerCardRenderer();
			}

			ITaskCardRenderer renderer;

			switch (viewModel.SectionPanelLayoutStyle)
			{
				case PanelLayoutTypeList.Codes.Stacked:
					renderer = new StackedCardRenderer();
					break;

				case PanelLayoutTypeList.Codes.Staggered:
				default:
					renderer = new StaggeredCardRenderer();
					break;
			}

			if (cachingRendererBuilder == null)
			{
				return renderer;
			}
			else
			{
				return cachingRendererBuilder(renderer);
			}
		}

#if DEBUG

		public static IDisposable OverrideRenderer(ITaskCardRenderer renderer)
		{
			taskCardRenderer_Override.Value = renderer;

			return new DisposableAction(() => taskCardRenderer_Override.ResetValue());
		}

		static readonly LazyOverridable<ITaskCardRenderer> taskCardRenderer_Override = new LazyOverridable<ITaskCardRenderer>(() => null);

#endif
	}
}
