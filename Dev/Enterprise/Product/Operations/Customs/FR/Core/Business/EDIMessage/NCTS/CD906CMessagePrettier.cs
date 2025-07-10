using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CD906C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	internal class CD906CMessagePrettier : NCTSMessagePrettier<Cd906CType>
	{
		public CD906CMessagePrettier(CD906CMessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Cd906CType messageObject)
		{
			HtmlTableCreator tableCreator = null;
			var functionalErrors = messageObject.FunctionalError;
			if (functionalErrors.Count > 0)
			{
				tableCreator = GetHtmlTableCreator();
				var cell1 = new CellWithFormatting("Level", "width", "75px");
				var cell2 = new CellWithFormatting("Error Code", "width", "100px");
				var cell3 = new CellWithFormatting("Error Code description", "width", "150px");
				var cell4 = new CellWithFormatting("Field Code", "width", "100px");
				var cell5 = new CellWithFormatting("Error Reason", "width", "150px");
				var cell6 = new CellWithFormatting("Original Value", "width", "125px");
				var cell7 = new CellWithFormatting("Remarks", "width", "150px");
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2, cell3, cell4, cell5, cell6, cell7 });

				foreach (var functionalError in functionalErrors)
				{
					tableCreator.WriteRow(ZString.Empty
				, GetErrorCodeNumber(functionalError.ErrorCode.ToString())
				, GetErrorCodeDescription(functionalError.ErrorCode.ToString())
				, GetFieldCode(functionalError.ErrorPointer)
				, functionalError.ErrorReason
				, AppendWordBreaksToPath(functionalError.OriginalAttributeValue)
				, GetRemark(functionalError.ErrorCode.ToString()));
				}
			}

			return ToTableSection("Functional Errors", tableCreator);
		}
		protected override ZString GetMessageInterpretationCore(Cd906CType messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", NCTS5DepartureCustomsStatusList.Descriptions.RejectedAtOrigin ?? ZString.Empty),
				("LRN", ZString.Empty),
				("CRN", messageObject.CorrelationIdentifier ?? ZString.Empty),
				("MRN", messageObject.Header?.Mrn ?? ZString.Empty),
				((NoResString)"Rejection Type", GetMessageTypeDescription(messageObject.MessageType.ToString())),
			});
		}
	}
}
