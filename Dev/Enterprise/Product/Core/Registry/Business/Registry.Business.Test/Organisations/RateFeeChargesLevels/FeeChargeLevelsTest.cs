using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FeeChargeLevelsSection))]
	sealed class FeeChargeLevelsTest : RegistryBusinessObjectTemplateTestCase<FeeChargeLevelsSection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override FeeChargeLevelsSection GetBusinessObjectToClone()
		{
			return (FeeChargeLevelsSection)GetNewBusinessObject();
		}

		protected override FeeChargeLevelsSection GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FeeChargeLevelsSection();
		}
	}
}
