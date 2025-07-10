using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMReleaseSequenceItemCollection))]
	class BMReleaseSequenceItemCollectionTest : ActiveBusinessObjectCollectionTestCase<BMReleaseSequenceItemCollection>
	{
		protected override BMReleaseSequenceItemCollection GetCollectionToTest()
		{
			return new BMReleaseSequenceItemCollection(Factory.New<BMReleaseSequence>());
		}
	}
}
