using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class VolumnCalculatorWebServicMethodDependCultureTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestGetParameterDecimalValueDependCulture()
		{
			var testParameters = new VolumeCalculatorParameters();
			testParameters.DefaultVolume = "0";
			testParameters.Pieces = "66";
			testParameters.Length = "1,200";
			testParameters.Width = "0,800";
			testParameters.Height = "1,25";
			testParameters.DimUnit = Constants.Length.Metres;
			testParameters.VolumeUnit = Constants.Volume.CubicMetres;
			testParameters.VolumeControlID = "TestID";

			DummyHttpApplication dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			DummyWorkerRequest dummyWorkerRequest = dummyApplication.WorkerRequest;
			dummyWorkerRequest.SetUserLanguagesSeparatedByComma("nl-NL");

			var defaultVolume = decimal.Parse(testParameters.DefaultVolume, Shared.WebEnvShared.ClientCulture);
			var pieces = int.Parse(testParameters.Pieces, Shared.WebEnvShared.ClientCulture);
			var length = decimal.Parse(testParameters.Length, Shared.WebEnvShared.ClientCulture);
			var width = decimal.Parse(testParameters.Width, Shared.WebEnvShared.ClientCulture);
			var height = decimal.Parse(testParameters.Height, Shared.WebEnvShared.ClientCulture);
			decimal resultValue = FreightUtilities.CalculateVolume(defaultVolume, pieces, length,
				width, height, testParameters.DimUnit, testParameters.VolumeUnit, JobPackLinesSchema.JL_ActualVolume.Scale);

			AssertEquals(resultValue, new VolumeCalculatorWebServiceMethodForTest().Execute(testParameters));
		}
	}
}
