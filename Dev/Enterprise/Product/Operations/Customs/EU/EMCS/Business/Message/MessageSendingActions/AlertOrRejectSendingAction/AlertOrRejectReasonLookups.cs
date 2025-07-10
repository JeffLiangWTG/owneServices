using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class AlertOrRejectReasonLookups : ZLookups
	{
		public AlertOrRejectReasonLookups(AlertOrRejectReason parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ReasonList => Factory.GetCachedValue<EMCSAlertRejectionCodeList>();
	}
}
