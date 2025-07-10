using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class NettingPeriodFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public NettingPeriodFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			result.AddTextFilter("Period", NettingSystemPeriodSchema.NSP_Period).MultilingualDescription = ResString.GetMultilingualString("4ac144a1-9014-433b-82ca-a206aee6fdd6", "Period");
			result.AddTextFilter("Description", NettingSystemPeriodSchema.NSP_Description).MultilingualDescription = ResString.GetMultilingualString("ba9dc49d-7af7-42e8-b9c5-766ced6b2b59", "Description");
			return result;
		}
	}
}
