using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.BorderWise
{
	public class BorderWiseBillingSystem : BillingSystemWithDatabase
	{
		public BorderWiseBillingSystem()
		{
		}

		public const string UserPriceCode = BillingConstants.BorderWise.UserPriceCode;
		public const string ExtraMachinePriceCode = BillingConstants.BorderWise.ExtraMachinePriceCode;

		public override string SystemCode => BillingConstants.BillingSystem.BorderWise;
		protected override SystemBill CreateSystemBill() => new BorderWiseSystemBill(Context.Factory);
		protected override bool AccumulateUnbilledMonths => false;

		protected override ClientChargeableUsage[] LoadChargeableUsages()
		{
			var usageSet = new BorderWiseUsageSet(Context);
			return usageSet.AllUsagesWithDatabase.ToArray();
		}

		protected override bool ShouldCreateSystemBillPerCurrency => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			var result = new List<SystemUsage>();
			var licHeaders = Context.Factory.Load<LicenceHeader>(new ZQuery(LicenceHeaderSchema.LA_LD, chargeableUsages.Select(x => x.U1_LD.ToGuid()).Distinct()));
			var databaseAndCompanyToLicHeader = licHeaders.ToDictionary(x => Tuple.Create(x.LA_LD.ToGuid(), x.LA_LC.ToGuid()));
			var purchasedGroups = EDIDataRegistry.Instance.BorderWisePurchasedGroups.Value;
			foreach (var chargeableUsagesByPeriod in chargeableUsages.GroupBy(x => x.U1_PeriodStart))
			{
				var databasePkToLicences = LoadPurchasedLicences(chargeableUsagesByPeriod.Where(x => !x.U1_LD.IsEmpty).Select(x => x.U1_LD).Distinct(), chargeableUsagesByPeriod.Key)
					.GroupBy(x => x.LS9_LD)
					.ToDictionary(x => x.Key);

				foreach (var chargeableUsagesByDatabase in chargeableUsagesByPeriod.GroupBy(x => x.U1_LD))
				{
					Dictionary<ClientChargeableUsage, int> usageToIncludedCount = null;

					if (databasePkToLicences.TryGetValue(chargeableUsagesByDatabase.Key, out var purchasedLicencesForDatabase))
					{
						usageToIncludedCount = BuildUsageToIncludedCount(chargeableUsagesByDatabase, purchasedLicencesForDatabase, purchasedGroups);
					}

					bool hasPurchasedLicences = purchasedLicencesForDatabase != null;

					foreach (var chargeableUsagesByCompany in chargeableUsagesByDatabase.GroupBy(x => x.U1_LC))
					{
						UsingParty user;
						if (databaseAndCompanyToLicHeader.TryGetValue(Tuple.Create(chargeableUsagesByDatabase.Key.ToGuid(), chargeableUsagesByCompany.Key.ToGuid())
							, out var licHeader))
						{
							user = new UsingParty(licHeader);
						}
						else
						{
							user = null;
						}

						foreach (var chargeableUsage in chargeableUsagesByCompany)
						{
							int includedCount;
							if (usageToIncludedCount == null || !usageToIncludedCount.TryGetValue(chargeableUsage, out includedCount))
							{
								includedCount = 0;
							}

							var systemUsage = new UniversalPriceSystemUsage(
								Context.Factory,
								SystemCode,
								BillingConstants.PriceHeaderType.BorderWise,
								user ?? new UsingParty(chargeableUsage),
								chargeableUsage.U1_PeriodStart,
								chargeableUsage.U1_SubCode,
								chargeableUsage.U1_UnitCountAsInt,
								hasPurchasedLicences ? "Additional Licences" : "Licences",
								"",
								includedCount,
								includedUnitCountDescription: "Licences Purchased",
								totalUnitCountDescription: "Total Licenses");

							if (systemUsage.HasPriceItem)
							{
								systemUsage.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;
								systemUsage.ChargeableUsagePKs.Add(chargeableUsage.PK);
								systemUsage.LicenceUnitsMultiplier = 0.02m;

								if (chargeableUsagesByPeriod.Key >= BorderWiseSystemBill.DateForFixedCurrencyPerItem)
								{
									var currency = systemUsage.CurrencyFromPriceItem;
									if (!currency.IsEmpty)
									{
										systemUsage.ApplyCurrencyCode(currency);
									}
								}
								else
								{
									systemUsage.ApplyCurrencyCode("AUD");
								}

								result.Add(systemUsage);
							}
						}
					}
				}
			}

			return result.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		Dictionary<ClientChargeableUsage, int> BuildUsageToIncludedCount(
			IEnumerable<ClientChargeableUsage> usages,
			IEnumerable<BorderWisePurchasedLicenceSetting> settings,
			ReadOnlyCodeDescriptionPairList groups)
		{
			var usageToIncludedCount = new Dictionary<ClientChargeableUsage, int>();

			var subCodeToUsageList = usages
				.GroupBy(x => (string)x.U1_SubCode)
				.ToDictionary(
					x => x.Key,
					y => y
						.OrderByDescending(z => z.U1_UnitCount)
						.ThenBy(z => z.LicenceCompany.LC_CompanyCode)
						.ToList(),
					StringComparer.OrdinalIgnoreCase);

			foreach (var setting in settings
				// Ordered so single code purchases come before group purchases
				.OrderBy(x => groups.ContainsCode(x.PriceCode) ? 1 : 0))
			{
				int purchasedCount = setting.LicenceCount;

				var groupMemberText = groups.GetDescriptionFromCode(setting.PriceCode);
				var priceCodes = !string.IsNullOrEmpty(groupMemberText)
					? groupMemberText.Split(',').Select(x => x.Trim())
					: new string[] { setting.PriceCode };

				ClientChargeableUsage first = null;
				foreach (var priceCode in priceCodes)
				{
					if (subCodeToUsageList.TryGetValue(priceCode, out var usageOrdered))
					{
						foreach (var usage in usageOrdered)
						{
							if (first == null)
							{
								first = usage;
							}

							int totalIncludedCount;
							if (!usageToIncludedCount.TryGetValue(usage, out totalIncludedCount))
							{
								totalIncludedCount = 0;
							}

							int extraIncludedCount = Math.Min(purchasedCount, usage.U1_UnitCountAsInt - totalIncludedCount);
							if (extraIncludedCount > 0)
							{
								totalIncludedCount += extraIncludedCount;
								purchasedCount -= extraIncludedCount;
								usageToIncludedCount[usage] = totalIncludedCount;
							}
						}
					}
				}

				if (purchasedCount != 0 && first != null)
				{
					// Show leftovers
					int totalIncludedCount;
					if (!usageToIncludedCount.TryGetValue(first, out totalIncludedCount))
					{
						totalIncludedCount = 0;
					}

					totalIncludedCount += purchasedCount;
					usageToIncludedCount[first] = totalIncludedCount;
				}
			}

			return usageToIncludedCount;
		}

		BorderWisePurchasedLicenceSetting[] LoadPurchasedLicences(IEnumerable<ZGuid> databasePks, ZDateTime periodStart)
		{
			if (!databasePks.Any())
			{
				return Array.Empty<BorderWisePurchasedLicenceSetting>();
			}

			var query = new ZQuery(EdiLicenceSettingSchema.LS9_Type, BillingConstants.LicenceSetting.BorderWisePurchasedLicences);
			query.AddToFilter(EdiLicenceSettingSchema.LS9_LD, databasePks);
			query.AddToFilter(EdiLicenceSettingSchema.LS9_ValidFrom, SQLComparisonOperator.LessThanOrEqualTo, periodStart);
			var toQuery = new ZQuery(EdiLicenceSettingSchema.LS9_ValidTo, SQLComparisonOperator.GreaterThan, periodStart.AddDays(15));
			toQuery.AddToFilter(JoinCondition.Or, EdiLicenceSettingSchema.LS9_ValidTo, ZDateTime.Empty);
			query.AddToFilter(toQuery);

			return Context.Factory.Load<BorderWisePurchasedLicenceSetting>(query);
		}

		protected override SystemUsage[] GetBillableSystemUsages(SystemUsage[] systemUsages)
		{
			return systemUsages;
		}

		#region Raw Usages

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			SystemCodeRawUsage raw = new SystemCodeRawUsage(context, SystemCode);
			raw.SummaryHeaderDescription = "BorderWise Licences";

			var list = new List<BorderWiseRawUsage>();

			while (reader.Read())
			{
				list.Add(new BorderWiseRawUsage(reader));
			}

			bool hasEditions = context.PeriodAsInt >= 201807;
			SetColumns(raw.Summary.Header, null, hasEditions);

			if (list.Any())
			{
				foreach (var rawUsage in list)
				{
					var line = raw.Summary.Lines.AddNew();
					SetColumns(line, rawUsage, hasEditions);
				}
			}

			return raw;
		}

		static void SetColumns(SummaryLine line, BorderWiseRawUsage rawUsage, bool hasEditions)
		{
			line.Column1 = rawUsage?.OrgCode ?? "Organization";
			line.Column2 = rawUsage?.Email ?? "User";
			line.Column3 = rawUsage?.MachineCountOrEdition ?? (hasEditions ? "Edition" : "Licence");
			line.Column4 = rawUsage?.MachineIdentification ?? "Machine";
			line.Column5 = rawUsage?.ValidFromUtcAsText ?? "Registered";
			line.Column6 = rawUsage?.SpecialUserComment ?? "Special Conditions";
			line.Column7 = rawUsage?.CountryCode ?? "User Country";
			line.Column8 = rawUsage?.MembershipAsText ?? "Membership";
			line.Column9 = rawUsage?.HasCargoWiseOneAsText ?? "CargoWiseOne";
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			bool hasEditions = context.PeriodAsInt >= 201807;
			var headerColumns = new string[] { "Organization", "User", hasEditions ? "Edition" : "Licence", "Machine", "Registered", "Special Conditions", "User Country", "Membership", "CargoWiseOne" };

			var headerCsvLine = new OCsvLine(headerColumns);
			action(headerCsvLine.ToString());

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var rawUsage = new BorderWiseRawUsage(reader);

					string[] dataValues;
					dataValues = new string[]
					{
						rawUsage.OrgCode,
						rawUsage.Email,
						rawUsage.MachineCountOrEdition,
						rawUsage.MachineIdentification,
						rawUsage.ValidFromUtcAsText,
						rawUsage.SpecialUserComment,
						rawUsage.CountryCode,
						rawUsage.MembershipAsText,
						rawUsage.HasCargoWiseOneAsText
					};

					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new InvalidOperationException("BorderWise usage should not be loaded by STL reader");
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			throw new InvalidOperationException("BorderWise usage should not be loaded by STL reader");
		}

		class BorderWiseRawUsage
		{
			public BorderWiseRawUsage(IDataReader reader)
			{
				OrgCode = (string)reader[OrgHeaderSchema.Constants.OH_Code];
				Email = (string)reader[OrgContactSchema.Constants.OC_Email];
				MachineIdentification = (string)reader["MachineIdentification"];
				ValidFromUtc = (DateTime)reader["ValidFromUtc"];
				MachineCount = 0;
				SpecialUserComment = (string)reader[GenRegCertAccredMaintListSchema.Constants.XZ_Comment];
				CountryCode = reader["CountryCode"].ToString();
				MembershipType = reader[EdiOrgMembershipSchema.Constants.EOR_MembershipType].ToString();
				Edition = reader["EditionName"].ToString();
				var boolOrNull = reader["IsCW"];
				HasCargoWiseOne = boolOrNull == DBNull.Value ? null : (bool)boolOrNull;
			}

			public string ValidFromUtcAsText => ValidFromUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
			public string MachineCountAsText => MachineCount.ToString("###", CultureInfo.InvariantCulture);
			public string MachineCountOrEdition => MachineCount > 0 ? MachineCountAsText : Edition;
			public string MembershipAsText => MembershipType;
			public string HasCargoWiseOneAsText => HasCargoWiseOne.HasValue ? (HasCargoWiseOne.Value ? "Y" : "N") : "";

			public readonly string OrgCode;
			public readonly string Email;
			public readonly string MachineIdentification;
			public readonly DateTime ValidFromUtc;
			public int MachineCount;
			public string SpecialUserComment;
			public string CountryCode;
			public string MembershipType;
			public string Edition;
			public bool? HasCargoWiseOne;
		}

		protected override string Query_Raw_Usage
		{
			get { return query_Raw_Usage; }
		}

		const string query_Raw_Usage =
@"
declare @BwDatabasePk uniqueidentifier =
(
	select top 1 LA_LD
	from dbo.LicenceHeader
	join dbo.LicenceDatabase on LA_LD = LD_PK and LD_Product in ('BOR') and LD_IsActive = 1
	join dbo.LicenceEnterprise on LD_LE = LE_PK
	join dbo.LicenceCompany on LA_LC = LC_PK
	where LA_IsActive = 1
		and LC_OH = @OrgPk
		and(LA_AgreedLiveDate is null or LA_AgreedLiveDate < DATEADD(DAY, 25, @DateFrom))
	order by(case LD_LicenceType when 'PRD' then 0 else 1 end),
		(case when LA_AgreedLiveDate is not null then 0 else 1 end),
		LA_AgreedLiveDate
)

declare @Orgs table(OrgPk uniqueidentifier not null)
insert @Orgs(OrgPk)
select @OrgPk
union
select LC_OH from dbo.LicenceCompany join dbo.LicenceHeader on LA_LC = LC_PK and LA_LD = @BwDatabasePk;

select
	OH_Code,
	OC_Email,
	MachineIdentification = MachineId,
	ValidFromUtc = TX_ServiceOccuredUtc,
	ValidToUtc = cast(null as datetime),
	XZ_Comment = ISNULL(XZ_Comment, ''),
	CountryCode = LocationCountryCode,
	EOR_MembershipType = ISNULL(EOR_MembershipType, ''),
	EditionName,
	IsCW
from EdiGetBorderWiseUsers(@Period) bw
join dbo.OrgContact on ContactPk = OC_PK
join dbo.OrgHeader on OC_OH = OH_PK
where OC_OH in (select OrgPK from @Orgs)
ORDER BY
	OH_Code, OC_Email, ValidFromUtc, MachineIdentification
";

		internal void CombineWithOdpl(SystemBill[] bwBills, IEnumerable<OdplSystemBill> odplBills)
		{
			var orgToBill = new Dictionary<Guid, OdplSystemBill>();
			foreach (var bill in odplBills)
			{
				if (orgToBill.TryGetValue(bill.OrganisationPK.ToGuid(), out var otherBill))
				{
					if (bill.TotalAmount > otherBill.TotalAmount)
					{
						orgToBill[bill.OrganisationPK.ToGuid()] = bill;
					}
				}
				else
				{
					orgToBill.Add(bill.OrganisationPK.ToGuid(), bill);
				}
			}

			foreach (BorderWiseSystemBill bwBill in bwBills)
			{
				if (orgToBill.TryGetValue(bwBill.OrganisationPK.ToGuid(), out var otherBill))
				{
					bwBill.CombineWith(otherBill);
				}
				else if (bwBill.CurrencyCode.IsEmpty)
				{
					bwBill.ApplyCurrencyCode(((UniversalPriceSystemUsage)bwBill.SystemUsages.First()).CurrencyFromPriceList);
				}
			}
		}

		#endregion
	}
}

