using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ModuleMappingWithSourceModule : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ModuleMappingWithSourceModule(ProductAreaModuleMapping moduleMapping, SourceModule sourceModule)
		{
			this.moduleMapping = moduleMapping;
			this.sourceModule = sourceModule;
			this.productArea =  moduleMapping.GetProductAreaWithSourceModule(SourceModuleCode);
		}

		#region Properties

		readonly ProductAreaModuleMapping moduleMapping;
		readonly SourceModule sourceModule;
		readonly ZString productArea;

		public ZString SourceModuleCode
		{
			get { return sourceModule.Code; }
		}

		public ZString SourceModuleDescription
		{
			get { return sourceModule.Description; }
		}

		public ZString SourceModulePath
		{
			get { return sourceModule.Path; }
		}

		public ZString ModuleCode
		{
			get { return moduleMapping.ModuleCode; }
		}

		public ZString ModuleDescription
		{
			get { return moduleMapping.ModuleDescription; }
		}

		public ZString ProductArea
		{
			get { return productArea; }
		}

		#endregion
	}
}

