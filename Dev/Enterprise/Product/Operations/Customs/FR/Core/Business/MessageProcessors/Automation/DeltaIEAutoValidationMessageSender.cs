using CargoWise.Types;
using Enterprise.Customs.Business.Logging;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEAutoValidationMessageSender : FRAutoValidationMessageSender
	{
		public DeltaIEAutoValidationMessageSender(ICommonLogger logger) : base(logger)
		{
		}

		protected override ZString MessageType => DeltaIESendMessageSubTypeList.Codes.PresentationNotification;

		protected override ZString CandidateEntriesRequiredStatus => DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;

		protected override bool IsUCC6 => true;
	}
}
