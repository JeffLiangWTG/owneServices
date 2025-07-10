using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class GoodsNotificationAESExportOperationLRNWrapper : IGoodsNotificationAESExportOperationLRN
	{
		public GoodsNotificationAESExportOperationLRNWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			_ = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration)); // throw if declaration is null
		}
		protected readonly CusEntryHeader entryHeader;

		public ZString LRN => entryHeader.CH_BGMReference;
	}
}
