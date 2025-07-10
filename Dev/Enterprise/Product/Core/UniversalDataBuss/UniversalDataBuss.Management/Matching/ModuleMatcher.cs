using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Res = Enterprise.UniversalDataBuss.DataObjects.Res;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public sealed class ModuleMatcher<T, U> : ModuleMatcher<T, U, U>
		where T : IShipmentDataObjectReader
		where U : BusinessObject
	{
		public ModuleMatcher(UniversalObjectFactory factory, ZQuery additionalFilter = null)
			: base(factory, additionalFilter)
		{
		}

		protected override U GetOuterMatchedBO(U innerMatchedBO)
		{
			return innerMatchedBO;
		}
	}

	public abstract class ModuleMatcher<T, U, V> : IModuleMatcher<T, V>
		where T : IShipmentDataObjectReader
		where U : BusinessObject
		where V : BusinessObject
	{
		protected ModuleMatcher(UniversalObjectFactory factory, ZQuery additionalFilter)
		{
			this.matchCriteria = new List<IMatchWithFilterAndScore>();
			this.factory = Argument.NotNull(factory, "UniversalObjectFactory factory");
			this.additionalFilter = additionalFilter;
		}

		readonly List<IMatchWithFilterAndScore> matchCriteria;
		protected readonly UniversalObjectFactory factory;
		readonly ZQuery additionalFilter;

		public ModuleMatchResult<V> GetBestMatch(T topLevelDataObjectReader)
		{
			return GetBestMatchCore(() => GetMatchCriteriaWithDataPresent(topLevelDataObjectReader));
		}

		ModuleMatchResult<V> GetBestMatchCore(Func<List<IMatchWithFilterAndScore>> matchWithFilterAndScore)
		{
			var matchResult = new ModuleMatchResult<V>();
			matchResult.LogVerboseOnly(LogType.Information, Res.GetString("4576ce47-b277-49f7-90d7-b7b309b51fa8", "Starting Reference/Party ID scoring."));

			var matchCriteriaWithDataPresent = matchWithFilterAndScore();
			var incomingValueNames = string.Join(", ", matchCriteriaWithDataPresent.Select(o => o.IncomingValueName).ToArray());
			matchResult.LogVerboseOnly(LogType.Information, Res.GetString("e61878e4-5739-40de-9315-9fdb9d79974f", "Have incoming values for {0}.", incomingValueNames));

			var matchingBOs = factory.Load<U>(BuildQueryForAllReferencesPresent(matchCriteriaWithDataPresent));
			if (matchingBOs == null || matchingBOs.Length == 0)
			{
				matchResult.LogVerboseOnly(LogType.Information, Res.GetString("7f567f50-f199-46e0-a8b3-b003cbfc05ba", "Found no potential matches using any of the incoming values."));
			}
			else
			{
				if (matchingBOs.Length == 1)
				{
					matchResult.LogVerboseOnly(LogType.Information, Res.GetString("7e1388e2-48f0-4b94-8a8f-df696f8c3c5d", "Found 1 potential match..."));
				}
				else
				{
					matchResult.LogVerboseOnly(LogType.Information, Res.GetString("4c58de96-8784-495c-b826-7fe891d3f91d", "Found {0} potential matches...", matchingBOs.Length.ToString()));
				}

				var potentialMatches = matchingBOs.OrderBy(o => o.HumanReadableName).Select(o => new PotentialMatch<U, V>(o, GetOuterMatchedBO)).ToArray();

				var match = new MatchingScorer<U, V>(matchResult).GetMatchWithHighestScore(potentialMatches, matchCriteriaWithDataPresent);

				if (match != null)
				{
					var matchTarget = match.MatchTarget;
					if (match.Score < MinimumScoreForMatch)
					{
						matchResult.Log(LogType.Information, Res.GetString("aa0b0015-7890-4889-a707-df30d6d11a64", "Best Reference/Party ID match is {0} with a score of {1}, but the minimum match score is {2}. Match failed.", matchTarget.HumanReadableName, match.Score, MinimumScoreForMatch));
					}
					else
					{
						matchResult.SetMatch(matchTarget.OuterTarget, match.Score);
						matchResult.Log(LogType.Information, Res.GetString("d9c81d47-b65e-42b5-99e1-78eae07cc1c9", "Matched to {0} with a Reference/Party ID match score of {1}.", matchTarget.HumanReadableName, match.Score));
					}

					return matchResult;
				}
			}

			matchResult.Log(LogType.Information, Res.GetString("b3d93044-6ecf-4fe2-b1fe-72d351183197", "No Reference/Party ID matches were found in this module."));
			return matchResult;
		}

		protected abstract V GetOuterMatchedBO(U innerMatchedBO);

		#region AddMatch Methods used for adding matching MetaData

		public void AddPossibleMatchReferenceAndJobDocAddress(ReferenceElementName referenceElementName, SchemaStringColumn referenceField, MatchableOrganizationType organisationAddressType, DocAddressType jobDocAddressType, Score score, OrganisationTypes unmatchedOrgNoteType = OrganisationTypes.None, string unmatchedOrgNoteSubType = null)
		{
			AddPossibleMatch(referenceElementName, referenceField, OrganizationAddressMatchToJobDocAddress.New(organisationAddressType, jobDocAddressType, OrganisationMatcher, unmatchedOrgNoteType, unmatchedOrgNoteSubType), score);
		}

		public void AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName referenceElementName, SchemaStringColumn referenceField, MatchableOrganizationType organisationAddressType, SchemaGuidColumn orgAddressPKField, Score score, OrganisationTypes unmatchedOrgNoteType = OrganisationTypes.None, string unmatchedOrgNoteSubType = null)
		{
			AddPossibleMatch(referenceElementName, referenceField, new OrganizationAddressMatchToOrgHeader(organisationAddressType, orgAddressPKField, OrganisationMatcher, unmatchedOrgNoteType, unmatchedOrgNoteSubType), score);
		}

		public void AddPossibleMatchReferenceAndOrgAddress(ReferenceElementName referenceElementName, SchemaStringColumn referenceField, MatchableOrganizationType organisationAddressType, SchemaGuidColumn orgHeaderPKField, Score score, OrganisationTypes unmatchedOrgNoteType = OrganisationTypes.None, string unmatchedOrgNoteSubType = null)
		{
			AddPossibleMatch(referenceElementName, referenceField, new OrganizationAddressMatchToOrgAddress(organisationAddressType, orgHeaderPKField, OrganisationMatcher, unmatchedOrgNoteType, unmatchedOrgNoteSubType), score);
		}

		public void AddPossibleMatchReferenceAndOrgHeaderPKs(ReferenceElementName referenceElementName, SchemaStringColumn referenceField, MatchableOrganizationType organisationAddressType, TargetOrgHeaderPKGetter<U> targetOrgHeaderPksGetter, Score score, OrganisationTypes unmatchedOrgNoteType = OrganisationTypes.None, string unmatchedOrgNoteSubType = null)
		{
			AddPossibleMatch(referenceElementName, referenceField, new OrganizationAddressMatch<U>(organisationAddressType, OrganisationMatcher, targetOrgHeaderPksGetter, unmatchedOrgNoteType, unmatchedOrgNoteSubType), score);
		}

		void AddPossibleMatch(ReferenceElementName referenceElementName, SchemaStringColumn referenceField, IOrganizationAddressMatch organisationMatch, Score score)
		{
			matchCriteria.Add(new ReferenceAndOrganisationMatch(referenceElementName, referenceField, organisationMatch, score));
		}

		#endregion

		#region Matching Implementation

		List<IMatchWithFilterAndScore> GetMatchCriteriaWithDataPresent(T topLevelDataObjectReader)
		{
			var result = new List<IMatchWithFilterAndScore>();
			foreach (var match in matchCriteria)
			{
				if (match.HasDataForMatching(topLevelDataObjectReader))
				{
					result.Add(match);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery BuildQueryForAllReferencesPresent(List<IMatchWithFilterAndScore> potentialMatches)
		{
			if (potentialMatches.Count == 0)
			{
				return ZQuery.NoResultQuery;
			}

			var result = new ZQuery();

			foreach (var potentialMatch in potentialMatches)
			{
				result.AddToFilter(potentialMatch.GetFilter(), JoinCondition.Or);
			}

			if (additionalFilter != null)
			{
				return new ZQuery(result, additionalFilter);
			}

			return result;
		}

		static int MinimumScoreForMatch
		{
			get { return 90; }
		}

		OrganizationAddressMatchPool organisationMatcher;
		OrganizationAddressMatchPool OrganisationMatcher
		{
			get { return organisationMatcher ?? (organisationMatcher = new OrganizationAddressMatchPool()); }
		}

		#endregion
	}

	public enum ReferenceElementName
	{
		AgentsReference,
		BookingConfirmationReference,
		CartageWaybillNumber,
		CFSReference,
		InterimReceiptNumber,
		OwnerRef,
		QuoteNumber,
		WayBillNumber,
	}

	public enum MatchableOrganizationType
	{
		SendingForwarderAddress,
		ReceivingForwarderAddress,
		ShippingLineAddress,
		ConsignorDocumentaryAddress,
		ConsigneeDocumentaryAddress,
		DepartureCFSAddress,
		ImporterDocumentaryAddress,
		SupplierDocumentaryAddress
	}
}
