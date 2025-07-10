using CargoWise.Common;
using CargoWise.Customs.IL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	public class ILDEC275MessageBuilder : ILMessageBuilderBase
	{
		public ILDEC275MessageBuilder(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		protected override ILEDIMessage GetMessage() => entryHeader.Factory.New<ILDEC275RequestMessage>();

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => entryHeader.Messages;

		protected override string GetMessageText()
		{
			var importDeclarationMessageBuilder = new ImportDeclarationMessageBuilder(ImportDeclarationWrapper.NewOrNull(entryHeader)) as CargoWise.Customs.Shared.MessageContracts.IXmlMessageBuilder;
			return importDeclarationMessageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		readonly CusEntryHeader entryHeader;
	}
}
