using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebCustomThemeImageBusinessObjectCollection))]
	sealed class WebCustomThemeImageBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WebCustomThemeImageBusinessObjectCollection>
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

		protected override WebCustomThemeImageBusinessObjectCollection GetCollectionToTest()
		{
			return new WebCustomThemeImageBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WebCustomThemeImageBusinessObject();
		}

		#endregion
	}
}
