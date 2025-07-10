using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMReleaseSequenceCollection))]
	class BMReleaseSequenceCollectionTest : ActiveBusinessObjectCollectionTestCase<BMReleaseSequenceCollection>
	{
		protected override BMReleaseSequenceCollection GetCollectionToTest()
		{
			return new BMReleaseSequenceCollection(Factory, new ZQuery());
		}
	}
}
