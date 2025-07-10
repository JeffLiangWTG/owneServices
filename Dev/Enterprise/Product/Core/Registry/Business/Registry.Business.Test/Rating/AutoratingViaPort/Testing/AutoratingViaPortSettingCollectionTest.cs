using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoratingViaPortSettingCollection))]
	sealed class AutoratingViaPortSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AutoratingViaPortSettingCollection>
	{
		#region Implementation

		protected override AutoratingViaPortSettingCollection GetCollectionToTest()
			=> new AutoratingViaPortSettingCollection(null, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new AutoratingViaPortSetting(null, Factory);

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}
