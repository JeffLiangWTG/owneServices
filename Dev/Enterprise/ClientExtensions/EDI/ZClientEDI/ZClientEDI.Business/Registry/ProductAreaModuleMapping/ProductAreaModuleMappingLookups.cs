using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ProductAreaModuleMappingLookups : ZLookups
	{
		public ProductAreaModuleMappingLookups(ProductAreaModuleMapping parent)
			: base(parent) { }

		public ProductAreaModuleMappingLookups(ServiceTypeProductAreaModuleMapping parent)
			: base(parent) { }

		public ReadOnlyCodeDescriptionPairList ProductAreaList
		{
			get { return EDIDataRegistry.Instance.ProductAreas.Value; }
		}

		#region Implementation

		protected new ProductAreaModuleMapping Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ProductAreaModuleMapping)base.Parent; }
		}

		#endregion
	}
}

