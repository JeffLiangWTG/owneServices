using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS016;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS016MessagePrettier : PNTSMessagePrettier<Iets016>
	{
		public IETS016MessagePrettier(IETS016MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Iets016 messageObject)
		{
			HtmlTableCreator tableCreator = null;

			var errors = messageObject.Error;
			if (errors.Count > 0)
			{
				tableCreator = GetHtmlTableCreator();
				var cell1 = new CellWithFormatting("Sequence Number", "width", "100px");
				var cell2 = new CellWithFormatting("Error Pointer", "width", "100px");
				var cell3 = new CellWithFormatting("Error Code", "width", "100px");
				var cell4 = new CellWithFormatting("Error Reason", "width", "100px");
				var cell5 = new CellWithFormatting("Remarks", "width", "135px");
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2, cell3, cell4, cell5 });

				foreach (var functionalError in errors)
				{
					tableCreator.WriteRow(functionalError.SequenceNumber
						, functionalError.ErrorPointer
						, functionalError.ErrorCode
						, functionalError.ErrorReason
						, functionalError.Remarks
					);
				}
			}

			return ToTableSection("Functional Errors", tableCreator);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretationCore(Iets016 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", "Functional rejection"),
				("LRN", messageObject.Lrn),
				("Notification Date", messageObject.NotificationDate.ToString()),
				("Business Validation", messageObject.BusinessValidationType),
			});
		}
	}
}
