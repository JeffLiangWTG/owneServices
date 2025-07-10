using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ModuleMappingWithSourceModuleCollection))]
	sealed class ModuleMappingWithSourceModuleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ModuleMappingWithSourceModuleCollection>
	{
		#region Implementation

		protected override ModuleMappingWithSourceModuleCollection GetCollectionToTest()
		{
			return new ModuleMappingWithSourceModuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ModuleMappingWithSourceModule(new ProductAreaModuleMapping(), new SourceModule());
		}

		#endregion
	}
}
