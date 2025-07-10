using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInBondCargoDesc
		{
			ZGuid PK { get; }
			ZDecimal BY_InvoiceQuantity { get; set; }
			ZString BY_ManifestUnitCode { get; set; }
			ZString BY_MarksAndNumbers { get; set; }
			ZDecimal BY_MonetaryValue { get; set; }
			ZGuid BY_OH_Supplier { get; set; }
			ZGuid BY_OP_Part { get; set; }
			ZGuid BY_ParentID { get; set; }
			ZString BY_ParentTableCode { get; set; }
			ZString BY_PartAttrib1 { get; set; }
			ZString BY_PartAttrib2 { get; set; }
			ZString BY_PartAttrib3 { get; set; }
			ZString BY_PartNumber { get; set; }
			ZInt BY_PieceCount { get; set; }
			ZString BY_WarehouseEntryNumber { get; set; }
			ZShort BY_WarehouseEntryLineNo { get; set; }
			ZDecimal BY_GrossWeight { get; set; }
			ZString BY_GrossWeightUnit { get; set; }
		}
	}
}
