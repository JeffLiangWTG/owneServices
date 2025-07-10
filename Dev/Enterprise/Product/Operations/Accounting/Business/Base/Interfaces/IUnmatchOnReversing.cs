using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IUnmatchingData
	{
		ActiveBusinessObjectCollection<TransactionMatchLink> MatchLinksToUnmatch { get; }
		ZDateTime MaxMatchDate { get; }
		ZDateTime MinUnmatchDate { get; }
		bool WasUnmatched { get; }
		bool AllowBackPosting { get; set; }
		void CalculateMaxMatchDate();
		void InitializaReverseTransactionData(IUnmatchingData originalTransactionData);
	}

	public interface IUnmatchOnReversing : ITransaction
	{
		IUnmatchingData UnmatchingData { get; }
	}
}
