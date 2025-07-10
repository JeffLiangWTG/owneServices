using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSectionCollection))]
	public class BMBoardSectionCollectionTest : ActiveBusinessObjectCollectionTestCase<BMBoardSectionCollection>
	{
		public void TestRelationship()
		{
			var board = Factory.New<BMBoard>();
			var collection = new BMBoardSectionCollection(board);
			var section = Factory.New<BMBoardSection>();
			collection.Add(section);
			AssertEquals(board, section.Board);
		}
	}
}
