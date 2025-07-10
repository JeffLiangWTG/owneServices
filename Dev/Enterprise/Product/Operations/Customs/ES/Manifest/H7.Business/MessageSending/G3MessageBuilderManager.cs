using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3MessageBuilderManager
	{
		public G3MessageBuilderManager(IEnumerable<G3MessageSendingObject> selectedSendingObjects, ICertificateProvider certificateProvider, string localReferenceNumber)
		{
			this.selectedSendingObjects = Argument.NotNull(selectedSendingObjects, nameof(selectedSendingObjects));
			this.certificateProvider = Argument.NotNull(certificateProvider, nameof(certificateProvider));
			this.localReferenceNumber = Argument.NotNullOrEmpty(localReferenceNumber, nameof(localReferenceNumber));
		}

		readonly IEnumerable<G3MessageSendingObject> selectedSendingObjects;
		readonly ICertificateProvider certificateProvider;
		readonly string localReferenceNumber;

		public IMessageBuilderBase NewMessageBuilder()
		{
			var bills = selectedSendingObjects.Select(x => x.Bill);
			var messageType = selectedSendingObjects.FirstOrDefault().Action;
			var messageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;

			switch (messageType)
			{
				case G3MessageTypes.Codes.G3Declaration:
					return new G3PresentGoodMessageBuilder(new G3PresentGoodMessageWrapper(bills, certificateProvider, localReferenceNumber), messageType, messageSubType);
				case G3MessageTypes.Codes.G3Revoke:
					var revokeReasonDictionary = selectedSendingObjects.ToDictionary(x => x.Bill.PK, x => new DocumentCommonWrapper(x.RevokeReasonDescription, x.RevokeReason) as IDocumentsCommon);
					return new G3RevokeGoodMessageBuilder(new G3RevokeGoodMessageWrapper(bills, revokeReasonDictionary, certificateProvider, localReferenceNumber), messageType, messageSubType);
				default:
					throw new NotImplementedException("CW1 doesn't yet support building message type " + messageType);
			}
		}
	}
}
