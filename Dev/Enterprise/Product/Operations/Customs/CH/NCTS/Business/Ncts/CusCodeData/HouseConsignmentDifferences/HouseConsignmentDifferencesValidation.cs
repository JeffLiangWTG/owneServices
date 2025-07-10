using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class HouseConsignmentDifferencesValidation : CusCodeDataValidation
{
	public HouseConsignmentDifferencesValidation(AutoCusCodeData parent) : base(parent)
	{
	}

	new HouseConsignmentDifferences Parent => (HouseConsignmentDifferences)base.Parent;

	protected override void CheckCY_Code()
	{
		if (!Parent.IsNotMissingGoodsItem)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
		}
	}

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		if (!Parent.IsNotMissingGoodsItem && Parent.CY_Code == UnloadingRemarkCodeList.Codes.Other && Parent.CY_Data.IsEmpty)
		{
			Parent.CY_DataInfo.AddMessageError(PassarValidationMessages.MessageNS30004);
		}
	}
}
