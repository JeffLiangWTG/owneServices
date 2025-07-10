using System.Collections.Generic;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business
{
	public class TransportModeTranslator : EU.Business.TransportModeTranslator
	{
		protected override Dictionary<string, string> CargoWiseToWCO
		{
			get
			{
				var result = base.CargoWiseToWCO;
				result.Add(GBTransportTypeList.Codes.ROR, GBModeOfTransportList.Codes._6_RoRoFreight);
				return result;
			}
		}

		protected override Dictionary<string, string> WCOToCargoWise
		{
			get
			{
				var result = base.WCOToCargoWise;
				result.Add(GBModeOfTransportList.Codes._6_RoRoFreight, GBTransportTypeList.Codes.ROR);
				return result;
			}
		}
	}
}
