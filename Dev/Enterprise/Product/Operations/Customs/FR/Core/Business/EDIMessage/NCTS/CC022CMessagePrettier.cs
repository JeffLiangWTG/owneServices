using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC022C;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
namespace Enterprise.Customs.FR.Business.EdiMessages
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
	public class CC022CMessagePrettier : NCTSMessagePrettier<Cc022CType>
	{
		public CC022CMessagePrettier(NCTSMessageDataObject<Cc022CType> messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc022CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;

			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", TP5ResponseMessageSubTypeList.Descriptions.NotificationToAmendDeclaration),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				("Amendment Notification Date and Time", transitOperation?.AmendmentNotificationDateAndTime.ToString() ?? ZString.Empty)
			});
		}

		protected override ZString GetErrorsTableIfNeeded(Cc022CType messageObject)
		{
			var functionalErrors = messageObject.FunctionalError;
			if (functionalErrors.Count == 0)
			{
				return ZString.Empty;
			}

			var tableCreator = GetHtmlTableCreator();
			var headers = new[]
			{
				new CellWithFormatting("Level", "width", "75px"),
				new CellWithFormatting("Error Code", "width", "100px"),
				new CellWithFormatting("Field Code", "width", "100px"),
				new CellWithFormatting("Error Reason", "width", "150px"),
				new CellWithFormatting("Original Value", "width", "125px"),
				new CellWithFormatting("Remarks", "width", "150px")
			};
			tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, headers);

			foreach (var functionalError in functionalErrors)
			{
				tableCreator.WriteRow(ZString.Empty,
					GetErrorCodeNumber(functionalError.ErrorCode.ToString()),
					GetFieldCode(functionalError.ErrorPointer),
					functionalError.ErrorReason,
					AppendWordBreaksToPath(functionalError.OriginalAttributeValue),
					GetRemark(functionalError.ErrorCode.ToString()));
			}

			return ToTableSection("Functional Errors", tableCreator);
		}
	}
}
