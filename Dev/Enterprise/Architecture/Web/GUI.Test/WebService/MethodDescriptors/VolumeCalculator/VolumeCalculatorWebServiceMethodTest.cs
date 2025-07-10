using System.Collections.Generic;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class VolumeCalculatorWebServiceMethodTest : WebServiceMethodTest<VolumeCalculatorWebServiceMethod>
	{
		#region Implementation

		protected override void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting)
		{
			VolumeCalculatorParameters testParameters = new VolumeCalculatorParameters();
			testParameters.DefaultVolume = "1";
			testParameters.Pieces = "1";
			testParameters.Length = "1";
			testParameters.Width = "1";
			testParameters.Height = "1";
			testParameters.DimUnit = Constants.Length.Metres;
			testParameters.VolumeUnit = Constants.Volume.CubicMetres;
			testParameters.VolumeControlID = "TestID";
			WebServiceResponse expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", Enterprise.ZArchitecture.Core.Utilities.FormatNumber(1, ZCalcEditCore.DefaultDecimals, Shared.WebEnvShared.ClientCulture)));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters = new VolumeCalculatorParameters();
			testParameters.DefaultVolume = "1.5";
			testParameters.Pieces = "0";
			testParameters.Length = "50";
			testParameters.Width = "50";
			testParameters.Height = "50";
			testParameters.DimUnit = Constants.Length.Centimetres;
			testParameters.VolumeUnit = Constants.Volume.CubicMetres;
			testParameters.VolumeControlID = "TestID";
			expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters = new VolumeCalculatorParameters();
			testParameters.DefaultVolume = "1.5";
			testParameters.Pieces = "10";
			testParameters.Length = "0.5";
			testParameters.Width = "0.5";
			testParameters.Height = "0.5";
			testParameters.DimUnit = Constants.Length.Metres;
			testParameters.VolumeUnit = Constants.Volume.CubicMetres;
			testParameters.VolumeControlID = "TestID";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", Enterprise.ZArchitecture.Core.Utilities.FormatNumber(1.25, ZCalcEditCore.DefaultDecimals, Shared.WebEnvShared.ClientCulture)));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters = new VolumeCalculatorParameters();
			testParameters.DefaultVolume = "1.5";
			testParameters.Pieces = "10";
			testParameters.Length = "50";
			testParameters.Width = "50";
			testParameters.Height = "50";
			testParameters.DimUnit = Constants.Length.Centimetres;
			testParameters.VolumeUnit = Constants.Volume.CubicMetres;
			testParameters.VolumeControlID = "TestID";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", Enterprise.ZArchitecture.Core.Utilities.FormatNumber(1.25, ZCalcEditCore.DefaultDecimals, Shared.WebEnvShared.ClientCulture)));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters = new VolumeCalculatorParameters();
			testParameters.DefaultVolume = "1.5";
			testParameters.Pieces = "10";
			testParameters.Length = "0.5";
			testParameters.Width = "0.5";
			testParameters.Height = "0.5";
			testParameters.DimUnit = Constants.Length.Metres;
			testParameters.VolumeUnit = Constants.Volume.CubicFeet;
			testParameters.VolumeControlID = "TestID";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", Enterprise.ZArchitecture.Core.Utilities.FormatNumber(44.14, ZCalcEditCore.DefaultDecimals, Shared.WebEnvShared.ClientCulture)));
			setting.Add(testParameters.ToString(), expectedResponse);

			testParameters = new VolumeCalculatorParameters();
			testParameters.DefaultVolume = "1.5";
			testParameters.Pieces = "10";
			testParameters.Length = "50";
			testParameters.Width = "50";
			testParameters.Height = "50";
			testParameters.DimUnit = "UNK";
			testParameters.VolumeUnit = "TST";
			testParameters.VolumeControlID = "TestID";
			expectedResponse = new WebServiceResponse();
			expectedResponse.Add(new UpdateValueResponseToken("TestID", Enterprise.ZArchitecture.Core.Utilities.FormatNumber(1.5, ZCalcEditCore.DefaultDecimals, Shared.WebEnvShared.ClientCulture)));
			setting.Add(testParameters.ToString(), expectedResponse);
		}

		protected override bool MethodNeverReturnsErrors
		{
			get { return true; }
		}

		protected override string GetExpectedMethodName()
		{
			return "CalculateVolume";
		}

		protected override VolumeCalculatorWebServiceMethod GetNewWebServiceMethod()
		{
			return new VolumeCalculatorWebServiceMethod();
		}

		#endregion
	}
}
