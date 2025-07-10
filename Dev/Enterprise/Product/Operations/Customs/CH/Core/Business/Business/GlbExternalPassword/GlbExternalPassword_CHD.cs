using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class GlbExternalPassword_CHD : GlbExternalPassword
{
	public GlbExternalPassword_CHD(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[MaxLength(6)]
	[ResourceStringData("CH.Business.GlbExternalPassword|GP_UserID", Caption = "Declarant Number")]
	public override ZString GP_UserID
	{
		get => base.GP_UserID;
		set => base.GP_UserID = value;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.CHD;
	}

	protected override GlbExternalPasswordValidation GetNewValidation()
	{
		return new GlbExternalPasswordValidation_CHD(this);
	}

	#region New Properties

	public new GlbExternalPasswordValidation_CHD Validation => (GlbExternalPasswordValidation_CHD)base.Validation;

	#endregion
}
