using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAContainerCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestCusSCAContainerCollectionSynchroniser()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			var destinationOceanBill = Factory.New<CusSCAOceanBill>();
			sourceConsol.Containers.AddNew().JC_ContainerNum = "C1";
			sourceConsol.Containers.AddNew().JC_ContainerNum = "C2";
			sourceConsol.Containers.AddNew().JC_ContainerNum = "C3";
			destinationOceanBill.Containers.AddNew().CN_ContainerNumber = "C1";
			destinationOceanBill.Containers.AddNew().CN_ContainerNumber = "C4";
			var synchnroniser = new CusSCAContainerCollectionSynchroniser(sourceConsol, destinationOceanBill);
			synchnroniser.Synchronise();

			AssertEquals("Destination should match source after synhcronise, no default non-container", 3, destinationOceanBill.Containers.Count);
			AssertNotNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C1"));
			AssertNotNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C2"));
			AssertNotNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C3"));
			AssertNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C4"));

			var newElement = sourceConsol.Containers.AddNew();
			newElement.JC_ContainerNum = "C5";
			AssertEquals("one element added", 4, destinationOceanBill.Containers.Count);
			var destinationNewElement = destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C5");
			AssertNotNull(destinationNewElement);

			newElement.JC_ContainerNum = "C6";
			AssertEquals("count not changed", 4, destinationOceanBill.Containers.Count);
			AssertEquals("container number changed", "C6", destinationNewElement.CN_ContainerNumber);

			sourceConsol.Containers.RemoveAndDelete(newElement);
			AssertEquals("one removed", 3, destinationOceanBill.Containers.Count);
			AssertNotNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C1"));
			AssertNotNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C2"));
			AssertNotNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C3"));
			AssertNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C4"));
			AssertNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C5"));
			AssertNull(destinationOceanBill.Containers.FirstOrDefault(container => container.CN_ContainerNumber == "C6"));
		}
	}
}
