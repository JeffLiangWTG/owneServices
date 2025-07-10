using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				IncidentTriageSchema.IMT_Product.Name,
				IncidentTriageSchema.IMT_ProductArea.Name,
				IncidentTriageSchema.IMT_Module.Name,
			};
		}
	}
}
