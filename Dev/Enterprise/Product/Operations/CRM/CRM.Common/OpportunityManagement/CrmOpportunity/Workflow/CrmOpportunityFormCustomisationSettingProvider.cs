using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityFormCustomisationSettingProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			// Must match CrmOpportunity.GetTemplateSelectionCriteria
			// and CrmOpportunityWorkflowDescriptor.SubTypeInformation
			return new[]
			{
				CrmOpportunitySchema.COP_SalesType.Name,
				CrmOpportunitySchema.COP_SourceType.Name,
				CrmOpportunitySchema.COP_ProductType.Name
			};
		}
	}
}
