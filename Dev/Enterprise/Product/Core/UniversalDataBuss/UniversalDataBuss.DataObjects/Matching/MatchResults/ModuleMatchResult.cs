using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public sealed class ModuleMatchResult<T> : LoggingMatchResult<T> where T : IBusiness
	{
		public int Score
		{
			get;
			private set;
		}

		public ModuleMatchResult<T> SetMatch(T matchFound, int score)
		{
			base.SetMatch(matchFound);
			this.Score = score;
			return this;
		}
	}
}