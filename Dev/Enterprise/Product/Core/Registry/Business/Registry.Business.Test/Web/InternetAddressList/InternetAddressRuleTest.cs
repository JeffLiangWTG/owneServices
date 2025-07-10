using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InternetAddressRule))]
	sealed class InternetAddressRuleTest : RegistryBusinessObjectTemplateTestCase<InternetAddressRule>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override InternetAddressRule GetBusinessObjectToClone()
		{
			return new InternetAddressRule();
		}

		protected override InternetAddressRule GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
