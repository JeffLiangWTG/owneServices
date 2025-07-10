using CargoWise.Customs.IL.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ILDOC271MessageBuilder : IL.Business.Message.MessageBuilder.ILDOC271MessageBuilder
	{
		public ILDOC271MessageBuilder(IEDIMessageCollectionOwner messageOwner, SupportingDocument supportingDocument)
			: base(messageOwner, supportingDocument)
		{ }

		protected override string GetMessageText()
		{
			var supportingDocumentMessageBuilder = new SupportingDocumentMessageBuilder(MessageSupportingDocumentWrapper.NewOrNull((SupportingDocument)supportingDocument)) as IXmlMessageBuilder;
			return supportingDocumentMessageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		protected override ILEDIMessage GetMessage() => supportingDocument.Factory.New<ILDOC271RequestMessage>();
	}
}
