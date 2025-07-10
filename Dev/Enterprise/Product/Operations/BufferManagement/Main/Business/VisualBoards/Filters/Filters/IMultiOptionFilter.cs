using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public interface IMultiOptionFilter : IBoardFilter
	{
		object CurrentlySelectedOption { get; set; }
	}
}
