using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using static Enterprise.ComplianceRisk.Business.BorderWiseApiHelper;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class BorderWiseApiHelperTest : TestCaseWithFactory
	{
		public void TestComplianceCheckAsync_Successfully()
		{
			var port = HttpServiceForTest.GetFreeTcpPort();
			var url = string.Format("http://localhost:{0}", port);

			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (RawDataRegistry.Instance.BorderWiseWebAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, url))
			{
				string requestStr = null;
				var dummyBorderWiseAPIService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "POST" },
					Processor = (_, request) =>
					{
						requestStr = request;
						return new Tuple<int, string>(200, ComplianceCheckResponse);
					},
					Uri = new Uri(url),
					ContentType = "application/json"
				};

				try
				{
					dummyBorderWiseAPIService.Start();
					Assert("Dummy API Service Started", dummyBorderWiseAPIService.IsStarted);

					using var setReturnDummyResponseForTestToFalse = new SetReturnDummyResponseForTestToFalse();
					var borderWiseApiHelper = new BorderWiseApiHelper();
					var request = new[] { new ComplianceCheckRequestModel() { JobNumber = "S400049750" }, new ComplianceCheckRequestModel() { JobNumber = "S400049751" } };
					var response = borderWiseApiHelper.ComplianceCheckAsync(request, CancellationToken.None).Result;

					using (var sha256Hash = SHA256.Create())
					{
						var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(DataRegistry.BorderWiseAPIKey));
						var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier(string.Empty);
						var sha256Value = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(decodedString + licenseCode)));
						AssertEquals(sha256Value, dummyBorderWiseAPIService.Headers.GetValues("Authentication").Single());
						AssertEquals(GlbCompany.CurrentCompany.GetLicenceKeyIdentifier(string.Empty), dummyBorderWiseAPIService.Headers.GetValues("LicenseCode").Single());
					}

					AssertContainsExactElementsInAnyOrder(request.Select(u => u.JobNumber), JsonConvert.DeserializeObject<ICollection<ComplianceCheckRequestModel>>(requestStr).Select(u => u.JobNumber));
					AssertContainsExactElementsInAnyOrder(new[]
					{
						("S400049750", "110419", new Guid("e77743b8-9211-41f4-bb22-6377e048c908")),
						("S400049751", "110433", new Guid("f72c6235-17c9-4de2-8216-c6062d9720e3"))
					}, response.Select(u => (u.JobNumber, u.Commodities.Single().HsCode, u.RequestId)));
				}
				finally
				{
					dummyBorderWiseAPIService.Stop();
				}
			}
		}

		public void TestComplianceCheckAsync_Failed()
		{
			var port = HttpServiceForTest.GetFreeTcpPort();
			var url = string.Format("http://localhost:{0}", port);

			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (RawDataRegistry.Instance.BorderWiseWebAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, url))
			{
				var dummyBorderWiseAPIService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(500, JsonConvert.SerializeObject(new ErrorMessage { Message = "Dummy Error Occur" })),
					Uri = new Uri(url),
					ContentType = "application/json"
				};

				try
				{
					dummyBorderWiseAPIService.Start();
					Assert("Dummy API Service Started", dummyBorderWiseAPIService.IsStarted);

					using var setReturnDummyResponseForTestToFalse = new SetReturnDummyResponseForTestToFalse();
					var borderWiseApiHelper = new BorderWiseApiHelper();
					_ = borderWiseApiHelper.ComplianceCheckAsync(new[] { new ComplianceCheckRequestModel() }, CancellationToken.None).Result;
					Assert(false);
				}
				catch (AggregateException ex)
				{
					AssertEquals($"Failed to do compliance check, failed code: InternalServerError, error message: Dummy Error Occur url: {url}/api/v4/compliance/check", ex.InnerException.Message);
				}
				finally
				{
					dummyBorderWiseAPIService.Stop();
				}
			}
		}

		const string ComplianceCheckResponse = @"
[{
  ""jobNumber"": ""S400049750"",
  ""requestId"": ""e77743b8-9211-41f4-bb22-6377e048c908"",
  ""nomenclatureWideConditionsApply"": ""Yes"",
  ""commoditySpecificConditionsApply"": ""Yes"",
  ""commodities"": [
    {
      ""hsCode"": ""110419"",
      ""description"": ""Of other cereals"",
      ""nomenclatureWideConditionsApply"": ""Yes"",
      ""commoditySpecificConditionsApply"": ""Yes"",
      ""pointPairs"": [
        {
          ""originPoint"": {
            ""country"": ""AU"",
            ""unloco"": ""AU2CO"",
            ""movementType"": ""export"",
            ""movementDescription"": ""Origin"",
            ""nomenclatureWideConditions"": [
              ""DAFF [Export Controlled Goods]"",
              ""PE Regs [Prohibited Exports]"",
              ""ABF Prohibited Goods [Prohibited Exports]""
            ],
            ""commoditySpecificConditions"": [],
            ""nomenclatureWideConditionsApply"": ""Yes"",
            ""commoditySpecificConditionsApply"": ""No""
          },
          ""destinationPoint"": {
            ""country"": ""UK"",
            ""unloco"": ""GB2AB"",
            ""movementType"": ""import"",
            ""movementDescription"": ""Destination"",
            ""nomenclatureWideConditions"": [
              ""WCO Conventions""
            ],
            ""commoditySpecificConditions"": [
              ""CITES Appendices""
            ],
            ""nomenclatureWideConditionsApply"": ""Yes"",
            ""commoditySpecificConditionsApply"": ""Yes""
          },
          ""mode"": ""SEA"",
          ""estimatedTimeOfDeparture"": ""2023-01-01T00:00:00+00:00"",
          ""estimatedTimeOfArrival"": ""2023-01-10T00:00:00+00:00"",
          ""nomenclatureWideConditionsApply"": ""Yes"",
          ""commoditySpecificConditionsApply"": ""Yes""
        }
      ]
    }
  ]
},
{
  ""jobNumber"": ""S400049751"",
  ""requestId"": ""f72c6235-17c9-4de2-8216-c6062d9720e3"",
  ""nomenclatureWideConditionsApply"": ""Yes"",
  ""commoditySpecificConditionsApply"": ""Yes"",
  ""commodities"": [
    {
      ""hsCode"": ""110433"",
      ""description"": ""Of other cereals"",
      ""nomenclatureWideConditionsApply"": ""Yes"",
      ""commoditySpecificConditionsApply"": ""Yes"",
      ""pointPairs"": [
        {
          ""originPoint"": {
            ""country"": ""AU"",
            ""unloco"": ""AU2CO"",
            ""movementType"": ""export"",
            ""movementDescription"": ""Origin"",
            ""nomenclatureWideConditions"": [
              ""DAFF [Export Controlled Goods]"",
              ""PE Regs [Prohibited Exports]"",
              ""ABF Prohibited Goods [Prohibited Exports]""
            ],
            ""commoditySpecificConditions"": [],
            ""nomenclatureWideConditionsApply"": ""Yes"",
            ""commoditySpecificConditionsApply"": ""No""
          },
          ""destinationPoint"": {
            ""country"": ""UK"",
            ""unloco"": ""GB2AB"",
            ""movementType"": ""import"",
            ""movementDescription"": ""Destination"",
            ""nomenclatureWideConditions"": [
              ""WCO Conventions""
            ],
            ""commoditySpecificConditions"": [
              ""CITES Appendices""
            ],
            ""nomenclatureWideConditionsApply"": ""Yes"",
            ""commoditySpecificConditionsApply"": ""Yes""
          },
          ""mode"": ""SEA"",
          ""estimatedTimeOfDeparture"": ""2023-01-01T00:00:00+00:00"",
          ""estimatedTimeOfArrival"": ""2023-01-10T00:00:00+00:00"",
          ""nomenclatureWideConditionsApply"": ""Yes"",
          ""commoditySpecificConditionsApply"": ""Yes""
        }
      ]
    }
  ]
}]";

		public void TestSupportedCountriesCheckAsyn_Successfully()
		{
			var port = HttpServiceForTest.GetFreeTcpPort();
			var url = string.Format("http://localhost:{0}", port);

			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (RawDataRegistry.Instance.BorderWiseWebAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, url))
			{
				string requestStr = null;
				var dummyBorderWiseAPIService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "GET" },
					Processor = (_, request) =>
					{
						requestStr = request;
						return new Tuple<int, string>(200, SupportedCountriesResponse);
					},
					Uri = new Uri(url),
					ContentType = "application/json"
				};

				using var setReturnDummyResponseForTestToFalse = new SetReturnDummyResponseForTestToFalse();
				var borderWiseApiHelper = new BorderWiseApiHelper();

				try
				{
					dummyBorderWiseAPIService.Start();
					Assert("Dummy API Service Started", dummyBorderWiseAPIService.IsStarted);

					var response = borderWiseApiHelper.SupportedCountriesCheckAsync(CancellationToken.None).Result;

					AssertNotNull(response.CommodityLevel.Import);
					AssertNotNull(response.CommodityLevel.Export);
					AssertNotNull(response.CommodityLevel.OriginOfGoods);
					AssertNotNull(response.LocationLevel.Transshipment);
					AssertNotNull(response.LocationLevel.Location);
				}
				finally
				{
					dummyBorderWiseAPIService.Stop();
				}
			}
		}

		public void TestSupportedCountriesCheckAsyn_Failed()
		{
			var port = HttpServiceForTest.GetFreeTcpPort();
			var url = string.Format("http://localhost:{0}", port);

			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (RawDataRegistry.Instance.BorderWiseWebAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, url))
			{
				var dummyBorderWiseAPIService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "GET" },
					Processor = (_, request) => new Tuple<int, string>(500, JsonConvert.SerializeObject(new ErrorMessage { Message = "Dummy Error Occur" })),
					Uri = new Uri(url),
					ContentType = "application/json"
				};

				using var setReturnDummyResponseForTestToFalse = new SetReturnDummyResponseForTestToFalse();
				var borderWiseApiHelper = new BorderWiseApiHelper();

				try
				{
					dummyBorderWiseAPIService.Start();
					Assert("Dummy API Service Started", dummyBorderWiseAPIService.IsStarted);

					_ = borderWiseApiHelper.SupportedCountriesCheckAsync(CancellationToken.None).Result;
					Assert(false);
				}
				catch (AggregateException ex)
				{
					AssertEquals($"Failed to do supported countries check, failed code: InternalServerError, error message: Dummy Error Occur url: {url}/api/v3/compliance/supported-countries", ex.InnerException.Message);
				}
				finally
				{
					dummyBorderWiseAPIService.Stop();
				}
			}
		}

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
	}
}
