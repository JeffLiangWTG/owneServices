using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.eHub;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.eHub.Testing
{
	[TestedType(typeof(ScavengingSettingsUserControl))]
	sealed class ScavengingSettingsUserControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ScavengingSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ScavengingSettingsUserControl)control).ScavengingSettingsGrid.ReadOnly;
		}
	}
}
