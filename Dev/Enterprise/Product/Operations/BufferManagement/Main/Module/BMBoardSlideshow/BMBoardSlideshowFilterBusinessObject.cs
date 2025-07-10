using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMBoardSlideshowFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Name", BMBoardSlideshowSchema.MD_Name).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardSlideshowFilter|Name", "Name");
			return result;
		}
	}
}
