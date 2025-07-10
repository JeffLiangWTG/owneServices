using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class DUAExportBorderTransportInfoWrapperTest : WrapperHelperTest<DUAExportBorderTransportInfoWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null Job Declaration", () => new DUAExportBorderTransportInfoWrapper(null));
		}

		public void TestTransportMode()
		{
			declaration.JE_TransportMode = BorderTransportAir.Mode;
			AssertEquals("Expected filled TransportMode Air (4)", BorderTransportAir.ModeCoded, wrapper.TransportMode);
		}

		public void TestTransportId_Air()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = BorderTransportAir.Mode;
				AssertEquals("Expected empty TransportId (Air)", ZString.Empty, wrapper.TransportId);
				declaration.JE_VoyageFlightNo = BorderTransportAir.Name;
				AssertEquals("Expected filled TransportId (Air)", BorderTransportAir.Name, wrapper.TransportId);
			});
		}

		public void TestTransportId_NonAir()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = BorderTransportRoad.Mode;
				AssertEquals("Expected empty TransportId (Road)", ZString.Empty, wrapper.TransportId);
				declaration.JE_VesselName = BorderTransportRoad.Name;
				AssertEquals("Expected filled TransportId (Road)", BorderTransportRoad.Name, wrapper.TransportId);
			});
		}

		public void TestTransportNationality()
		{
			declaration.JE_RN_NKTransportNationality = BorderTransportAir.Nationality;
			AssertEquals("Expected filled TransportNationality Air (4)", BorderTransportAir.Nationality, wrapper.TransportNationality);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			wrapper = new DUAExportBorderTransportInfoWrapper(declaration);
		}
		JobDeclaration declaration;
		DUAExportBorderTransportInfoWrapper wrapper;

		protected override DUAExportBorderTransportInfoWrapper GetProvider() => wrapper;
	}
}
