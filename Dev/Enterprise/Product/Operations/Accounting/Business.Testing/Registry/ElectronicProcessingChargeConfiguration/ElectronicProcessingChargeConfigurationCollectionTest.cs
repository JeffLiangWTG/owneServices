using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeConfigurationCollection))]
	public class ElectronicProcessingChargeConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ElectronicProcessingChargeConfigurationCollection>
	{
		#region Implementation

		protected override ElectronicProcessingChargeConfigurationCollection GetCollectionToTest()
		{
			return new ElectronicProcessingChargeConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ElectronicProcessingChargeConfiguration();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion

	}
}
