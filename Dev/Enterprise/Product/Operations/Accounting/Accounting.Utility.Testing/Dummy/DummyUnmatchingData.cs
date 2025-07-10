using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;

namespace Enterprise.Accounting.Utility.Testing
{
	public class DummyUnmatchingData : IUnmatchingData
	{
		#region IUnmatchingData Members

		public ActiveBusinessObjectCollection<TransactionMatchLink> MatchLinksToUnmatch
		{
			get { throw new System.NotImplementedException(); }
		}

		public ZDateTime MaxMatchDate
		{
			get;
			set;
		}

		public ZDateTime MinUnmatchDate
		{
			get { throw new System.NotImplementedException(); }
		}

		public bool WasUnmatched
		{
			get { throw new System.NotImplementedException(); }
		}

		public bool AllowBackPosting
		{
			get;
			set;
		}

		public void CalculateMaxMatchDate()
		{
			throw new System.NotImplementedException();
		}

		public void CalculateBackPostingAbility()
		{
			throw new System.NotImplementedException();
		}

		public void InitializaReverseTransactionData(IUnmatchingData originalTransactionData)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
