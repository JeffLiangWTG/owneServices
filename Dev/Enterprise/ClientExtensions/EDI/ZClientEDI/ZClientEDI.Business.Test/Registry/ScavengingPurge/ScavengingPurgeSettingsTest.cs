using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ScavengingPurgeSettings))]
	class ScavengingPurgeSettingsTest : RegistryBusinessObjectCollectionTemplateTestCase<ScavengingPurgeSettings>
	{
		protected override ScavengingPurgeSettings GetCollectionToTest()
		{
			return new ScavengingPurgeSettings();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ScavengingPurgeItem();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
