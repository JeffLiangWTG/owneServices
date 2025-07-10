using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebThemeUrl))]
	sealed class WebThemeUrlTest : RegistryBusinessObjectTemplateTestCase<WebThemeUrl>
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

		protected override WebThemeUrl GetBusinessObjectToClone()
		{
			return new WebThemeUrl();
		}

		protected override WebThemeUrl GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
