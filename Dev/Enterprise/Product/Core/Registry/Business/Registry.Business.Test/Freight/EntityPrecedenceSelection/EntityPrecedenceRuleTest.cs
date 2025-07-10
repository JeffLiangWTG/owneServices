using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EntityPrecedenceRule))]
	sealed class EntityPrecedenceRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new EntityPrecedenceRule();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
