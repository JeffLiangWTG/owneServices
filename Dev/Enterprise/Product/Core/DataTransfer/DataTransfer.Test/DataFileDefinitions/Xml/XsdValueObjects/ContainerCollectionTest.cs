using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ContainerCollection))]
	sealed class ContainerCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestGetContainerFromCollection()
		{
			Xsd.ContainerCollection collection = new Xsd.ContainerCollection();

			AssertNull(collection.GetContainerFromCollection(""));
			AssertNull(collection.GetContainerFromCollection("CONT1"));

			Xsd.Container container1 = collection.AddNew();
			container1.ContainerNumber = "CONT";

			AssertNull(collection.GetContainerFromCollection("CONT1"));

			Xsd.Container container2 = collection.AddNew();
			container2.ContainerNumber = "CONT1";

			AssertEquals(container2, collection.GetContainerFromCollection("CONT1"));
		}

		public void TestCompileTimeCheck()
		{
			Xsd.ContainerCollection value = null;
			value = new Xsd.ConsolConsolDetail().Containers;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
