using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
	{
		public new const int CSI_SubTypeMaxLength = 1;
		public new const int CSI_ReferenceNumberMaxLength = 70;
		public new const int CSI_ReferenceNumber2MaxLength = 35;
		public new const int CSI_DescriptionMaxLength = 35;
		public new const int CSI_CodeMaxLength = 4;
		public new const int CSI_UnitOfQuantity2MaxLength = 2;
		public const int CSI_PackQtyMaxLength = 8;
		public const int CSI_QuantityDecimalPlaces = 15;
		public const int CSI_QuantityDecimalPrecision = 5;
		public const int CSI_Quantity2DecimalPlaces = 8;
		public const int CSI_Quantity2DecimalPrecision = 0;
	}

	[MaxLength(Schema.CSI_CodeMaxLength)]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[MaxLength(Schema.CSI_SubTypeMaxLength)]
	public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

	[ResourceStringData("BECusSupportingInfo|CSI_ArticleNo", Caption = "Article No.")]
	public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

	[ResourceStringData("BECusSupportingInfo|CSI_ReferenceNumber2", Caption = "Bill of Loading")]
	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[ResourceStringData("CusSupportingInfo|CSI_ItemNumber", Caption = "Item No.")]
	public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CustomsOffices))]
	public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CustomsUQList))]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	[ResourceStringData("1F5651B7-30FD-47B1-99B4-3D785E0ADC4F", Caption = "Package Code", ShortCaption = "Pkg. Code")]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.PackageCodeList))]
	[MaxLength(Schema.CSI_UnitOfQuantity2MaxLength)]
	public override ZString CSI_UnitOfQuantity2 { get => base.CSI_UnitOfQuantity2; set => base.CSI_UnitOfQuantity2 = value; }

	[DecimalPlaces(Schema.CSI_QuantityDecimalPlaces)]
	[DecimalPrecision(Schema.CSI_QuantityDecimalPrecision)]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = Utilities.Round(value, Schema.CSI_QuantityDecimalPrecision); }

	[ResourceStringData("B84CA008-33BF-48CC-9B6E-28DAB87C6E9C", Caption = "Number of Packages", ShortCaption = "No. of Pkgs.")]
	[DecimalPlaces(Schema.CSI_Quantity2DecimalPlaces)]
	[DecimalPrecision(Schema.CSI_Quantity2DecimalPrecision)]
	public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = Utilities.Round(value, Schema.CSI_Quantity2DecimalPrecision); }

	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[MaxLength(Schema.CSI_PackQtyMaxLength)]
	[ResourceStringData("CCE760BA-0E1D-460D-8490-5D3F0191FD9F", Caption = "Number of Packages", MediumCaption = "Pack Qty", ShortCaption = "#Pkgs.")]
	public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

	[ResourceStringData("7456796E-8EBC-4FD0-827A-CC4876B52245", Caption = "Type of Packages", MediumCaption = "Pack Type", ShortCaption = "Pack Type")]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.PackageCodeList))]
	public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

	public void JobDeclarationMessageTypeChanged(ZString newMessageType)
	{
		if (newMessageType != JobMessageTypeList.Codes.Import)
		{
			CSI_Quantity2 = ZDecimal.Zero;
			CSI_UnitOfQuantity2 = ZString.Empty;
		}
	}

	public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)GetNewLookups();

	protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

	public new PreviousDocumentValidation Validation => (PreviousDocumentValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);
}
