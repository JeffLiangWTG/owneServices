using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ShipmentExportAgentOrganisationWrapper : ExportAgentOrganisationWrapper
	{
		#region Constructors

		public ShipmentExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrgHeader organisation, ContactType mainContactType, BusinessObjectFactory factory)
			: base(usageType, organisation, mainContactType, factory)
		{
		}

		public ShipmentExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrgAddress address, ContactType mainContactType, BusinessObjectFactory factory)
			: base(usageType, address, mainContactType, factory)
		{
		}

		public ShipmentExportAgentOrganisationWrapper(OrganisationUsageType usageType, JobDocAddress docAddress, BusinessObjectFactory factory)
			: base(usageType, docAddress, factory)
		{
		}

		internal ShipmentExportAgentOrganisationWrapper(OrganisationUsageType usageType, OrganisationWrapper wrapperBeingCloned)
			: base(usageType, wrapperBeingCloned)
		{
		}

		#endregion

		protected override string GenerateCustomCodesString()
		{
			var result = !SCAC.IsEmpty ? Res.GetString("80dbb66e-dcd5-47bc-b43c-dd336a9e8496", "SCAC NO. {0}", SCAC) : string.Empty;

			return result;
		}
	}
}
