using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface ISupportingDocument
	{
		ZString Code { get; }   // HDR-DOC-CODE and ITEM-*
		ZString Status { get; } // HDR-DOC-STATUS
		ZString Reference { get; }  // HDR-DOC-REF
		ZString Part { get; }   // HDR-DOC-PART
		ZDecimal Quantity { get; }  // HDR-DOC-QTY
		ZString Reason { get; } // HDR-DOC-REASON
	}
}
