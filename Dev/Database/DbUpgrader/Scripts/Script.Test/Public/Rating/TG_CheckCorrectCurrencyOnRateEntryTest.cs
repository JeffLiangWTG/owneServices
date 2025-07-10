using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating.Testing
{
	[TestedType(typeof(TG_CheckCorrectCurrencyOnRateEntry))]
	class TG_CheckCorrectCurrencyOnRateEntryTest : DBCreateTriggerScriptTest
	{
		public void TestInsetRateEntry_EntryHasInvalidCurrency_ThrowException()
		{
			var publisher = new ActiveRowWrapper(GlbCompanySchema.Instance)
			{
				[GlbCompanySchema.GC_Code] = "CO1",
				[GlbCompanySchema.GC_Name] = "Company 1",
				[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
				[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
			};

			var rate = new ActiveRowWrapper(RatingHeaderSchema.Instance)
			{
				[RatingHeaderSchema.TH_RateType] = "COS",
			};

			publisher.Save();
			rate.Save();

			var sql = $@"
INSERT INTO dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_DestinationLRC, TI_RX_NKCurrency, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('{Guid.NewGuid()}', '{rate.PK}', '{publisher.PK}', '2019-01-10', 'AIR', 'LSE', 'UA', 'AU', 'USD', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{Guid.NewGuid()}', '{rate.PK}', '{publisher.PK}', '2019-01-10', 'AIR', 'LSE', 'AU', 'UA', 'BTC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				var expectedMessage = @"Attempt to insert Rate Entries with invalid currency:
COS||LSE|AIR|AU->UA|BTC";

				AssertExceptionThrown(typeof(SqlException), expectedMessage, () => command.ExecuteNonQuery());
			}
		}
	}
}

