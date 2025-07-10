using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Client.EDI.Business
{
	public static class EDIFreightWrapperDecider
	{
		public static void RegisterThisSubTypeOverride()
		{
			FreightWrapper.OverridableNewFreightWrapperDelegate.Value = delegate(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
			{
				ProfessionalServicesQuote professionalServicesQuote = businessObjectToWrap as ProfessionalServicesQuote;
				if (professionalServicesQuote != null)
				{
					return new FreightWrapperFromProfessionalServicesQuote(professionalServicesQuote, factory);
				}

				SupportIncident supportIncident = businessObjectToWrap as SupportIncident;
				if (supportIncident != null)
				{
					return new FreightWrapperFromSupportIncident(supportIncident, factory);
				}

				if (businessObjectToWrap is IncidentManagementGroup incidnetGroup)
				{
					return new FreightWrapperFromIncidentManagementGroup(incidnetGroup, factory);
				}

				EDIProject project = businessObjectToWrap as EDIProject;
				if (project != null)
				{
					return new FreightWrapperFromProject(project, factory);
				}

				return null;
			};
		}
	}
}
