using System;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class SupportedCountriesHelperTest : TestCaseWithFactory
	{
		const string SupportedCountriesResponse = @"
{
    ""commodityLevel"": {
        ""import"": [
            ""AU"",
            ""CA"",
            ""EU"",
            ""NZ"",
            ""XI"",
            ""SG"",
            ""ZA"",
            ""UK"",
            ""US"",
            ""GB"",
            ""AT"",
            ""BE"",
            ""BG"",
            ""HR"",
            ""CY"",
            ""CZ"",
            ""DK"",
            ""EE"",
            ""FI"",
            ""FR"",
            ""DE"",
            ""GR"",
            ""HU"",
            ""IT"",
            ""LV"",
            ""LT"",
            ""LU"",
            ""MT"",
            ""NL"",
            ""PL"",
            ""PT"",
            ""RO"",
            ""SK"",
            ""SI"",
            ""ES"",
            ""SE"",
            ""IE""
        ],
        ""export"": [
            ""AU"",
            ""CA"",
            ""EU"",
            ""NZ"",
            ""XI"",
            ""SG"",
            ""ZA"",
            ""UK"",
            ""US"",
            ""GB"",
            ""AT"",
            ""BE"",
            ""BG"",
            ""HR"",
            ""CY"",
            ""CZ"",
            ""DK"",
            ""EE"",
            ""FI"",
            ""FR"",
            ""DE"",
            ""GR"",
            ""HU"",
            ""IT"",
            ""LV"",
            ""LT"",
            ""LU"",
            ""MT"",
            ""NL"",
            ""PL"",
            ""PT"",
            ""RO"",
            ""SK"",
            ""SI"",
            ""ES"",
            ""SE"",
            ""IE""
        ],
        ""originOfGoods"": [
            ""US""
        ]
    },
    ""locationLevel"": {
        ""transshipment"": [],
        ""location"": []
    }
}";

		class SupportedCountriesModelForTest
		{
			public SupportedCountriesCheckResponseModel SupportedCountries { get; set; }

			public DateTime LastModifyUtcTime { get; set; }

			public bool ErrorReported { get; set; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateStmDataByTimeInterval(int hourInterval)
		{
			var sql = @"-- UpdateStmDataRow
            DELETE FROM dbo.StmData
            WHERE SD_Name = @SD_Name;
            INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue)
            VALUES (NEWID(), @SD_Name, @SD_BinaryValue);";

			var temp = JsonConvert.SerializeObject(GetSupportedCountriesModelInDb(hourInterval));
			using (var connection = Db.DisposableActionForDbConnection())
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", "ComplianceSupportedCountriesDbCache", StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetSupportedCountriesModelInDb(hourInterval))), StmDataSchema.SD_BinaryValue);
				cmd.ExecuteNonQuery();
			}
		}

		SupportedCountriesModelForTest GetSupportedCountriesModelInDb(int hourInterval)
		{
			return new SupportedCountriesModelForTest
			{
				ErrorReported = false,
				SupportedCountries = new SupportedCountriesCheckResponseModel
				{
					CommodityLevel = new CommodityLevelModel
					{
						Export = new[] { "AU" },
						Import = new[] { "AU" },
						OriginOfGoods = new[] { "US" }
					}
				},
				LastModifyUtcTime = DateTime.UtcNow.AddHours(-hourInterval)
			};
		}

		SupportedCountriesCheckResponseModel SupportedCountriesCheckResponseModelFromBW => JsonConvert.DeserializeObject<SupportedCountriesCheckResponseModel>(SupportedCountriesResponse);

		public void TestSupportedCountriesCache_LessThan24Hours()
		{
			var borderWiseApiHelper = new BorderWiseApiHelper();
			using (BorderWiseApiHelper.SetSupportedCountriesResponse(SupportedCountriesCheckResponseModelFromBW))
			{
				var supportedCountriesHelper = new SupportedCountriesHelper(borderWiseApiHelper);

				UpdateStmDataByTimeInterval(1);
				var response = supportedCountriesHelper.GetSupportedCountriesCheckResponseModelAsync(CancellationToken.None, Factory).Result;
				AssertObjectsAreEqual(GetSupportedCountriesModelInDb(1).SupportedCountries, response);
			}
		}

		public void TestSupportedCountriesCache_MoreThan24Hours()
		{
			var borderWiseApiHelper = new BorderWiseApiHelper();
			using (BorderWiseApiHelper.SetSupportedCountriesResponse(SupportedCountriesCheckResponseModelFromBW))
			{
				var supportedCountriesHelper = new SupportedCountriesHelper(borderWiseApiHelper);

				UpdateStmDataByTimeInterval(25);
				var response = supportedCountriesHelper.GetSupportedCountriesCheckResponseModelAsync(CancellationToken.None, Factory).Result;
				AssertObjectsAreEqual(SupportedCountriesCheckResponseModelFromBW, response);
			}
		}

		void AssertObjectsAreEqual<T>(T expected, T actual)
		{
			var expectedJson = JsonConvert.SerializeObject(expected);
			var actualJson = JsonConvert.SerializeObject(actual);
			AssertEquals(expectedJson, actualJson);
		}
	}
}
