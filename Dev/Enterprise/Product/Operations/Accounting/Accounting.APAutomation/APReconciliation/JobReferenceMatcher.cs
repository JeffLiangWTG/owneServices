using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using GlowIndexQueryService.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class JobReferenceMatcher : IJobReferenceMatcher
	{
		public (ZGuid jobPK, ZString parentTableCode) FindBestMatching(string referenceNumber, string[] searchFields = null)
		{
			var result = (ZGuid.Invalid, ZString.Empty);
			if (searchFields == null || searchFields.Length == 0)
			{
				searchFields = SearchFieldsForJobMatching;
			}
			else if (searchFields.Any(x => !SearchFieldsForJobMatching.Contains(x)))
			{
				throw new ArgumentException(string.Format("Unknown search fields: '{0}'", string.Join(",", searchFields.Where(x => !SearchFieldsForJobMatching.Contains(x)))));
			}

			var searchResults = GlowIndexQueryEngine.Query(new GlowIndexQueryParam(referenceNumber, searchFields, APACategoryCode));

			if (searchResults != null && searchResults.Status == GlowIndexQueryStatus.Success)
			{
				var bestMatch = searchResults.Results.OrderBy(x => FindPriorityLevelBasedOnJobType(x)).ThenByDescending(x => FindCreateTime(x)).FirstOrDefault();
				if (bestMatch != null)
				{
					var pk = ZGuid.ParseSafe(bestMatch.PK);
					if (EntityTypeToParentTableCodeMap.TryGetValue(bestMatch.EntityType, out var parentTableCode))
					{
						result = (pk, parentTableCode);
					}
					else
					{
						throw new ApplicationException(string.Format("Entity type '{0}' is not configured for job matching.", bestMatch.EntityType));
					}
				}
			}

			return result;
		}

		int FindPriorityLevelBasedOnJobType(GlowIndexQueryResult queryResult)
		{
			if (queryResult.EntityType == HighestPriorityType)
			{
				return 1;
			}
			else if (queryResult.EntityType == SecondPriorityType)
			{
				return 2;
			}
			else if (ThirdPriorityTypes.Contains(queryResult.EntityType))
			{
				return 3;
			}
			else
			{
				return 4;
			}
		}

		ZDateTime FindCreateTime(GlowIndexQueryResult queryResult)
		{
			var createTime = ZDateTime.Empty;
			var keyFields = queryResult.KeyFields;
			var createTimeItem = keyFields.FirstOrDefault(item => item.Key == "CREATETIME");
			if (createTimeItem != default)
			{
				createTime = ZDateTime.ParseISO8601DateSafe(createTimeItem.Value);
			}
			return createTime;
		}

		readonly string APACategoryCode = "APA";
		readonly string[] SearchFieldsForJobMatching = { SearchFieldsForCategoryAPA.JobNumber, SearchFieldsForCategoryAPA.HouseBillNumber, SearchFieldsForCategoryAPA.MasterBillNumber, SearchFieldsForCategoryAPA.ContainerNumber };

		public static class SearchFieldsForCategoryAPA
		{
			public const string JobNumber = "JOBNUMBER";
			public const string MasterBillNumber = "MASTERBILLNUMBER";
			public const string HouseBillNumber = "HOUSEBILLNUMBER";
			public const string ContainerNumber = "CONTAINERNUMBER";
		}

		readonly string HighestPriorityType = "IJobConsol";
		readonly string SecondPriorityType = "IJobShipment";
		readonly string[] ThirdPriorityTypes = { "IDtbConsignmentRunSheet", "IJobCartageRunSheet", "IDtbBookingConsolidation", "IWhsItemDispatchTransportationUnit", "IWhsItemReceiveTransportationUnit" };

		readonly internal Dictionary<string, string> EntityTypeToParentTableCodeMap = new Dictionary<string, string>
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

		public void ResetQueryEngine()
		{
			glowIndexQueryEngine = null;
		}

		public static IGlowIndexQueryEngine GlowIndexQueryEngine
		{
			get { return glowIndexQueryEngine ??= ObjectFactory.Get<IGlowIndexQueryEngine>(); }
		}
		[ThreadSafe]
		static IGlowIndexQueryEngine glowIndexQueryEngine;
	}
}
