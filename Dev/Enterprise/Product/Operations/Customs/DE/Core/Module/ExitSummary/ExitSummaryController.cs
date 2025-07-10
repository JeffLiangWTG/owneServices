using CargoWise.EntityFramework;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.ZArchitecture.PlugIn;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.Module
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public class ExitSummaryController : EU.Module.ExitSummaryController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new ExitSummaryPlugIn(businessEntity);
	}
}
