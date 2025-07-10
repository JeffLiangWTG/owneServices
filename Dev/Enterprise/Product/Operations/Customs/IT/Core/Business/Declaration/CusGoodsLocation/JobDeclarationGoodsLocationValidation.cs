using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class ExportJobDeclarationValidation
{
	public void ValidateGoodsLocationDescription()
	{
		ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
	}

	protected void CheckGoodsLocationDescription()
	{
		var declaration = Parent;
		if (declaration.IsUCC6AndIsExport && declaration.GoodsLocation is CusGoodsLocation cusGoodsLocation)
		{
			cusGoodsLocation.Validation.ValidateAll();
			cusGoodsLocation.Address.Validation.ValidateAll();

			var goodsLocationDescriptionInfo = declaration.GoodsLocationDescriptionInfo;
			if (declaration.GoodsLocationDescription.IsEmpty && HasAtLeastOneEntryInstructionNotInDEF())
			{
				goodsLocationDescriptionInfo.AddMessageError(ValidationCaptions.JobDeclaration.YouHaveNotEnteredLocationOfGoodsC0392);
			}

			EU.Business.CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(Parent);
		}
	}

	#region Implementation

	bool HasAtLeastOneEntryInstructionNotInDEF()
	{
		return Parent.CustomsEntryInstructions
			.Cast<CusEntryInstruction>()
			.Any(x => !x.CEI_SubStyle.In(entrySubStyleListDEF));
	}

	readonly ImmutableArray<ZString> entrySubStyleListDEF = new ZString[]
	{
		ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationE,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationF,
	}.ToImmutableArray();

	#endregion
}
