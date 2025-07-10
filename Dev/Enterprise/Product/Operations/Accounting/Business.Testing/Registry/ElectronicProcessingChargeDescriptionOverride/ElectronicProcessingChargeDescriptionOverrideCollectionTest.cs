using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeDescriptionOverrideCollection))]
	public class ElectronicProcessingChargeDescriptionOverrideCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ElectronicProcessingChargeDescriptionOverrideCollection>
	{
		#region Implementation

		protected override ElectronicProcessingChargeDescriptionOverrideCollection GetCollectionToTest()
		{
			return new ElectronicProcessingChargeDescriptionOverrideCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ElectronicProcessingChargeDescriptionOverride();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion

	}
}
