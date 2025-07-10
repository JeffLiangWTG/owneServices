using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(Hyperlink))]
	sealed class HyperlinkTest : RegistryBusinessObjectTemplateTestCase<Hyperlink>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override Hyperlink GetBusinessObjectToClone()
		{
			return new Hyperlink();
		}

		protected override Hyperlink GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
