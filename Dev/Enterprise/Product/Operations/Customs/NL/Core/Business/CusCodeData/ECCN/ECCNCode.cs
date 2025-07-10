using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

[DependentBusinessObject(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ECCNCodes))]
public class ECCNCode : CusCodeData
{
	public ECCNCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : CusCodeData.Schema
	{
		public new const int CY_CodeMaxLength = 10;
	}

	protected override ZString HumanReadableNameCore => Res.GetString("D9317371-C145-41B0-9C45-EB31657075F5", "Export Control Classification Number");

	[ResourceStringData("Enterprise.Customs.NL.Business.ECCNCode|CY_Code", Caption = "Additional ECCN Code", ShortCaption = "Add. ECCN")]
	[MaxLength(Schema.CY_CodeMaxLength)]
	public override ZString CY_Code
	{
		get => base.CY_Code;
		set
		{
			var oldValue = CY_Code;
			base.CY_Code = value;
			if (Parent != null)
			{
				Parent.ECCNCodesAsStringInfo.RefreshBinding();
			}
		}
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_Type = CusCodeDataTypeList.Codes.ExportControlClassificationNumber;
	}
}
