using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DeleteExpiredRatesUserControl))]
	sealed class WDeleteExpiredRatesUserControlTest : RegistryZUserControlTestCase
	{
		protected override RegistryZUserControl GetNewControl()
		{
			return new DeleteExpiredRatesUserControl(new DeleteExpiredRatesWrapper(new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = 100 }));
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DeleteExpiredRatesWrapper(new DeleteExpiredRates());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
