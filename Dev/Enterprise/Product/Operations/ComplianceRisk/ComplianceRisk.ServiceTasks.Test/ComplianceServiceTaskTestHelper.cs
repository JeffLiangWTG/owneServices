using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Messaging;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Enterprise.ComplianceRisk.ServiceTasks.Test
{
	public static class ComplianceServiceTaskTestHelper
	{
		public static Mock<HttpMessageHandler> MockHttpMessageHandlerWithEmptyResponse(HttpStatusCode code)
		{
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = code
				})
				.Verifiable();
			return handlerMock;
		}

		public static Mock<HttpMessageHandler> MockHttpMessageHandler(params (string hsCode, string jobNumber)[] responses)
		{
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(
						JsonConvert.SerializeObject(ComplianceServiceTaskTestHelper.MockCleanBwResponse(responses))),
				})
				.Verifiable();
			return handlerMock;
		}

		public static ComplianceCheckResponseModel[] MockCleanBwResponse(params (string hsCode, string jobNumber)[] responses)
		{
			var destinationPoint = new ComplianceCheckResponsePointPairLocationModel
			{
				NomenclatureWideConditionsApply = "No",
				CommoditySpecificConditionsApply = "No",
				Country = "US"
			};

			return responses.Select(response => new ComplianceCheckResponseModel
			{
				RequestId = Guid.NewGuid(),
				JobNumber = response.jobNumber,
				Commodities = new[]
				{
					new ComplianceCheckResponseCommodityModel
					{
						HsCode = response.hsCode,
						CommoditySpecificConditionsApply = "No",
						NomenclatureWideConditionsApply = "No",
						PointPairs = new[]
						{
							new ComplianceCheckResponsePointPairModel
							{
								OriginPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									NomenclatureWideConditionsApply = "No",
									CommoditySpecificConditionsApply = "No",
									Country = "AU",
									MatchedHsCode = response.hsCode
								},
								DestinationPoint = new ComplianceCheckResponsePointPairLocationModel
								{
									NomenclatureWideConditionsApply = "No",
									CommoditySpecificConditionsApply = "No",
									Country = "US",
									MatchedHsCode = response.hsCode
								}
							}
						}
					}
				}.ToArray(),
				Locations = new[]
				{
					new ComplianceCheckResponseLocationModel
					{
						Country = "AU",
						IsSupportedCountry = true
					},
					new ComplianceCheckResponseLocationModel
					{
						Country = "US",
						IsSupportedCountry = true
					}
				}
			}).ToArray();
		}

		public static ComplianceRiskAssessmentEdiMessage CreateNewEdiMessage(BusinessObjectFactory factory, BusinessObject bizO)
		{
			var message = factory.New<ComplianceRiskAssessmentEdiMessage>();
			message.EM_LinkTable = bizO.TableName;
			message.EM_LinkUniqueID = bizO.PK;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			return message;
		}

		public static IQuotedBooking CreateNewQuotedBooking(BusinessObjectFactory factory, QuoteBookingType bookingType)
		{
			return (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember("New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null, null, new object[] { bookingType, factory });
		}

		public static JobVoyage CreateNewSailingVoyage(BusinessObjectFactory factory, string vessel)
		{
			var r1 = factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;
			var r2 = factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			var r3 = factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;

			var voyage = factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "12";
			voyage.JV_RV_NKVessel = vessel;

			var o1 = factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			var o2 = factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			var o3 = factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			o1.JA_RL_NKPortOfLoading = r1.RL_Code;
			o1.JA_E_DEP = ZDateTime.Today.AddDays(5);
			voyage.Origins.Add(o1);

			o2.JA_RL_NKPortOfLoading = r2.RL_Code;
			o2.JA_E_DEP = ZDateTime.Today.AddDays(8);
			voyage.Origins.Add(o2);

			o3.JA_RL_NKPortOfLoading = r3.RL_Code;
			o3.JA_E_DEP = ZDateTime.Today.AddDays(35);
			voyage.Origins.Add(o3);

			var d1 = factory.New(typeof(VoyageDestination)) as VoyageDestination;
			var d2 = factory.New(typeof(VoyageDestination)) as VoyageDestination;
			var d3 = factory.New(typeof(VoyageDestination)) as VoyageDestination;
			d1.JB_RL_NKPortOfDischarge = r1.RL_Code;
			d1.JB_E_ARV = ZDateTime.Today.AddDays(7);

			voyage.Destinations.Add(d1);
			d2.JB_RL_NKPortOfDischarge = r2.RL_Code;
			d2.JB_E_ARV = ZDateTime.Today.AddDays(37);

			voyage.Destinations.Add(d2);
			d3.JB_RL_NKPortOfDischarge = r3.RL_Code;
			d3.JB_E_ARV = ZDateTime.Today.AddDays(55);
			voyage.Destinations.Add(d3);

			voyage.GenerateSailings();
			return voyage;
		}

		public static JobSailing GetSailingFromPortPair(JobVoyage voyageToSearch, ZString load, ZString discharge)
		{
			JobSailing result = null;
			foreach (JobSailing sailing in voyageToSearch.Sailings)
			{
				if ((sailing.Origin.JA_RL_NKPortOfLoading == load)
					&& (sailing.Destination.JB_RL_NKPortOfDischarge == discharge))
				{
					result = sailing;
					result.JX_IsPublished = true;
					break;
				}
			}
			return result;
		}

		public static ComplianceRiskStatus CreateNewComplianceRiskStatus(BusinessObjectFactory factory, BusinessObject bizO)
		{
			var complianceRiskStatus = factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = bizO.PK;
			complianceRiskStatus.COR_ParentTableCode = bizO.TablePrefix;
			return complianceRiskStatus;
		}
	}
}
