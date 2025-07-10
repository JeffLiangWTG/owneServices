using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Module
{
	public class ExportStatusRequestFilterStripBusinessObjectLookups
	{
		public ExportStatusRequestFilterStripBusinessObjectLookups(ExportStatusRequestFilterBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}
		readonly ExportStatusRequestFilterBusinessObject filterBizObj;

		public CodeDescriptionPairList ModuleCodeList => filterBizObj.Factory.GetCachedValue<ExportStatusRequestModuleCodeList>();
	}
}
