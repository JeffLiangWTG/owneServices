using System;

namespace Enterprise.VisualBoards.Business
{
	[DescendantRefreshableFilter]
	public class BoardMeetingModeFilter : IBoardFilter, IUndoableFilter
	{
		public BoardMeetingModeFilter()
		{
			FilterName = Res.GetString("89fec4f8-a6f6-403a-b159-9d355b893e9e", "Board Meeting");
		}

		#region IBoardFilter Members

		public bool AllowMultiple
		{
			get { return false; }
		}

		public string FilterName { get; protected set; }

		public bool RequiresRedraw
		{
			get { return false; }
		}

		public bool RequiresRemoval { get; set; }

		public Action RestoreVisualStateAfterFilterRemovedAction { get; set; }

		#endregion

		#region IUndoableFilter Members

		public bool RequiresUndo { get; set; }

		#endregion

		#region Equals

		public override bool Equals(object obj)
		{
			return obj is BoardMeetingModeFilter;
		}

		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode();
		}

		#endregion
	}
}
