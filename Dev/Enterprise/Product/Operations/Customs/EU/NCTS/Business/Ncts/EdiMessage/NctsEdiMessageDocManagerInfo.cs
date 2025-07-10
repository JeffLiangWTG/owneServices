using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEdiMessageDocManagerInfo : DocManagerInfo
	{
		public NctsEdiMessageDocManagerInfo(NctsEdiMessage nctsEdiMessage)
			: base(nctsEdiMessage, Enterprise.Core.Constants.DocManagerCodes.EDIMessage)
		{ }
	}
}
