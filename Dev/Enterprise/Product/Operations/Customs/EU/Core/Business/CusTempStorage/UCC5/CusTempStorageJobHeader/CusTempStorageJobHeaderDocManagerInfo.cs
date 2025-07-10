using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderDocManagerInfo : DocManagerInfo
	{
		public CusTempStorageJobHeaderDocManagerInfo(CusTempStorageJobHeader parent)
			: base(parent, Core.Constants.DocManagerCodes.TempStorageHeader)
		{
		}
	}
}
