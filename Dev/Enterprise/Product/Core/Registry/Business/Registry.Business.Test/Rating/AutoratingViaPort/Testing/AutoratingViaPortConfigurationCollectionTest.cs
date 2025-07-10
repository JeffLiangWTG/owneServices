using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoratingViaPortConfigurationCollection))]
	sealed class AutoratingViaPortConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AutoratingViaPortConfigurationCollection>
	{
		#region Implementation

		protected override AutoratingViaPortConfigurationCollection GetCollectionToTest()
			=> new AutoratingViaPortConfigurationCollection(null, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new AutoratingViaPortConfiguration(null, Factory);

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}
