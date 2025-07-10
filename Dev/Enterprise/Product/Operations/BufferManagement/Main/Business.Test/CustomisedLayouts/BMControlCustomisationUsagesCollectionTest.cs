using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisationUsagesCollection))]
	class BMControlCustomisationUsagesCollectionTest : ActiveBusinessObjectCollectionTestCase<BMControlCustomisationUsagesCollection>
	{
		public void TestCollectionActuallyWorks()
		{
			var customisation1 = BMSTestHelper.CreateControlCustomisation(Factory);
			var customisation2 = BMSTestHelper.CreateControlCustomisation(Factory);

			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system);
			var board2 = BMSTestHelper.CreateBoard(system);

			var link1 = BMSTestHelper.CreateControlCustomisationLink(Factory, board1, customisation1);
			var link2 = BMSTestHelper.CreateControlCustomisationLink(Factory, board2, customisation1);
			var link3 = BMSTestHelper.CreateControlCustomisationLink(Factory, board2, customisation2);

			AssertContainsExactElementsInAnyOrder(new[] { link1, link2 }, new BMControlCustomisationUsagesCollection(customisation1));
			AssertContainsExactElementsInAnyOrder(new[] { link3 }, new BMControlCustomisationUsagesCollection(customisation2));
		}

		#region Implementation

		protected override BMControlCustomisationUsagesCollection GetCollectionToTest()
		{
			return new BMControlCustomisationUsagesCollection(Factory.NewWithValidTestData<BMControlCustomisation>());
		}

		#endregion
	}
}
