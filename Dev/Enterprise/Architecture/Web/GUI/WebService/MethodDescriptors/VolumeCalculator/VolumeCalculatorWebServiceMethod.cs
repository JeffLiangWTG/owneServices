using System;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class VolumeCalculatorWebServiceMethod : WebServiceMethod<VolumeCalculatorParameters>
	{
		#region Overrides

		protected override void ExecuteCore(VolumeCalculatorParameters parameters, WebServiceResponse response)
		{
			decimal resultValue;
			resultValue = FreightUtilities.CalculateVolume(GetParameterDecimalValue(parameters.DefaultVolume),
														   (int)GetParameterDecimalValue(parameters.Pieces),
														   GetParameterDecimalValue(parameters.Length),
														   GetParameterDecimalValue(parameters.Width),
														   GetParameterDecimalValue(parameters.Height),
														   parameters.DimUnit,
														   parameters.VolumeUnit,
														   JobPackLinesSchema.JL_ActualVolume.Scale);
			if (resultValue != 0)
			{
				response.Add(new UpdateValueResponseToken(parameters.VolumeControlID, Core.Utilities.FormatNumber(resultValue, ZCalcEditCore.DefaultDecimals, Shared.WebEnvShared.ClientCulture)));
			}
		}

		protected override bool AddErrorMessageToResponse()
		{
			return false;
		}

		protected override string GetMethodName()
		{
			return "CalculateVolume";
		}

		#endregion
		#region Implementation

		decimal GetParameterDecimalValue(string stringValue)
		{
			try
			{
				return decimal.Parse(stringValue, Shared.WebEnvShared.ClientCulture); // try parse requires a NumberStyle and can still throw an argException.
			}
			catch (ArgumentNullException) { }
			catch (FormatException) { }
			catch (OverflowException) { }
			return 0;
		}

		#endregion

	}
}
