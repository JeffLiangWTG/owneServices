using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business;

public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, Integration.Customs.IN.IOrgCusCodeValidation
{
	public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
	{
		this.parent = (OrgCusCode)parent;
	}

	readonly OrgCusCode parent;

	protected override void CheckOK_CustomsRegNo()
	{
		base.CheckOK_CustomsRegNo();

		var customsRegNo = parent.OK_CustomsRegNo;
		switch (parent.OK_CodeType)
		{
			case IndiaOrgCusCodeInfo.OrgCusCodes.BSN when !customsRegNo.IsEmpty && (!Regex.IsMatch(customsRegNo, @"^[0-9]{1,3}$") || Regex.IsMatch(customsRegNo, @"^[0]{1,3}$")):
				parent.OK_CustomsRegNoInfo.AddError(Res.GetString("E07C2C90-9765-4912-AFC5-A701BF3F665B", "Invalid BSN - Branch Serial Number. Expected code value is any number between 1 to 999."));
				break;
			case IndiaOrgCusCodeInfo.OrgCusCodes.ADC when customsRegNo.Length > 10:
				parent.OK_CustomsRegNoInfo.AddError(Res.GetString("9D83F05A-FD11-4F6C-A092-31A8080BBA37", "Invalid Authorized Dealer Code. The expected code cannot be greater than 10 Char."));
				break;
			case IndiaOrgCusCodeInfo.OrgCusCodes.IEC when !customsRegNo.IsEmpty && customsRegNo.Length != 10:
				parent.OK_CustomsRegNoInfo.AddError(Res.GetString("831950B4-DF2D-4FA7-90D5-F7524E0E1640", "Invalid IEC. The expected code should be exactly 10 Character long."));
				break;
			case IndiaOrgCusCodeInfo.OrgCusCodes.AEO when customsRegNo.Length > 17:
				parent.OK_CustomsRegNoInfo.AddError(Res.GetString("252D17D5-830E-4501-BE51-A6EB8AB87FEA", "Invalid AEO - Authorized Economic Operator. The expected code cannot be greater than 17 Char."));
				break;
		}
	}
}

