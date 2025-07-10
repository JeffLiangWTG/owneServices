using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ES.Module
{
	public class ExitSummaryController : EU.Module.ExitSummaryController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var hasAnyVersionCodeAes = MessageVersionRegistryProvider.IsExportAndAnyVersionAes();
			return hasAnyVersionCodeAes
				? base.GetPlugIn(businessEntity)
				: new ExitSummaryPlugIn(businessEntity);
		}
	}
}
