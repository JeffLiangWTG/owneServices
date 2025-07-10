using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE426MessagePrettier : DeltaIEMessagePrettier<CC426BType>
	{
		public IE426MessagePrettier(IE426MessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC426BType messageObject)
		{
			var importOperation = messageObject.ImportOperation;
			var declarationStatus = messageObject.DeclarationStatus;
			var result = ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", declarationStatus?.State ?? ZString.Empty),
				("Status Date", declarationStatus?.StateDateTime ?? ZString.Empty),
				("Registration Date Time", importOperation?.DeclarationRegistrationDateAndTime ?? ZString.Empty),
				("LRN", importOperation?.LRN ?? ZString.Empty),
				("CRN", importOperation?.CustomsRegistrationNumber ?? ZString.Empty),
				("Presentation Notification Due Date", importOperation?.PresentationNotificationDueDate ?? ZString.Empty),
				("Estimated Presentation Notification Date", importOperation?.PresentationNotificationEstimatedDateAndTime ?? ZString.Empty),
			});

			var content = ZString.Empty;

			if (messageObject.Remarks != null && messageObject.Remarks.Count > 0)
			{
				result += "<style> table, th, td {border: 1px solid black; border-collapse: collapse;} th, td { padding: 10px; text-align: left;}</style><table>";
				var totalRows = messageObject.Remarks.Count * 3;
				content = ToTd(ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Remarks")), totalRows, 1);

				foreach (var remark in messageObject.Remarks)
				{
					content += ToTd("Sequence Number", 1, 1);
					content += ToTd(remark?.SequenceNumber ?? ZString.Empty, 1, 1);
					EndOfARow(ref result, ref content);

					content += ToTd("Code", 1, 1);
					content += ToTd(remark?.Code ?? ZString.Empty, 1, 1);
					EndOfARow(ref result, ref content);

					content += ToTd("Reason", 1, 1);
					content += ToTd(remark?.Reason ?? ZString.Empty, 1, 1);
					EndOfARow(ref result, ref content);
				}
				result += "<table>";
			}

			return result;
		}
	}
}
