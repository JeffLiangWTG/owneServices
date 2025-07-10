using System.Collections.Specialized;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE456;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE456MessagePrettier : DeltaIEMessagePrettier<CC456BType>
	{
		public IE456MessagePrettier(IE456MessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC456BType messageObject)
		{
			var importOperation = messageObject.ImportOperation.FirstOrDefault();
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", "Functional rejection"), ("LRN", importOperation?.LRN ?? ZString.Empty),
				("CRN", importOperation?.CustomsRegistrationNumber ?? ZString.Empty),
				("MRN", importOperation?.MRN ?? ZString.Empty),
				("Rejection Type", importOperation?.BusinessRejectionType ?? ZString.Empty),
				("Rejection Date Time", importOperation?.RejectionDateAndTime ?? ZString.Empty),
				("Rejection Reason", importOperation?.RejectionReason ?? ZString.Empty)
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(CC456BType messageObject)
		{
			HtmlTableCreator tableCreator = null;
			var functionalErrors = messageObject.FunctionalError;
			if (functionalErrors.Count > 0)
			{
				tableCreator = GetHtmlTableCreator();
				var cell1 = new CellWithFormatting("Sequence Number", "width", "100px");
				var cell2 = new CellWithFormatting("Error Code", "width", "100px");
				var cell3 = new CellWithFormatting("Field Code", "width", "75px");
				var cell4 = new CellWithFormatting("Error Reason", "width", "100px");
				var cell5 = new CellWithFormatting("Path", "width", "75px");
				var cell6 = new CellWithFormatting("Remarks", "width", "135px");
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2, cell3, cell4, cell5, cell6 });

				foreach (var functionalError in functionalErrors)
				{
					tableCreator.WriteRow(functionalError.SequenceNumber
						, functionalError.ErrorCode
						, GetFieldCode(functionalError.ErrorPointer)
						, functionalError.ErrorReason
						, AppendWordBreaksToPath(functionalError.ErrorPointer)
						, functionalError.Remarks
					);
				}
			}

			return ToTableSection("Functional Errors", tableCreator);
		}
	}
}
