using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	public abstract class FilterApplicatorTestCase<T> : BMSGUITestCase
		where T : IBoardFilter
	{
		public abstract void TestIsApplicable();
		public abstract void TestApply_DbHits();
	}
}
