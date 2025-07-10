using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	[TestedType(typeof(BoardFilterBusinessObject))]
	class BoardFilterBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BoardFilterBusinessObject(new TestFilter(), new DummyFilterable());
		}
	}
}
