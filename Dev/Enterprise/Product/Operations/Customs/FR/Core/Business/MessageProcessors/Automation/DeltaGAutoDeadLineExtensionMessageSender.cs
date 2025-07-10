using CargoWise.Types;
using Enterprise.Customs.Business.Logging;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaGAutoDeadLineExtensionMessageSender : FRAutoDeadLineExtensionMessageSender
	{
		public DeltaGAutoDeadLineExtensionMessageSender(ICommonLogger logger, ZString deltaMode) : base(logger, deltaMode)
		{
			this.deltaMode = deltaMode;
		}
		readonly ZString deltaMode;

		protected override ZString MessageType => deltaMode == OrgCusAccountDeltaGTypeList.Codes.G1 ? EntryActionCodeList.Codes.MAP : EntryActionCodeList.Codes.MDA;

		protected override ZString CandidateEntriesRequiredStatus => EntryStatusDescriptionCodeList.Codes.ES050;
	}
}
