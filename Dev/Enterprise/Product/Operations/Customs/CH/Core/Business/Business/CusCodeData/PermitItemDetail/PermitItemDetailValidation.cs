using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PermitItemDetailValidation : CusCodeDataValidation
{
	public PermitItemDetailValidation(PermitItemDetail parent) : base(parent)
	{
	}

	new PermitItemDetail Parent => (PermitItemDetail)base.Parent;

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		var thisPermitItemDetail = Parent;
		var permit = thisPermitItemDetail.Parent;

		if (permit != null)
		{
			var thisPermitItemDetailCode = thisPermitItemDetail.CY_Code;

			if (permit.PermitItemDetails.OfType<PermitItemDetail>().Any(otherPermitItemDetail => otherPermitItemDetail != thisPermitItemDetail && otherPermitItemDetail.CY_Code == thisPermitItemDetailCode))
			{
				Parent.CY_CodeInfo.AddMessageError(Res.GetString("6C3AA289-DBDB-400A-AEDA-26C2CAB7219E", "A key may be entered only once."));
			}
		}
	}

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();

		switch (Parent.CY_Code)
		{
			case PermitItemDetailKeyList.Codes.Key1:
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo, Res.GetString("E9356A0E-AF07-4467-B00F-7814A33D486B", "a whole number for key 1."));
				if (!Parent.CY_Data.IsEmpty && !ZInt.CanParse(Parent.CY_Data))
				{
					Parent.CY_DataInfo.AddMessageError(Res.GetString("C13006BE-6BD7-43D0-92B1-5BAA9E3254BF", "For key 1 the value must be a whole number."));
				}
				break;
			case PermitItemDetailKeyList.Codes.Key2:
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo, Res.GetString("B9545867-9021-4DB1-978F-1918EBA08272", "a quantity for key 2."));
				if (!Parent.CY_Data.IsEmpty && !ZDecimal.CanParse(Parent.CY_Data))
				{
					Parent.CY_DataInfo.AddMessageError(Res.GetString("C64462BD-03F5-42E8-880D-0B24684A822B", "For key 2 the value must be a quantity."));
				}
				break;
			case PermitItemDetailKeyList.Codes.Key3:
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_DataInfo, Parent.Lookups.CITESCommodityTypeList, ResString.GetMultilingualString("F5CE7CB4-13C3-4D6B-AEF2-312014ADD767", "CITES commodity type code."));
				break;
			case PermitItemDetailKeyList.Codes.Key4:
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_DataInfo, Parent.Lookups.CITESScientificNameList, ResString.GetMultilingualString("{C3608D6A-0B5E-4B78-BBAF-A27109EF543F}", "CITES scientific name code."));
				break;
			default:
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
				break;
		}
	}
}
