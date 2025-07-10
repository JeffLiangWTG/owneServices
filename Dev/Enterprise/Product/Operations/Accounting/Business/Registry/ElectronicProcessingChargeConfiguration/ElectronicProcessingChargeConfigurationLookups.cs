using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business
{
	public class ElectronicProcessingChargeConfigurationLookups
	{
		public ElectronicProcessingChargeConfigurationLookups()
		{
		}

		public CodeDescriptionPairList JobTypeList => JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
	}
}
