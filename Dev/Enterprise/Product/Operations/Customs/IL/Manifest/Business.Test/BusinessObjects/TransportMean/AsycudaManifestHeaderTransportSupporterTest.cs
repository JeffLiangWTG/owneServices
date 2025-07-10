using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderTransportSupporterTest : BusinessObjectValidationTestCase
	{
		public void TestTransportValidatorType()
		{
			var asycudaManifestHeaderTransportSupporter = new AsycudaManifestHeaderTransportSupporter(asycudaManifestHeader);
			AssertType<TransportMeanRoadValidation>("TransportValidatorType", asycudaManifestHeaderTransportSupporter.GetNewTransportValidator(transportMean));
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertType<TransportMeanValidation>("TransportValidatorType", asycudaManifestHeaderTransportSupporter.GetNewTransportValidator(transportMean));
		}

		protected override void SetUp()
		{
			base.SetUp();

			asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			transportMean = (TransportMean)asycudaManifestHeader.TransportMeans.AddNew();
		}

		AsycudaManifestHeader asycudaManifestHeader;
		TransportMean transportMean;
	}
}
