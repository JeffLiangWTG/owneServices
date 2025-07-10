using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using ZClientEDI.Business;

[assembly: HostedService(
	Enterprise.Client.EDI.Billing.ServiceTasks.ChargeableUsageServiceTask.Code,
	"Chargeable Usage",
	"CSP",
	typeof(Enterprise.Client.EDI.Billing.ServiceTasks.ChargeableUsageServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "6Hours",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "4hours",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.Billing.ServiceTasks
{
	public class ChargeableUsageServiceTask : BillingServiceTask
	{
		public const string Code = "CHU";

		public override void RunTask(CancellationToken token)
		{
			if (!EdiProdDbHelper.IsRunningOnEdiProdDatabase)
			{
				ServiceLogger.Log(LogType.Error, "This service task should only run on the production instance of ediProd");
				return;
			}

			ServiceLogger.Log(LogType.Information, "Begin processing");

			// Do this before ODPL since ODPL needs the STL users to determine Registered User counts
			UpdateBillingDb();

			ZDateTime billingDate = ZDateTime.UtcNow.AddHours(10).Date;

			// Update last month until the 26th
			// Update this month starting on the 3rd
			ZDateTime firstPeriod = billingDate.AddDays(-25);
			firstPeriod = new ZDateTime(firstPeriod.Year, firstPeriod.Month, 1);
			ZDateTime lastPeriod = billingDate.AddDays(-2);
			lastPeriod = new ZDateTime(lastPeriod.Year, lastPeriod.Month, 1);

			// ODPL usage depends on LA_AgreedLiveDate which can change retrospectively.
			// Also, we can retrieve back usage of ODPL if it goes missing.
			// Rather than trying to monitor when back usage arrives or LA_AgreedLiveDate changes
			// we just recalculate everything up to a certain number of months in the past.
			const int OdplMonthsToCalculate = 3;
			var firstMonthOverall = new ZDateTime(2010, 10, 1);
			var odplStart = lastPeriod;
			ZDateTime firstPeriodStart = odplStart;
			for (int i = 0; i < OdplMonthsToCalculate; ++i)
			{
				token.ThrowIfCancellationRequested();

				firstPeriodStart = odplStart;
				var odplBillingPeriod = new BillingPeriod(odplStart);
				UpdateOdpl(odplBillingPeriod);
				UpdateCPT(odplBillingPeriod);
				odplStart = odplStart.AddMonths(-1);
				if (odplStart < firstMonthOverall)
				{
					break;
				}
			}

			UpdateContactCountry(new DateTime(billingDate.Year, billingDate.Month, 1).AddMonths(-1));

			// Take Purchase Seats snapshot
			UpdateLicenceModulesHistory();

			//LicenceDatabase Consolidation
			UpdateLicenceDatabaseConsolidation(billingDate);

			UpdateClientMappingNames();

			for (var period = firstPeriod; period <= lastPeriod; period = period.AddMonths(1))
			{
				var billingPeriod = new BillingPeriod(period);
				UpdateOtherBillingSystems(billingPeriod);
			}

			UpdateStl(lastPeriod);
			DeleteInternalUsage(firstPeriodStart);

			for (var period = firstPeriod; period <= lastPeriod; period = period.AddMonths(1))
			{
				UpdateFreeTrials(period);
			}

			AirlineMessagingBillingSystem.CheckUnmappedUsages(ServiceLogger, billingDate);
			ServiceLogger.Log(LogType.Information, "End processing");
		}

		protected virtual void UpdateStl(ZDateTime currentPeriod)
		{
			DateTime start;
			try
			{
				ServiceLogger.Log(LogType.Information, "Begin GetFirstStlPeriod");
				start = GetFirstStlPeriod(currentPeriod);
			}
			catch (System.Data.Common.DbException ex)
			{
				HandleException(ex, "GetFirstStlPeriod");
				start = currentPeriod.ToDateTime();
			}
			if (start == DateTime.MinValue)
			{
				start = currentPeriod.ToDateTime();
			}
			ServiceLogger.Log(LogType.Information, "End GetFirstStlPeriod");

			DateTime stlPeriod = new DateTime(start.Year, start.Month, 1);
			if (stlPeriod > currentPeriod)
			{
				stlPeriod = currentPeriod.ToDateTime();
			}
			do
			{
				ChargeableUsageUpdate(new BillingPeriod(stlPeriod),
					BillingConstants.BillingSystem.Descriptions.STL + " " + stlPeriod.ToShortDateString(),
					BillingConstants.BillingSystem.STL,
					null, 0);
				stlPeriod = stlPeriod.AddMonths(1);
			}
			while (stlPeriod <= currentPeriod);
		}

		protected virtual void UpdateOtherBillingSystems(BillingPeriod billingPeriod)
		{
			foreach (var systemCode in BillingSystemCodesToUpdate)
			{
				if (IsBillingSystemReady(systemCode, billingPeriod))
				{
					ChargeableUsageUpdate(billingPeriod, systemCode);
				}
			}

			foreach (BillingDbUsageCodes item in EDIDataRegistry.Instance.BillingDbUsageCodesList.Value)
			{
				var category = item.Category;
				var priceCode = item.PriceItemCode;
				var keyRefIndex = item.KeyRefIndex1;
				ChargeableUsageUpdate(
					billingPeriod,
					"Billing DB Usage Category=" + category + " PriceCode=" + priceCode + " KeyRefIndex=" + keyRefIndex,
					category,
					priceCode,
					keyRefIndex);
			}

			foreach (var item in EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.OfType<UsageBillingPriceList>().GroupBy(x => x.ProductCode).Select(x => x.First()))
			{
				var productCode = item.ProductCode;
				var category = item.RawUsageCategory;
				var description = item.Lookups.ProductList.GetDescriptionFromCode(productCode);
				ChargeableUsageUpdate(billingPeriod, $"Product={productCode},Category={category},{description}", category, null, 0);
			}
		}

		protected bool IsBillingSystemReady(string systemCode, BillingPeriod billingPeriod)
		{
			if (systemCode == BillingConstants.BillingSystem.BorderWise && billingPeriod.PeriodStartDate < new ZDateTime(2018, 7, 1))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		DateTime GetFirstStlPeriod(ZDateTime currentPeriod)
		{
			return BillingDb.GetFirstStlPeriod(currentPeriod.ToDateTime());
		}

		protected string[] BillingSystemCodesToUpdate = new string[]
			{
				BillingConstants.BillingSystem.Fax,
				//BillingConstants.BillingSystem.eBACCA,
				BillingConstants.BillingSystem.DeniedPartyScreening,
				//BillingConstants.BillingSystem.ExDocs,
				BillingConstants.BillingSystem.DistanceCalculatorGeneric,
				BillingConstants.BillingSystem.DistanceCalculatorPcMiler,
				BillingConstants.BillingSystem.HostingStorage,
				BillingConstants.BillingSystem.HostingDataAccess,
				BillingConstants.BillingSystem.AirlineMessaging,
				BillingConstants.BillingSystem.NZCustoms,
				BillingConstants.BillingSystem.JapanAFR,
				BillingConstants.BillingSystem.USCustoms,
				BillingConstants.BillingSystem.ClientMapping,
				BillingConstants.BillingSystem.eAdaptor,
				BillingConstants.BillingSystem.E2E,
				BillingConstants.BillingSystem.RailincByMessage,
				BillingConstants.BillingSystem.OceanTracing,
				BillingConstants.BillingSystem.ForwardAir,
				BillingConstants.BillingSystem.ShippingPortMessaging,
				BillingConstants.BillingSystem.GBCustoms,
				BillingConstants.BillingSystem.ImporterSecurityFiling,
				BillingConstants.BillingSystem.ASYCUDA,
				BillingConstants.BillingSystem.ZACustoms,
				BillingConstants.BillingSystem.BorderWise,
				BillingConstants.BillingSystem.FlightStats
			};

		protected virtual void UpdateLicenceModulesHistory()
		{
			if (Globals.IsTest)
			{
				return;
			}

			if (ZDateTime.UtcToday.Day < 26)
			{
				return;
			}

			const string sql =
@"declare @utcnow smalldatetime
set @utcnow = GetUtcDate()

declare @maxDateStampUtc smalldatetime
SELECT @maxDateStampUtc = MAX(LMH_DateStampUtc) from ClientLicenceModulesHistory

if DAY(@utcnow) >= 27 and (@maxDateStampUtc is null or MONTH(@maxDateStampUtc) != MONTH(@utcnow))
begin
	INSERT INTO ClientLicenceModulesHistory
		 (LMH_UserCount, LMH_DateStampUtc, LMH_LM)
	SELECT LM_UserCount, @utcnow, LM_PK
	FROM  dbo.LicenceModules
	join dbo.LicenceHeader on LM_LA = LA_PK
	join dbo.LicenceDatabase on LA_LD = LD_PK
	join dbo.LicenceEnterprise on ld_le = le_pk
	where LM_UserCount != 0
		and LD_LicenceType = 'PRD'
		and (LM_ExpiryDate is null or LM_ExpiryDate > @utcnow)
		and LE_EnterpriseCode not in (select EnterpriseCode from " + BillingUsageSchema.NonBilledEnterpriseCodes + @"())
end";

			ExecuteNonQuery(sql, "UpdateLicenceModulesHistory");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual void UpdateClientMappingNames()
		{
			ServiceLogger.Log(LogType.Information, "Begin ClientMappingNameSync");

			try
			{
				using (DbCommand cmd = Db.Connection.Command("ClientMappingNameSync"))
				{
					const int TimeoutMinutes = 15;
					cmd.CommandTimeout = TimeoutMinutes * 60;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.ExecuteNonQuery();
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				HandleException(ex, "ClientMappingNameSync");
			}

			ServiceLogger.Log(LogType.Information, "End ClientMappingNameSync");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual void UpdateContactCountry(DateTime periodDate)
		{
			int period = periodDate.Year * 100 + periodDate.Month;
			ServiceLogger.Log(LogType.Information, "Begin UpdateContactCountry " + period.ToString(CultureInfo.InvariantCulture));

			try
			{
				using (DbCommand cmd = Db.Connection.Command(BillingUsageSchema.UpdateContactCountry))
				{
					const int TimeoutMinutes = 15;
					cmd.CommandTimeout = TimeoutMinutes * 60;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Period", SqlDbType.Int, period);
					cmd.ExecuteNonQuery();
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				HandleException(ex, "UpdateContactCountry");
			}

			ServiceLogger.Log(LogType.Information, "End UpdateContactCountry");
		}

		public virtual void UpdateOdpl(BillingPeriod billingPeriod)
		{
			ChargeableUsageUpdate(billingPeriod, BillingConstants.BillingSystem.ODM);
		}

		internal void ChargeableUsageUpdate(BillingPeriod billingPeriod, string systemCode)
		{
			ChargeableUsageUpdate(billingPeriod, BillingConstants.BillingSystemList.GetDescriptionFromCode(systemCode), systemCode, null, 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal virtual void ChargeableUsageUpdate(
			BillingPeriod billingPeriod,
			string systemDescription,
			string systemCode,
			string billingDbPriceItemCode,
			int keyRefIndex1,
			string storedProcedureName = BillingUsageSchema.ChargeableUsageUpdate)
		{
			ServiceLogger.Log(LogType.Information, "Begin processing " + systemDescription);

			try
			{
				AllBillingSystems.FirstOrDefault(x => x.SystemCode == systemCode)?.OnChargeableUsageUpdate(billingPeriod, ServiceLogger);

				using (DbCommand cmd = Db.Connection.Command(storedProcedureName))
				{
					const int TimeoutMinutes = 60;
					cmd.CommandTimeout = TimeoutMinutes * 60;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, billingPeriod.PeriodStartDate.ToDateTime());
					cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, billingPeriod.StartTimeUtc.ToDateTime());
					cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, billingPeriod.EndTimeUtc.ToDateTime());
					cmd.AddParameter("@Code", SqlDbType.VarChar, 3, systemCode);
					if (!string.IsNullOrEmpty(billingDbPriceItemCode))
					{
						cmd.AddParameter("@BillingDbPriceItemCode", SqlDbType.VarChar, 3, billingDbPriceItemCode);
					}
					if (keyRefIndex1 != 0)
					{
						cmd.AddParameter("@BillingDbKeyRefIndex1", SqlDbType.TinyInt, keyRefIndex1);
					}
					cmd.ExecuteProcedureWithReturnValue();
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				HandleException(ex, $"{storedProcedureName}({systemCode})");
			}

			ServiceLogger.Log(LogType.Information, "End processing " + systemDescription);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal virtual void DeleteInternalUsage(ZDateTime period)
		{
			string internalEnterpriseCodes = string.Join(",", EDIDataRegistry.Instance.NonBilledEnterpriseCodesAsStringArray.Select(s => '\'' + s + '\'').ToArray());

			if (!string.IsNullOrEmpty(internalEnterpriseCodes))
			{
				string sql =
					"DELETE " + ClientChargeableUsageSchema.Constants.TableName
					+ " FROM " + ClientChargeableUsageSchema.Constants.TableName
					+ " JOIN " + LicenceCompanySchema.Constants.TableName + " ON " + LicenceCompanySchema.Constants.PK + " = " + ClientChargeableUsageSchema.Constants.U1_LC
					+ " JOIN " + LicenceEnterpriseSchema.Constants.TableName + " ON " + LicenceEnterpriseSchema.Constants.PK + " = " + LicenceCompanySchema.Constants.LC_LE
					+ " WHERE " + ClientChargeableUsageSchema.Constants.U1_PeriodStart + " >= @PeriodStart"
					+ " AND " + ClientChargeableUsageSchema.Constants.U1_AH_Invoice + " IS NULL"
					+ " AND " + LicenceEnterpriseSchema.Constants.LE_EnterpriseCode + " IN (" + internalEnterpriseCodes + ')';
				using (DbCommand command = Db.Connection.Command(sql))
				{
					command.AddParameter("@PeriodStart", SqlDbType.DateTime, period.ToDateTime());
					command.ExecuteNonQuery();
				}
			}
		}

		protected virtual void UpdateFreeTrials(ZDateTime period)
		{
			new FreeTrials().CreateFreeTrialsFromNewUsage(period.ToDateTime(), ServiceLogger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void ApplySystemCodeChange(string oldCode, string newCode)
		{
			string sql =
				"UPDATE " + ClientChargeableUsageSchema.Constants.TableName
				+ " SET " + ClientChargeableUsageSchema.Constants.U1_Code + " = '" + newCode + "'"
				+ " WHERE " + ClientChargeableUsageSchema.Constants.U1_Code + " = '" + oldCode + "'";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}

		internal virtual void UpdateCPT(BillingPeriod billingPeriod)
		{
			ChargeableUsageUpdate(billingPeriod, "Licence Usage CPT", Enterprise.Licensing.LicenceTypes.Codes.CPT, null, 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "uses complex SQL scripts that can't be accomplished by using Business Objects")]
		protected void UpdateLicenceDatabaseConsolidation(ZDateTime billingDate)
		{
			var productCodes = EDIDataRegistry.Instance.ConsolidatedBillingSettings.Value.OfType<ConsolidatedBillingSetting>()
								.Where(x => !x.ProductCode.IsEmpty)
								.Select(x => x.ProductCode.ToString());
			if (!productCodes.Any())
			{
				return;
			}

			using (var cmd = Db.Connection.Command($"EXEC {BillingUsageSchema.UpdateLicenceDatabaseConsolidation} @Period, @ProductCodes;"))
			{
				cmd.AddParameter("@Period", SqlDbType.Int, (int)new BillingPeriod(billingDate).Period);
				cmd.AddParameter("@ProductCodes", SqlDbType.VarChar, string.Join(",", productCodes));
				ExecuteNonQuery(cmd, nameof(UpdateLicenceDatabaseConsolidation));
			}
		}

		internal IBillingDatabase BillingDb
		{
			get
			{
				return billingDb ?? (billingDb = new BillingDatabase());
			}
			set
			{
				billingDb = value;
			}
		}

		protected virtual BillingSystemList AllBillingSystems { get; } = new BillingSystemList();

		IBillingDatabase billingDb;
	}

	internal interface IBillingDatabase
	{
		DateTime GetFirstStlPeriod(DateTime currentPeriod);
	}

	internal class BillingDatabase : IBillingDatabase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public DateTime GetFirstStlPeriod(DateTime currentPeriod)
		{
			var periodList = new StringBuilder();
			for (int monthsAgo = 12; monthsAgo >= 0; --monthsAgo)
			{
				DateTime period = currentPeriod.AddMonths(-monthsAgo);
				if (periodList.Length != 0)
				{
					periodList.Append(", ");
				}
				periodList.Append(period.Year * 100 + period.Month);
			}

			string sql = @"
declare @maxtime smalldatetime = ISNULL(DATEADD(HOUR, -1, (select max(U1_UpdateTime) from dbo.ClientChargeableUsage where U1_Code = 'STL' and U1_PeriodStart > DATEADD(MONTH, -6, getutcdate()))), DATEFROMPARTS(2016, 1, 1))

select TX_Period = ISNULL(min(TX_Period), 0)
FROM dbo.BillingViewChargeable where TX_Period in (" + periodList + @")
	and TX_Category = 'STL' and TX_PriceItemCode = 'USR' 
	and TX_CapturedUtc >= @maxTime;";

			using (DbCommand cmd = Db.Connection.Command(sql))
			{
				const int TimeoutMinutes = 15;
				cmd.CommandTimeout = TimeoutMinutes * 60;
				cmd.CommandType = CommandType.Text;
				var result = (int)cmd.ExecuteScalar();
				if (result != 0)
				{
					return new DateTime(result / 100, result % 100, 1);
				}
				else
				{
					return ZDateTime.Now.ToDateTime();
				}
			}
		}
	}
}
