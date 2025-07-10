using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return
				[
					NctsHeader.Schema.Principal
				];
		}
	}
}
