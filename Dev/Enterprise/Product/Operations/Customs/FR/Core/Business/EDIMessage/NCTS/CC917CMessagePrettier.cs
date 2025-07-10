using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC917C;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC917CMessagePrettier : NCTSMessagePrettier<Cc917CType>
	{
		public CC917CMessagePrettier(CC917CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Cc917CType messageObject)
		{
			var tableCreator = GetHtmlTableCreator();
			var cellStatus = new CellWithFormatting($"Status: Technical Rejection", "colspan", "6");
			var lrn = messageObject.Header?.Lrn ?? "N/A";
			var cellLRN = new CellWithFormatting($"LRN: {lrn}", "colspan", "6");
			var mrn = messageObject.Header?.Mrn ?? "N/A";
			var cellMRN = new CellWithFormatting($"MRN: {mrn}", "colspan", "6");
			var crn = messageObject.CorrelationIdentifier ?? "N/A";
			var cellCRN = new CellWithFormatting($"CRN: {crn}", "colspan", "6");
			tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "left" } }, cellStatus);
			tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "left" } }, cellLRN);
			tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "left" } }, cellCRN);
			tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "left" } }, cellMRN);

			var functionalErrors = messageObject.XmlError;
			if (functionalErrors?.Count > 0)
			{
				var cell1 = new CellWithFormatting("Error Code", "width", "100px");
				var cell2 = new CellWithFormatting("Error Code description", "width", "100px");
				var cell3 = new CellWithFormatting("Field Code", "width", "75px");
				var cell4 = new CellWithFormatting("Error Reason", "width", "100px");
				var cell5 = new CellWithFormatting("Original Value", "width", "100px");
				var cell6 = new CellWithFormatting("Remarks", "width", "100px");
				tableCreator.WriteRowWithFormatting(
					new NameValueCollection { { "align", "center" } },
					new[] { cell1, cell2, cell3, cell4, cell5, cell6 });

				var errorCodeList = RefCusCodeListTypes.GetCachedList(
					Factory,
					Core.Constants.CountryCodes.France,
					EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL030,
					ZDateTime.Today);

				foreach (var functionalError in functionalErrors)
				{
					var errorCode = functionalError.ErrorCode.ToString().Substring(4);

					tableCreator.WriteRow(
						errorCode,
						errorCodeList.GetDescriptionFromCode(errorCode),
						functionalError.ErrorPointer,
						functionalError.ErrorText,
						functionalError.OriginalAttributeValue,
						GetRemark(functionalError.ErrorCode.ToString()));
				}
			}

			return ToTableSection("Errors", tableCreator);
		}

		protected override ZString GetMessageInterpretationCore(Cc917CType messageObject)
		{
			return null;
		}
	}
}
