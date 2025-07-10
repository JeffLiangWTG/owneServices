using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ProductAreaSourceModuleMappingLookups : ZLookups
	{
		public ProductAreaSourceModuleMappingLookups(ProductAreaSourceModuleMapping parent)
			: base(parent) { }

		public ReadOnlyCodeDescriptionPairList SourceModuleList
		{
			get { return SupportIncidentLookups.GetNewSearchableSourceModuleList(); }
		}

		public ReadOnlyCodeDescriptionPairList MenuSectionSourceModuleList
		{
			get { return SupportIncidentLookups.GetNewSearchableSourceModuleList(ModuleListType.MenuSection); }
		}

		public ReadOnlyCodeDescriptionPairList Cr8SourceModuleList
		{
			get { return SupportIncidentLookups.GetNewSearchableSourceModuleList(ModuleListType.Cr8); }
		}

		public ReadOnlyCodeDescriptionPairList Cr9SourceModuleList
		{
			get { return SupportIncidentLookups.GetNewSearchableSourceModuleList(ModuleListType.Cr9); }
		}

		public ReadOnlyCodeDescriptionPairList ProductAreaList
		{
			get { return EDIDataRegistry.Instance.ProductAreas.Value; }
		}

		#region Implementation

		protected new ProductAreaSourceModuleMapping Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ProductAreaSourceModuleMapping)base.Parent; }
		}

		#endregion
	}
}

