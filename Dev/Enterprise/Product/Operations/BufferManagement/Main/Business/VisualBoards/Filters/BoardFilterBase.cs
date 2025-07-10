using System;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public abstract class BoardFilterBase : IBoardFilter
	{
		public abstract bool AllowMultiple { get; }
		public abstract bool RequiresRedraw { get; }
		public abstract string FilterName { get; }
		public abstract bool RequiresRemoval { get; set; }
		public Action RestoreVisualStateAfterFilterRemovedAction { get; set; }
	}
}
