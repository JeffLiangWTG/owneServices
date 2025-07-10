using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	class TransportDetailWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when declaration parameter is null", () => new TransportDetailWrapper(declaration: null));
		}

		[ExpectNoExceptions]
		public void TestVoyage()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "Vessel 1";
			declaration.JE_VoyageFlightNo = "Voyage 1";
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.Voyage, NUnit.Framework.Is.EqualTo("VESSEL 1 VOYAGE 1").Using(CustomComparers.TypeComparison), $"When Transport mode is SEA, Voyage");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "az123";
			wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.Voyage, NUnit.Framework.Is.EqualTo("AZ123").Using(CustomComparers.TypeComparison), $"When Transport mode is AIR, Voyage");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.JE_VesselName = "Rail";
			wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.Voyage, NUnit.Framework.Is.EqualTo("RAIL").Using(CustomComparers.TypeComparison), $"When Transport mode is \"Other transports (RAIL, ROA, ...)\", Voyage");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		ITransportDetail GetNewWrapper() => new TransportDetailWrapper(declaration);
	}
}
