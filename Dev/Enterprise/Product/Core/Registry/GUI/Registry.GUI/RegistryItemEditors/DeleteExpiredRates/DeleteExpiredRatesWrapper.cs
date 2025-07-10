using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class DeleteExpiredRatesWrapper : NonPersistentBusinessObject
	{
		public DeleteExpiredRatesWrapper(DeleteExpiredRates deleteExpiredRates)
		{
			ExpiredRates = deleteExpiredRates;
		}

		internal DeleteExpiredRates ExpiredRates { get; set; }

		public ZInt BatchSize
		{
			get { return ExpiredRates.BatchSize; }
			set { ExpiredRates.BatchSize = value; }
		}

		public ZInt ExpiredRatesPeriodInYears
		{
			get { return ExpiredRates.ExpiredRatesPeriodInYears; }
			set { ExpiredRates.ExpiredRatesPeriodInYears = value; }
		}
	}
}
