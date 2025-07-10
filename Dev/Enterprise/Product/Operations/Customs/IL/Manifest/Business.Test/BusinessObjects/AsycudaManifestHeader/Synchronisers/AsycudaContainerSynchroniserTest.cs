using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerSynchroniser))]
	sealed class AsycudaContainerSynchroniserTest : TestCaseWithFactory
	{
		public void TestACN_EmptyFullIndicator()
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
			Factory.Save();

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "IL";
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var destinationContainer = manifestHeader.Containers.AddNew();
			destinationContainer.ACN_ContainerNumber = "123";
			destinationContainer.ACN_AMA_Manifest = ZGuid.Empty;
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("When JC_IsEmptyContainer = true", "A", destinationContainer.ACN_EmptyFullIndicator);

			sourceContainer.JC_IsEmptyContainer = false;
			AssertEquals("When JC_IsEmptyContainer = false", "B", destinationContainer.ACN_EmptyFullIndicator);

			sourceContainer.JC_ContainerMode = "FCL";
			AssertEquals("Changing JC_ContainerMode do not change ACN_EmptyFullIndicator", "B", destinationContainer.ACN_EmptyFullIndicator);
		}

		public void TestSealUnloadingStates()
		{
			var factory = Factory;
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "SEA";
			var sourceContainer = consol.Containers.AddNew();
			sourceContainer.JC_ContainerNum = "123";
			sourceContainer.JC_IsEmptyContainer = true;
			sourceContainer.JC_IsSealOk = true;
			sourceContainer.JC_SealNum = "SEAL1";
			sourceContainer.JC_AdditionalSealNum = "SEAL2";
			sourceContainer.JC_Additional2SealNum = "SEAL3";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			sourceContainer.PackLines.Add(packLine);
			Factory.Save();

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "IL";
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var destinationContainer = manifestHeader.Containers.AddNew();
			destinationContainer.ACN_ContainerNumber = "123";
			destinationContainer.ACN_AMA_Manifest = ZGuid.Empty;
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Seal number should be synced", "SEAL1", destinationContainer.ACN_Seal1);
			AssertEquals("Seal unloading state should be synced", "DEC", destinationContainer.ACN_Seal1UnloadingState);
			AssertEquals("Seal number should be synced", "SEAL2", destinationContainer.ACN_Seal2);
			AssertEquals("Seal unloading state should be synced", "DEC", destinationContainer.ACN_Seal2UnloadingState);
			AssertEquals("Seal number should be synced", "SEAL3", destinationContainer.ACN_Seal3);
			AssertEquals("Seal unloading state should be synced", "DEC", destinationContainer.ACN_Seal3UnloadingState);

			sourceContainer.JC_IsSealOk = false;

			AssertEquals("Seal number should be synced", "SEAL1", destinationContainer.ACN_Seal1);
			AssertEquals("Seal unloading state should be synced", ZString.Empty, destinationContainer.ACN_Seal1UnloadingState);
			AssertEquals("Seal number should be synced", "SEAL2", destinationContainer.ACN_Seal2);
			AssertEquals("Seal unloading state should be synced", ZString.Empty, destinationContainer.ACN_Seal2UnloadingState);
			AssertEquals("Seal number should be synced", "SEAL3", destinationContainer.ACN_Seal3);
			AssertEquals("Seal unloading state should be synced", ZString.Empty, destinationContainer.ACN_Seal3UnloadingState);
		}
	}
}
