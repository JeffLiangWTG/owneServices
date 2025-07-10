using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ModuleMappingWithSourceModule))]
	sealed class ModuleMappingWithSourceModuleTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var moduleMapping = new ProductAreaModuleMapping();
			var sourceModule = new SourceModule();

			return new ModuleMappingWithSourceModule(moduleMapping, sourceModule);
		}
	}
}
