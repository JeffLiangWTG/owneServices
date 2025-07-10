using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(UnitMeasurementTextOverrideCollection))]
	public class UnitMeasurementTextOverrideCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UnitMeasurementTextOverrideCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override UnitMeasurementTextOverrideCollection GetCollectionToTest()
		{
			return new UnitMeasurementTextOverrideCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UnitMeasurementTextOverride();
		}
	}
}
