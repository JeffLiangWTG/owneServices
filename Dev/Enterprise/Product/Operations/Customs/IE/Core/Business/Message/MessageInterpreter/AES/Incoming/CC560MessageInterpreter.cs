using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC560MessageInterpreter : InboundMessageInterpreter<CC560CProvider>
	{
		public CC560MessageInterpreter(AESInboundEDIMessage message, CC560CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"DD46A009-49DA-4F22-AA2C-EBFCC73FDCD9",
			"An Export Control message has been received from Customs for Job {0} through the IE560 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("35751903-4CDF-42E6-9D37-67BFBDC5EC7E", "Controlled for Export (CON1)"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.ControlNotificationDateAndTime, provider.ControlNotificationDateAndTime.ToLongTimeString());
			yield return (Res.GetString("CABCD240-3897-4F98-9A48-EB49A7AEA769", "Control Notification Type"), GetCodeAndDescription(provider.ControlNotificationType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.NotificationType));
			yield return (Res.GetString("7682750A-FFFF-4BF1-A2B8-2FA5274B817D", "Notification Text"), provider.ControlNotificationText);

			var documentTypeCodes = RefCusCodeListTypes.GetCachedList(message.Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.SupportingDocumentExportType, MessageCreatedDate);
			var controlTypeCodes = RefCusCodeListTypes.GetCachedList(message.Factory, Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, MessageCreatedDate);
			foreach (var pair in provider.ControlTypes.UnevenZipExtension(provider.RequestedDocuments, (c, d) => new { Control = c, DocRequest = d }))
			{
				if (pair.Control != null)
				{
					yield return (CommonResStrings.ControlType, MessageInterpreterHelper.GetCodeAndDescription(pair.Control.Type, controlTypeCodes));
					yield return (CommonResStrings.ControlText, pair.Control.Text);
				}
				if (pair.DocRequest != null)
				{
					yield return (CommonResStrings.RequestedDocumentType, MessageInterpreterHelper.GetCodeAndDescription(pair.DocRequest.DocumentType, documentTypeCodes));
					yield return (CommonResStrings.RequestedDocumentDescription, pair.DocRequest.Description);
				}
			}
		}
	}
}
