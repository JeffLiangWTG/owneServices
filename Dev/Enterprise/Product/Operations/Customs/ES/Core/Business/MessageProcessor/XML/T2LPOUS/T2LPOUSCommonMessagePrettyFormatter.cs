using System.Collections.ObjectModel;
using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Business
{
	public abstract class T2LPOUSCommonMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		protected ZString GetMessageDetailsAccepted(string preparationDateAndTime, string mrn, string circuit, string csvT2L, string csvElectronicDecl)
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);
			messageDetails.Append(blankLine);

			ZDateTime.TryParseExact(preparationDateAndTime, out var admissionDate, CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss);
			AppendAcceptanceDateIfNotEmpty(messageDetails, admissionDate);
			AppendMrnDataIfNotEmpty(messageDetails, mrn);
			AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(circuit));
			AppendGroupCSV(messageDetails, csvT2L, admissionDate, csvElectronicDecl);

			return messageDetails.ToString();
		}

		protected ZString GetT2LMessageDetailsRejected(IT2LPOUSErrors responseWithErrors)
		{
			return GetT2LMessageDetailsRejectedCommon(responseWithErrors.T2LPOUSErrors);
		}

		protected ZString GetT2LMessageDetailsRejectedCommon(Collection<IT2LPOUSError> errors)
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(RejectedDeclarationText);
			messageDetails.Append(ListOfErrorsText);

			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorCodeColumnText, ErrorPlaceColumnText, ErrorReasonColumnText, ErrorOriginalValueColumnText);

			foreach (var error in errors)
			{
				tableCreator.WriteRow(error.Code, error.Pointer, error.Reason, error.OriginalValue);
			}

			messageDetails.Append(tableCreator.ToHtml());

			return messageDetails.ToString();
		}
	}
}
