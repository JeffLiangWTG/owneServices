using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAHouseCollectionForOceanBill : ActiveBusinessObjectCollection<CusSCAHouse>, IBusinessObjectCollection
	{
		public CusSCAHouseCollectionForOceanBill(CusSCAOceanBill cusSCAOceanBill)
			: base(cusSCAOceanBill)
		{
		}

		protected override void SetDefaultsForNewElementCore(CusSCAHouse newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			using (newElement.GetValidationSuspender())
			{
				if (newElement.Consol is ForwardingConsol consol && consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.Canada) &&
					!((consol.DischargePort?.RL_RN_NKCountryCode ?? ZString.Empty) == Core.Constants.CountryCodes.Canada))
				{
					newElement.CA_FROBTransitImportCode = InTransitCodeList.Codes.FROB;
				}
				else if (Count > 0)
				{
					var previousLine = this[Count - 1];
					newElement.CA_FROBTransitImportCode = previousLine.CA_FROBTransitImportCode;
				}

				newElement.IsSettingDefaults = false;
			}
		}
	}
}
