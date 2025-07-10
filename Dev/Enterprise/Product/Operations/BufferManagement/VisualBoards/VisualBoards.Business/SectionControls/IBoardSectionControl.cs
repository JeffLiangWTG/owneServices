using System;

namespace Enterprise.VisualBoards.Business
{
	public interface IBoardSectionControl
	{
		string SectionType { get; }

		/// <summary>
		/// Called during timed board refresh when the content of the board section should be refreshed.
		/// </summary>
		void Refresh(BoardRefreshEventArgs args);

		/// <summary>
		/// Called when a control is dragged onto this board section from another section.
		/// </summary>
		/// <returns>True when the control can be accepted, or False when it should return to its previous location.</returns>
		bool AcceptDraggedControl(object control);

		/// <summary>
		/// Determines whether this section control should prevent the refresh of the Visual Board.
		/// </summary>
		bool SuppressBoardRefresh(BoardRefreshEventArgs args);

		/// <summary>
		/// Fire this event after refresh has occured to inform the board that your refresh has completed.
		/// </summary>
		event EventHandler<BoardRefreshEventArgs> RefreshCompleted;

		BoardSectionViewModel SectionViewModel { get; }
	}
}
