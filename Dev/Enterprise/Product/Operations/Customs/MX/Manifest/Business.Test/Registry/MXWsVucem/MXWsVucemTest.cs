using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(MXWsVucem))]
	public partial class MXWsVucemTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => BizObj;

		protected new MXWsVucem BizObj => (MXWsVucem)base.BizObj;
	}
}
