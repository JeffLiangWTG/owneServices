using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.SupplyChainSecurityEU
{
	[TestedType(typeof(AviationSecurityTrainingRestriction))]
	public class AviationSecurityTrainingRestrictionTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new AviationSecurityTrainingRestriction();
		}

		public void TestDefaultValue()
		{
			var defaultValue = new AviationSecurityTrainingRestriction();
			AssertEquals(true, defaultValue.Enabled);
			AssertEquals(false, defaultValue.ApplyCertificationRestriction);
		}
	}
}
