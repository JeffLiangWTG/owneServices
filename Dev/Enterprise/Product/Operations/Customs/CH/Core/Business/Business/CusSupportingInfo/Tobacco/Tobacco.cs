using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class Tobacco : CusSupportingInfo
{
	public new class Schema : Customs.Business.CusSupportingInfo.Schema
	{
		public const int CodeMaxLength = 1;
		public const int SubTypeMaxLength = 2;
		public const int DescriptionMaxLength = 50;
		public const int ItemNumberMaxLength = 5;
		public const int AdditionalDescriptionMaxLength = 3;
	}

	public Tobacco(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

	public new TobaccoLookups Lookups => (TobaccoLookups)base.Lookups;

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	protected override CusSupportingInfoLookups GetNewLookups() => new TobaccoLookups(this);

	protected override CusSupportingInfoValidation GetNewValidation() => new TobaccoValidation(this);

	public override bool SupportsNotes => false;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.Tobacco;
	}

	#region Properties
	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_Code", Caption = "Product main group")]
	[MaxLength(Schema.CodeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(TobaccoLookups.CodeList))]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set => base.CSI_Code = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_SubType", Caption = "Product subgroup")]
	[MaxLength(Schema.SubTypeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(TobaccoLookups.SubTypeList))]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set => base.CSI_SubType = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_Description", Caption = "Designation")]
	[MaxLength(Schema.DescriptionMaxLength)]
	public override ZString CSI_Description
	{
		get => base.CSI_Description;
		set => base.CSI_Description = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_ItemNumber", Caption = "Sequential number", MediumCaption = "Seq. number", ShortCaption = "Seq. no.")]
	[MaxLength(Schema.ItemNumberMaxLength)]
	public override ZInt CSI_ItemNumber
	{
		get => base.CSI_ItemNumber;
		set => base.CSI_ItemNumber = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_Value|IMP", Caption = "Retail price", MultipleKey = MessageTypeCodeList.Codes.Import)]
	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_Value|EXP", Caption = "Rate", MultipleKey = MessageTypeCodeList.Codes.Export)]
	[DecimalPlaces(4)]
	public override ZDecimal CSI_Value
	{
		get => base.CSI_Value;
		set => base.CSI_Value = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_AdditionalDescription", Caption = "Tobacco brand")]
	[MaxLength(Schema.AdditionalDescriptionMaxLength)]
	[List(nameof(Lookups) + "." + nameof(TobaccoLookups.AdditionalDescriptionList))]
	public override ZString CSI_AdditionalDescription
	{
		get => base.CSI_AdditionalDescription;
		set => base.CSI_AdditionalDescription = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_ReferenceNumber", Caption = "Reverse number")]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.Tobacco|CSI_UnitOfQuantity", Caption = "Special Unit of measure")]
	[MaxLength(Schema.CSI_UnitOfQuantityMaxLength)]
	[List(nameof(Lookups) + "." + nameof(TobaccoLookups.SpecialUnitOfMeasureList))]
	public override ZString CSI_UnitOfQuantity
	{
		get => base.CSI_UnitOfQuantity;
		set => base.CSI_UnitOfQuantity = value;
	}
	#endregion
}
