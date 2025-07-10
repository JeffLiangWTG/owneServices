using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderDepartureMessageSendingObjectLookups : NctsHeaderCommonMessageSendingObjectLookups
{
	public NctsHeaderDepartureMessageSendingObjectLookups(NctsHeaderDepartureMessageSendingObject parent) : base(parent)
	{
	}

	new NctsHeaderDepartureMessageSendingObject Parent => (NctsHeaderDepartureMessageSendingObject)base.Parent;

	public new CodeDescriptionPairList MessageTypeList
	{
		get
		{
			var movementHeader = Parent.NctsHeader?.MovementHeader;

			if (movementHeader != null)
			{
				var hasMRN = !movementHeader.Header.MovementReferenceNumber.IsEmpty;
				var customsStatus = movementHeader.BM_CustomsStatus;

				return movementHeader.IsNationalTransitSwitzerland
					? GetMessageTypeListForNationalTransit(hasMRN, customsStatus)
					: GetMessageTypeListForInternationalTransit(hasMRN, customsStatus);
			}

			return new CodeDescriptionPairList();
		}
	}

	CodeDescriptionPairList GetMessageTypeListForNationalTransit(bool hasMRN, ZString customsStatus) => customsStatus.ToString() switch
	{
		"" => GetMessageTypeListForNationalTransitForStatusEmpty(hasMRN),
		NCTS5DepartureCustomsStatusList.Codes.MrnAllocated => GetMessageTypeListForNationalTransitForStatusMrnAllocated(hasMRN),
		_ => GetMessageTypeListForNationalTransitForStatusOther(hasMRN)
	};

	CodeDescriptionPairList GetMessageTypeListForInternationalTransit(bool hasMRN, ZString customsStatus) => customsStatus.ToString() switch
	{
		"" => GetMessageTypeListForInternationalTransitForStatusEmpty(hasMRN),
		NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry => GetMessageTypeListForInternationalTransitForStatusUnderEnquirey(hasMRN),
		NCTS5DepartureCustomsStatusList.Codes.MrnAllocated => GetMessageTypeListForInternationalTransitForStatusMrnAllocated(hasMRN),
		_ => GetMessageTypeListForInternationalTransitForStatusOther(hasMRN)
	};

	CodeDescriptionPairList GetMessageTypeListForNationalTransitForStatusEmpty(bool hasMRN) => Factory.GetCachedValue($"{MessageTypeListCacheKeyPrefix}|T-CH|empty|{hasMRN}", () =>
	{
		var result = new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT515, PassarMessageTypeList.Descriptions.NT515)
		};
		ExtendMessageTypeListIfMrnIsAvailable(result, hasMRN);
		return result;
	});

	CodeDescriptionPairList GetMessageTypeListForNationalTransitForStatusMrnAllocated(bool hasMRN) => Factory.GetCachedValue($"{MessageTypeListCacheKeyPrefix}|T-CH|MRN|{hasMRN}", () =>
	{
		var result = new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT513, PassarMessageTypeList.Descriptions.NT513),
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT014, PassarMessageTypeList.Descriptions.NT014),
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NC123, PassarMessageTypeList.Descriptions.NC123),
		};
		ExtendMessageTypeListIfMrnIsAvailable(result, hasMRN);
		return result;
	});

	CodeDescriptionPairList GetMessageTypeListForNationalTransitForStatusOther(bool hasMRN) => Factory.GetCachedValue($"{MessageTypeListCacheKeyPrefix}|T-CH|other|{hasMRN}", () =>
	{
		var result = new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT513, PassarMessageTypeList.Descriptions.NT513),
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT014, PassarMessageTypeList.Descriptions.NT014),
		};
		ExtendMessageTypeListIfMrnIsAvailable(result, hasMRN);
		return result;
	});

	CodeDescriptionPairList GetMessageTypeListForInternationalTransitForStatusEmpty(bool hasMRN) => Factory.GetCachedValue($"{MessageTypeListCacheKeyPrefix}|notT-CH|empty|{hasMRN}", () =>
	{
		var result = new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT015, PassarMessageTypeList.Descriptions.NT015)
		};
		ExtendMessageTypeListIfMrnIsAvailable(result, hasMRN);
		return result;
	});

	CodeDescriptionPairList GetMessageTypeListForInternationalTransitForStatusUnderEnquirey(bool hasMRN) => Factory.GetCachedValue($"{MessageTypeListCacheKeyPrefix}|notT-CH|ENQ|{hasMRN}", () =>
	{
		var result = new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT141, PassarMessageTypeList.Descriptions.NT141)
		};
		ExtendMessageTypeListIfMrnIsAvailable(result, hasMRN);
		return result;
	});

	CodeDescriptionPairList GetMessageTypeListForInternationalTransitForStatusMrnAllocated(bool hasMRN) => Factory.GetCachedValue($"{MessageTypeListCacheKeyPrefix}|notT-CH|MRN|{hasMRN}", () =>
	{
		var result = new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT013, PassarMessageTypeList.Descriptions.NT013),
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT014, PassarMessageTypeList.Descriptions.NT014),
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NC123, PassarMessageTypeList.Descriptions.NC123),
		};
		ExtendMessageTypeListIfMrnIsAvailable(result, hasMRN);
		return result;
	});

	CodeDescriptionPairList GetMessageTypeListForInternationalTransitForStatusOther(bool hasMRN) => Factory.GetCachedValue($"{MessageTypeListCacheKeyPrefix}|notT-CH|other|{hasMRN}", () =>
	{
		var result = new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT013, PassarMessageTypeList.Descriptions.NT013),
			new CodeDescriptionPair(PassarMessageTypeList.Codes.NT014, PassarMessageTypeList.Descriptions.NT014),
		};
		ExtendMessageTypeListIfMrnIsAvailable(result, hasMRN);
		return result;
	});

	void ExtendMessageTypeListIfMrnIsAvailable(CodeDescriptionPairList result, bool hasMRN)
	{
		if (hasMRN)
		{
			result.Add(new CodeDescriptionPair(PassarMessageTypeList.Codes.NC016, PassarMessageTypeList.Descriptions.NC016));
		}
	}

	public CodeDescriptionPairList ReasonCodeList
	{
		get
		{
			var codeType = ZString.Empty;
			switch (Parent.MessageType)
			{
				case PassarMessageTypeList.Codes.NT013:
				case PassarMessageTypeList.Codes.NT513:
					codeType = CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1053;
					break;
				case PassarMessageTypeList.Codes.NT014:
					codeType = CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1054;
					break;
				case PassarMessageTypeList.Codes.NT141:
					codeType = CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1141;
					break;
			}

			return codeType.IsEmpty ? new CodeDescriptionPairList()
						: RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, codeType, ValutationDate);
		}
	}

	public new ConsigneeCollection Consignees => new ConsigneeCollection(Factory);

	public CustomsOfficeCodeCollection ActualDestinationCustomsOfficeList => Factory.GetCachedValue($"CH.NCTS.Business.ActualDestinationCustomsOfficeList_{ValutationDate}", () =>
	{
		var dataGroupingCodes = Factory.GetEuropeanUnionAndCtCountries().Select(x => new ZString(x)).Concat(new ZString[] {
				Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes,
				Core.Constants.CountryCodes.SanMarino,
				Core.Constants.CountryCodes.Serbia_ForEUTrading,
				Core.Constants.CountryCodes.IsleOfMan,
				Core.Constants.CountryCodes.Turkey,
				Core.Constants.CountryCodes.Serbia,
				Core.Constants.CountryCodes.Norway,
				Core.Constants.CountryCodes.Macedonia,
				Core.Constants.CountryCodes.Liechtenstein,
				Core.Constants.CountryCodes.Iceland
			}).ToArray();

		var customsOffices = EUCustomsOfficeCodeCollection.CustomsOfficesWithRequiredRoles(Factory, dataGroupingCodes, Parent.NctsHeader.GetRolesForDestinationOfficeLookup());
		customsOffices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice), false));
		customsOffices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", Parent.NctsHeader.DefaultDataGroupingCode));
		customsOffices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ValutationDate));
		return customsOffices;
	});

	ZDateTime ValutationDate => Parent.NctsHeader.MovementHeader?.ValuationDate.Date ?? ZDateTime.Today;

	const string MessageTypeListCacheKeyPrefix = "CH.NCTS.Business.NctsHeaderDepartureMessageSendingObjectLookups.MessageTypeList";
}
