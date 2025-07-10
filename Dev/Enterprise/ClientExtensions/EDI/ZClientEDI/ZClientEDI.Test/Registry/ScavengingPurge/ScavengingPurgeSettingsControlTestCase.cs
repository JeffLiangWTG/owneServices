using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ScavengingPurgeSettingsControl))]
	internal sealed class ScavengingPurgeSettingsControlTestCase : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return ScavengingPurgeSettings.GetDefaults();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ScavengingPurgeSettingsControl)control).gridScavengingItems.ReadOnly;
		}
	}
}
