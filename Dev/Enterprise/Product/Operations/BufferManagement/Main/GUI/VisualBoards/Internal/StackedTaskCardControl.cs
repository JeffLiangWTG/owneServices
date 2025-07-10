using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class StackedTaskCardControl : TaskCardControl
	{
		internal StackedTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache, bool shouldEnableDragDrop)
			: base(sharedMenuStrip, cardContent, viewModel, cell, shouldEnableDragDrop: shouldEnableDragDrop, canUseBitmapCache: canUseBitmapCache)
		{
		}

		public StackedTaskCardControl(KContextMenuStrip sharedMenuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
			: base(sharedMenuStrip, cardContent, viewModel, cell, canUseBitmapCache, shouldEnableDragDrop, renderAsBitmapOnLoad, model, bitmaps)
		{
		}

		protected override bool IsFrontMostCard => true; // All cards are top-level
	}
}
