using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStoragePreviousDocument : EU.Business.CusTempStorage.TemporaryStoragePreviousDocument
{
	public TemporaryStoragePreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusSupportingInfo.Schema
	{
		public const int ReferenceNumberMaxLength = 35;
		public const int PackTypeMaxLength = 2;
		public const int QuantityPrecision = 16;
		public const int QuantityScale = 6;
		public const int UnitOfQuantityMaxLength = 4;
	}

	[MaxLength(Schema.ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("cd7d4247-403e-40ea-9d93-3012d3b1e408", ShortCaption = "Package", MediumCaption = "Package Qty", Caption = "Package Quantity")]
	public override ZInt CSI_PackQty { get => base.CSI_PackQty; set => base.CSI_PackQty = value; }

	[ResourceStringData("27908722-245a-45a6-85a9-387c8189fa27", ShortCaption = "Type", MediumCaption = "Package Type", Caption = "Type of Packages")]
	[MaxLength(Schema.PackTypeMaxLength)]
	public override ZString CSI_PackType { get => base.CSI_PackType; set => base.CSI_PackType = value; }

	[ResourceStringData("6cd4e883-3eb3-477c-ab01-a42746ab8d69", ShortCaption = "Qty", Caption = "Quantity")]
	[DecimalPrecision(Schema.QuantityPrecision), DecimalPlaces(Schema.QuantityScale)]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	[ResourceStringData("226b256e-73a8-478c-baac-53ddb05fbf8f", ShortCaption = "UQ", MediumCaption = "Quantity Unit", Caption = "Unit of Quantity")]
	[MaxLength(Schema.UnitOfQuantityMaxLength)]
	public override ZString CSI_UnitOfQuantity { get => base.CSI_UnitOfQuantity; set => base.CSI_UnitOfQuantity = value; }

	protected override CusSupportingInfoValidation GetNewValidation() => new TemporaryStoragePreviousDocumentValidation(this);

	protected override CusSupportingInfoLookups GetNewLookups() => new TemporaryStoragePreviousDocumentLookups(this);
}
