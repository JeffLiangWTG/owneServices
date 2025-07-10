using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SalesRelationDirectionRule))]
	sealed class SalesRelationDirectionRuleTest : RegistryBusinessObjectTemplateTestCase<SalesRelationDirectionRule>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override SalesRelationDirectionRule GetBusinessObjectToClone()
		{
			return new SalesRelationDirectionRule();
		}

		protected override SalesRelationDirectionRule GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
