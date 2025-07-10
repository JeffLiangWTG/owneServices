using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMHeaderWarehouseIdentificationWrapper : IWarehouseIdentification
{
	public IMHeaderWarehouseIdentificationWrapper(Customs.Business.CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
	}
	readonly Customs.Business.CusEntryInstruction entryInstruction;

	ZString WarehouseCode => entryInstruction.ToWarehouseCode;

	public ZString Type => WarehouseCode.SubstringSafe(0, 1);

	public ZString Identification => WarehouseCode.SubstringSafe(1, 6);

	public ZString CinIdentification => WarehouseCode.SubstringSafe(7, 1);

	public ZString AuthorizingCountry => WarehouseCode.SubstringSafe(8, 2);
}
