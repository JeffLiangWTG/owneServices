using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class LoadingUnloadingLocationProvider : ILoadingUnloadingLocation
	{
		public static LoadingUnloadingLocationProvider New(RefUNLOCO unloco) => unloco == null ? null : new (unloco);

		LoadingUnloadingLocationProvider(RefUNLOCO unloco)
		{
			this.unloco = unloco;
		}
		readonly RefUNLOCO unloco;

		public string Location => null;

		public string UNLOCODE => unloco.RL_Code;

		public string Country => unloco.RL_RN_NKCountryCode;
	}
}
