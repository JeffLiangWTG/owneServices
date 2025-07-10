
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodCharacteristicCollection : BusinessObjectCollection<CMRTreatmentRatePeriodCharacteristic>
	{
		public CMRTreatmentRatePeriodCharacteristicCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CMRTreatmentRatePeriodCharacteristicCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public bool IsThisGSTExempt(ZString treatmentCode, ZString treamentRateNumber, ZString prefScheme)
		{
			return GSTExmptionCalculator.IsThisGSTExempt(treatmentCode, treamentRateNumber, prefScheme);
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
