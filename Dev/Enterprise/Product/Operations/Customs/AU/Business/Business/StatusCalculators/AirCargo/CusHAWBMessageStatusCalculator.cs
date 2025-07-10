
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBMessageStatusCalculator : CMRStatusCalculator<CusHAWBBase>, ICMRCargoReportEventsLogger
	{
		public CusHAWBMessageStatusCalculator(CusHAWBBase hAWB)
			: base(hAWB)
		{
			hawb = hAWB;
		}
		readonly CusHAWBBase hawb;

		protected internal override ZPropertyInfo StatusInfo => Parent.CS_MsgStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.AIRCR };

		protected internal override ZString StatusChangedEventLogPrefix => "CusHAWB Message Status - ";

		protected override bool ShouldLogStatusChange(ZString status) => status == CMRBaseStatuses.Codes.OriginalAccepted
			|| status == CMRBaseStatuses.Codes.AmendmentAccepted
			|| status == CMRBaseStatuses.Codes.WithdrawalAccepted;

		IParentForCargoReporter ICMRCargoReportEventsLogger.ParentForCargoReportingEvents => (IParentForCargoReporter)hawb.ParentForCargoReportingEvents;
	}
}
