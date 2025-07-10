using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE917;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE917MessagePrettier : DeltaIEMessagePrettier<CC917BType>
	{
		public IE917MessagePrettier(IE917MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(CC917BType messageObject)
		{
			var tableCreator = GetHtmlTableCreator();
			var mrn = messageObject.Operation?.MRN ?? "N/A";
			var cellMRN = new CellWithFormatting($"MRN# {mrn}", "colspan", "6");
			tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "left" } }, cellMRN);

			var functionalErrors = messageObject.XMLError;
			if (functionalErrors?.Count > 0)
			{
				var cell1 = new CellWithFormatting("Line Number", "width", "75px");
				var cell2 = new CellWithFormatting("Column Number", "width", "75px");
				var cell3 = new CellWithFormatting("Pointer", "width", "75px");
				var cell4 = new CellWithFormatting("Error Code", "width", "100px");
				var cell5 = new CellWithFormatting("Error Text", "width", "135px");
				var cell6 = new CellWithFormatting("Original Attribute Value", "width", "100px");
				tableCreator.WriteRowWithFormatting(
					new NameValueCollection { { "align", "center" } },
					new[] { cell1, cell2, cell3, cell4, cell5, cell6 });

				foreach (var functionalError in functionalErrors)
				{
					tableCreator.WriteRow(
						functionalError.ErrorLineNumber,
						functionalError.ErrorColumnNumber,
						functionalError.ErrorPointer,
						functionalError.ErrorCode,
						functionalError.ErrorText,
						functionalError.OriginalAttributeValue);
				}
			}

			return ToTableSection("Functional Errors", tableCreator);
		}

		protected override ZString GetMessageInterpretationCore(CC917BType messageObject)
		{
			var result = ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", (NoResString)"Message was technically rejected by customs")
			});
			return result;
		}
	}
}
