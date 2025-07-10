using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	public class ILDEC751MessageBuilder : ILMessageBuilderBase
	{
		public ILDEC751MessageBuilder(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		protected override ILEDIMessage GetMessage() => entryHeader.Factory.New<ILDEC751RequestMessage>();

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => entryHeader.Messages;

		protected override string GetMessageText()
		{
			return ZString.Empty;
		}

		readonly CusEntryHeader entryHeader;
	}
}
