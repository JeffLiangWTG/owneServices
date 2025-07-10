using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IDocMachedTransaction
	{
		ZString TransactionType { get; }
		ZDecimal OSAmount { get; }
		ZDecimal Amount { get; }
		ZString PreparedBy { get; }
	}
}
