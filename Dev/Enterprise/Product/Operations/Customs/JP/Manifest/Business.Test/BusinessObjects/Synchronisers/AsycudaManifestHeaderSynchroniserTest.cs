using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
	sealed class AsycudaManifestHeaderSynchroniserTest : SynchroniserTestCase
	{
		public void TestSyncMoveInDestination()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var containerYard = Factory.New<OrgHeader>();
			containerYard.OH_FullName = "ContainerYard";

			containerYard.OH_RL_NKClosestPort = "JPTYO";
			containerYard.MainAddress.OA_RN_NKCountryCode = "JP";

			var mainAddress = containerYard.MainAddress;
			mainAddress.CustomsCodes.AddNew(CodeTypes.ControlledPremisesID, "99999", CountryCodes.Japan);

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ContainerYardEmptyPickupAddress = containerYard.MainAddress.PK;
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Japan;

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Should sync Move-In Destination", "99999", manifestHeader.AMA_RL_NKPortOfFinalDeparture);
		}

		public void TestSyncCarrier()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Japan;

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("should sync carrier", carrier.MainAddress.PK, manifestHeader.AMA_OA_Carrier);
		}

		public void TestCustomsOfficeSynchroniser()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Courier;
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("", manifestHeader.AMA_AgentType);
			AssertEquals(Core.Constants.AgentType.Courier, consol.JK_AgentType);
		}

		public void TestSyncBookingNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			number1.CE_EntryNum = "AMS12345";

			var number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = CusEntryNumberTypes.JP.BookingNumber;
			number2.CE_EntryNum = "BGK12345";

			var number3 = consol.Numbers.AddNew();
			number3.CE_EntryType = CusEntryNumberTypes.JP.BookingNumber;
			number3.CE_EntryNum = "BGK67890";
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("BGK12345", manifestHeader.AMA_BookingNumber);
		}

		public void TestGetNewConsolContainerCollectionSynchroniser()
		{
			var factory = Factory;
			var consol = factory.New<ForwardingConsol>();
			var header = factory.New<AsycudaManifestHeader>();
			header.SetParent(consol);

			var synchroniser = new AsycudaManifestHeaderSynchroniserForTesting(header, consol);
			var result = synchroniser.GetNewConsolContainerCollectionSynchroniser_Expose;
			AssertType<AsycudaConsolContainerCollectionSynchroniser>(result);
		}
	}

	class AsycudaManifestHeaderSynchroniserForTesting : AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniserForTesting(AsycudaManifestHeader destination, ForwardingConsol sourceConsol) : base(destination, sourceConsol)
		{
		}

		public BusinessObjectCollectionSynchroniser GetNewConsolContainerCollectionSynchroniser_Expose => this.GetNewConsolContainerCollectionSynchroniser();
	}
}
