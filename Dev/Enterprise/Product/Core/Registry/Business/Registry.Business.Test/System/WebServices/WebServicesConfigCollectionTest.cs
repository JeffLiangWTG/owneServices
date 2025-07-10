using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebServicesConfigCollection))]
	public class WebServicesConfigCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WebServicesConfigCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override WebServicesConfigCollection GetCollectionToTest()
		{
			return new WebServicesConfigCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WebServicesConfig();
		}

		#endregion
	}
}
