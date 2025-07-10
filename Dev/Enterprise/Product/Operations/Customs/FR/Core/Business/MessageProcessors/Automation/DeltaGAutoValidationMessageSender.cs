using CargoWise.Types;
using Enterprise.Customs.Business.Logging;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaGAutoValidationMessageSender : FRAutoValidationMessageSender
	{
		public DeltaGAutoValidationMessageSender(ICommonLogger logger) : base(logger)
		{
		}

		protected override ZString MessageType => EntryActionCodeList.Codes.VAA;

		protected override ZString CandidateEntriesRequiredStatus => EntryStatusDescriptionCodeList.Codes.ES050;
	}
}
