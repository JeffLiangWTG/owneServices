using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardCollection))]
	public class BMBoardCollectionTest : ActiveBusinessObjectCollectionTestCase<BMBoardCollection>
	{
		public void TestRelationship()
		{
			var system = Factory.New<BMSystem>();
			var collection = new BMBoardCollection(system, new ZQuery());
			var board = Factory.New<BMBoard>();
			collection.Add(board);
			AssertEquals(system, board.System);
		}
	}
}
