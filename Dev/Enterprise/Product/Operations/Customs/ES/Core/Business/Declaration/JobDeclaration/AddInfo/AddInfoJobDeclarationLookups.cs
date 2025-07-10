using CargoWise.Integration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class JobDeclarationLookups
	{
		protected override CodeDescriptionPairList GetExportCommunityTransitStatusIDList() => Factory.GetCachedValue<ExportCommunityTransitStatusList>();

		public override CodeDescriptionPairList SpecificCircumstanceIndicatorList
		{
			get
			{
				return Factory.GetCachedValue("AddInfoJobDeclarationLookups.SpecificCircumstanceIndicatorList_" + Declaration.IsUCC6AndIsExport, () =>
				{
					if (Declaration.IsUCC6AndIsExport)
					{
						var result = new SpecificCircumstanceIndicatorForUCCList();
						result.AddPair(SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments, SpecificCircumstanceIndicator.Descriptions.PostalAndExpressConsignments);
						result.AddPair(SpecificCircumstanceIndicator.Codes.ShipAndAircraftSupplies, SpecificCircumstanceIndicator.Descriptions.ShipAndAircraftSupplies);
						result.AddPair(SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators, SpecificCircumstanceIndicator.Descriptions.AuthorizedEconomicOperators);
						return result;
					}
					else
					{
						var result = new CodeDescriptionPairList();

						var baseList = base.SpecificCircumstanceIndicatorList;
						foreach (var code in baseList.GetAllCodesZString())
						{
							if (code != SpecificCircumstanceIndicator.Codes.RoadModeOfTransport &&
								code != SpecificCircumstanceIndicator.Codes.RailModeOfTransport)
							{
								result.AddPair(code, baseList.GetDescriptionFromCode(code));
							}
						}
						result.Sort();
						return result;
					}
				});
			}
		}

		public CodeDescriptionPairList DestinationStateIslandCodeList => CustomsFiscalTerritoriesList.GetSpainFullList(Parent.Factory);

		public override CodeDescriptionPairList BorderTransportMeansList
		{
			get
			{
				var transportMode = Parent.JE_TransportMode;
				var cacheKey = "BorderTransportMeansList_" + transportMode;
				var cacheExportBorderTransportMeansList = Factory.GetCachedValue<ExportBorderTransportMeansList>();
				return CreateNewListDependingOnTransportMode(cacheExportBorderTransportMeansList, transportMode, cacheKey);
			}
		}

		CodeDescriptionPairList CreateNewListDependingOnTransportMode(CodeDescriptionPairList codePairList, string transportMode, string cacheKey)
		{
			if (!IsUCC6)
			{
				return codePairList;
			}
			else
			{
				switch (transportMode)
				{
					case Customs.Business.TransportTypeList.Codes.Sea:
						return NewListOfRequiredItems(cacheKey, codePairList, Core.Constants.TransportCodes.Sea);
					case Customs.Business.TransportTypeList.Codes.Rail:
						return NewListOfRequiredItems(cacheKey, codePairList, Core.Constants.TransportCodes.Rail);
					case Customs.Business.TransportTypeList.Codes.Road:
						return NewListOfRequiredItems(cacheKey, codePairList, Core.Constants.TransportCodes.Road);
					case Customs.Business.TransportTypeList.Codes.Air:
						return NewListOfRequiredItems(cacheKey, codePairList, Core.Constants.TransportCodes.Air);
					case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
						return NewListOfRequiredItems(cacheKey, codePairList, ESTransportCodeInlandWaterwayTransport);
					default:
						return codePairList;
				}
			}
		}

		CodeDescriptionPairList NewListOfRequiredItems(string cacheKey, CodeDescriptionPairList transportMeansCodeList, string requiredCodesPrefix)
		{
			var newList = new CodeDescriptionPairList();
			foreach (ICodeDescription element in transportMeansCodeList)
			{
				if (element.Code.StartsWith(requiredCodesPrefix))
				{
					newList.Add(element);
				}
			}

			return Factory.GetCachedValue("ES.JobDeclarationLookups." + cacheKey, () =>
			{
				newList.Sort();
				return newList;
			});
		}

		bool IsUCC6 => Declaration.IsUCC6;

		const string ESTransportCodeInlandWaterwayTransport = "8";
	}
}
