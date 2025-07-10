using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(MinimumIntervalSubLedgerTakeUpControl))]
	sealed class MinimumIntervalSubLedgerTakeUpControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new MinimumIntervalSubLedgerTakeUp();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((MinimumIntervalSubLedgerTakeUpControl)control).ReadOnly;
		}
	}
}
