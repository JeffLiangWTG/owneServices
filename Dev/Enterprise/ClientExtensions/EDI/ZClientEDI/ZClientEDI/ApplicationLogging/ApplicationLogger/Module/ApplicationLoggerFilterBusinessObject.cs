using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	internal class ApplicationLoggerFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Product", ApplicationLoggerSchema.ALG_Product);
			filters.AddTextFilter("Name", ApplicationLoggerSchema.ALG_Name);
			filters.AddTextFilter("Description", ApplicationLoggerSchema.ALG_Description);
			return filters;
		}
	}
}
