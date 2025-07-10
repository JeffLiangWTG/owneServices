using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.ECom;
using CargoWise.Customs.CH.MessageDefinitions.Edec.ECom.Version1_0;

namespace Enterprise.Customs.CH.Business;

public class EdecComplaintRequestLineDataProvider : IEdecComplaintRequestLine
{
	public static IEnumerable<EdecComplaintRequestLineDataProvider> NewCollection(EComplaintMessageSendingObject sendingObject)
	{
		return sendingObject?.SendingObjectLines.Cast<EComplaintMessageSendingObjectLine>().Select(x => New(x)) ?? Enumerable.Empty<EdecComplaintRequestLineDataProvider>();
	}

	public static EdecComplaintRequestLineDataProvider New(EComplaintMessageSendingObjectLine sendingObjectLine)
	{
		return sendingObjectLine == null ? null : new EdecComplaintRequestLineDataProvider(sendingObjectLine);
	}

	EdecComplaintRequestLineDataProvider(EComplaintMessageSendingObjectLine sendingObjectLine)
	{
		this.sendingObjectLine = Argument.NotNull(sendingObjectLine, nameof(sendingObjectLine));
	}
	readonly EComplaintMessageSendingObjectLine sendingObjectLine;

	public string Location => (sendingObjectLine.Location == EComplaintLocationList.Codes.Line ? ComplaintLineTypeLocation.Position : ComplaintLineTypeLocation.Header).ToString();

	public string TraderItemID => sendingObjectLine.EntryLineNumber;

	public string ElementName => sendingObjectLine.FieldName;

	public string Remark => sendingObjectLine.Remark;
}
