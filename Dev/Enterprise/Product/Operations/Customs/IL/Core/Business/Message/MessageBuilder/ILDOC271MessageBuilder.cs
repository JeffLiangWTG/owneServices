using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	public class ILDOC271MessageBuilder : ILMessageBuilderBase
	{
		public ILDOC271MessageBuilder(IEDIMessageCollectionOwner messageOwner, SupportingDocument supportingDocument)
			: base(messageOwner?.MessageOwner)
		{
			this.messageOwner = Argument.NotNull(messageOwner, nameof(messageOwner));
			this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		}

		protected override string GetMessageText() => string.Empty;

		protected override ILEDIMessage GetMessage() => supportingDocument.Factory.New<ILDOC271RequestMessage>();

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => messageOwner.Messages;

		readonly IEDIMessageCollectionOwner messageOwner;
		protected readonly SupportingDocument supportingDocument;
	}
}
