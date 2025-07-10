using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerSynchroniser))]
	sealed class AsycudaContainerSynchroniserTest : TestCaseWithFactory
	{
		public void TestContainerSynchroniser()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var address1 = orgHeader.Addresses.AddNew();
			address1.OA_Address1 = "Address1";
			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_Address1 = "Address2";
			mainAddress.CustomsCodes.AddNew(CodeTypes.ControlledPremisesID, "1234", CountryCodes.Japan);
			address1.CustomsCodes.AddNew(CodeTypes.ControlledPremisesID, "654321", CountryCodes.Japan);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "SEA";
			consol.JK_PackDepotDispatchRequested = new ZDateTime(2021, 1, 1, 1, 2, 3);
			consol.JK_OA_PackDepotAddress = mainAddress.PK;
			var sourceContainer = consol.Containers.AddNew();
			sourceContainer.JC_ContainerNum = "123";
			sourceContainer.JC_TareWeight = 10m;
			sourceContainer.JC_GrossWeightUQ = Weight.Kilograms;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			sourceContainer.PackLines.Add(packLine);
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "JP";
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);

			var destinationContainer = manifestHeader.Containers.AddNew();
			destinationContainer.ACN_ContainerNumber = "123";
			destinationContainer.ACN_AMA_Manifest = ZGuid.Empty;
			manifestHeader.Synchroniser.Synchronise();

			CombineAssertions("When JC_TareWeight = 10m, JC_GrossWeightUQ = 'KG', JK_PackDepotDispatchRequested = 2021/1/1 01:02:03", () =>
			{
				AssertEquals("CustomsTareWeight", 10m, destinationContainer.CustomsTareWeight);
				AssertEquals("CustomsWeightUQ", Weight.Kilograms, destinationContainer.CustomsWeightUQ);
				AssertEquals("ACN_MoveOutDate", new ZDateTime(2021, 1, 1, 1, 2, 3), destinationContainer.ACN_MoveOutDate);
				AssertEquals("VanningLocationCode", "1234", destinationContainer.VanningLocationCode);
			});

			sourceContainer.JC_GrossWeightUQ = Weight.Pounds;
			consol.JK_PackDepotDispatchRequested = new ZDateTime(2021, 1, 1, 1, 2, 4);
			consol.JK_OA_PackDepotAddress = address1.PK;
			CombineAssertions("When JC_TareWeight = 10m, JC_GrossWeightUQ = 'LB', JK_PackDepotDispatchRequested = 2021/1/1 01:02:04", () =>
			{
				AssertEquals("CustomsTareWeight", 10m, destinationContainer.CustomsTareWeight);
				AssertEquals("CustomsWeightUQ", Weight.Pounds, destinationContainer.CustomsWeightUQ);
				AssertEquals("ACN_MoveOutDate", new ZDateTime(2021, 1, 1, 1, 2, 4), destinationContainer.ACN_MoveOutDate);
				AssertEquals("VanningLocationCode", "65432", destinationContainer.VanningLocationCode);
			});

			sourceContainer.JC_GrossWeightUQ = Weight.Tonnes;
			consol.JK_OA_PackDepotAddress = address2.PK;
			CombineAssertions("When JC_TareWeight = 10m, JC_GrossWeightUQ = 'T'", () =>
			{
				AssertEquals("CustomsTareWeight", 10m, destinationContainer.CustomsTareWeight);
				AssertEquals("CustomsWeightUQ", Weight.Tonnes, destinationContainer.CustomsWeightUQ);
				AssertEquals("VanningLocationCode", "1234", destinationContainer.VanningLocationCode);
			});

			sourceContainer.JC_GrossWeightUQ = Weight.Grams;
			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			CombineAssertions("When JC_TareWeight = 10m, JC_GrossWeightUQ = 'G'", () =>
			{
				AssertEquals("CustomsTareWeight", 0.01m, destinationContainer.CustomsTareWeight);
				AssertEquals("CustomsWeightUQ", Weight.Kilograms, destinationContainer.CustomsWeightUQ);
				AssertEquals("VanningLocationCode", ZString.Empty, destinationContainer.VanningLocationCode);
			});
		}
	}
}
