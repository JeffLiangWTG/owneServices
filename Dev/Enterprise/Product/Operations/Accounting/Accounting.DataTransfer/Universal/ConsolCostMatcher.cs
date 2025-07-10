using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Core;
using Enterprise.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class ConsolCostMatcher
	{
		public ConsolCostMatcher(
			BusinessObjectFactory factory,
			GlbCompany company,
			ZString dataProvider,
			IXmlImportLogger importLogger,
			Dictionary<Enum, SchemaColumn> mapping,
			Dictionary<Enum, MatchingCriteriaDelegate> filterMapping
			)
		{
			Factory = factory;
			Company = company;
			DataProvider = dataProvider;
			ImportLogger = importLogger;
			Mapping = mapping;
			FilterMapping = filterMapping;
		}

		readonly BusinessObjectFactory Factory;
		readonly GlbCompany Company;
		readonly ZString DataProvider;
		readonly IXmlImportLogger ImportLogger;
		readonly Dictionary<Enum, SchemaColumn> Mapping;
		readonly Dictionary<Enum, MatchingCriteriaDelegate> FilterMapping;

		public delegate bool MatchingCriteriaDelegate(JobConsolCost consolCost, MatchingCriteria criteria);

		public JobConsolCost[] GetMatchingConsolCosts(ApportionmentListing apportionmentListing, ConsolCostLine consolCostLine)
		{
			JobConsolCost[] result = null;
			JobConsolCost[] matchingConsolCosts = null;

			if (consolCostLine.ImportMetaData != null
				&& consolCostLine.ImportMetaData.MatchingCriteriaCollection != null
				&& consolCostLine.ImportMetaData.MatchingCriteriaCollection.Count > 0)
			{
				matchingConsolCosts = Factory.Load<JobConsolCost>(getQueryFromMatchingCriteriaCollection(apportionmentListing.ConsolPK, apportionmentListing.ConsolType, consolCostLine.ImportMetaData.MatchingCriteriaCollection)).
					Where(x => MatchCriterias(x, consolCostLine.ImportMetaData.MatchingCriteriaCollection)).ToArray();
			}

			if (matchingConsolCosts != null)
			{
				List<ZGuid> consolCostPKList = matchingConsolCosts.Select(c => c.PK).ToList();
				result = apportionmentListing.CostsCollection.Find(x => consolCostPKList.Contains(x.PK)).ToArray();
			}
			return result;
		}

		object getValue(SchemaColumn column, ZString value)
		{
			return Helpers.GetValue(column, value, Company.PK, Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching criteria fixed label.")]
		ZQuery getQueryFromMatchingCriteriaCollection(ZGuid parentID, ZString parentTableCode, List<MatchingCriteria> matchingCriteriaCollection)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobConsolCostSchema.E6_ParentID, parentID);
			query.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, parentTableCode);

			CodeMapper codeMapper = new CodeMapper(DataProvider, ImportLogger, Factory);

			foreach (MatchingCriteria matchingCriteria in matchingCriteriaCollection)
			{
				string code = null;

				switch (matchingCriteria.FieldName)
				{
					case "ChargeCode": code = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes; break;
					case "CostOSCurrency": code = Constants.OrgPatternMatchOverrideRelationships.Currency; break;
					case "Creditor": code = Constants.OrgPatternMatchOverrideRelationships.Organisation; break;
				}

				if (!string.IsNullOrEmpty(code))
				{
					matchingCriteria.Value = codeMapper.GetMappedOrInput(matchingCriteria.Value.Value, code, 0);
				}

				ConsolCostLineElementType key = GetFieldKey(matchingCriteria);
				SchemaColumn column;

				if (Mapping.TryGetValue(key, out column) && matchingCriteria.Value.HasValue)
				{
					object value = getValue(column, matchingCriteria.Value.Value);
					query.AddToFilter(column, value);
				}
				else if (!FilterMapping.ContainsKey(key))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to find SchemaColumn for MatchingCriteria: {0}", matchingCriteria.FieldName.ToString()));
				}
			}
			return query;
		}

		bool MatchCriterias(JobConsolCost consolCost, List<MatchingCriteria> matchingCriteriaCollection)
		{
			bool result = true;

			foreach (MatchingCriteria matchingCriteria in matchingCriteriaCollection)
			{
				var key = GetFieldKey(matchingCriteria);
				MatchingCriteriaDelegate criteriaDelegate;
				if (FilterMapping.TryGetValue(key, out criteriaDelegate))
				{
					result &= criteriaDelegate(consolCost, matchingCriteria);
					if (!result)
					{
						break;
					}
				}
			}

			return result;
		}

		ConsolCostLineElementType GetFieldKey(MatchingCriteria matchingCriteria)
		{
			ConsolCostLineElementType key;

			if (matchingCriteria.FieldName.HasValue)
			{
				if (!Enum.TryParse(matchingCriteria.FieldName, true, out key))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid MatchingCriteria FieldName: {0}", matchingCriteria.FieldName.Value));
				}
			}
			else
			{
				throw new InvalidOperationException("MatchingCriteria FieldName not specified.");
			}

			return key;
		}
	}
}
