using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Country and user count for a company.
	/// </summary>
	public class CompanyCountryUsers
	{
		/// <summary>
		/// CountryCode is the actual country code while mainCountryCode is the code of the main country.
		/// Will differ for countries that are part of a group like Hong Kong (HK) has a main of China (CN).
		/// Normally will be the same.
		/// </summary>
		public CompanyCountryUsers(int activeUsers, string countryCode, string mainCountryCode)
		{
			ActiveUsers = activeUsers;
			CountryCode = countryCode;
			MainCountryCode = mainCountryCode;
		}

		public int ActiveUsers { get; private set; }
		public string CountryCode { get; private set; }
		public string MainCountryCode { get; private set; }
	}

	public class CountryUserGroup
	{
		public int TotalCompanyCount { get; set; }

		/// <summary>
		///  The domestic country - has the most users
		/// </summary>
		public string DomesticCountry { get; set; }

		/// <summary>
		/// Count of companies in the domestic country
		/// </summary>
		public int DomesticCompanyCount { get; set; }

		/// <summary>
		/// Count of users in the domestic country
		/// </summary>
		public int DomesticUserCount { get; set; }

		public int TotalUserCount { get; set; }

		/// <summary>
		/// Number of main countries
		/// </summary>
		public int MainCountryCount { get; set; }

		public int AverageUsersPerCountry => MainCountryCount != 0 ? TotalUserCount / MainCountryCount : 0;

		public int ForeignCompanyCount { get { return TotalCompanyCount - DomesticCompanyCount; } }
		public int ForeignUserCount { get { return TotalUserCount - DomesticUserCount; } }
		public decimal ForeignUserPercent { get { return TotalUserCount != 0 ? ForeignUserCount * 100m / TotalUserCount : 0; } }
		public string SingleDomesticEntityCountry { get { return TotalCompanyCount == 1 ? DomesticCountry : ""; } }
	}

	/// <summary>
	/// Country and users for companies sharing a database.
	/// </summary>
	public class DatabaseCountryUsers
	{
		/// <summary>
		/// Constructor for BillingRunContext.DatabaseCountryUsers
		/// Only considers companies with users. Ignores countries without users.
		/// </summary>
		public DatabaseCountryUsers(List<CompanyCountryUsers> domesticCompanies, List<CompanyCountryUsers> developingRegionCompanies)
			: this()
		{
			InitUserGroup(domesticCompanies, domesticCountryUserGroup);
			InitUserGroup(developingRegionCompanies, developingRegionUserGroup);
		}

		static void InitUserGroup(List<CompanyCountryUsers> companies, CountryUserGroup userGroup)
		{
			if (companies.Count > 0)
			{
				userGroup.TotalCompanyCount = companies.Count;
				var mainCountryGroups = companies.GroupBy(x => x.MainCountryCode);
				userGroup.MainCountryCount = mainCountryGroups.Count();
				int maxUsers = -1;
				userGroup.TotalUserCount = 0;
				foreach (var mainCountryGroup in mainCountryGroups)
				{
					var localUsers = mainCountryGroup.Sum(x => x.ActiveUsers);
					userGroup.TotalUserCount += localUsers;
					if (localUsers > maxUsers)
					{
						maxUsers = localUsers;
						userGroup.DomesticCountry = mainCountryGroup.Key;
						userGroup.DomesticCompanyCount = mainCountryGroup.Count();
						userGroup.DomesticUserCount = localUsers;
					}
				}
			}
		}

		/// <summary>
		/// Constructor for main STL/ODPL billing.
		/// Included all active client companies, even if they have no users.
		/// </summary>
		/// <param name="clientCompanies"></param>
		/// <param name="usages"></param>
		public DatabaseCountryUsers(ClientCompany[] clientCompanies, IEnumerable<ClientChargeableUsage> usages)
			: this()
		{
			if (clientCompanies == null)
			{
				return;
			}

			CalculateCountryGroups(clientCompanies, usages, DomesticCountryUserGroup, isDevelopingRegionGrouping: false);
			CalculateCountryGroups(clientCompanies, usages, DevelopingRegionUserGroup, isDevelopingRegionGrouping: true);
		}

		static void CalculateCountryGroups(ClientCompany[] clientCompanies, IEnumerable<ClientChargeableUsage> usages, CountryUserGroup userGroup, bool isDevelopingRegionGrouping)
		{
			userGroup.TotalCompanyCount = clientCompanies.Length;

			var countryGroupRegistryItem = EDIDataRegistry.Instance.BillingCountryGroups.Value;
			var countryGroups = isDevelopingRegionGrouping ? countryGroupRegistryItem.GetCodeDescriptionPairList() : countryGroupRegistryItem.GetActiveCodeDescriptionPairList();
			var countryGroupsDict = countryGroups.Cast<CodeDescriptionPair>().ToDictionary(x => x.Code, y => y.Description);

			var mainCountryGroups = clientCompanies.GroupBy(x => GetUserCountryGroup(x, countryGroupsDict));
			userGroup.MainCountryCount = mainCountryGroups.Count();

			var clientCompanyPkToActiveUsers = usages.Where(x => x.U1_Code == "STL" && x.U1_SubCode == "USR").ToDictionary(x => x.U1_LCC, y => (int)y.U1_UnitCount);

			int maxUsers = -1;
			userGroup.TotalUserCount = 0;
			foreach (var mainCountryGroup in mainCountryGroups)
			{
				int localUsers = 0;
				foreach (var clientCompany in mainCountryGroup)
				{
					clientCompanyPkToActiveUsers.TryGetValue(clientCompany.PK, out var users);
					localUsers += users;
				}

				userGroup.TotalUserCount += localUsers;
				if (localUsers > maxUsers)
				{
					maxUsers = localUsers;
					userGroup.DomesticCountry = mainCountryGroup.Key;
					userGroup.DomesticCompanyCount = mainCountryGroup.Count();
					userGroup.DomesticUserCount = localUsers;
				}
			}

			var usersWithoutClientCompany = clientCompanyPkToActiveUsers.Values.Sum() - userGroup.TotalUserCount;
			if (usersWithoutClientCompany > 0)
			{
				// if users were allocated to the demo company or some other unknown company
				// then add them to the domestic company
				userGroup.TotalUserCount += usersWithoutClientCompany;
				userGroup.DomesticUserCount += usersWithoutClientCompany;
			}
		}

		static string GetUserCountryGroup(ClientCompany company, Dictionary<string, string> countryGroupsDict)
		{
			countryGroupsDict.TryGetValue(company.LCC_RN_NKCountryCode, out var countryGroup);
			return string.IsNullOrEmpty(countryGroup) ? company.LCC_RN_NKCountryCode.ToString() : countryGroup;
		}

		DatabaseCountryUsers()
		{
			domesticCountryUserGroup = new CountryUserGroup();
			developingRegionUserGroup = new CountryUserGroup();
		}
		readonly CountryUserGroup domesticCountryUserGroup;
		readonly CountryUserGroup developingRegionUserGroup;
		public CountryUserGroup DomesticCountryUserGroup => domesticCountryUserGroup;
		public CountryUserGroup DevelopingRegionUserGroup => developingRegionUserGroup;
	}

	public class DatabaseCountryUserSet
	{
		readonly Dictionary<DateTime, Dictionary<Guid, DatabaseCountryUsers>> periodToDatabasePkToUsers = new Dictionary<DateTime, Dictionary<Guid, DatabaseCountryUsers>>();

		public DatabaseCountryUsers[] Get(DateTime mostRecentPeriodStart, int periodCount, Guid databasePk)
		{
			List<DateTime> monthsToLoad = new List<DateTime>(periodCount);
			var result = new DatabaseCountryUsers[periodCount];

			DateTime periodStart = mostRecentPeriodStart;
			for (int i = 0; i < periodCount; ++i, periodStart = periodStart.AddMonths(-1))
			{
				Dictionary<Guid, DatabaseCountryUsers> databasePkToUsers;
				if (periodToDatabasePkToUsers.TryGetValue(periodStart, out databasePkToUsers))
				{
					DatabaseCountryUsers countryUsers;
					if (databasePkToUsers.TryGetValue(databasePk, out countryUsers))
					{
						result[i] = countryUsers;
					}
					else
					{
						monthsToLoad.Add(periodStart);
						databasePkToUsers.Add(databasePk, null);
					}
				}
				else
				{
					databasePkToUsers = new Dictionary<Guid, DatabaseCountryUsers>();
					databasePkToUsers.Add(databasePk, null);
					periodToDatabasePkToUsers.Add(periodStart, databasePkToUsers);
					monthsToLoad.Add(periodStart);
				}
			}

			if (monthsToLoad.Count > 0)
			{
				var periodToDbUsers = Load(monthsToLoad, databasePk);

				periodStart = mostRecentPeriodStart;
				for (int i = 0; i < periodCount; ++i, periodStart = periodStart.AddMonths(-1))
				{
					DatabaseCountryUsers countryUsers;
					if (periodToDbUsers.TryGetValue(periodStart, out countryUsers))
					{
						var databasePkToUsers = periodToDatabasePkToUsers[periodStart];
						databasePkToUsers[databasePk] = countryUsers;
						result[i] = countryUsers;
					}
				}
			}

			return result;
		}

		Dictionary<DateTime, DatabaseCountryUsers> Load(List<DateTime> monthsToLoad, Guid databasePk)
		{
			Dictionary<DateTime, DatabaseCountryUsers> periodToDbUsers = new Dictionary<DateTime, DatabaseCountryUsers>();

			var domesticCountryGroups = EDIDataRegistry.Instance.BillingCountryGroups.Value.GetActiveCodeDescriptionPairList();
			var developingRegionCountryGroups = EDIDataRegistry.Instance.BillingCountryGroups.Value.GetCodeDescriptionPairList();

			var datesInSqlFormat = string.Join("', '", monthsToLoad.Select(x => SqlFormatInfo.ToSqlDateString(x)));

			string sql = @"
select U1_PeriodStart, ActiveUsers,
	CountryCode = ISNULL(NULLIF(LCC_RN_NKCountryCode, ''), LC_CompanyCountry)
from
(
	select U1_PeriodStart, U1_LCC, 
		ActiveUsers = cast(sum(case when U1_Code = 'STL' and U1_SubCode = 'USR' then U1_UnitCount else 0 end) as int)
	from dbo.ClientChargeableUsage
	where U1_PeriodStart in ('" + datesInSqlFormat + @"') and U1_LCC is not null
		and U1_LD = @DatabasePk
	group by U1_PeriodStart, U1_LCC
) a
join dbo.ClientCompany on U1_LCC = LCC_PK
left join dbo.LicenceCompany on LCC_OH = LC_OH
where ActiveUsers != 0 or ISNULL(NULLIF(LCC_RN_NKCountryCode, ''), LC_CompanyCountry) is not null
order by U1_PeriodStart
";

			var domesticCompanies = new List<CompanyCountryUsers>();
			var developingRegionCompanies = new List<CompanyCountryUsers>();

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@DatabasePk", System.Data.SqlDbType.UniqueIdentifier, databasePk);
				DatabaseCountryUsers dbUsers = null;
				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					DateTime lastPeriodStart = DateTime.MinValue;

					while (reader.Read())
					{
						// blank country code can occur when usage is assigned to the demo company
						int i = 0;
						var periodStart = reader.GetDateTime(i++);
						var activeUsers = reader.GetInt32(i++);
						var countryCode = reader.GetString(i++);

						var domesticCountryGroup = domesticCountryGroups.GetDescriptionFromCode(countryCode);
						if (string.IsNullOrEmpty(domesticCountryGroup))
						{
							domesticCountryGroup = countryCode;
						}
						var developingRegionCountryGroup = developingRegionCountryGroups.GetDescriptionFromCode(countryCode);
						if (string.IsNullOrEmpty(developingRegionCountryGroup))
						{
							developingRegionCountryGroup = countryCode;
						}

						if (periodStart != lastPeriodStart && (domesticCompanies.Count > 0 || developingRegionCompanies.Count > 0))
						{
							// end prev
							dbUsers = new DatabaseCountryUsers(domesticCompanies, developingRegionCompanies);
							periodToDbUsers.Add(lastPeriodStart, dbUsers);
							domesticCompanies.Clear();
							developingRegionCompanies.Clear();
						}
						domesticCompanies.Add(new CompanyCountryUsers(activeUsers, countryCode, domesticCountryGroup));
						developingRegionCompanies.Add(new CompanyCountryUsers(activeUsers, countryCode, developingRegionCountryGroup));
						lastPeriodStart = periodStart;
					}

					if (domesticCompanies.Count > 0 || developingRegionCompanies.Count > 0)
					{
						dbUsers = new DatabaseCountryUsers(domesticCompanies, developingRegionCompanies);
						periodToDbUsers.Add(lastPeriodStart, dbUsers);
					}
				}
			}

			return periodToDbUsers;
		}
	}

	//logic shared by STL/ODPL
	public class DatabaseCountryLanguageBillingHelper
	{
		public DatabaseCountryLanguageBillingHelper()
		{
			CountryCodeToLanguages = EDIDataRegistry.Instance.LicenceCountryLanguages.Value
				.OfType<ICodeDescription>()
				.Select(x => new { CountryCode = x.Code, Languages = new HashSet<string>(x.Description.Split(',')) })
				.ToDictionary(x => x.CountryCode, x => x.Languages);
		}

		public bool ShouldBillPriceItem(ClientLicencePriceItem priceItem, DatabaseCountryUsers countryUsers)
		{
			var result = true;

			if (priceItem != null)
			{
				var feeType = priceItem.L7_FeeType;
				var language = priceItem.L7_Language;
				var userGroup = countryUsers.DomesticCountryUserGroup;

				//DAL means free if all users are in the same country group, and the language of the item is a local language.
				if (feeType == BillingConstants.FeeType.DatabaseLanguage)
				{
					if (userGroup.MainCountryCount == 0
						|| (userGroup.MainCountryCount == 1 && IsLocalLanguage(userGroup.DomesticCountry, language)))
					{
						result = false;
					}
				}
				//DAZ means free if the country group with most users has the majority of users 
				//and the language of the item is a local language.
				else if (feeType == BillingConstants.FeeType.DatabaseLanguageZ)
				{
					if (userGroup.MainCountryCount == 0
						|| (2 * userGroup.DomesticUserCount > userGroup.TotalUserCount && IsLocalLanguage(userGroup.DomesticCountry, language)))
					{
						result = false;
					}
				}
				//DAC means free if all users are in the same country group.
				else if (feeType == BillingConstants.FeeType.DatabaseCountry)
				{
					if (userGroup.MainCountryCount <= 1)
					{
						result = false;
					}
				}
			}

			return result;
		}

		bool IsLocalLanguage(string countryCode, string language)
		{
			return CountryCodeToLanguages.TryGetValue(countryCode, out var languages)
				&& languages.Contains(language);
		}

		readonly Dictionary<string, HashSet<string>> CountryCodeToLanguages;
	}
}

