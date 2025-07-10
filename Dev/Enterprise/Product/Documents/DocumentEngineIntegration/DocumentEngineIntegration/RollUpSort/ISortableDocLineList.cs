using System.Collections;

namespace Enterprise.DocumentEngineIntegration.RollUpSort
{
	public interface ISortableDocLineList : IList
	{
		void Sort(IComparer comparer);
	}
}
