using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating.Testing
{
	[TestedType(typeof(TG_CheckCorrectChargeCodeCompanyOnRateLine))]
	class TG_CheckCorrectChargeCodeCompanyOnRateLineTest : DBCreateTriggerScriptTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Company1 = new ActiveRowWrapper(GlbCompanySchema.Instance)
			{
				[GlbCompanySchema.GC_Code] = "CO1",
				[GlbCompanySchema.GC_Name] = "Company 1",
				[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
				[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
			};
			Company1.Save();
			Company2 = new ActiveRowWrapper(GlbCompanySchema.Instance)
			{
				[GlbCompanySchema.GC_Code] = "CO2",
				[GlbCompanySchema.GC_Name] = "Company 2",
				[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
				[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
			};
			Company2.Save();

			GlobalChargeCode = new ActiveRowWrapper(AccChargeCodeSchema.Instance)
			{
				[AccChargeCodeSchema.AC_Code] = "Charge",
				[AccChargeCodeSchema.AC_ChargeGroup] = "FRT",
			};
			GlobalChargeCode.Save();

			LocalChargeCodeCompany1 = new ActiveRowWrapper(AccChargeCodeSchema.Instance)
			{
				[AccChargeCodeSchema.AC_Code] = "Charge",
				[AccChargeCodeSchema.AC_ChargeGroup] = "FRT",
				[AccChargeCodeSchema.AC_GC] = Company1.PK,
			};
			LocalChargeCodeCompany1.Save();

			LocalChargeCodeCompany2 = new ActiveRowWrapper(AccChargeCodeSchema.Instance)
			{
				[AccChargeCodeSchema.AC_Code] = "Charge",
				[AccChargeCodeSchema.AC_ChargeGroup] = "FRT",
				[AccChargeCodeSchema.AC_GC] = Company2.PK,
			};
			LocalChargeCodeCompany2.Save();

			GlobalRate = new ActiveRowWrapper(RatingHeaderSchema.Instance)
			{
				[RatingHeaderSchema.TH_RateType] = "COS",
			};
			GlobalRate.Save();
			EntryOnGlobalRate = new ActiveRowWrapper(RateEntrySchema.Instance)
			{
				[RateEntrySchema.TI_TH] = GlobalRate.PK,
				[RateEntrySchema.TI_GC_Publisher] = Company1.PK,
				[RateEntrySchema.TI_RateStartDate] = new DateTime(2012, 01, 10),
				[RateEntrySchema.TI_RateCategory] = "AIR",
				[RateEntrySchema.TI_Mode] = "LSE",
				[RateEntrySchema.TI_OriginLRC] = "NZ",
			};
			EntryOnGlobalRate.Save();

			LocalRateCompany1 = new ActiveRowWrapper(RatingHeaderSchema.Instance)
			{
				[RatingHeaderSchema.TH_RateType] = "COS",
				[RatingHeaderSchema.TH_GC] = Company1.PK,
			};
			LocalRateCompany1.Save();
			EntryOnLocalRateCompany1 = new ActiveRowWrapper(RateEntrySchema.Instance)
			{
				[RateEntrySchema.TI_TH] = LocalRateCompany1.PK,
				[RateEntrySchema.TI_GC_Publisher] = Company1.PK,
				[RateEntrySchema.TI_RateStartDate] = new DateTime(2012, 01, 10),
				[RateEntrySchema.TI_RateCategory] = "AIR",
				[RateEntrySchema.TI_Mode] = "LSE",
				[RateEntrySchema.TI_OriginLRC] = "US",
			};
			EntryOnLocalRateCompany1.Save();

			LocalRateCompany2 = new ActiveRowWrapper(RatingHeaderSchema.Instance)
			{
				[RatingHeaderSchema.TH_RateType] = "COS",
				[RatingHeaderSchema.TH_GC] = Company2.PK,
			};
			LocalRateCompany2.Save();
			EntryOnLocalRateCompany2 = new ActiveRowWrapper(RateEntrySchema.Instance)
			{
				[RateEntrySchema.TI_TH] = LocalRateCompany2.PK,
				[RateEntrySchema.TI_GC_Publisher] = Company2.PK,
				[RateEntrySchema.TI_RateStartDate] = new DateTime(2012, 01, 10),
				[RateEntrySchema.TI_RateCategory] = "AIR",
				[RateEntrySchema.TI_Mode] = "LSE",
				[RateEntrySchema.TI_OriginLRC] = "AU",
			};
			EntryOnLocalRateCompany2.Save();
		}

		public void TestGlobalChargeCodeOnGlobalRateAndLocalRate()
		{
			var linePK1 = Guid.NewGuid();
			var linePK2 = Guid.NewGuid();
			var linePK3 = Guid.NewGuid();

			var sql = $@"
insert into {RateLinesSchema.Constants.SqlSchemaName}.{RateLinesSchema.Constants.TableName} (TL_PK, TL_AC, TL_TI, TL_RateCalculator, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
values
	('{linePK1}', '{GlobalChargeCode.PK}', '{EntryOnGlobalRate.PK}', 'FLT', 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown("Global cc on global rate", () => command.ExecuteNonQuery());
			}

			sql = $@"
insert into {RateLinesSchema.Constants.SqlSchemaName}.{RateLinesSchema.Constants.TableName} (TL_PK, TL_AC, TL_TI, TL_RateCalculator, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
values
	('{linePK2}', '{GlobalChargeCode.PK}', '{EntryOnLocalRateCompany1.PK}', 'FLT', 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				AssertExceptionThrown("Global cc on local rates", typeof(SqlException), "Wrong charge code used in rate", () => command.ExecuteNonQuery());
			}
		}

		public void TestLocalChargeCodeOnLocalRate_UpdateChargeCodeOtherCompany()
		{
			var rateLineWithLocalChargeCode = new ActiveRowWrapper(RateLinesSchema.Instance)
			{
				[RateLinesSchema.TL_AC] = LocalChargeCodeCompany1.PK,
				[RateLinesSchema.TL_TI] = EntryOnLocalRateCompany1.PK,
				[RateLinesSchema.TL_RateCalculator] = "FLT",
				[RateLinesSchema.TL_RX_NKCurrency] = "USD",
			};
			AssertNoExceptionThrown(() => rateLineWithLocalChargeCode.Save());

			rateLineWithLocalChargeCode[RateLinesSchema.TL_AC] = LocalChargeCodeCompany2.PK;
			AssertExceptionThrown(typeof(SqlException), "Wrong charge code used in rate", () => rateLineWithLocalChargeCode.Save());
		}

		public void TestLocalChargeCodeOnLocalRate_UpdateToGlobalRate()
		{
			var rateLineWithLocalChargeCode = new ActiveRowWrapper(RateLinesSchema.Instance)
			{
				[RateLinesSchema.TL_AC] = LocalChargeCodeCompany1.PK,
				[RateLinesSchema.TL_TI] = EntryOnLocalRateCompany1.PK,
				[RateLinesSchema.TL_RateCalculator] = "FLT",
				[RateLinesSchema.TL_RX_NKCurrency] = "USD",
			};
			AssertNoExceptionThrown(() => rateLineWithLocalChargeCode.Save());

			rateLineWithLocalChargeCode[RateLinesSchema.TL_TI] = EntryOnGlobalRate.PK;
			AssertExceptionThrown(typeof(SqlException), "Wrong charge code used in rate", () => rateLineWithLocalChargeCode.Save());
		}

		public void TestLocalChargeCodeOnGlobalRate()
		{
			var rateLineWithLocalChargeCode = new ActiveRowWrapper(RateLinesSchema.Instance)
			{
				[RateLinesSchema.TL_AC] = LocalChargeCodeCompany2.PK,
				[RateLinesSchema.TL_TI] = EntryOnGlobalRate.PK,
				[RateLinesSchema.TL_RateCalculator] = "FLT",
				[RateLinesSchema.TL_RX_NKCurrency] = "USD",
			};

			AssertExceptionThrown(typeof(SqlException), "Wrong charge code used in rate", () => rateLineWithLocalChargeCode.Save());
		}

		ActiveRowWrapper Company1;
		ActiveRowWrapper Company2;
		ActiveRowWrapper GlobalChargeCode;
		ActiveRowWrapper LocalChargeCodeCompany1;
		ActiveRowWrapper LocalChargeCodeCompany2;
		ActiveRowWrapper GlobalRate;
		ActiveRowWrapper EntryOnGlobalRate;
		ActiveRowWrapper LocalRateCompany1;
		ActiveRowWrapper EntryOnLocalRateCompany1;
		ActiveRowWrapper LocalRateCompany2;
		ActiveRowWrapper EntryOnLocalRateCompany2;
	}
}
