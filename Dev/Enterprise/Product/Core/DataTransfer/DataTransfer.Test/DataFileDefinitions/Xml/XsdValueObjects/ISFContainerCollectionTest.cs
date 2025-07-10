using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ISFContainerCollection))]
	sealed class ISFContainerCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestGetISFContainerFromCollection()
		{
			Xsd.ISFContainerCollection collection = new Xsd.ISFContainerCollection();

			AssertNull(collection.GetContainerFromCollection(""));
			AssertNull(collection.GetContainerFromCollection("CONT1"));

			Xsd.ISFContainer container1 = collection.AddNew();
			container1.ContainerNumber = "CONT";

			AssertNull(collection.GetContainerFromCollection("CONT1"));

			Xsd.ISFContainer container2 = collection.AddNew();
			container2.ContainerNumber = "CONT1";

			AssertEquals(container2, collection.GetContainerFromCollection("CONT1"));
		}
	}
}
