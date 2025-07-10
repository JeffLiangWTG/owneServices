using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Client.EDI.Licencing.AutoLicensing
{
	public class Matcher
	{
		public IEnumerable<Match> GetMatches(LicenceDatabase licenceDatabase)
		{
			var result = Enumerable.Empty<Match>();

			var additionalInfoNote = licenceDatabase.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationAdditionalInfoNote.Description, false, false)
				.Concat(licenceDatabase.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, false, false))
				.OrderByDescending(x => x.ST_CreatedDateUtc).FirstOrDefault()?.ST_NoteDataAsText ?? ZString.Empty;

			if (!additionalInfoNote.IsEmpty)
			{
				var additionalInfo = ParseAdditionalInfo(additionalInfoNote);
				var matchesById = GetMatchesById(licenceDatabase, additionalInfo);

				if (matchesById.Count() == 1)
				{
					result = matchesById;
				}
				else
				{
					var matchesByAddress = GetMatchesByAddress(licenceDatabase, additionalInfo);
					result = CalculateMatches(licenceDatabase, matchesById, matchesByAddress);
				}
			}

			return result.OrderByDescending(x => x.TotalScore);
		}

		IEnumerable<Match> CalculateMatches(LicenceDatabase licenceDatabase, IEnumerable<Match> matchesById, IEnumerable<Match> matchesByAddress)
		{
			var result = Enumerable.Empty<Match>();

			if (matchesById.Any())
			{
				result = matchesById.ToArray().Select(x =>
				{
					x.TotalScore += matchesByAddress.FirstOrDefault(m => m.Org.PK == x.Org.PK)?.TotalScore ?? 0;
					return x;
				});
			}
			else
			{
				result = matchesByAddress;
			}

			if (result.Count() > 1)
			{
				result = result.ToArray();
				if (licenceDatabase.LD_Product == ProductTypes.Codes.CargoSphere)
				{
					result.ForEach(x => CalculateMatchForCargoSphere(x));
				}
			}

			return result;
		}

		IEnumerable<Match> GetMatchesById(LicenceDatabase licenceDatabase, LicenceDatabaseRegistrationAdditionalInfo additionalInfo)
		{
			var result = Enumerable.Empty<Match>();
			if (!string.IsNullOrWhiteSpace(additionalInfo.EnterpriseID) || !string.IsNullOrWhiteSpace(additionalInfo.EnterpriseCode))
			{
				var filter = new ZDBOnlyQuery(typeof(LicenceDatabase));
				if (!string.IsNullOrWhiteSpace(additionalInfo.ServerCode))
				{
					filter.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, SQLComparisonOperator.Equal, additionalInfo.ServerCode);
				}
				filter.AddToFilter(LicenceDatabaseSchema.LD_OH_WebAccessOrg, SQLComparisonOperator.NotEqual, null);

				var subquery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceEnterpriseSchema.PK);
				if (!string.IsNullOrWhiteSpace(additionalInfo.EnterpriseID))
				{
					subquery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseID, SQLComparisonOperator.Equal, additionalInfo.EnterpriseID);
				}
				if (!string.IsNullOrWhiteSpace(additionalInfo.EnterpriseCode))
				{
					subquery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.Equal, additionalInfo.EnterpriseCode);
				}
				filter.AddSubQuery(LicenceDatabaseSchema.LD_LE, subquery, JoinCondition.And);

				var databases = licenceDatabase.Factory.Load<LicenceDatabase>(filter);
				result = databases.OrderByDescending(ld => ld.LD_DatabaseNumber)
								.Where(ld => ld.WebAccessOrg != null && ld.WebAccessOrg.OH_IsActive).Select(ld => ld.WebAccessOrg).Distinct()
								.Select(x => new Match() { Org = (EDIOrgHeader)x, TotalScore = EntIdCodeMatchingRuleScore });
			}

			return result;
		}

		IEnumerable<Match> GetMatchesByAddress(LicenceDatabase licenceDatabase, LicenceDatabaseRegistrationAdditionalInfo additionalInfo)
		{
			var result = Enumerable.Empty<Match>();

			var dedupOrgHeader = new DeduplicationOrgHeader(additionalInfo.OrgName ?? string.Empty)
			{
				OH_PK = Guid.NewGuid(),
				OH_Code = ZString.Empty
			};

			var mainAddress = new DeduplicationOrgAddress
			{
				OA_PK = Guid.NewGuid(),
				OA_OH = dedupOrgHeader.OH_PK,
				OA_RL_NKRelatedPortCode = string.Empty,
				OA_RN_NKCountryCode = additionalInfo.OrgCountry ?? string.Empty,
				OA_AdditionalAddressInformation = string.Empty,
				OA_Address1 = additionalInfo.Address1 ?? string.Empty,
				OA_Address2 = additionalInfo.Address2 ?? string.Empty,
				OA_City = additionalInfo.City ?? string.Empty,
				OA_Code = string.Empty,
				OA_PostCode = additionalInfo.Postcode ?? string.Empty,
				OA_State = additionalInfo.State ?? string.Empty,
				OA_Email = string.Empty,
				OA_Fax = string.Empty,
				OA_Phone = string.Empty,
				OA_Mobile = string.Empty,
				OA_CompanyNameOverride = additionalInfo.OrgName ?? string.Empty,
				OA_ValidationStatus = "NTC",
			};
			dedupOrgHeader.OrgAddresses = new[] { mainAddress };

			var potentialDuplicates = FindPotentialDuplicates(dedupOrgHeader);
			if (potentialDuplicates != null || potentialDuplicates.Any())
			{
				var duplicates = potentialDuplicates.OrderByDescending(x => x.Score).Take(5).ToArray();
				var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				orgQuery.AddToFilter(OrgHeaderSchema.PK, duplicates.Select(x => x.TargetPK));
				orgQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				var activeOrgs = licenceDatabase.Factory.Load<EDIOrgHeader>(orgQuery);

				result = activeOrgs.Join(duplicates, org => org.PK, dup => dup.TargetPK,
							(org, dup) => new Match() { Org = org, TotalScore = Convert.ToInt32(dup.Score * 100.0d) });
			}

			return result;
		}

		protected virtual IReadOnlyList<ScoringResult> FindPotentialDuplicates(DeduplicationOrgHeader header)
			=> new LowThresholdOrgHeaderMatchEngineFinder(header, true, true).FindPotentialDuplicates(false);

		LicenceDatabaseRegistrationAdditionalInfo ParseAdditionalInfo(string additionalInfoNote)
		{
			var result = new LicenceDatabaseRegistrationAdditionalInfo();

			if (!string.IsNullOrWhiteSpace(additionalInfoNote))
			{
				var type = result.GetType();

				foreach (var pair in Regex.Split(additionalInfoNote, "\r\n|\r|\n").Select(line => line.Split(':'))
					.Where(cols => cols.Length == 2 && !cols.Any(col => string.IsNullOrWhiteSpace(col)))
					.Select(x => new KeyValuePair<string, string>(x.First(), x.Last())))
				{
					var propertyInfo = type.GetProperty(pair.Key, typeof(string));
					if (propertyInfo != null)
					{
						propertyInfo.SetValue(result, pair.Value);
					}
				}
			}

			return result;
		}

		void CalculateMatchForCargoSphere(Match match)
		{
			var dbs = match.Org.LicCompany.ActiveOrAllLicDatabases.OfType<LicenceDatabase>();
			if (dbs.Any(x => x.LD_Product == ProductTypes.Codes.CargoWiseOne
				|| x.LD_Product == ProductTypes.Codes.CargoWiseNext
				|| x.LD_Product == ProductTypes.Codes.CargoWise))
			{
				match.TotalScore += ProductCodeMatchingRuleScore;
			}
		}

		const int ProductCodeMatchingRuleScore = 100;
		const int EntIdCodeMatchingRuleScore = 4000;

		class LowThresholdOrgHeaderMatchEngineFinder : OrgHeaderMatchEngineFinder
		{
			public LowThresholdOrgHeaderMatchEngineFinder(DeduplicationOrgHeader header, bool shouldUseCache, bool useMaxRecords) : base(header, shouldUseCache, useMaxRecords)
			{
			}

			protected override double GetScoreThresholdFromConfidenceRating() => 0.20;
		}
	}
}
