using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	internal class UnmatchingDataProvider : IUnmatchingData
	{
		public UnmatchingDataProvider(ITransaction transaction)
		{
			Transaction = transaction;
		}

		readonly ITransaction Transaction;

		#region IUnmatchingData Members

		public ActiveBusinessObjectCollection<TransactionMatchLink> MatchLinksToUnmatch
		{
			get
			{
				return matchLinksToUnmatch ?? (matchLinksToUnmatch = new ActiveBusinessObjectCollection<TransactionMatchLink>(Transaction.Factory, new ZQuery(AccTransactionMatchLinkSchema.AP_AH, Transaction.PK)));
			}
		}
		ActiveBusinessObjectCollection<TransactionMatchLink> matchLinksToUnmatch;

		public ZDateTime MaxMatchDate
		{
			get;
			private set;
		}

		public ZDateTime MinUnmatchDate
		{
			get;
			private set;
		}

		public bool WasUnmatched
		{
			get;
			private set;
		}

		public bool AllowBackPosting
		{
			get;
			set;
		}

		public void CalculateMaxMatchDate()
		{
			MaxMatchDate = MatchLinksToUnmatch.Any() ? MatchLinksToUnmatch.Max(matchLink => matchLink.AP_MatchDate) : ZDateTime.Empty;
		}

		public void InitializaReverseTransactionData(IUnmatchingData originalTransactionData)
		{
			WasUnmatched = !originalTransactionData.MaxMatchDate.IsEmpty;
			AllowBackPosting = originalTransactionData.AllowBackPosting;
			MinUnmatchDate = originalTransactionData.MaxMatchDate;
			if (!MinUnmatchDate.IsEmpty && Transaction.UnmatchDate.IsEmpty)
			{
				Transaction.UnmatchDate = ZDateTime.Now;
			}
		}

		#endregion
	}
}
