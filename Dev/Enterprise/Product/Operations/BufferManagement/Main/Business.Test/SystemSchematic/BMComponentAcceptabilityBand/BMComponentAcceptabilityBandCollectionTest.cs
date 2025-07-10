using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentAcceptabilityBandCollection))]
	class BMComponentAcceptabilityBandCollectionTest : ActiveBusinessObjectCollectionTestCase<BMComponentAcceptabilityBandCollection>
	{
		protected override BMComponentAcceptabilityBandCollection GetCollectionToTest()
		{
			return new BMComponentAcceptabilityBandCollection(Factory);
		}
	}
}
