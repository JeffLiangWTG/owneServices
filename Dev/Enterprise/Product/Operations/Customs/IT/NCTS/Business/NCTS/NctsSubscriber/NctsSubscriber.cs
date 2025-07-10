using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSubscriber : Customs.Business.CusInBondPerson
{
	public NctsSubscriber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Constants

	public static class Constants
	{
		public const string CodeType = "SBR";
	}

	#endregion

	#region CP_BH_Header

	[RelatedBusinessObject("NctsHeader")]
	public override ZGuid CP_BH_Header { get => base.CP_BH_Header; set => base.CP_BH_Header = value; }

	public NctsHeader NctsHeader => Factory.Load<NctsHeader>(CP_BH_Header);

	#endregion

	#region CP_Type

	public override ZString CP_Type
	{
		get => base.CP_Type;
		set
		{
			if (value != Constants.CodeType)
			{
				throw new InvalidOperationException(Res.GetString("48A8CB5D-91F8-453A-9C17-3434B3ABBDFD", "Invalid Type, must be: {0}", Constants.CodeType));
			}
			base.CP_Type = value;
			CP_TypeInfo.RefreshBinding();
		}
	}

	#endregion

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CP_Type = Constants.CodeType;
	}
}
