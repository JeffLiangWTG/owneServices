using CargoWise.EntityFramework;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ServiceTypeProductAreaModuleMappingLookups : ZLookups
	{
		public ServiceTypeProductAreaModuleMappingLookups(ServiceTypeProductAreaModuleMapping parent)
			: base(parent) { }

		public ReadOnlyCodeDescriptionPairList Modules
		{
			get
			{
				if (Parent.ProductArea.IsEmpty)
				{
					return new ReadOnlyCodeDescriptionPairList();
				}

				return GetRegistry(Parent.ProductCriticality).GetModuleList(Parent.ProductCode, Parent.ProductArea);
			}
		}

		public ProductAreaModuleMapping GetModule(string moduleCode)
		{
			return GetRegistry(Parent.ProductCriticality).GetMapping(Parent.ProductCode, moduleCode);
		}

		public ReadOnlyCodeDescriptionPairList ProductAreaList
		{
			get { return EDIDataRegistry.Instance.ProductAreas.Value; }
		}

		SystemProductCollection GetRegistry(ModuleListType criticality)
		{
			if (criticality == CustomerService.Business.ModuleListType.Cr8)
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value;
			}
			else if (criticality == CustomerService.Business.ModuleListType.Cr9)
			{
				return EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value;
			}
			else
			{
				return EDIDataRegistry.Instance.SystemProductMappings.Value;
			}
		}

		#region Implementation

		protected new ServiceTypeProductAreaModuleMapping Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ServiceTypeProductAreaModuleMapping)base.Parent; }
		}

		#endregion
	}
}

