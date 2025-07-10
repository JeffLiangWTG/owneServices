
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentTariffGroupCollection : BusinessObjectCollection<CMRInstrumentTariffGroup>
	{
		public CMRInstrumentTariffGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CMRInstrumentTariffGroupCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public bool HasThisTariffAndStatNumber(ZString tariffAndStatNumber)
		{
			ZString cleanTariffNumber = tariffAndStatNumber.Replace(".", "").Replace(" ", "");
			if (!cleanTariffNumber.IsEmpty)
			{
				foreach (CMRInstrumentTariffGroup instrumentTariff in this)
				{
					if (cleanTariffNumber.StartsWith(instrumentTariff.IG_TariffGroupItem))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
