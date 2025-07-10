using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public interface ITaskCardRenderer
	{
		TaskCardControl ConstructTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache);

		TaskCardControl ConstructTaskCardBasedOnTemplate(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps);

		void OnRenderingCardsCompleted();
	}
}
