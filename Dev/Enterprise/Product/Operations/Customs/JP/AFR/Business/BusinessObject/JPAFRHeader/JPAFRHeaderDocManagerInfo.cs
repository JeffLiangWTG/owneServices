using Enterprise.MasterFiles.Business;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderDocManagerInfo : DocManagerInfo
	{
		public JPAFRHeaderDocManagerInfo(JPAFRHeader header)
			: base(header, CoreConstants.DocManagerCodes.JPAFRHeader)
		{
		}
	}
}
