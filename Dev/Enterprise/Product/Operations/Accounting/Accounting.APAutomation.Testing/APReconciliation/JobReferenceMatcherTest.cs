using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using GlowIndexQueryService.Business;
using GlowIndexQueryService.Tests.Common;
using static Enterprise.Accounting.APAutomation.APReconciliation.JobReferenceMatcher;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class JobReferenceMatcherTest : TestCaseWithFactory
	{
		public void TestFindBestMatchRequiresValidSearchFields()
		{
			var shipmentPK = ZGuid.NewZGuid();
			glowSearchEngine.Results = new GlowIndexQueryResultCollection
			{
				Status = GlowIndexQueryStatus.Success,
				Results = new List<GlowIndexQueryResult>
				{
					new GlowIndexQueryResult(shipmentPK.ToString(), "IJobShipment")
				},
			};

			using (ObjectFactory.Substitute<IGlowIndexQueryEngine>(glowSearchEngine))
			{
				var (jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(shipmentPK, jobPK);
				AssertEquals("JS", parentTableCode);

				jobReferenceMatcher.FindBestMatching("s000100", null);
				AssertEquals(shipmentPK, jobPK);
				AssertEquals("JS", parentTableCode);

				jobReferenceMatcher.FindBestMatching("s000100", Array.Empty<string>());
				AssertEquals(shipmentPK, jobPK);
				AssertEquals("JS", parentTableCode);

				jobReferenceMatcher.FindBestMatching("s000100", new string[] { SearchFieldsForCategoryAPA.JobNumber, SearchFieldsForCategoryAPA.HouseBillNumber });
				AssertEquals(shipmentPK, jobPK);
				AssertEquals("JS", parentTableCode);

				AssertExceptionThrown<ArgumentException>("Unknown search fields: 'Bad Field 1,Bad Field 2'", () => jobReferenceMatcher.FindBestMatching("s000100", new string[2] { "Bad Field 1", "Bad Field 2" }));
			}
		}

		public void TestFindBestMatchWhenThereAreNoJobInResults()
		{
			var shipmentPK = ZGuid.NewZGuid();
			glowSearchEngine.Results = new GlowIndexQueryResultCollection
			{
				Status = GlowIndexQueryStatus.Success,
				Results = new List<GlowIndexQueryResult>
				{ },
			};

			using (ObjectFactory.Substitute<IGlowIndexQueryEngine>(glowSearchEngine))
			{
				var (jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(ZGuid.Invalid, jobPK);
				AssertEquals("", parentTableCode);
			}
		}

		public void TestFindBestMatchWhenThereIsOnlyOneJobInResults()
		{
			var shipmentPK = ZGuid.NewZGuid();
			glowSearchEngine.Results = new GlowIndexQueryResultCollection
			{
				Status = GlowIndexQueryStatus.Success,
				Results = new List<GlowIndexQueryResult>
				{
					new GlowIndexQueryResult(shipmentPK.ToString(), "IJobShipment")
				},
			};

			using (ObjectFactory.Substitute<IGlowIndexQueryEngine>(glowSearchEngine))
			{
				var (jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(shipmentPK, jobPK);
				AssertEquals("JS", parentTableCode);
			}
		}

		public void TestFindBestMatchChooseHighestPriorityJobType()
		{
			var shipmentPK = ZGuid.NewZGuid();
			var runSheetPK = ZGuid.NewZGuid();
			var consolPK = ZGuid.NewZGuid();
			var receiveAdvicePK = ZGuid.NewZGuid();
			var runSheet = new GlowIndexQueryResult(runSheetPK.ToString(), "IDtbConsignmentRunSheet");
			var shipment = new GlowIndexQueryResult(shipmentPK.ToString(), "IJobShipment");
			var consol = new GlowIndexQueryResult(consolPK.ToString(), "IJobConsol");
			var receiveAdvice = new GlowIndexQueryResult(receiveAdvicePK.ToString(), "ICYDReceiveAdvice");

			glowSearchEngine.Results = new GlowIndexQueryResultCollection
			{
				Status = GlowIndexQueryStatus.Success,
				Results = new List<GlowIndexQueryResult>
				{
					runSheet,
					shipment,
					consol,
					receiveAdvice
				},
			};

			using (ObjectFactory.Substitute<IGlowIndexQueryEngine>(glowSearchEngine))
			{
				var (jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(consolPK, jobPK);
				AssertEquals("JK", parentTableCode);

				glowSearchEngine.Results.Results.Remove(consol);
				(jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(shipmentPK, jobPK);
				AssertEquals("JS", parentTableCode);

				glowSearchEngine.Results.Results.Remove(shipment);
				(jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(runSheetPK, jobPK);
				AssertEquals("KG", parentTableCode);

				glowSearchEngine.Results.Results.Remove(runSheet);
				(jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(receiveAdvicePK, jobPK);
				AssertEquals("YRA", parentTableCode);
			}
		}

		public void TestFindBestMatchChooseJobBasedOnCreateTime()
		{
			var runSheetPK = ZGuid.NewZGuid();
			var transportUnitPK = ZGuid.NewZGuid();
			var dtbBookingConsol = ZGuid.NewZGuid();
			glowSearchEngine.Results = new GlowIndexQueryResultCollection
			{
				Status = GlowIndexQueryStatus.Success,
				Results = new List<GlowIndexQueryResult>
				{
					new GlowIndexQueryResult(runSheetPK.ToString(), "IJobCartageRunSheet", new Collection<(string, string)> { ("CREATETIME", "2024-08-08T06:37:00Z") }),
					new GlowIndexQueryResult(transportUnitPK.ToString(), "IWhsItemDispatchTransportationUnit", new Collection<(string, string)> { ("CREATETIME", "2024-12-12T06:37:00Z") }),
					new GlowIndexQueryResult(dtbBookingConsol.ToString(), "IDtbBookingConsolidation", new Collection<(string, string)> { ("CREATETIME", "2024-10-10T06:37:00Z") }),
				},
			};

			using (ObjectFactory.Substitute<IGlowIndexQueryEngine>(glowSearchEngine))
			{
				var (jobPK, parentTableCode) = jobReferenceMatcher.FindBestMatching("s000100");
				AssertEquals(transportUnitPK, jobPK);
				AssertEquals("WDH", parentTableCode);
			}
		}

		public void TestEntityTypeToParentTableCodeMap()
		{
			var expectedEntityTypeToParentTableCodeMap = new Dictionary<string, string>
			{
				{ "ICusISFHeader", "BF" },
				{ "INCTMHeader", "BH" },
				{ "IUnderbond[[ICusMAWB]]", "C4" },
				{ "ICusMAWB", "CM" },
				{ "ICarrierShipment", "CSH" },
				{ "IJobSundryCharge", "D4" },
				{ "IJobStorage", "ET" },
				{ "IJobCartageRunSheet", "EY" },
				{ "IJobDeclaration", "JE" },
				{ "IJobCartage", "JJ" },
				{ "IJobConsol", "JK" },
				{ "IJobShipment", "JS" },
				{ "IDtbBookingConsolidation", "KB" },
				{ "IDtbConsignmentRunSheet", "KG" },
				{ "IDtbBooking", "KM" },
				{ "IDtbConsignment", "LTC" },
				{ "IJobVoyAccount", "NA" },
				{ "IJobContainerDetention", "NC" },
				{ "IWhsReceive", "WD" },
				{ "IWhsOrder", "WD" },
				{ "IWhsItemDispatchConsignment", "WDC" },
				{ "IWhsItemDispatchTransportationUnit", "WDH" },
				{ "IWorkItem", "WKI" },
				{ "IWorkProject", "WKP" },
				{ "IWorkRequest", "WKR" },
				{ "IWhsItemReceiveConsignment", "WRC" },
				{ "IWhsItemReceiveTransportationUnit", "WRH" },
				{ "IWhsStocktake", "WS" },
				{ "IWhsAdHocServiceJob", "WSJ" },
				{ "IWhsVASOrder", "WVO" },
				{ "ICYDReceiveAdvice", "YRA" },
				{ "ICYDReleaseAdvice", "YRE" },
				{ "ICYDTransportationUnit", "YTU" }
			};

			foreach (var map in expectedEntityTypeToParentTableCodeMap)
			{
				AssertEquals(expectedEntityTypeToParentTableCodeMap[map.Key], jobReferenceMatcher.EntityTypeToParentTableCodeMap[map.Key]);
			}
		}

		public void TestExceptionThrownWhenInvalidEntityTypeIsReturned()
		{
			var boPK = ZGuid.NewZGuid();
			glowSearchEngine.Results = new GlowIndexQueryResultCollection
			{
				Status = GlowIndexQueryStatus.Success,
				Results = new List<GlowIndexQueryResult>
				{
					new GlowIndexQueryResult(boPK.ToString(), "IUnknown")
				},
			};

			using (ObjectFactory.Substitute<IGlowIndexQueryEngine>(glowSearchEngine))
			{
				var exceptionThrown = AssertExceptionThrown<ApplicationException>(() => jobReferenceMatcher.FindBestMatching("ABC123"));
				AssertEquals("Entity type 'IUnknown' is not configured for job matching.", exceptionThrown.Message);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			glowSearchEngine = new MockGlowIndexQuerySearchEngine();
			jobReferenceMatcher = new JobReferenceMatcher();
		}

		protected override void TearDown()
		{
			base.TearDown();
			glowSearchEngine.Results.Results.Clear();
			glowSearchEngine.Results.Status = GlowIndexQueryStatus.Uninitialised;
			jobReferenceMatcher.ResetQueryEngine();
		}

		MockGlowIndexQuerySearchEngine glowSearchEngine;
		JobReferenceMatcher jobReferenceMatcher;
	}
}
