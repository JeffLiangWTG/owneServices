using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Billing for system licences
	/// - CargoWise Cloud Base Production Fee (Charged Per Hosted Production Instance) (price code #HP)
	/// - Non-production system licence fee (charged for every test system, whether it is hosted or not) (price code configured on header, usually #NP).
	/// Implemented by creating a temporary (non-persistent) service for each licence, since services are ideal for charges not based on usage.
	/// </summary>
	public class SystemLicenceBilling
	{
		/// <summary>
		/// Create production system licence services
		/// </summary>
		/// <param name="startDate"></param>
		/// <returns></returns>
		public IReadOnlyList<ClientPremiumService> CreateHostedProductionServices(BusinessObjectFactory factory,
			IEnumerable<IBilledDatabase> mainDbs,
			ZDateTime startDate)
		{
			Argument.NotNull(mainDbs, nameof(mainDbs));

			var result = new List<ClientPremiumService>();
			foreach (var db in mainDbs.Where(x => x.LD_LicenceType == DatabaseTypes.Codes.Production
				&& x.IsHostedOnWiseCloud))
			{
				var service = factory.New<ClientPremiumService>();
				using (service.GetValidationSuspender())
				{
					service.CPS_LD = db.PK;
					service.CPS_Type = BillingConstants.Hosting.WiseCloudProductionLicenceCode;
					service.CPS_StartDate = startDate;
					service.CPS_Units = 1;
					service.CPS_ClientRef = "Server " + db.LD_ServerCode;
					service.IsSystemLicenceFee = true;
				}
				result.Add(service);
			}

			return result;
		}

		/// <summary>
		/// Create services for non-production databases belonging to the given production databases / test databases
		/// </summary>
		/// <param name="maxStartDate">Service must start on or before this date to be included</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IReadOnlyList<ClientPremiumService> CreateNonProductionServices(BusinessObjectFactory factory,
			Guid[] mainDbPks,
			Dictionary<Guid, ClientLicencePriceHeader> dbPkToPriceHeader,
			ZDateTime maxStartDate)
		{
			Argument.NotNull(mainDbPks, nameof(mainDbPks));

			var dbPkToDates = GetSiteLiveDates(mainDbPks.ToHashSet()).ToDictionary(x => x.DatabasePk);
			var testDbs = LoadTestDbs(factory, mainDbPks);

			var result = new List<ClientPremiumService>();
			foreach (var testDb in testDbs)
			{
				DbDates dbDates;
				ClientLicencePriceHeader priceHeader;
				var mainDbPk = new[] { testDb.LD_LD_ParentDatabase, testDb.PK }.First(x => !x.IsEmpty).ToGuid();

				if (dbPkToPriceHeader.TryGetValue(mainDbPk, out priceHeader)
					&& dbPkToDates.TryGetValue(mainDbPk, out dbDates))
				{
					if (!priceHeader.L6_TestDbPriceCode.IsEmpty)
					{
						if (ShouldBillTestDb(testDb, priceHeader, dbDates, maxStartDate))
						{
							ZDateTime billingDate = dbDates.CalculateTestBillingDate(testDb, priceHeader.L6_LiveMonthsUntilTestDbBilling);
							var service = factory.New<ClientPremiumService>();
							using (service.GetValidationSuspender())
							{
								service.CPS_LD = testDb.PK;
								service.CPS_Type = priceHeader.L6_TestDbPriceCode;
								service.CPS_StartDate = billingDate;
								service.CPS_Units = 1;
								service.CPS_ClientRef = "Server " + testDb.LD_ServerCode;
								service.IsSystemLicenceFee = true;
							}
							result.Add(service);
						}
					}
				}
			}

			return result;
		}

		public IEnumerable<ZGuid> CalculateMinimumFees(BusinessObjectFactory factory, LicenceDatabase mainDb, ClientLicencePriceHeader priceHeader, ZDateTime maxStartDate)
		{
			var mainDbPks = new Guid[] { mainDb.PK.ToGuid() };
			var siteLiveDate = GetSiteLiveDates(mainDbPks.ToHashSet()).First();
			var testDbs = LoadTestDbs(factory, mainDbPks);
			return testDbs.Where(x => ShouldBillTestDb(x, priceHeader, siteLiveDate, maxStartDate)).Select(x => x.PK);
		}

		LicenceDatabase[] LoadTestDbs(BusinessObjectFactory factory, Guid[] mainDbPks)
		{
			var testDbQuery = new ZDBOnlyQuery(typeof(LicenceDatabase)) { AllowTableValuedParameters = true };
			testDbQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, SQLComparisonOperator.NotEqual, DatabaseTypes.Codes.Production);

			var dbQuery = new ZQuery() { AllowTableValuedParameters = true };
			dbQuery.AddToFilter(JoinCondition.Or, LicenceDatabaseSchema.LD_LD_ParentDatabase, mainDbPks);
			dbQuery.AddToFilter(JoinCondition.Or, LicenceDatabaseSchema.PK, mainDbPks);

			testDbQuery.AddToFilter(dbQuery);
			testDbQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, ZBool.True);
			testDbQuery.AddToFilter(LicenceDatabaseSchema.LD_Billable, SQLComparisonOperator.NotEqual, Licencing.Business.DatabaseBillableFlagList.Codes.No);

			// must also have at least one active licence company
			ZDBOnlySubQuery activeLicenceSubQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LD);
			activeLicenceSubQuery.AddToFilter(LicenceHeaderSchema.LA_IsActive, ZBool.True);
			testDbQuery.AddSubQuery(activeLicenceSubQuery, JoinCondition.And);

			return factory.Load<LicenceDatabase>(testDbQuery);
		}

		bool ShouldBillTestDb(LicenceDatabase testDb, ClientLicencePriceHeader priceHeader, DbDates dbDates, ZDateTime maxStartDate)
		{
			ZDateTime billingDate = dbDates.CalculateTestBillingDate(testDb, priceHeader.L6_LiveMonthsUntilTestDbBilling);
			return (!billingDate.IsEmpty && (billingDate <= maxStartDate || maxStartDate.IsEmpty));
		}

		class DbDates
		{
			public Guid DatabasePk;
			public ZDateTime LiveDate;
			public ZDateTime FirstBilledDate;
			public ZDateTime StlPricesValidFromDate;

			public ZDateTime CalculateTestBillingDate(LicenceDatabase testDb, int liveMonthsUntilTestDbBilling)
			{
				var productionDate = Min(FirstBilledDate, Max(LiveDate, StlPricesValidFromDate));
				return testDb.LD_Billable == DatabaseBillableFlagList.Codes.YesPartner ? productionDate : AddMonthsIfNotEmpty(productionDate, liveMonthsUntilTestDbBilling);
			}

			static ZDateTime Min(ZDateTime a, ZDateTime b)
			{
				return b.IsEmpty || a < b ? a : b;
			}

			static ZDateTime Max(ZDateTime a, ZDateTime b)
			{
				return b.IsEmpty || a > b ? a : b;
			}

			static ZDateTime AddMonthsIfNotEmpty(ZDateTime a, int months)
			{
				return a.IsEmpty ? a : a.AddMonths(months);
			}
		}

		List<DbDates> GetSiteLiveDates(HashSet<Guid> mainDbPks)
		{
			var result = new List<DbDates>();

			if (!mainDbPks.Any())
			{
				return result;
			}

			string pkList = string.Join("', '", mainDbPks);
			string sql = @"
select LA_LD
	, LA_AgreedLiveDate = MIN(LA_AgreedLiveDate)
	, FirstBilled = MIN(BU9_PeriodStart)
	, PHL_ValidFrom = MIN(PHL_ValidFrom)
from dbo.LicenceHeader
left join
(
	select
		BU9_LD,
		min(BU9_PeriodStart) BU9_PeriodStart
	from dbo.EdiBilledUsage
	join dbo.AccTransactionHeader on BU9_AH_Invoice = AH_PK and AH_IsCancelled = 0
" + (mainDbPks.Count < 20 ? "	where BU9_LD in ('" + pkList + @"')" : string.Empty) + @"
	group by BU9_LD
) billed on BU9_LD = LA_LD
left join dbo.EdiPriceHeaderLink on PHL_LD = LA_LD
" + (mainDbPks.Count < 20 ? "	where LA_LD in ('" + pkList + @"')" : string.Empty) + @"
group by LA_LD";

			using (var cmd = Db.Connection.Command(sql))
			{
				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var databasePK = reader.GetGuid(0);
						// this mechanism is much faster (4 seconds at time of testing) instead of 100's of seconds to run the filtered query, so filtering is done client side.
						if (mainDbPks.Contains(databasePK))
						{
							var dbDates = new DbDates();
							dbDates.DatabasePk = databasePK;
							dbDates.LiveDate = AsDate(reader.GetValue(1));
							dbDates.FirstBilledDate = AsDate(reader.GetValue(2));
							dbDates.StlPricesValidFromDate = AsDate(reader.GetValue(3));
							result.Add(dbDates);
						}
					}
				}
			}

			return result;
		}

		ZDateTime AsDate(object date)
		{
			return date is DBNull ? ZDateTime.Empty : (DateTime)date;
		}
	}
}

