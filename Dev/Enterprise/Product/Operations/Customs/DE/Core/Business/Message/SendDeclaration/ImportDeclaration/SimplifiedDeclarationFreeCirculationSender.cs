using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business
{
	public class SimplifiedDeclarationFreeCirculationSender : ImportDeclarationSender
	{
		public SimplifiedDeclarationFreeCirculationSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.SimplifiedDeclarationIntoFreeCirculation, new CFCRECMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override void CreateCusReconEntry()
		{
			var entryBuilder = new CFCRECReconEntryBuilder(entryHeader, (ICFCRECHeader)DataProvider);
			entryBuilder.CreateLodgedEntryAndSnapshot();
		}

		protected override bool PreSend()
		{
			ResetEntryStatus();
			return base.PreSend();
		}
	}
}
