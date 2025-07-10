using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.UniversalDataBuss.DataObjects.Res;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	class MatchingScorer<U, V>
		where U : BusinessObject
		where V : BusinessObject
	{
		internal MatchingScorer(ISimpleLogger logger)
		{
			this.logger = Argument.NotNull(logger, "ISimpleLogger logger");
		}
		readonly ISimpleLogger logger;

		internal ScoreResult<U, V> GetMatchWithHighestScore(PotentialMatch<U, V>[] potentialMatches, List<IMatchWithFilterAndScore> matchCriteriaWithDataPresent)
		{
			var matchesWithCurrentHighScore = GetMatchesWithHighestScore(potentialMatches, matchCriteriaWithDataPresent);

			if (matchesWithCurrentHighScore.Count == 0)
			{
				return null;
			}

			if (matchesWithCurrentHighScore.Count == 1)
			{
				return matchesWithCurrentHighScore[0];
			}

			logger.Log(LogType.Information, Res.GetString("aa0b0015-7890-4889-a707-df30d6d11a62", "{0} matches with the same score, falling back to most recently created...", matchesWithCurrentHighScore.Count));
			return GetMostRecentlyCreatedMatch(matchesWithCurrentHighScore);
		}

		static ScoreResult<U, V> GetMostRecentlyCreatedMatch(List<ScoreResult<U, V>> matchesWithCurrentHighScore)
		{
			ScoreResult<U, V> mostRecentEntity = null;
			var mostRecentEntityCreateDate = ZDateTime.Empty;

			foreach (var match in matchesWithCurrentHighScore)
			{
				var matchingBO = match.MatchTarget.TargetData;

				var matchCreateDate = matchingBO.GetLogs().CreatedDateUtc;
				if (mostRecentEntity == null || matchCreateDate > mostRecentEntityCreateDate)
				{
					mostRecentEntity = match;
					mostRecentEntityCreateDate = matchCreateDate;
				}
			}

			return mostRecentEntity;
		}

		List<ScoreResult<U, V>> GetMatchesWithHighestScore(PotentialMatch<U, V>[] potentialMatches, List<IMatchWithFilterAndScore> matchCriteriaWithDataPresent)
		{
			int currentHighScore = 0;
			var matchesWithCurrentHighScore = new List<ScoreResult<U, V>>();

			foreach (var potentialMatch in potentialMatches)
			{
				logger.LogVerboseOnly(LogType.Information, Res.GetString("755be53b-db8a-48c5-8792-f511c0d661f9", "Scoring {0}:-", potentialMatch.OuterTarget.HumanReadableName));

				var scoredMatch = new ScoreResult<U, V>(potentialMatch, matchCriteriaWithDataPresent, logger);
				if (scoredMatch.Score == currentHighScore)
				{
					matchesWithCurrentHighScore.Add(scoredMatch);
				}
				else if (scoredMatch.Score > currentHighScore)
				{
					matchesWithCurrentHighScore = new List<ScoreResult<U, V>>() { scoredMatch };
					currentHighScore = scoredMatch.Score;
				}

				logger.LogVerboseOnly(LogType.Information, Res.GetString("e7a90568-99eb-41e6-bc02-73e1fb2cda49", "Final Score: {0} points.", scoredMatch.Score.ToString()));
			}
			return matchesWithCurrentHighScore;
		}
	}
}
