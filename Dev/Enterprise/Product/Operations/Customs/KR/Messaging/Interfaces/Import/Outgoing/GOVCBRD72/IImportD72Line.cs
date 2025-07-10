using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportD72Line
	{
		ZInt EntryLineNo { get; }
		ZInt DetailLineNo { get; }
		ZString HSDescription { get; }
		ZString ItemDescription { get; }
		ZString QuantityUnit { get; }
		ZDecimal Quantity { get; }
		ZString AmountCurrency { get; }
		ZDecimal Amount { get; }
		ZString Remark { get; }
	}
}
