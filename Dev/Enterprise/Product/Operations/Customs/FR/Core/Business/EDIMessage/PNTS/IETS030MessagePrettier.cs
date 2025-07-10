using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS030;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS030MessagePrettier : PNTSMessagePrettier<Iets030>
	{
		public IETS030MessagePrettier(IETS030MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Iets030 messageObject)
		{
			HtmlTableCreator tableCreator = null;
			var errors = messageObject.LinkingError;
			if (messageObject.LinkingErrorSpecified && errors.Count > 0)
			{
				tableCreator = GetHtmlTableCreator();
				var cell1 = new CellWithFormatting("Error reason", "width", "100px");
				var cell2 = new CellWithFormatting("Remarks", "width", "200px");
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2 });

				foreach (var linkingError in errors)
				{
					tableCreator.WriteRow(linkingError.ErrorReason, linkingError.Remarks);
				}
			}

			return ToTableSection("Linking Errors", tableCreator);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretationCore(Iets030 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", MessageDataObject.GetCustomsStatusDescriptionFromMessage()),
				("MRN", messageObject.RelatedTsd?.Mrn ?? ZString.Empty),
				("CRN", messageObject.RelatedTsd?.Crn ?? ZString.Empty),
				("FRN", messageObject.Frn),
				("Notification Date", messageObject.NotificationDate.ToString()),
				("Remarks", messageObject.Remarks),
			}, true);
		}
	}
}
