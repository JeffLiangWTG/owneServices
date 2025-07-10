using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLineCustomsLineLinkedObjectAdapter : ISadCustomsLineLinkedObjectAdapter
{
	public CusEntryLineCustomsLineLinkedObjectAdapter(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}

	readonly CusEntryLine entryLine;

	ZInt ISadCustomsLineLinkedObjectAdapter.LineNo => entryLine.CL_LineNumber;

	ZString ISadCustomsLineLinkedObjectAdapter.NBStatus => entryLine.ZG_NBStatus;

	void ISadCustomsLineLinkedObjectAdapter.SetNBStatus(ZString nbStatus) => entryLine.ZG_NBStatus = nbStatus;
}
