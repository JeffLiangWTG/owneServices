using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	class NonReorderingTaskCardControl : StackedTaskCardControl
	{
		internal NonReorderingTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache)
			: base(sharedMenuStrip, cardContent, viewModel, cell, shouldEnableDragDrop: false, canUseBitmapCache: canUseBitmapCache)
		{
		}

		public NonReorderingTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
			: base(sharedMenuStrip, cardContent, viewModel, cell, shouldEnableDragDrop, canUseBitmapCache, renderAsBitmapOnLoad, model, bitmaps)
		{
		}
	}
}
