using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class LocationOfGoodsCalculator
{
	public ZString Calculate(CusEntryHeader entryHeader)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		var entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));

		var locationQualifier = declaration.JE_LocationQualifier.Trim();
		var locationOfGoods = declaration.JE_LocationOfGoods.Trim();
		return locationQualifier.IsEmpty
			? CalculateCodeAndCinFromGoodsLocation(entryInstruction, locationOfGoods)
			: CalculateCodeAndCinFromQualifierAndGoodsLocation(entryInstruction, locationQualifier, locationOfGoods);
	}

	#region Implementation

	ZString CalculateCodeAndCinFromQualifierAndGoodsLocation(CusEntryInstruction entryInstruction, ZString locationQualifier, ZString locationOfGoods)
	{
		var result = SADWrapperHelper.AppendFEIfElectronicDocument(entryInstruction, locationQualifier);
		if (!locationOfGoods.IsEmpty)
		{
			result = ZString.Format("{0}-{1}", result, locationOfGoods);
		}
		return result;
	}

	ZString CalculateCodeAndCinFromGoodsLocation(CusEntryInstruction entryInstruction, ZString locationOfGoods) => SADWrapperHelper.AppendFEIfElectronicDocument(entryInstruction, locationOfGoods);

	#endregion
}
