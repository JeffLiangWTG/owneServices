using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransitReferenceMappingConfiguration))]
	class TransitReferenceMappingConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new TransitReferenceMappingConfiguration();
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new TransitReferenceMappingConfiguration BizObj
		{
			get { return (TransitReferenceMappingConfiguration)base.BizObj; }
		}

		#endregion
	}
}
