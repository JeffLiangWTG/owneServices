using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	class ScoreResult<U, V>
		where U : BusinessObject
		where V : BusinessObject
	{
		internal ScoreResult(PotentialMatch<U, V> matchTarget, List<IMatchWithFilterAndScore> matchCriteria, ISimpleLogger logger)
		{
			this.MatchTarget = Argument.NotNull(matchTarget, "PotentialMatch<U, V> matchTarget");
			this.matchCriteria = Argument.NotNull(matchCriteria, "List<IMatchWithFilterAndScore> matchCriteria");
			this.logger = Argument.NotNull(logger, "ISimpleLogger logger");
		}

		internal readonly PotentialMatch<U, V> MatchTarget;
		readonly List<IMatchWithFilterAndScore> matchCriteria;
		readonly ISimpleLogger logger;

		int? score;
		internal int Score
		{
			get
			{
				if (!score.HasValue)
				{
					score = GetScore();
				}

				return score.Value;
			}
		}

		int GetScore()
		{
			int baseScore = 0;
			int exponentialBonusCount = 0;
			int exponentialPenaltyCount = 0;
			var incomingValuesAlreadyScored = new HashSet<string>();

			foreach (var matchCriterion in matchCriteria)
			{
				if (!incomingValuesAlreadyScored.Contains(matchCriterion.IncomingValueName)
					&& matchCriterion.IsMatch(MatchTarget, logger))
				{
					var score = matchCriterion.Score;
					if (score > 0)
					{
						exponentialBonusCount++;
					}
					else if (score < 0)
					{
						exponentialPenaltyCount--;
					}
					else
					{
						continue;
					}

					incomingValuesAlreadyScored.Add(matchCriterion.IncomingValueName);
					baseScore += score;
				}
			}

			int exponentialBonus = GetExponentialBonus(exponentialBonusCount);
			int exponentialPenalty = GetExponentialBonus(exponentialPenaltyCount);

			return baseScore + exponentialBonus - exponentialPenalty;
		}

		static int GetExponentialBonus(int exponentialCount)
		{
			if (exponentialCount > 1)
			{
				var result = 2;
				for (int index = 2; index < exponentialCount; index++)
				{
					result *= 2;
				}
				return result * 20;
			}
			else
			{
				return 0;
			}
		}
	}
}
