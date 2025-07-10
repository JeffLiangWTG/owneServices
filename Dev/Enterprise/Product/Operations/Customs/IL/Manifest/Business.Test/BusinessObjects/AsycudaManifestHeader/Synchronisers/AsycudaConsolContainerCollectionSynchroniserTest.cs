using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaConsolContainerCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestGetNewContainerSynchroniser()
		{
			var factory = Factory;
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "SEA";
			var sourceContainer = consol.Containers.AddNew();
			sourceContainer.JC_ContainerNum = "123";
			sourceContainer.JC_IsEmptyContainer = true;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			sourceContainer.PackLines.Add(packLine);

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "IL";
			var synchroniser = new AsycudaConsolContainerCollectionSynchroniserForTesting(consol, manifestHeader);

			var destinationContainer = manifestHeader.Containers.AddNew();
			destinationContainer.ACN_ContainerNumber = "123";
			destinationContainer.ACN_AMA_Manifest = ZGuid.Empty;

			var result = synchroniser.GetNewContainerSynchroniser_Expose(destinationContainer, sourceContainer);

			AssertNotNull("Should not be null.", result);
			AssertType<AsycudaContainerSynchroniser>(result);
		}
	}

	sealed class AsycudaConsolContainerCollectionSynchroniserForTesting : AsycudaConsolContainerCollectionSynchroniser
	{
		public AsycudaConsolContainerCollectionSynchroniserForTesting(ForwardingConsol source, ASYCUDA.Business.AsycudaManifestHeader destination) : base(source, destination)
		{
		}

		public ASYCUDA.Business.AsycudaContainerSynchroniser GetNewContainerSynchroniser_Expose(ASYCUDA.Business.AsycudaContainer cusContainer, ForwardingContainer sourceContainer) => base.GetNewContainerSynchroniser(cusContainer, sourceContainer);
	}
}
