using System.Collections;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDepartureMovementHeaderLookups
	{
		CodeDescriptionPairList NctsMovementHeaderTransactionStatusList { get; }
		CodeDescriptionPairList BorderModeOfTransportList { get; }
		CodeDescriptionPairList TransportAtBorderTypeOfIdList { get; }
		CodeDescriptionPairList CountryOfDestinationList { get; }
		GlbStaffCollection CusAgents { get; }
		CodeDescriptionPairList DeclarationTypeList { get; }
		CodeDescriptionPairList AdditionalDeclarationTypeList { get; }
		RefUNLOCOCollection ForeignDestPorts { get; }
		CodeDescriptionPairList LocationOfGoodsCodeList { get; }
		CodeDescriptionPairList ModeOfTransportList { get; }
		CodeDescriptionPairList NctsControlResultList { get; }
		CodeDescriptionPairList NctsTransitStatusList { get; }
		CodeDescriptionPairList NctsMessageStatusList { get; }
		CodeDescriptionPairList OfficeCodeList { get; }
		RefUNLOCOCollection PortsOfUnloading { get; }
		OrganisationsFindBoxCollection Representatives { get; }
		CodeDescriptionPairList SealTypeList { get; }
		CodeDescriptionPairList SpecificCircumstanceIndicatorList { get; }
		CodeDescriptionPairList NctsSpecificCircumstanceIndicatorList { get; }
		RefCountryCollection TOLCarrierNationalities { get; }
		RefCountryCollection TransportAtDepartureCountries { get; }
		RefCountryCollection TransportAtDepartureTrailer1Nationalities { get; }
		RefCountryCollection TransportAtDepartureTrailer2Nationalities { get; }
		CodeDescriptionPairList TransportAtDepartureTypeOfIdList { get; }
		CodeDescriptionPairList TransportChargesModeOfPaymentList { get; }
		ZZRefCusCodeListCombinedCollection TransportNationalityList { get; }
		CodeDescriptionPairList TypeOfSecurityList { get; }
		RefVesselCollection Vessels { get; }
		CodeDescriptionPairList CountryOfDispatchList { get; }
		OrgHeaderCollection Organisations { get; }
		ICollection TOLCarrierIDList { get; }
		ICollection TOLCarrierNationalityList { get; }
		ICollection ForeignDestPortCodes { get; }
		ICollection PortOfPresentationCodes { get; }
		BondedWarehouseCollection BondedWarehouseCollection { get; }
		CodeDescriptionPairList WeightUnitList { get; }
	}
}
