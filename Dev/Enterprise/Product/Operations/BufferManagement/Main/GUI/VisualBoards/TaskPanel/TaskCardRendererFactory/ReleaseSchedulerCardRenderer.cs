using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	class ReleaseSchedulerCardRenderer : ITaskCardRenderer
	{
		public TaskCardControl ConstructTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache)
		{
			return new NonReorderingTaskCardControl(sharedMenuStrip, cardContent, viewModel, cell, canUseBitmapCache);
		}

		TaskCardControl ITaskCardRenderer.ConstructTaskCardBasedOnTemplate(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
		{
			return new NonReorderingTaskCardControl(sharedMenuStrip, cardContent, viewModel, cell, shouldEnableDragDrop, canUseBitmapCache, renderAsBitmapOnLoad, model, bitmaps);
		}

		void ITaskCardRenderer.OnRenderingCardsCompleted()
		{
			// Nothing necessary.
		}
	}
}
