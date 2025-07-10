using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS029;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS029MessagePrettier : PNTSMessagePrettier<Iets029>
	{
		public IETS029MessagePrettier(IETS029MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Iets029 messageObject)
		{
			HtmlTableCreator tableCreator = null;
			var errors = messageObject.ActivationError;
			if (messageObject.ActivationErrorSpecified && errors.Count > 0)
			{
				tableCreator = GetHtmlTableCreator();
				var cell1 = new CellWithFormatting("Error reason", "width", "100px");
				var cell2 = new CellWithFormatting("Remarks", "width", "200px");
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2 });

				foreach (var activationError in errors)
				{
					tableCreator.WriteRow(activationError.ErrorReason, activationError.Remarks);
				}
			}

			return ToTableSection("Activation Errors", tableCreator);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretationCore(Iets029 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", MessageDataObject.GetCustomsStatusDescriptionFromMessage()),
				("MRN", messageObject.Mrn),
				("CRN", messageObject.Crn),
				("FRN", messageObject.RelatedPn?.Frn ?? ZString.Empty),
				("Notification Date", messageObject.NotificationDate.ToString()),
				("Date and time of presentation of goods", messageObject.RelatedPn?.DateAndTimeOfPresentationOfTheGoods.ToString() ?? ZString.Empty),
			});
		}
	}
}
