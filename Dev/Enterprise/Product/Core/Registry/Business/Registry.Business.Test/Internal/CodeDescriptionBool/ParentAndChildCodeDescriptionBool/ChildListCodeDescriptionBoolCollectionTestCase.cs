using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChildListCodeDescriptionBoolCollection))]
	sealed class ChildListCodeDescriptionBoolCollectionTestCase : CodeDescriptionBoolCollectionAbstractTest<ChildListCodeDescriptionBoolCollection>
	{
		protected override ChildListCodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new ChildListCodeDescriptionBoolCollection();
		}
	}
}
