
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodCharacteristicCollection : BusinessObjectCollection<CMRTariffRatePeriodCharacteristic>
	{
		public CMRTariffRatePeriodCharacteristicCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CMRTariffRatePeriodCharacteristicCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public bool IsThisGSTExempt(ZString tariffNumber, ZString rateNumber, ZString prefScheme)
		{
			return GSTExmptionCalculator.IsThisGSTExempt(tariffNumber, rateNumber, prefScheme);
		}

		GSTExemptionCalculator GSTExmptionCalculator
		{
			get
			{
				if (fGSTExmptionCalculator == null)
				{
					fGSTExmptionCalculator = new GSTExemptionCalculator((IGSTExempt[])this.ToArray(typeof(IGSTExempt)));
				}
				return fGSTExmptionCalculator;
			}
		}
		GSTExemptionCalculator fGSTExmptionCalculator;
	}
}
