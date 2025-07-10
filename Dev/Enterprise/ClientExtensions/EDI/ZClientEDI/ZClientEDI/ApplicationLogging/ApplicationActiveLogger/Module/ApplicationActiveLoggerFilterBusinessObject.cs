using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	public class ApplicationActiveLoggerFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddGuidFilter("Application Logger", ClientModuleRegistration.ApplicationLogger, ApplicationActiveLoggerSchema.AAL_ALG_ApplicationLogger, new ApplicationLoggerCollection(Factory));
			filters.AddTextFilter("Environment", ApplicationActiveLoggerSchema.AAL_Environment);
			filters.AddDateFilter("Active Until (UTC)", ApplicationActiveLoggerSchema.AAL_ActiveUntil);
			return filters;
		}
	}
}
