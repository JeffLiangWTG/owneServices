using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETHeaderAgreedLocationOfGoodsWrapper : IETHeaderAgreedLocationOfGoods
{
	public ETHeaderAgreedLocationOfGoodsWrapper(CusEntryHeader entryHeader)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
	}

	readonly JobDeclaration declaration;
	readonly CusEntryInstruction entryInstruction;

	public ZString AgreedLocationOfGoodsCode => ZString.Empty;

	public ZString AgreedLocationOfGoodsDescription
	{
		get
		{
			var locationOfGoods = declaration.JE_LocationOfGoods;
			return entryInstruction.ElectronicDocuments && IsGoodsLocationDOrF(locationOfGoods) ? locationOfGoods : ZString.Empty;
		}
	}

	public ZString AuthorizedLocationOfGoodsCodeAndCin
	{
		get
		{
			var locationOfGoods = declaration.JE_LocationOfGoods;
			var result = ZString.Empty;
			if (!IsGoodsLocationDOrF(locationOfGoods))
			{
				result = SADWrapperHelper.AppendFEIfElectronicDocument(entryInstruction, locationOfGoods);
			}
			return result;
		}
	}

	public ZString CustomsSubPlace => ZString.Empty;

	ZBool IsGoodsLocationDOrF(ZString locationOfGoods) => locationOfGoods == GoodsLocationList.Codes.GoodsAreInCustomsAreaForInspection || locationOfGoods == GoodsLocationList.Codes.GoodsAreOutOfCustomsCircuit;
}
