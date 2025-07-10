using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.UniversalDataBuss.DataObjects.Res;

namespace Enterprise.UniversalDataBuss.Management
{
	public abstract class CombinationKeyMatcher<T, U> : CombinationKeyMatcherCore<T>, IMatchingBusinessEntityFinder<T>
		where T : BusinessObject
		where U : IReferencesParent
	{
		public CombinationKeyMatcher(BusinessObjectFactory factory, U referencesParent, IXmlImportLogger logger)
			: base(factory)
		{
			this.referencesParent = Argument.NotNull(referencesParent, "U referencesParent");
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
		}

		protected readonly U referencesParent;
		protected readonly IXmlImportLogger logger;
		ZQuery initialMatchingQuery;
		List<MatchDelegate> possibleMatches;
		List<MatchDelegate> fallbackMatches;

		protected delegate int MatchDelegate(T parent);

		protected abstract bool CheckLatestParent(T parent, T parentToCompare);

		protected static int GetMatchCount(IZType actualValue, IZType valueToMatch)
		{
			return actualValue.Equals(valueToMatch) ? 1 : 0;
		}

		protected static int GetMatchCount(BusinessObjectCollection collection, SchemaColumn column, List<ZString> valuesToMatch)
		{
			return collection.Find(new ZQuery(column, valuesToMatch)).Length;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected static int GetMatchCount(BusinessObjectCollection collection, SchemaColumn keyColumn, SchemaColumn valueColumn, List<KeyValuePair<ZString, ZString>> valuesToMatch)
		{
			if (valuesToMatch.Count > 0)
			{
				var query = new ZQuery();

				foreach (var valueToMatch in valuesToMatch)
				{
					var subQuery = new ZQuery(keyColumn, valueToMatch.Key);
					subQuery.AddToFilter(valueColumn, valueToMatch.Value);
					query.AddToFilter(subQuery, JoinCondition.Or);
				}

				return collection.Find(query).Length;
			}

			return 0;
		}

		public T GetBestMatch()
		{
			initialMatchingQuery = new ZQuery();
			possibleMatches = new List<MatchDelegate>();
			fallbackMatches = new List<MatchDelegate>();
			BuildMatchingQueryAndMatchDelegates(referencesParent);
			BuildFallbackMatchDelegates(referencesParent);
			return GetBestMatchCore();
		}

		protected abstract void BuildMatchingQueryAndMatchDelegates(U referencesParent);
		protected abstract void BuildFallbackMatchDelegates(U referencesParent);

		T GetBestMatchCore()
		{
			if (!initialMatchingQuery.IsEmpty)
			{
				var parents = GetBusinessObjectUsingModuleSpecificBusinessRules(referencesParent);
				if (parents.Length == 1)
				{
					logger.LogVerboseOnly(LogType.Information, Res.GetString("8125c17a-ce7b-4f25-8906-a891ca8633cd", "Found match using Combination Key Match. 1 possible match found."));
					return parents[0];
				}

				if (parents.Length > 1)
				{
					if (parents.Length > CombinationKeyMatcherParentLimit.Value)
					{
						throw new MessageProcessingBusinessFailureException(Res.GetString("CombinationKeyMatcher|TooManyMatches", "Too many parents to match against. Please ensure that any Additional References used are unique identifiers."));
					}
					return GetBestMatchingParent(parents);
				}
			}

			return null;
		}

		protected virtual T[] GetBusinessObjectUsingModuleSpecificBusinessRules(U parent)
		{
			return factory.Load<T>(GetFullQuery(initialMatchingQuery, parent));
		}

		protected virtual ZQuery GetFullQuery(ZQuery initialMatchingQuery, U referencesParent)
		{
			return initialMatchingQuery;
		}

		T GetBestMatchingParent(T[] parents)
		{
			var parentsToLookThrough = new List<T>();

			if (possibleMatches.Count > 0)
			{
				var parentsWithMostMatches = GetParentsWithMostMatches(parents);
				if (parentsWithMostMatches.Count == 1)
				{
					logger.LogVerboseOnly(LogType.Information, Res.GetString("a579632b-2075-4bc5-aa59-40a2ee639d62", "Found match using Combination Key Match. Picked using highest number of matching references from {0} candidates.", parents.Length.ToString()));
					return parentsWithMostMatches[0];
				}

				parentsToLookThrough = parentsWithMostMatches;

				if (possibleMatches.Count > 1)
				{
					foreach (var possibleMatch in possibleMatches)
					{
						var parentsWithBestMatch = new List<T>();

						foreach (var parent in parentsToLookThrough)
						{
							if (possibleMatch(parent) > 0)
							{
								parentsWithBestMatch.Add(parent);
							}
						}

						if (parentsWithBestMatch.Count == 1)
						{
							logger.LogVerboseOnly(LogType.Information, Res.GetString("68b9efe4-71ad-4b65-a388-1533a247e75b", "Found match using Combination Key Match. Picked using best matching reference from {0} candidates.", parents.Length.ToString()));
							return parentsWithBestMatch[0];
						}

						if (parentsWithBestMatch.Count > 1)
						{
							parentsToLookThrough = parentsWithBestMatch;
						}
					}
				}
			}

			if (fallbackMatches.Count > 0)
			{
				var parentsMatchingOnFallbacks = GetParentsMatchingOnFallbacks(parentsToLookThrough);
				if (parentsMatchingOnFallbacks.Count == 1)
				{
					logger.LogVerboseOnly(LogType.Information, Res.GetString("4a2783f6-ea6c-46ac-9d17-ab850f97766a", "Found match using Combination Key Match. Picked using best matching references with fallback eliminations from {0} candidates.", parents.Length.ToString()));
					return parentsMatchingOnFallbacks[0];
				}

				parentsToLookThrough = parentsMatchingOnFallbacks;
			}

			var result = GetLatestParentIfApplicable(parentsToLookThrough);
			if (result != null)
			{
				logger.LogVerboseOnly(LogType.Information, Res.GetString("fc82449f-8e5a-4bac-94e3-68a45559fd6a", "Found match using Combination Key Match. Picked using best matching references falling back to most recent from {0} candidates.", parents.Length.ToString()));
			}
			return result;
		}

		List<T> GetParentsWithMostMatches(T[] parents)
		{
			var maxMatches = 0;
			var parentsWithMostMatches = new List<T>();

			foreach (var parent in parents)
			{
				var matches = 0;
				foreach (var possibleMatch in possibleMatches)
				{
					matches += possibleMatch(parent);
				}

				if (matches == maxMatches)
				{
					parentsWithMostMatches.Add(parent);
				}
				else if (matches > maxMatches)
				{
					parentsWithMostMatches.Clear();
					maxMatches = matches;
					parentsWithMostMatches.Add(parent);
				}
			}

			return parentsWithMostMatches;
		}

		List<T> GetParentsMatchingOnFallbacks(List<T> parentsToLookThrough)
		{
			var parentsWithBestMatch = new List<T>();

			foreach (var parent in parentsToLookThrough)
			{
				foreach (var possibleMatch in fallbackMatches)
				{
					if (possibleMatch(parent) > 0)
					{
						parentsWithBestMatch.Add(parent);
						break;
					}
				}
			}

			if (parentsWithBestMatch.Count == 1)
			{
				return parentsWithBestMatch;
			}

			if (parentsWithBestMatch.Count > 1)
			{
				if (fallbackMatches.Count > 1)
				{
					parentsToLookThrough = parentsWithBestMatch;
					parentsWithBestMatch = new List<T>();
					foreach (var parent in parentsToLookThrough)
					{
						var addParent = true;

						foreach (var possibleMatch in fallbackMatches)
						{
							if (possibleMatch(parent) == 0)
							{
								addParent = false;
								break;
							}
						}

						if (addParent)
						{
							parentsWithBestMatch.Add(parent);
						}
					}
				}

				if (parentsWithBestMatch.Count > 0)
				{
					return parentsWithBestMatch;
				}
			}

			return parentsToLookThrough;
		}

		protected virtual T GetLatestParentIfApplicable(List<T> parentsToLookThrough)
		{
			T latestParent = null;

			foreach (var parent in parentsToLookThrough)
			{
				if (latestParent == null)
				{
					latestParent = parent;
				}
				else if (CheckLatestParent(parent, latestParent))
				{
					latestParent = parent;
				}
			}

			return latestParent;
		}

		protected void AddPossibleMatch(SchemaColumn schemaColumn, IZType matchValue, MatchDelegate possibleMatchDelegate)
		{
			if (!matchValue.IsEmpty)
			{
				AddPossibleMatch(schemaColumn, (object)matchValue, possibleMatchDelegate);
			}
		}

		protected void AddPossibleMatch(SchemaColumn schemaColumn, object matchValue, MatchDelegate possibleMatchDelegate)
		{
			if (matchValue != null)
			{
				AddPossibleMatch(new ZQuery(schemaColumn, matchValue), possibleMatchDelegate);
			}
		}

		protected void AddPossibleMatch(ZQuery additionalQuery, MatchDelegate possibleMatchDelegate)
		{
			if (additionalQuery != null)
			{
				possibleMatches.Add(possibleMatchDelegate);
				initialMatchingQuery.AddToFilter(additionalQuery, JoinCondition.Or);
			}
		}

		protected void AddFallbackMatch(IZType matchValue, MatchDelegate fallbackMatch)
		{
			if (!matchValue.IsEmpty)
			{
				AddFallbackMatch((object)matchValue, fallbackMatch);
			}
		}

		protected void AddFallbackMatch(object matchValue, MatchDelegate fallbackMatch)
		{
			if (matchValue != null)
			{
				fallbackMatches.Add(fallbackMatch);
			}
		}
	}

	public static class CombinationKeyMatcherParentLimit
	{
		public static int Value => value;
		[ThreadSafe] // this is only changable during tests.
		static int value = 1000;

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Test only")]
		public static void OverrideParentLimitForTesting(int value)
		{
			CombinationKeyMatcherParentLimit.value = value;
		}
#endif
	}
}
