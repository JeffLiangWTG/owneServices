using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationBorderTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationBorderTransportMeans>
	{
		public void TestNewOrNull()
		{
			AssertNull("When asycudaManifestHeader is null", DeclarationBorderTransportMeansWrapper.NewOrNull(null));
			AssertNotNull("When asycudaManifestHeader is not null", DeclarationBorderTransportMeansWrapper.NewOrNull(borderTransportMeans));
		}
		public void TestArrivalDateTime()
		{
			var wrapper = GetProvider();
			AssertEquals("Arrival Date should be as in JW_ETA ", "2025-01-28T12:55:36", wrapper.ArrivalDateTime);
		}

		public void TestDepartureDateTime()
		{
			var wrapper = GetProvider();
			AssertEquals("Departure Date should be as in JW_ETD ", "2025-01-27T12:47:36", wrapper.DepartureDateTime);
		}

		public void TestId()
		{
			var wrapper = GetProvider();
			AssertEquals("ID should be as in JW_Vessel", "FASTVESSEL", wrapper.Id.Value);
		}

		public void TestFirstArrivalLocationId()
		{
			var wrapper = GetProvider();
			AssertEquals("First Arrival Location ID should be as in JW_RL_NKDiscPort", "ILTLV", wrapper.FirstArrivalLocationId.Value);
		}

		public void TestJourneyId()
		{
			var wrapper = GetProvider();
			AssertNull("Journey ID", wrapper.JourneyId);
		}

		public void TestRegistrationNationalityId()
		{
			var wrapper = GetProvider();
			AssertEquals("Registration Nationality ID should be as in VehicleCountry", Core.Constants.CountryCodes.France, wrapper.RegistrationNationalityId.Value);
		}

		public void TestTypeCode()
		{
			var wrapper = Provider;
			AssertEquals("TypeCode should be as in TruckKind ", "1234", wrapper.TypeCode.Value);
		}

		public void TestItinerary()
		{
			var wrapper = Provider;
			AssertNotNull("Itinerary", wrapper.Itinerary);
			AssertEquals("There should be 1 item", 1, wrapper.Itinerary.Count);
		}

		public void TestName()
		{
			var wrapper = Provider;
			AssertNull("Name", wrapper.Name);
		}

		public void TestTransportMeansOperator()
		{
			var wrapper = Provider;
			AssertNull("TransportMeansOperator", wrapper.TransportMeansOperator);
		}

		protected override IDeclarationBorderTransportMeans GetProvider() => DeclarationBorderTransportMeansWrapper.NewOrNull(borderTransportMeans);

		protected override void SetUp()
		{
			base.SetUp();

			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			asycudaManifestHeader.AMA_ApplicationCode = "NVC";
			asycudaManifestHeader.AMA_ManifestType = "785";

			borderTransportMeans = (TransportMean)asycudaManifestHeader.TransportMeans.AddNew();
			borderTransportMeans.JW_ETA = new ZDateTime(2025,01,28,12,55,36);
			borderTransportMeans.JW_ATD = new ZDateTime(2025, 01, 27, 12, 47, 36);
			borderTransportMeans.JW_RL_NKDiscPort = "ILTLV";
			borderTransportMeans.JW_RL_NKLoadPort = "ILHFA";
			borderTransportMeans.JW_Vessel = "FASTVESSEL";
			borderTransportMeans.TruckKind = "1234";
			borderTransportMeans.VehicleCountry = Core.Constants.CountryCodes.France;
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction.Dispose();
		}

		IDisposable disposableAction;
		AsycudaManifestHeader asycudaManifestHeader;
		TransportMean borderTransportMeans;
	}
}
