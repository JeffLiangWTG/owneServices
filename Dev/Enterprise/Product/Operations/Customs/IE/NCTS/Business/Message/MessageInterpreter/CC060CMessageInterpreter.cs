using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC060CMessageInterpreter : InboundMessageInterpreter<CC060CProvider>
	{
		public CC060CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC060CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"FDDCA86E-C641-4F7E-A115-5205B02579D9",
			"A Control Decision Notification (IE060) message has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.ControlNotificationDateAndTime, provider.ControlNotificationDateAndTime.ToLongTimeString());
			yield return (NctsCommonResStrings.NotificationType, GetCodeAndDescription(provider.NotificationType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL384));
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var typeOfControl in provider.TypeOfControls)
			{
				yield return (Res.GetString("B870C45E-4E0E-43C7-B13B-CF33EC658245", "Type of Control:"), new (string, string)[]
				{
					(CommonResStrings.SequenceNumber, typeOfControl.SequenceNumber.ToString()),
					(CommonResStrings.ControlType, GetCodeAndDescription(typeOfControl.Type, IE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL716)),
					(CommonResStrings.ControlText, typeOfControl.Text),
				});
			}

			foreach (var requestedDocument in provider.RequestedDocuments)
			{
				yield return (Res.GetString("8F4AAC02-7C66-4317-ACEF-9510DA49392C", "Requested Document:"), new (string, string)[]
				{
					(CommonResStrings.SequenceNumber, requestedDocument.SequenceNumber.ToString()),
					(CommonResStrings.RequestedDocumentType, GetCodeAndDescription(requestedDocument.DocumentType, IE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL215)),
					(CommonResStrings.RequestedDocumentDescription, requestedDocument.Description),
				});
			}
		}
	}
}
