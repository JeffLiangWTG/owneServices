
using Enterprise.Accounting.Business.Base.Transaction;

using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class TransactionMatchLinkFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public TransactionMatchLinkFetchStrategy(TransactionMatchLink matchLink)
			: base(matchLink)
		{
		}

		TransactionMatchLink MatchLink
		{
			get { return BusinessObject as TransactionMatchLink; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(TransactionHeader), MatchLink.AP_AH);
		}
	}
}
