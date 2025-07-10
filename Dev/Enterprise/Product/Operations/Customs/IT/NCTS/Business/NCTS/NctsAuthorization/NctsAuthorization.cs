using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsAuthorization : CusReference, Integration.Customs.IT.INctsAuthorization
{
	public NctsAuthorization(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region constants
	public static class Constants
	{
		public const string CfrType = Customs.Business.CusReferenceTypeList.Codes.NctsAuthorization;
		public const string CfrCode = "AUT";
	}

	#endregion

	#region CFR_Type

	public override ZString CFR_Type
	{
		get => base.CFR_Type;
		set
		{
			if (value != Constants.CfrType)
			{
				throw new InvalidOperationException(Res.GetString("7E23E9BE-CCBD-4C4E-B877-0F0E27266954", "Invalid Type, must be: {0}", Constants.CfrType));
			}
			base.CFR_Type = value;
			CFR_TypeInfo.RefreshBinding();
		}
	}

	#endregion

	#region CFR_Code

	public override ZString CFR_Code
	{
		get => base.CFR_Code;
		set
		{
			if (value != Constants.CfrCode)
			{
				throw new InvalidOperationException(Res.GetString("E3ED9D29-931B-496F-ACA2-0830FF099236", "Invalid Code, must be: {0}", Constants.CfrCode));
			}
			base.CFR_Code = value;
			CFR_CodeInfo.RefreshBinding();
		}
	}

	#endregion

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		CFR_Type = Constants.CfrType;
		CFR_Code = Constants.CfrCode;
	}
}
