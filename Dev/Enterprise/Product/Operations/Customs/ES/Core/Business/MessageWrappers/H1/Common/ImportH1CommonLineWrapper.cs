using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonLineWrapper : ICommonH1GoodsShipmentItem
{
	public ImportH1CommonLineWrapper(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}
	protected readonly CusEntryLine entryLine;

	public ZString DeclarationGoodsItemNumber => entryLine.CL_LineNumber.ToString();
}
