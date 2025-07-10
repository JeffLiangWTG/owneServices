using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class ImportDeclarationConfirmationSender : ImportDeclarationSender
	{
		public ImportDeclarationConfirmationSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.ImportDeclarationConfirmation, new CUSCONMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override ZString LogbookRegistrationNumber => ((ICUSCONHeader)DataProvider).TemporaryReferenceNumber;
	}
}
