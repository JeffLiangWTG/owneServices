using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBillofLadingContainerCollection))]
	sealed class DocBillofLadingContainerCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocBillofLadingContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var freightContainer = Factory.NewWithValidTestData<CommonContainer>();
			DocContainer docContainer = DocContainer.New(freightContainer, Factory);
			return DocBillofLadingContainer.New(docContainer);
		}

		protected override DocBillofLadingContainerCollection GetCollectionToTest()
		{
			return new DocBillofLadingContainerCollection(Factory);
		}

		public void TestContainsContainerWithNumber()
		{
			CommonContainer freightContainer1 = Factory.NewWithValidTestData<CommonContainer>();
			CommonContainer freightContainer2 = Factory.NewWithValidTestData<CommonContainer>();
			freightContainer1.JC_ContainerNum = "CONT1";
			freightContainer2.JC_ContainerNum = "CONT2";

			DocContainer docContainer1 = DocContainer.New(freightContainer1, Factory);
			DocContainer docContainer2 = DocContainer.New(freightContainer2, Factory);

			DocBillofLadingContainerCollection containers = new DocBillofLadingContainerCollection(Factory);
			AssertEquals("Should not contain container 1", ZBool.False, containers.ContainsContainerWithNumber("CONT1"));
			AssertEquals("Should not contain container 2", ZBool.False, containers.ContainsContainerWithNumber("CONT2"));

			containers.Add(DocBillofLadingContainer.New(docContainer1));
			Assert("Should NOW contain container 1", containers.ContainsContainerWithNumber("CONT1"));
			AssertEquals("Should not contain container 2", ZBool.False, containers.ContainsContainerWithNumber("CONT2"));

			containers.Add(DocBillofLadingContainer.New(docContainer2));
			Assert("Should still contain container 1", containers.ContainsContainerWithNumber("CONT1"));
			Assert("Should NOW contain container 2", containers.ContainsContainerWithNumber("CONT2"));
		}
	}
}
