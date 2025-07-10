using System.Collections.Generic;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIRelatedActivityLinkLookups : RelatedActivityLinkLookups
	{
		protected EDIRelatedActivityLinkLookups(RelatedActivityLink parent)
			: base(parent)
		{
		}

		public new static RelatedActivityLinkLookups New(RelatedActivityLink parent)
		{
			return new EDIRelatedActivityLinkLookups(parent);
		}

		#region Register/Unregister SubType Override

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		#region RelatableActivityTypeInformations

		protected override IDictionary<string, RelatableActivityTypeDefinition> GetNewRelatableActivityTypeDefinitions()
		{
			var result = base.GetNewRelatableActivityTypeDefinitions();
			result.Add(EDIRelatableActivityTypeList.Codes.Incident, new RelatableActivityTypeDefinition(EDIRelatableActivityTypeList.Descriptions.Incident, ClientModuleRegistration.SupportIncident, ClientControllerRegistration.SupportIncident, IncidentMainSchema.Constants.Prefix));
			result.Add(EDIRelatableActivityTypeList.Codes.ProfessionalServicesQuote, new RelatableActivityTypeDefinition(EDIRelatableActivityTypeList.Descriptions.ProfessionalServicesQuote, ClientModuleRegistration.ProfessionalServicesQuote, ClientControllerRegistration.ProfessionalServicesQuote, IncidentMainSchema.Constants.Prefix));
			return result;
		}

		#endregion
	}
}

