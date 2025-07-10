using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeListLoader = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// A temporary class for use while transitioning from RefDb_CMR_AU to CW-RefDatabase;
	/// Governed by UseRefDatabaseData registry value.
	/// </summary>
	public static class CMRReferenceDataHelper
	{
		public static bool UseReferenceData => AUCustomsDataRegistry.Instance.UseRefDatabaseData.Value;

		public static CodeDescriptionPairList SetupAQISCommodityCodeList(BusinessObjectFactory factory)
		{
			var aqisCommodityCodeList = new CodeDescriptionPairList();
			if (UseReferenceData)
			{
				var commodities = RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAC, ZDateTime.Today);
				aqisCommodityCodeList.AddRange(commodities);
			}
			else
			{
				var commodities = new CMRAqisCommodityCollection(factory);
				commodities.Load();

				foreach (CMRAqisCommodity currentCommodity in commodities)
				{
					aqisCommodityCodeList.AddPair(currentCommodity.QC_AQISCommodityCode, currentCommodity.QC_AQISCommodityDescription);
				}
			}
			return aqisCommodityCodeList;
		}

		public static CodeDescriptionPairList SetupAQISConcernCodeList(BusinessObjectFactory factory)
		{
			var aqisConcernCodeList = new CodeDescriptionPairList();
			if (UseReferenceData)
			{
				var aqisConcerns = RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRCN, ZDateTime.Today);
				aqisConcernCodeList.AddRange(aqisConcerns);
			}
			else
			{
				var aqisConcerns = new CMRAqisConcernCollection(factory);
				aqisConcerns.Load();

				foreach (CMRAqisConcern currentConcern in aqisConcerns)
				{
					aqisConcernCodeList.AddPair(currentConcern.QN_AQISConcernType, currentConcern.QN_AQISConcernDescription);
				}
			}
			return aqisConcernCodeList;
		}

		public static CodeDescriptionPairList SetupAQISDocumentTypeList(BusinessObjectFactory factory)
		{
			var aqisDocumentTypeList = new CodeDescriptionPairList();
			if (UseReferenceData)
			{
				var documentTypes = RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRDT, ZDateTime.Today);
				aqisDocumentTypeList.AddRange(documentTypes);
			}
			else
			{
				var documentTypes = new CMRAqisDocumentTypeCollection(factory);
				documentTypes.Load();

				foreach (CMRAqisDocumentType currentType in documentTypes)
				{
					aqisDocumentTypeList.AddPair(currentType.QD_AQISDocumentType, currentType.QD_AQISDocumentDescription);
				}
			}
			return aqisDocumentTypeList;
		}

		public static BusinessObjectCollection SetupAQISPremisesIdList(BusinessObjectFactory factory)
		{
			if (UseReferenceData)
			{
				var premisesIds = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAP, ZDateTime.Today);
				premisesIds.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AUAQISPremisesPortCode, false));
				premisesIds.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", ZString.Empty, true));
				premisesIds.Sort(AutoZZRefCusCodeListCombined.Schema.ZZD_Description, ListSortDirection.Ascending);
				return premisesIds;
			}
			else
			{
				return new CMRAqisPremisesCollection(factory);
			}
		}

		public static CodeDescriptionPairList SetupBerthCodeList(BusinessObjectFactory factory, ZDateTime referenceDate, ZString arrivalPort)
		{
			var codeList = new CodeDescriptionPairList();
			if (UseReferenceData)
			{
				var attributeFilters = new List<RefCusCodeListAttributeFilter>();
				if (!arrivalPort.IsEmpty)
				{
					attributeFilters.Add(new RefCusCodeListAttributeFilter(AUConstants.RefCusCodeAttributesNames.BerthPortCode, SQLComparisonOperator.Equal, arrivalPort));
				}
				var berthCodes = RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRBC, referenceDate, attributeFilters);
				codeList.AddRange(berthCodes);
			}
			else
			{
				var filter = new ZQuery();
				if (!arrivalPort.IsEmpty)
				{
					filter.AddToFilter(CMRBerthCodeSchema.BC_PortCode, SQLComparisonOperator.Equal, arrivalPort);
				}
				if (referenceDate.IsValid)
				{
					var startDateFilter = new ZQuery(CMRBerthCodeSchema.BC_BerthCodeStartDate, SQLComparisonOperator.Equal, null);
					startDateFilter.AddToFilter(JoinCondition.Or, CMRBerthCodeSchema.BC_BerthCodeStartDate, SQLComparisonOperator.LessThanOrEqualTo, referenceDate);

					var endDateFilter = new ZQuery(CMRBerthCodeSchema.BC_BerthCodeEndDate, SQLComparisonOperator.Equal, null);
					endDateFilter.AddToFilter(JoinCondition.Or, CMRBerthCodeSchema.BC_BerthCodeEndDate, SQLComparisonOperator.GreaterThanOrEqualTo, referenceDate);

					var dateFilter = new ZQuery(startDateFilter, endDateFilter);
					filter.AddToFilter(dateFilter);
				}
				var berthCodeCollection = new CMRBerthCodeCollection(factory, filter);
				berthCodeCollection.Load();
				codeList.AddRange(berthCodeCollection);
			}

			codeList.Sort();
			return codeList;
		}

		public static CodeDescriptionPairList SetupAQISEntityIdList(BusinessObjectFactory factory)
		{
			var aqisEntityIdList = new CodeDescriptionPairList();
			if (UseReferenceData)
			{
				var aqisEntityIds = RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRAE, ZDateTime.Today);
				aqisEntityIdList.AddRange(aqisEntityIds);
			}
			else
			{
				var cmrAqisEntities = new CMRAqisEntityCollection(factory);
				cmrAqisEntities.Load();

				foreach (CMRAqisEntity currentEntity in cmrAqisEntities)
				{
					aqisEntityIdList.AddPair(currentEntity.QE_AQISEntityIdentifier, currentEntity.QE_AQISEntityName);
				}
			}
			return aqisEntityIdList;
		}

		public static CodeDescriptionPairList SetupAQISProcessingTypeList(BusinessObjectFactory factory, ZString cargoType)
		{
			var aqisProcessingTypeList = new CodeDescriptionPairList();
			if (cargoType.IsEmpty)
			{
				return aqisProcessingTypeList;
			}

			if (UseReferenceData)
			{
				var attributeFilters = new List<RefCusCodeListAttributeFilter>();
				attributeFilters.Add(new RefCusCodeListAttributeFilter(AUConstants.RefCusCodeAttributesNames.AQISProcessingCargoType, SQLComparisonOperator.Equal, cargoType));

				var processingTypes = RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRPT, ZDateTime.Today, attributeFilters);
				aqisProcessingTypeList.AddRange(processingTypes);
			}
			else
			{
				var query = new ZQuery(CMRAqisProcessingTypeSchema.QT_AQISProcessingCargoType, cargoType);
				var processingTypes = new CMRAqisProcessingTypeCollection(factory);
				processingTypes.LoadWithMoreFiltering(query);

				foreach (CMRAqisProcessingType currentProcessingType in processingTypes)
				{
					aqisProcessingTypeList.AddPair(currentProcessingType.QT_AQISProcessingType, currentProcessingType.QT_AQISProcessingDescription);
				}
			}
			return aqisProcessingTypeList;
		}

		public static CodeDescriptionPairList SetupAQISTreatmentCodeList(BusinessObjectFactory factory)
		{
			var aqisTreatmentCodeList = new CodeDescriptionPairList();

			var treatmentCodes = RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.EXE30, ZDateTime.Today);
			foreach (var treatmentCode in treatmentCodes)
			{
				aqisTreatmentCodeList.InsertInSortOrder(treatmentCode);
			}

			return aqisTreatmentCodeList;
		}

		public static ICollection<ZString> LoadCMRSACThesaurus(BusinessObjectFactory factory)
		{
			return RefCusCodeListLoader.Load(factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.SACTH, ZDateTime.Today).Select(x => x.ZZD_Code).OrderBy(x => x).ToList();
		}
	}
}
