using System.Linq;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ClassificationAttributeCollection))]
	sealed class ClassificationAttributeCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestGetAttributesByType()
		{
			var collection = new ClassificationAttributeCollection();
			var attribute1 = collection.AddNew();
			attribute1.Type = ClassificationAttributeType.AT2;

			var attribute2 = collection.AddNew();
			attribute2.Type = ClassificationAttributeType.AT3;

			var attribute3 = collection.AddNew();
			attribute3.Type = ClassificationAttributeType.AT1;

			var attribute4 = collection.AddNew();
			attribute4.Type = ClassificationAttributeType.AT1;

			Assert(collection.GetAttributesByType(ClassificationAttributeType.AT1).First(x => x == attribute3) != null);
			Assert(collection.GetAttributesByType(ClassificationAttributeType.AT1).First(x => x == attribute4) != null);
			Assert(collection.GetAttributesByType(ClassificationAttributeType.AT2).First(x => x == attribute1) != null);
			Assert(collection.GetAttributesByType(ClassificationAttributeType.AT3).First(x => x == attribute2) != null);
		}
	}
}
