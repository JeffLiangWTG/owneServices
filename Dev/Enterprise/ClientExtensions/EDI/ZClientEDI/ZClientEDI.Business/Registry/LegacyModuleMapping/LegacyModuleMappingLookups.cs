using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class LegacyModuleMappingLookups : ZLookups
	{
		public LegacyModuleMappingLookups(LegacyModuleMapping parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList CriticalityList
		{
			get { return new IncidentApprovalLookups(null).CriticalityList; }
		}

		public virtual CodeDescriptionPairList ModuleMappingList
		{
			get
			{
				var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(Parent.ModuleTypeMapping);
				if (productAreaModuleMappingsRegistryItem != null)
				{
					return productAreaModuleMappingsRegistryItem.Value.GetModuleList(ProductTypes.Codes.Enterprise);
				}

				return new CodeDescriptionPairList();
			}
		}

		public virtual IBusinessObjectCollection CountryList
		{
			get { return new RefCountryCollection(RegistryFactory.Instance); }
		}

		#region Implementation

		protected new LegacyModuleMapping Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (LegacyModuleMapping)base.Parent; }
		}

		#endregion
	}
}

