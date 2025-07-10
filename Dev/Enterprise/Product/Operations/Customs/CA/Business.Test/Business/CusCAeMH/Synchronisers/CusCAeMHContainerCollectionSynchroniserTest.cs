using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHContainerCollectionSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHContainerCollectionSynchroniser()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew().JC_ContainerNum = "C1";
			consol.Containers.AddNew().JC_ContainerNum = "C2";
			consol.Containers.AddNew().JC_ContainerNum = "C3";

			var masterBill = Factory.New<CusCAeMHMaster>();
			masterBill.BP_ParentID = consol.PK;
			masterBill.BP_ParentTableCode = consol.Prefix;
			masterBill.Containers.AddNew().BQ_ContainerNumber = "C1";
			masterBill.Containers.AddNew().BQ_ContainerNumber = "C4";

			var synchroniser = new CusCAeMHContainerCollectionSynchroniser(masterBill.Consol, masterBill);
			synchroniser.Synchronise();

			AssertEquals(3, masterBill.Containers.Count);
			AssertNotNull(masterBill.Containers.FirstOrDefault(x => x.BQ_ContainerNumber == "C1"));
			AssertNotNull(masterBill.Containers.FirstOrDefault(x => x.BQ_ContainerNumber == "C2"));
			AssertNotNull(masterBill.Containers.FirstOrDefault(x => x.BQ_ContainerNumber == "C3"));
			AssertNull(masterBill.Containers.FirstOrDefault(x => x.IsNonContainerized));
			AssertNull(masterBill.Containers.FirstOrDefault(x => x.BQ_ContainerNumber == "C4"));

			var newElement = consol.Containers.AddNew();
			newElement.JC_ContainerNum = "C5";
			AssertEquals("one element added", 4, masterBill.Containers.Count);
			var destinationNewElement = masterBill.Containers.FirstOrDefault(container => container.BQ_ContainerNumber == "C5");
			AssertNotNull(destinationNewElement);

			newElement.JC_ContainerNum = "C6";
			AssertEquals("count not changed", 4, masterBill.Containers.Count);
			AssertEquals("container number changed", "C6", destinationNewElement.BQ_ContainerNumber);

			consol.Containers.RemoveAndDelete(newElement);
			AssertEquals("one removed", 3, masterBill.Containers.Count);
			AssertNull(masterBill.Containers.FirstOrDefault(container => container.IsNonContainerized));
			AssertNotNull(masterBill.Containers.FirstOrDefault(container => container.BQ_ContainerNumber == "C1"));
			AssertNotNull(masterBill.Containers.FirstOrDefault(container => container.BQ_ContainerNumber == "C2"));
			AssertNotNull(masterBill.Containers.FirstOrDefault(container => container.BQ_ContainerNumber == "C3"));
			AssertNull(masterBill.Containers.FirstOrDefault(container => container.BQ_ContainerNumber == "C4"));
			AssertNull(masterBill.Containers.FirstOrDefault(container => container.BQ_ContainerNumber == "C5"));
			AssertNull(masterBill.Containers.FirstOrDefault(container => container.BQ_ContainerNumber == "C6"));
		}
	}
}
