using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM429;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM429Provider : IIM429Provider
	{
		public IM429Provider(Im429 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im429 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.Declaration?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZString AdditionalDeclarationType => xmlObject.Declaration?.AdditionalDeclarationType;

		public ZString PreferredPaymentMethod => xmlObject.Declaration?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public string GetEntryStatus(EDIMessage message)
		{
			messageCreatedDate = message.EM_SystemCreateTimeUtc;
			return AISEntryStatusList.Codes.Released;
		}

		public void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				var releaseDate = messageCreatedDate ?? ZDateTime.Now;
				aisMessageAttachee.SetEntryReleaseDate(releaseDate);
				aisMessageAttachee.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsCleared, releaseDate.ToOffset()));

				foreach (var requestedDocument in aisMessageAttachee.RequestedDocumentsProvider.RequestedDocuments.Where(x => !x.CSI_Status.EqualsIgnoringCase(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived)))
				{
					requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
				}
			}
		}

		ZDateTime? messageCreatedDate;
	}
}
