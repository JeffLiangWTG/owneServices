using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public sealed class CC917CMessageInterpreter : BaseMessageInterpreter<ICC917CDataProvider>
{
	public override string Interpret(ICC917CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var note = new ZStringBuilder();
		var xmlErrorCodes = new XMLErrorCodes();
		note.Append((NoResString)"New transaction status: XML error. Xml gives xsd errors.");
		foreach (var xmlError in dataProvider.XMLErrorList)
		{
			ZString xmlErrorCode = xmlError.ErrorCode;
			note.Append("");
			note.Append($"Error Line: {xmlError.ErrorLineNumber}");
			note.Append($"Error Column: {xmlError.ErrorColumnNumber}");
			note.Append($"Error Pointer: {xmlError.ErrorPointer}");
			note.Append($"Error Code: {xmlErrorCode} {xmlErrorCodes.GetDescriptionFromCode(xmlErrorCode)}");
			note.Append($"Error Text: {xmlError.ErrorText}");
			note.Append($"Original value: {xmlError.OriginalAttributeValue}");
		}
		return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
	}
}
