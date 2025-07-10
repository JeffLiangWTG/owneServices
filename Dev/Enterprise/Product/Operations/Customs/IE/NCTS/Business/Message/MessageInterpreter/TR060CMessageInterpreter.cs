using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR060CMessageInterpreter : InboundMessageInterpreter<TR060CProvider>
	{
		public TR060CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR060CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"E4814D08-0E29-4CB2-82D4-2390F198BE2F",
			"A control message (TR060) from Office of Destination has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (NctsCommonResStrings.CustomsOfficeOfDestination, provider.CustomsOfficeOfDestination);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.ControlNotificationDateAndTime, provider.ControlNotificationDateAndTime.ToShortDateString());
			yield return (NctsCommonResStrings.NotificationType, GetCodeAndDescription(provider.NotificationType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL384));
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var controlType in provider.ControlTypes)
			{
				yield return (Res.GetString("4785D32F-D57A-41AD-9801-C30E33D6CC5B", "Control Types:"), new (string, string)[]
				{
					(CommonResStrings.ControlType, controlType.Type),
					(CommonResStrings.ControlText, controlType.Text),
				});
			}

			foreach (var requestedDocuments in provider.RequestedDocuments)
			{
				yield return (Res.GetString("0628D709-4124-49E7-BAA6-6060EE98A85B", "Requested Documents:"), new (string, string)[]
				{
					(CommonResStrings.RequestedDocumentType, requestedDocuments.DocumentType),
					(CommonResStrings.RequestedDocumentDescription, requestedDocuments.Description),
				});
			}
		}
	}
}
