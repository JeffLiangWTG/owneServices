using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyEventSetting))]
	public class IntercompanyEventSettingTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new IntercompanyEventSetting();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
