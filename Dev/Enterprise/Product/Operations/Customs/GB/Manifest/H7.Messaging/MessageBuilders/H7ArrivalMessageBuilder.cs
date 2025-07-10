using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class H7ArrivalMessageBuilder : H7ArrivalAndCancelBaseMessageBuilder, IGbCDSMessageBuilder
	{
		public H7ArrivalMessageBuilder(MessageSendingObject objectToSend, string functionCode)
			: base(objectToSend, functionCode)
		{
		}

		public override ZString Build()
		{
			ZString messageText = string.Empty;

			var metaData = CreateMetaData();
			var metaDeclaration = CreateMetaDeclaration();

			messageText = EnsureCorrectOrder(metaData
							.SetDeclaration(metaDeclaration)
							.Serialize());

			return messageText.FormatXml();
		}

		protected override ZString TypeCode => Constants.ThreeCharFunctionCodes.DeclarationArrivalNotification;

		protected ZString EnsureCorrectOrder(ZString messageText)
		{
			var messageTextWithOrderedAdditionalInformations = AmendmentMessageHelper.OrderAdditionalInformations(messageText);
			var metaData = XmlObjectSerializer.Deserialize<MetaData>(messageTextWithOrderedAdditionalInformations);
			var declaration = metaData.GetDeclaration();
			var fullyOrderedDeclaration = new MetaData().SetDeclaration(declaration).Serialize();
			return fullyOrderedDeclaration;
		}

		protected override IEnumerable<DeclarationAmendment> Amendments => null;

		protected override DeclarationAdditionalInformation[] AdditionalInformation => null;
	}
}
