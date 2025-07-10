using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ModuleMappingWithSourceModuleCollection : NonPersistentBusinessObjectCollection<ModuleMappingWithSourceModule>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ModuleMappingWithSourceModule(new ProductAreaModuleMapping(), new SourceModule());
		}
	}
}

