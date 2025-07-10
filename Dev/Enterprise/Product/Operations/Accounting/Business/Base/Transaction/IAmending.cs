using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public interface IAmending : ITransaction
	{
		bool IsAmendingTransaction { get; }
		bool IsOriginalTransaction { get; }
		ITransaction OriginalTransaction { get; }
		IAmending GenerateAmendingTransaction(string transactionType);
		ZGuid[] OriginalTransactionJobPKs { get; }
		ZGuid OriginalTransactionAccountPK { get; }
		ZString AmendingReason { get; set; }
		ZString AmendingReasonCode { get; set; }
		void FlagAsCreatedAmending();
		bool IsAmendingTransaction_SoftReference { get; }
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public interface IAmendingTest
	{
		void TestIsAmendingTransaction();
		void TestIsOriginalTransaction();
		void TestOriginalTransaction();
		void TestGenerateAmendingTransaction();
		void TestOriginalTransactionJobPKs();
		void TestOriginalTransactionAccountPK();
		void TestAmendingReason();
		void TestAmendingReasonCode();
	}
}
#endif
#endregion
