using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM460MessageInterpreter : InboundMessageInterpreter<IIM460Provider>
	{
		public IM460MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM460Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM460Provider messageProvider;

		protected override string Summary => Res.GetString("52C821F7-8D52-43C4-BAB1-830550A5F51A", "A Control Notice (IM460) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.CustomsRegistrationNumber, messageProvider.CustomsRegistrationNumber);
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (Res.GetString("8C01EBB0-3947-4F66-AA7A-F3E344732B6F", "Notification Date"), messageProvider.NotificationDate.ToShortDateString());
			yield return (Res.GetString("0B993B2B-CDB1-4182-AF45-4094B5B14841", "Notification Type"), messageProvider.NotificationType);
			yield return (Res.GetString("3DDEA2AA-1FB7-49E3-A650-B01B42DECDB5", "Anticipated Control Date"), messageProvider.AnticipatedControlDate.ToShortDateString());
			yield return (Res.GetString("E2D2E7E7-8AB8-4657-AF89-46F36E3B5A25", "Text"), messageProvider.Text);
			yield return (Res.GetString("F968F981-A0AC-4099-891B-601C11A3CF3F", "Overall Control Type"), MessageInterpreterHelper.GetCodeAndDescription(messageProvider.OverallControlTypeCode, new OverallControlTypeList()));
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var typeOfControl in messageProvider.TypeOfControls)
			{
				yield return (Res.GetString("A729171A-4F10-4700-BDC1-EFE1F9A19EEE", "Type of Control:"), new (string, string)[]
				{
					(CommonResStrings.SequenceNumber, typeOfControl.SequenceNumber),
					(CommonResStrings.ControlType, GetCodeAndDescription(typeOfControl.Type, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL716)),
					(CommonResStrings.ControlText, typeOfControl.Text),
				});
			}

			foreach (var requestedDocument in messageProvider.RequestedDocuments)
			{
				yield return (Res.GetString("170A0B6A-283D-4036-AA1A-E84F0F68A506", "Requested Document:"), new (string, string)[]
				{
					(CommonResStrings.SequenceNumber, requestedDocument.SequenceNumber),
					(CommonResStrings.RequestedDocumentType, GetCodeAndDescription(requestedDocument.DocumentType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL215)),
					(Res.GetString("E47AE3DD-7BF9-48F3-ADE2-2D976925D384", "cc Qualifier"), requestedDocument.CcQualifier),
					(CommonResStrings.ReferenceNumber, requestedDocument.ReferenceNumber),
					(CommonResStrings.RequestedDocumentDescription, requestedDocument.RequestInformation),
				});
			}
		}
	}
}
