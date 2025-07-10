using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ShapeAffinityLinkCollection))]
	class ShapeAffinityLinkCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShapeAffinityLinkCollection>
	{
		protected override ShapeAffinityLinkCollection GetCollectionToTest()
		{
			return new ShapeAffinityLinkCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShapeAffinityLink(Factory);
		}
	}
}
