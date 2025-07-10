using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagDefinitionCollection))]
	class TagDefinitionCollectionTest : ActiveBusinessObjectCollectionTestCase<TagDefinitionCollection>
	{
		protected override TagDefinitionCollection GetCollectionToTest()
		{
			return new TagDefinitionCollection(Factory);
		}
	}
}
