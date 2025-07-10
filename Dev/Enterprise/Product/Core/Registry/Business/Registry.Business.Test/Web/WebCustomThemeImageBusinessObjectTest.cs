using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebCustomThemeImageBusinessObject))]
	sealed class WebCustomThemeImageBusinessObjectTest : RegistryBusinessObjectTemplateTestCase<WebCustomThemeImageBusinessObject>
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

		protected override WebCustomThemeImageBusinessObject GetBusinessObjectToClone()
		{
			return new WebCustomThemeImageBusinessObject();
		}

		protected override WebCustomThemeImageBusinessObject GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
