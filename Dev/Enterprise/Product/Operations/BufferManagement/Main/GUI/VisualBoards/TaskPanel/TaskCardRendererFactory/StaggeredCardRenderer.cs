using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class StaggeredCardRenderer : ITaskCardRenderer
	{
		public TaskCardControl ConstructTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache)
		{
			return new TaskCardControl(sharedMenuStrip, cardContent, viewModel, cell, shouldEnableDragDrop: true, canUseBitmapCache: canUseBitmapCache);
		}

		TaskCardControl ITaskCardRenderer.ConstructTaskCardBasedOnTemplate(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
		{
			return new TaskCardControl(sharedMenuStrip, cardContent, viewModel, cell, shouldEnableDragDrop, canUseBitmapCache, renderAsBitmapOnLoad, model, bitmaps);
		}

		void ITaskCardRenderer.OnRenderingCardsCompleted()
		{
			// Nothing necessary.
		}
	}
}
