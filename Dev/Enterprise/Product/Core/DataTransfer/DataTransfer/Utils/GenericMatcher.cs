using CargoWise.Types;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.Business
{
	public abstract class Matcher<ResultType> : IMatcher<ResultType> where ResultType : IZType
	{
		#region Constructor

		public Matcher()
		{
		}

		#endregion

		#region Match Method

		public bool Match()
		{
			matchHasBeenDone = true;
			MatchResult matchedResult;

			foreach (MatchDelegate potentialMatch in matchDelegates)
			{
				matchedResult = potentialMatch();
				if (matchedResult.HasMatched)
				{
					result = matchedResult.Value;
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Public Properties

		public ResultType Result
		{
			get
			{
				if (!matchHasBeenDone)
				{
					Match();
				}
				return result;
			}
		}
		protected ResultType result;

		#endregion

		#region Implementation

		#region MatchResult Struct

		protected struct MatchResult
		{
			public MatchResult(bool hasMatched, ResultType value)
			{
				this.hasMatched = hasMatched;
				this.fValue = value;
			}

			#region Properties

			public bool HasMatched
			{
				get { return hasMatched; }
				set { hasMatched = value; }
			}
			bool hasMatched;

			public ResultType Value
			{
				get { return fValue; }
				set
				{
					hasMatched = true;
					fValue = value;
				}
			}
			ResultType fValue;

			#endregion
		}

		#endregion

		protected delegate MatchResult MatchDelegate();

		protected abstract MatchDelegate[] matchDelegates { get; }

		protected bool matchHasBeenDone;

		#endregion
	}
}
