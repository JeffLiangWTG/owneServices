

using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ViewMatchGroupFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ViewMatchGroupFetchStrategy(ViewMatchGroup matchGroup)
			: base(matchGroup)
		{
		}

		ViewMatchGroup MatchGroup
		{
			get { return BusinessObject as ViewMatchGroup; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(TransactionMatchLink), AccTransactionMatchLinkSchema.PK, MatchGroup.PK);
		}
	}
}
