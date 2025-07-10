using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StatisticsFoldupInfoUserControl))]
	sealed class StatisticsFoldupInfoUserControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (StatisticsFoldupInfoUserControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new StatisticsFoldupInfoCollection();
		}
	}
}
