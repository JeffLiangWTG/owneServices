using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing
{
	class TransportDetailWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when declaration parameter is null", () => new TransportDetailWrapper(declaration: null));
		}

		public void TestVoyage()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "Vessel 1";
			declaration.JE_VoyageFlightNo = "Voyage 1";
			var wrapper = GetNewWrapper();
			AssertEquals($"When Transport mode is SEA, Voyage", "VESSEL 1", wrapper.Voyage);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "az123";
			wrapper = GetNewWrapper();
			AssertEquals($"When Transport mode is AIR, Voyage", "AZ123", wrapper.Voyage);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.JE_VesselName = "Rail";
			wrapper = GetNewWrapper();
			AssertEquals($"When Transport mode is \"Other transports (RAIL, ROA, ...)\", Voyage", "RAIL", wrapper.Voyage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		EU.Business.Documents.CertificateOfOrigin.ITransportDetail GetNewWrapper() => new TransportDetailWrapper(declaration);
	}
}
