using System.Collections.Generic;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class TransportModeTranslator : Customs.Business.TransportModeTranslator
	{
		protected override Dictionary<string, string> CargoWiseToWCO
		{
			get
			{
				var result = base.CargoWiseToWCO;
				result.Remove(Core.Constants.TransportModes.Other);
				result.Remove(Core.Constants.TransportModes.Unknown);
				result.Add(Core.Constants.TransportModes.OwnPropulsion, ModeOfTransportList.Codes._9_OwnPropulsion);
				return result;
			}
		}

		protected override Dictionary<string, string> WCOToCargoWise
		{
			get
			{
				var result = base.WCOToCargoWise;
				result.Remove(TransportModeCodeList.Codes.Other);
				result.Remove(TransportModeCodeList.Codes.Unknown);
				result.Add(ModeOfTransportList.Codes._9_OwnPropulsion, Core.Constants.TransportModes.OwnPropulsion);
				return result;
			}
		}
	}
}
