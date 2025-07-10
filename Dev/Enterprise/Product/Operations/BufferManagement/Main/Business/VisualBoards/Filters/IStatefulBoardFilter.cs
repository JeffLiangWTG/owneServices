using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public interface IStatefulBoardFilter : IBoardFilter
	{
		void Refresh();
	}
}
