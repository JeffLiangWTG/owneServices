using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class StackedCardRenderer : ITaskCardRenderer
	{
		public TaskCardControl ConstructTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache)
		{
			return new StackedTaskCardControl(sharedMenuStrip, cardContent, viewModel, cell, canUseBitmapCache, shouldEnableDragDrop: true);
		}

		TaskCardControl ITaskCardRenderer.ConstructTaskCardBasedOnTemplate(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
		{
			return new StackedTaskCardControl(sharedMenuStrip, cardContent, viewModel, cell, shouldEnableDragDrop, canUseBitmapCache, renderAsBitmapOnLoad, model, bitmaps);
		}

		void ITaskCardRenderer.OnRenderingCardsCompleted()
		{
			// Nothing necessary.
		}
	}
}
