using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business;

public class SafeFoodLicense : CusCodeData, Integration.Customs.CA.ISafeFoodLicense
{
	public SafeFoodLicense(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new SafeFoodLicenseValidation Validation
	{
		get { return (SafeFoodLicenseValidation)base.Validation; }
	}

	protected override CusCodeDataValidation GetNewValidation()
	{
		return new SafeFoodLicenseValidation(this);
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_Type = CusCodeDataTypeList.Codes.SafeFoodLicense;
	}

	protected override TypeLoaderCollection parentLoaders
	{
		get { return new TypeLoaderCollection(typeof(OrgHeader)); }
	}

	[ResourceStringData("01AD3A06-C6F1-40D8-A04C-2A172FF2E1B5", Caption = "License No")]
	public override ZString CY_Code
	{
		get { return base.CY_Code; }
		set { base.CY_Code = value; }
	}

	[MaxLength(100)]
	[ResourceStringData("F608C732-83B1-4FED-923D-7C523FD47E9D", Caption = "Description")]
	public override ZString CY_Data
	{
		get { return base.CY_Data; }
		set { base.CY_Data = value; }
	}
}
