using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOMessageSendingObjectParentLookups : ZLookups
	{
		public LPCOMessageSendingObjectParentLookups(LPCOMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		public GlbStaffCollection BrokerList => new GlbStaffCollection(Factory);
	}
}

