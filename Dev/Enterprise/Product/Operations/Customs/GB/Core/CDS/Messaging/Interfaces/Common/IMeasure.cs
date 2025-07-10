using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IMeasure
	{
		ZDecimal MeasureValue { get; }
		ZString MeasureUQ { get; }
	}

	class MeasureWrapper : IMeasure
	{
		MeasureWrapper(ZDecimal measureValue, ZString measureUQ)
		{
			this.measureValue = measureValue;
			this.measureUQ = measureUQ;
		}

		public static MeasureWrapper New(ZDecimal measureValue, ZString measureUQ, bool isCurrencyMeasure = false)
		{
			return new MeasureWrapper(isCurrencyMeasure ? measureValue.Round(NoOfDecimalPlacesForCurrency) : measureValue.Round(NoOfDecimalPlacesForMeasuresOtherThanCurrency), measureUQ);
		}

		ZDecimal IMeasure.MeasureValue => measureValue;

		ZString IMeasure.MeasureUQ => measureUQ.Length == 4 ? measureUQ.InsertSafe(3, "#") : measureUQ;

		readonly ZDecimal measureValue;
		readonly ZString measureUQ;
		static int NoOfDecimalPlacesForCurrency => 2;
		static int NoOfDecimalPlacesForMeasuresOtherThanCurrency => 6; //for duty values
	}
}
