using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SimplifiedDeclarationInwardProcessingSender : ImportDeclarationSender
	{
		public SimplifiedDeclarationInwardProcessingSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.SimplifiedDeclarationForInwardProcessing, new SCIRECMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override void CreateCusReconEntry()
		{
			var entryBuilder = new SCIRECReconEntryBuilder(entryHeader, (ISCIRECHeader)DataProvider);
			entryBuilder.CreateLodgedEntryAndSnapshot();
		}

		protected override bool PreSend()
		{
			ResetEntryStatus();
			return base.PreSend();
		}
	}
}
