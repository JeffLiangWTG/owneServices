using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating.Testing
{
	[TestedType(typeof(TG_CheckCorrectCurrencyOnRateLine))]
	class TG_CheckCorrectCurrencyOnRateLineTest : DBCreateTriggerScriptTest
	{
		public void TestInsetRateLine_LineHasInvalidCurrency_ThrowException()
		{
			var publisher = new ActiveRowWrapper(GlbCompanySchema.Instance)
			{
				[GlbCompanySchema.GC_Code] = "CO1",
				[GlbCompanySchema.GC_Name] = "Company 1",
				[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
				[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
			};
			publisher.Save();

			var chargeCode1 = new ActiveRowWrapper(AccChargeCodeSchema.Instance)
			{
				[AccChargeCodeSchema.AC_Code] = "Charge1",
				[AccChargeCodeSchema.AC_ChargeGroup] = "FRT",
			};
			chargeCode1.Save();

			var chargeCode2 = new ActiveRowWrapper(AccChargeCodeSchema.Instance)
			{
				[AccChargeCodeSchema.AC_Code] = "Charge2",
				[AccChargeCodeSchema.AC_ChargeGroup] = "FRT",
			};
			chargeCode2.Save();

			var rate = new ActiveRowWrapper(RatingHeaderSchema.Instance)
			{
				[RatingHeaderSchema.TH_RateType] = "COS",
			};
			rate.Save();

			var entry = new ActiveRowWrapper(RateEntrySchema.Instance)
			{
				[RateEntrySchema.TI_TH] = rate.PK,
				[RateEntrySchema.TI_GC_Publisher] = publisher.PK,
				[RateEntrySchema.TI_RateStartDate] = new DateTime(2019, 01, 10),
				[RateEntrySchema.TI_RateCategory] = "AIR",
				[RateEntrySchema.TI_Mode] = "LSE",
				[RateEntrySchema.TI_OriginLRC] = "UA",
				[RateEntrySchema.TI_DestinationLRC] = "AU",
			};
			entry.Save();

			var sql = $@"
INSERT INTO dbo.RateLines (TL_PK, TL_AC, TL_TI, TL_RateCalculator, TL_RX_NKCurrency, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
VALUES
	('{Guid.NewGuid()}', '{chargeCode1.PK}', '{entry.PK}', 'FLT', 'UAH', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', '{chargeCode2.PK}', '{entry.PK}', 'FLT', 'BTC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				var expectedMessage = @"Attempt to insert Rate Lines with invalid currency:
COS||LSE|AIR|UA->AU|Charge2|FLT|BTC";

				AssertExceptionThrown(typeof(SqlException), expectedMessage, () => command.ExecuteNonQuery());
			}
		}
	}
}

