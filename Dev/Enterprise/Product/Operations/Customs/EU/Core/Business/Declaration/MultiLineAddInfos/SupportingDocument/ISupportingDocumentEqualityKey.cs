using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface ISupportingDocumentEqualityKey
	{
		ZDateTime CSI_DateOfExpiry { get; }
		ZDateTime CSI_DateOfIssue { get; }
		ZString CSI_RX_NKCurrency { get; }
		ZDecimal CSI_Value { get; }
		ZString CSI_UnitOfQuantity2 { get; }
		ZDecimal CSI_Quantity2 { get; }
		ZString CSI_UnitOfQuantity { get; }
		ZDecimal CSI_Quantity { get; }
		ZString CSI_ReferenceNumber { get; }
		ZString CSI_Code { get; }
		ZString CSI_Procedure { get; }
		ZString CSI_AdditionalDescription { get; }
		ZInt CSI_ItemNumber { get; }
	}
}
