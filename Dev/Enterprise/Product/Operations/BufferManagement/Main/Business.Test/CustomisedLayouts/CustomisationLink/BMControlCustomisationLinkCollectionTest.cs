using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisationLinkCollection))]
	class BMControlCustomisationLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<BMControlCustomisationLinkCollection>
	{
		public void TestCollection()
		{
			var layout = BMSTestHelper.CreateControlCustomisation(Factory);
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			var board = BMSTestHelper.CreateBoard(system);
			var section1 = BMSTestHelper.CreateBoardSection(bucket, board);
			var section2 = BMSTestHelper.CreateBoardSection(bucket, board);

			var link1 = BMSTestHelper.CreateControlCustomisationLink(Factory, system, layout);
			var link2 = BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, layout);
			var link3 = BMSTestHelper.CreateControlCustomisationLink(Factory, board, layout);
			var link4 = BMSTestHelper.CreateControlCustomisationLink(Factory, section1, layout);
			var link5 = BMSTestHelper.CreateControlCustomisationLink(Factory, section2, layout);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { link1 }, system.CustomisedLayoutLinks);
			AssertContainsExactElementsInAnyOrder(new[] { link2 }, releaseGroup.CustomisedLayoutLinks);
			AssertContainsExactElementsInAnyOrder(new[] { link3 }, board.CustomisedLayoutLinks);
			AssertContainsExactElementsInAnyOrder(new[] { link4 }, section1.SectionConfiguration.CustomisedLayoutLinks);
			AssertContainsExactElementsInAnyOrder(new[] { link5 }, section2.SectionConfiguration.CustomisedLayoutLinks);
		}

		#region Implementation

		protected override BMControlCustomisationLinkCollection GetCollectionToTest()
		{
			return new BMControlCustomisationLinkCollection(BMSTestHelper.CreateSystem(Factory));
		}

		#endregion
	}
}
