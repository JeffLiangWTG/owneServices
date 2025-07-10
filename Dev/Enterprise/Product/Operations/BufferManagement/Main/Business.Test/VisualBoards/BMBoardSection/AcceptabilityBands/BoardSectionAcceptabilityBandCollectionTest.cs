using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BoardSectionAcceptabilityBandCollection))]
	class BoardSectionAcceptabilityBandCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BoardSectionAcceptabilityBandCollection>
	{
		protected override BoardSectionAcceptabilityBandCollection GetCollectionToTest()
		{
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory)));
			return new BoardSectionAcceptabilityBandCollection(section);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory)));
			return new BoardSectionAcceptabilityBand(section);
		}
	}
}
