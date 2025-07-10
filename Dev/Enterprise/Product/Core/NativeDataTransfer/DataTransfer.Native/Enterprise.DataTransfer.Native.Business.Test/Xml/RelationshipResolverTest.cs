using Enterprise.DataTransfer.Native.Common;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xml.Testing
{
	public class RelationshipResolverTest : TestCase
	{
		public void TestGetAttributes()
		{
			var iPropertyDef = new Mock<IPropertyDef>();
			var property = new Property(iPropertyDef.Object) { Value = "AUSYD" };
			var attribute = RelationshipResolver.CreateXAttribute(property, RelationshipResolver.LocationRelationshipResolver);

			AssertEquals("Relationship", attribute.Name.LocalName);
			AssertEquals("PTC", attribute.Value);
		}

		public void TestGetAttributeValue()
		{
			var iPropertyDef = new Mock<IPropertyDef>();
			var property = new Property(iPropertyDef.Object) { Value = "AAAA" };
			property.AddAttributeValue("Relationship", "IZN");
			var attributeValue = RelationshipResolver.GetRelationshipValue(property);

			AssertEquals("IZN", attributeValue);
		}
	}
}
