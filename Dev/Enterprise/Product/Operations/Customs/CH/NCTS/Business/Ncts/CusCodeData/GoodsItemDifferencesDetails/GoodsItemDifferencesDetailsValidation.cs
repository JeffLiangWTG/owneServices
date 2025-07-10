using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class GoodsItemDifferencesDetailsValidation : CusCodeDataValidation
{
	public GoodsItemDifferencesDetailsValidation(GoodsItemDifferencesDetails parent) : base(parent)
	{
	}

	new GoodsItemDifferencesDetails Parent => (GoodsItemDifferencesDetails)base.Parent;

	protected override void CheckCY_Code()
	{
		if (!Parent.IsUnloadingStateOfGoodsItemDeclared && ((Parent.Parent?.Bill?.MovementDetail?.B9_UnloadedState ?? ZString.Empty) != NctsUnloadedStateList.Codes.MIS))
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
		}
	}

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		if (!Parent.IsUnloadingStateOfGoodsItemDeclared && Parent.CY_Code == UnloadingRemarkCodeList.Codes.Other && Parent.CY_Data.IsEmpty)
		{
			Parent.CY_DataInfo.AddMessageError(PassarValidationMessages.MessageNS30004);
		}
	}
}
