using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public class IE906MessageInterpreter : BaseMessageInterpreter<IIE906DataProvider>
{
	public override string Interpret(IIE906DataProvider dataProvider, EDIMessage ediMessage)
	{
		var note = new ZStringBuilder();
		note.Append((NoResString)"New status: XML error. Xml gives technical errors.");
		foreach (var functionalError in dataProvider.FunctionalErrors)
		{
			note.Append(string.Empty);
			note.Append($"Error SequenceNumber: {functionalError.SequenceNumber}");
			note.Append($"Error Pointer: {functionalError.ErrorPointer}");
			note.Append($"Error Code: {functionalError.ErrorCode}");
			note.Append($"Error Reason: {functionalError.ErrorReason}");
			note.Append($"Value associated with error: {functionalError.OriginalAttributeValue}");
		}
		return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
	}
}
