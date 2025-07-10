using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class PlaceProvider : IPlace
	{
		readonly RefUNLOCO refUnLoco;

		public PlaceProvider(string locode, RefUNLOCO refUnLoco)
		{
			this.refUnLoco = refUnLoco;
			UnLocode = locode;
		}

		public string UnLocode { get; }

		public string Country => refUnLoco?.RL_RN_NKCountryCode;

		public string Location => refUnLoco?.RL_PortName;
	}
}
