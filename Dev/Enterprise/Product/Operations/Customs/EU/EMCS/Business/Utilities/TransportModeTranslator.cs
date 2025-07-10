using System.Collections.Generic;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class TransportModeTranslator : Customs.Business.TransportModeTranslator
	{
		protected override Dictionary<string, string> CargoWiseToWCO
		{
			get
			{
				var result = base.CargoWiseToWCO;
				result.Remove(Core.Constants.TransportModes.Unknown);
				return result;
			}
		}

		protected override Dictionary<string, string> WCOToCargoWise
		{
			get
			{
				var result = base.WCOToCargoWise;
				result.Remove(TransportModeCodeList.Codes.Unknown);
				return result;
			}
		}
	}
}
