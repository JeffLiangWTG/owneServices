using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			// Must match IncidentManagementGroup.GetTemplateSelectionCriteria
			return new[]
			{
				IncidentManagementGroupSchema.ING_Type.Name,
				IncidentManagementGroupSchema.ING_Product.Name,
				IncidentManagementGroupSchema.ING_ProductArea.Name,
			};
		}
	}
}
