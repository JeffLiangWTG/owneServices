using System;

namespace Enterprise.VisualBoards.Business
{
	public interface IBoardFilter
	{
		bool AllowMultiple { get; }
		string FilterName { get; }
		bool RequiresRedraw { get; }
		bool RequiresRemoval { get; set; }

		Action RestoreVisualStateAfterFilterRemovedAction { get; set; }
	}
}
