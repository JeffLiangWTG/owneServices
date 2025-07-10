using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillDocManagerInfo : DocManagerInfo
	{
		public AsycudaBillDocManagerInfo(BusinessObject parent) : base(parent, Core.Constants.DocManagerCodes.AsycudaBill)
		{
		}
	}
}
