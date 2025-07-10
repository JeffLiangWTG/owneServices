using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystemCollection))]
	class BMSystemCollectionTest : ActiveBusinessObjectCollectionTestCase<BMSystemCollection>
	{
		protected override BMSystemCollection GetCollectionToTest()
		{
			return new BMSystemCollection(Factory);
		}
	}
}
