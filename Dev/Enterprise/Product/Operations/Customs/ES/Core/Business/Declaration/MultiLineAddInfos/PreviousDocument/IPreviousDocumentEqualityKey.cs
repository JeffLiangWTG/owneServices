using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public interface IPreviousDocumentEqualityKey
	{
		ZString CSI_Code { get; }
		ZString CSI_SubType { get; }
		ZString CSI_ReferenceNumber { get; }
		ZDateTime CSI_DateOfIssue { get; }
		ZInt CSI_LineNo { get; }
		ZString CSI_UnitOfQuantity { get; }
		ZDecimal CSI_Quantity { get; }
	}
}
