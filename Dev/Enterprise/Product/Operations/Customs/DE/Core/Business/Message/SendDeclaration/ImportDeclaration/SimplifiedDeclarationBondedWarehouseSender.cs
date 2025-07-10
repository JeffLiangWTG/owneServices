using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business
{
	public class SimplifiedDeclarationBondedWarehouseSender : ImportDeclarationSender
	{
		public SimplifiedDeclarationBondedWarehouseSender(ImportEntryMessageSendingAction action)
			: base(action.MessagingObject, ImportDeclarationMessageBuilderLoader.SimplifiedDeclarationIntoBondedWarehouse, new SCWRECMessageHeaderProvider(action.MessagingObject))
		{
		}

		protected override void CreateCusReconEntry()
		{
			var entryBuilder = new SCWRECReconEntryBuilder(entryHeader, (ISCWRECHeader)DataProvider);
			entryBuilder.CreateLodgedEntryAndSnapshot();
		}

		protected override bool PreSend()
		{
			ResetEntryStatus();
			return base.PreSend();
		}
	}
}
