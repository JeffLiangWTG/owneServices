using CargoWise.Types;
using Enterprise.Customs.Business.Logging;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaIEAutoDeadLineExtensionMessageSender : FRAutoDeadLineExtensionMessageSender
	{
		public DeltaIEAutoDeadLineExtensionMessageSender(ICommonLogger logger) : base(logger, ZString.Empty)
		{
		}

		protected override ZString MessageType => DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;

		protected override ZString CandidateEntriesRequiredStatus => DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;

		protected override bool IsUCC6 => true;
	}
}
