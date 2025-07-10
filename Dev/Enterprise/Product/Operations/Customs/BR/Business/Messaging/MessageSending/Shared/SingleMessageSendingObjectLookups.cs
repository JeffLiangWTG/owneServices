using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class SingleMessageSendingObjectLookups : ZLookups
	{
		public SingleMessageSendingObjectLookups(BaseSingleMessageSendingObject parent)
			: base(parent)
		{
		}

		public GlbStaffCollection BrokerList => new GlbStaffCollection(Factory);
	}
}
