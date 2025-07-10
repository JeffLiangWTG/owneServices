using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChildListCodeDescriptionBoolCollection))]
	sealed class ChildListCodeDescriptionBoolCollectionTest : CodeDescriptionBoolCollectionAbstractTest<ChildListCodeDescriptionBoolCollection>
	{
		protected override ChildListCodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new ChildListCodeDescriptionBoolCollection();
		}
	}
}
