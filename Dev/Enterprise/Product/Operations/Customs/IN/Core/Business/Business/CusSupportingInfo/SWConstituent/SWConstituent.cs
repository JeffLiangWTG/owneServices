using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class SWConstituent : CusSupportingInfoWithSerialNo
{
	public SWConstituent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoINCusSupportingInfo.Schema
	{
		public new const int CSI_DescriptionMaxLength = 256;
		public new const int CSI_CodeMaxLength = 17;
		public new const int CSI_StatusMaxLength = 1;
	}

	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.SWConstituent|CSI_Description", Caption = "Constituent Element Name", MediumCaption = "Element Name", ShortCaption = "Name")]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[MaxLength(Schema.CSI_CodeMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.SWConstituent|CSI_Code", Caption = "Constituent Code", MediumCaption = "Code", ShortCaption = "Code")]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWConstituent|CSI_Quantity", Caption = "Constituent Percentage", MediumCaption = "Percentage", ShortCaption = "%%")]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWConstituent|CSI_Quantity2", Caption = "Constituent Yield Percentage", MediumCaption = "Yield Percentage", ShortCaption = "Yield %%")]
	public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

	[MaxLength(Schema.CSI_StatusMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.SWConstituent|CSI_Status", Caption = "Constituent Active Ingredient", MediumCaption = "Active Ingredient", ShortCaption = "Act. Indnt.")]
	public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.SingleWindowConstituent;
	}

	public new SWConstituentValidation Validation => (SWConstituentValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new SWConstituentValidation(this);

	public new SWConstituentLookups Lookups => (SWConstituentLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new SWConstituentLookups(this);

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
}
