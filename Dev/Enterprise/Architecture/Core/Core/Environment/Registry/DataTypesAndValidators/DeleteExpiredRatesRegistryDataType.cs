using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core.Environment.Registry.DataTypesAndValidators
{
	public class DeleteExpiredRatesRegistryDataType : RegistryDataTypeWithJsonSerializer<DeleteExpiredRates>
	{
		public DeleteExpiredRatesRegistryDataType(DeleteExpiredRates deleteExpiredRates)
			: base(RegistryDataTypes.Codes.Binary, deleteExpiredRates)
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new DeleteExpiredRatesEditorInfo();
		}

		protected override DeleteExpiredRates CloneValue(DeleteExpiredRates value)
		{
			DeleteExpiredRates cloneValue = null;
			if (value != null)
			{
				cloneValue = new DeleteExpiredRates
				{
					BatchSize = value.BatchSize,
					ExpiredRatesPeriodInYears = value.ExpiredRatesPeriodInYears
				};
			}
			return cloneValue;
		}
	}
}
