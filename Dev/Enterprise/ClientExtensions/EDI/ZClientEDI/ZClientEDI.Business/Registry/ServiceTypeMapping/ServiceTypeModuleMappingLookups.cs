using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ServiceTypeModuleMappingLookups : ZLookups
	{
		public ServiceTypeModuleMappingLookups(ServiceTypeModuleMapping parent)
			: base(parent) { }

		public ReadOnlyCodeDescriptionPairList ServiceTypeList
		{
			get { return EDIDataRegistry.Instance.ServiceTypes.Value; }
		}

		#region Implementation

		protected new ServiceTypeModuleMapping Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ServiceTypeModuleMapping)base.Parent; }
		}

		#endregion
	}
}

