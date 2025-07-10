using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS906;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS906MessagePrettier : PNTSMessagePrettier<Iets906>
	{
		public IETS906MessagePrettier(IETS906MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Iets906 messageObject)
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
				var cell5 = new CellWithFormatting("Original Attribute Value", "width", "135px");
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2, cell3, cell4, cell5 });

				foreach (var technicalError in errors)
				{
					tableCreator.WriteRow(technicalError.SequenceNumber
						, technicalError.ErrorPointer
						, technicalError.ErrorCode
						, technicalError.ErrorReason
						, technicalError.OriginalAttributeValue
					);
				}
			}

			return ToTableSection("Technical Errors", tableCreator);
		}

		protected override ZString GetMessageInterpretationCore(Iets906 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", (NoResString)"Message was technically rejected by customs"),
				((NoResString)"Correlation ID", messageObject.MessageHeader.CorrelationId),
			});
		}
	}
}
