using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class ComplianceWiseRegistryHelper
	{
		public static EnableComplianceWiseRegistryBusinessObject SetValue(bool value)
		{
			return new EnableComplianceWiseRegistryBusinessObject { EnableComplianceWise = value };
		}

		public static CodeDescriptionBoolCollection GetGlobalCommercialInvoice(bool value)
		{
			return new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.ComplianceWiseFeatureCodes.GlobalCommercialInvoice, (NoResString)"Global Commercial Invoice", value },
			};
		}

		public static CodeDescriptionBoolCollection GetJobEntitiesCaching(bool value)
		{
			return new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.ComplianceWiseFeatureCodes.JobEntitiesCaching, (NoResString)"Job Entities Caching", value },
			};
		}
	}
}
