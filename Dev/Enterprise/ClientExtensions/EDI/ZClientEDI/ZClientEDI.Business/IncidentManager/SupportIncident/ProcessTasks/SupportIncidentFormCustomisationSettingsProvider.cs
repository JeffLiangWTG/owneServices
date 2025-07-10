using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			// Must match SupportIncident.GetTemplateSelectionCriteria
			return new[]
			{
				IncidentMainSchema.IM_OH_Client.Name,
				IncidentMainSchema.IM_Product.Name,
				IncidentMainSchema.IM_Category.Name,
				IncidentMainSchema.IM_Source.Name,
				IncidentMainSchema.IM_ProgramArea.Name,
				IncidentMainSchema.IM_Language.Name
			};
		}
	}
}
