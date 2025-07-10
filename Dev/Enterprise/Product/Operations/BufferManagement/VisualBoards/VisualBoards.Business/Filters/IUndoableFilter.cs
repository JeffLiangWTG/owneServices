namespace Enterprise.VisualBoards.Business
{
	public interface IUndoableFilter : IBoardFilter
	{
		bool RequiresUndo { get; set; }
	}
}
