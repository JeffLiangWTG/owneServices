using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public sealed class CC556CMessageInterpreter : BaseMessageInterpreter<ICC556CDataProvider>
{
	public override string Interpret(ICC556CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var noteHTMLPresentation = new ZStringBuilder();
		var businessRejectionTypeDescription = new ExportOperationBusinessRejectionTypeList().GetWithDescription(dataProvider.BusinessRejectionType);
		AddHTMLNoteTextLineInterpretation(noteHTMLPresentation, $"Declaration received an <span style='color:red'> error </span> for type ({businessRejectionTypeDescription}) on {dataProvider.RejectionDateAndTime:dd/MM/yyyy HH:mm:ss}");
		var rejectionDescription = new ExportOperationBusinessRejectionTypeList().GetWithDescription(dataProvider.RejectionCode);
		AddHTMLNoteTextLineInterpretation(noteHTMLPresentation, $"Reason:({rejectionDescription}) {dataProvider.RejectionReason}");
		AddHtmlTableInterpretation(noteHTMLPresentation, "300px", GetInterpretationSummaryItems(dataProvider.FunctionalErrorList));
		return noteHTMLPresentation.ToString();
	}

	static IEnumerable<KeyValuePair<string, string>> GetInterpretationSummaryItems(IEnumerable<IFunctionalError> functionalErrorList)
	{
		var functionErrorCodeList = new FunctionErrorCodeList();
		foreach (var item in functionalErrorList)
		{
			yield return new KeyValuePair<string, string>((NoResString)"Functional error code:", "(" + functionErrorCodeList.GetWithDescription(item.ErrorCode) + ")");
			yield return new KeyValuePair<string, string>((NoResString)"Reason:", item.ErrorReason);
			yield return new KeyValuePair<string, string>((NoResString)"Attribute:", item.ErrorPointer);
			yield return new KeyValuePair<string, string>((NoResString)"Element in declaration contains now the value:", item.OriginalAttributeValue);
		}
	}
}
