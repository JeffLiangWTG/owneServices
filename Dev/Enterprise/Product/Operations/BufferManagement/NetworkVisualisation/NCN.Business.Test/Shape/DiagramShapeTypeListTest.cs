using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class DiagramShapeTypeListTest : NetworkTestCase
	{
		public void TestList()
		{
			var list = new DiagramShapeTypeList();

			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(ShapeTypeList.Codes.DefaultDiagram));
			Assert(list.ContainsCode(ShapeTypeList.Codes.Diagram));
			Assert(list.ContainsCode(ShapeTypeList.Codes.Shape));
			Assert(list.ContainsCode(ShapeTypeList.Codes.Buffer));
		}
	}
}
