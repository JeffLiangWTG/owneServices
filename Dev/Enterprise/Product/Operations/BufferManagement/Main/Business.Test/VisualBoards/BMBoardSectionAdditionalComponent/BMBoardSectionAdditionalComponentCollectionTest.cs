using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSectionAdditionalComponentCollection))]
	class BMBoardSectionAdditionalComponentCollectionTest : ActiveBusinessObjectCollectionTestCase<BMBoardSectionAdditionalComponentCollection>
	{
		protected override BMBoardSectionAdditionalComponentCollection GetCollectionToTest()
		{
			return new BMBoardSectionAdditionalComponentCollection(Factory.NewWithValidTestData<BMBoardSection>());
		}
	}
}
