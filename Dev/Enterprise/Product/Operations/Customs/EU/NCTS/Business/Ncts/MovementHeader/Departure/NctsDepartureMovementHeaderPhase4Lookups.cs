using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureMovementHeaderPhase4Lookups : NctsCommonMovementHeaderLookups, INctsDepartureMovementHeaderLookups
	{
		public NctsDepartureMovementHeaderPhase4Lookups(NctsDepartureMovementHeader parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList NctsMessageStatusList => Parent.Header.Lookups.NctsMessageStatusList;

		public BondedWarehouseCollection BondedWarehouseCollection => new BondedWarehouseCollection(Parent.Factory);

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		public virtual CodeDescriptionPairList SpecificCircumstanceIndicatorList => Factory.GetCachedValue<SpecificCircumstanceIndicator>();

		public CodeDescriptionPairList NctsSpecificCircumstanceIndicatorList => new CodeDescriptionPairList();

		public CodeDescriptionPairList TransportChargesModeOfPaymentList => Factory.GetCachedValue<TransportChargesModeOfPayment>();

		public virtual CodeDescriptionPairList NctsControlResultList => Factory.GetCachedValue<NctsControlResult>();

		public CodeDescriptionPairList SealTypeList => Factory.GetCachedValue<SealTypeList>();

		public CodeDescriptionPairList ModeOfTransportList => Factory.GetCachedValue<ModeOfTransportList>();

		public CodeDescriptionPairList BorderModeOfTransportList => Factory.GetCachedValue<ModeOfTransportList>();

		public CodeDescriptionPairList TransportAtBorderTypeOfIdList => new CodeDescriptionPairList();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public OrganisationsFindBoxCollection Representatives
		{
			get
			{
				const string categoryFilterCode = "Category";
				var representatives = new OrganisationsFindBoxCollection(Factory);
				representatives.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(categoryFilterCode, "Property", (ZString)OrgConstants.Category.NaturalPersonIndividual, true));
				return representatives;
			}
		}

		public virtual CodeDescriptionPairList LocationOfGoodsCodeList => new CodeDescriptionPairList();

		public CodeDescriptionPairList TypeOfSecurityList => new CodeDescriptionPairList();

		public CodeDescriptionPairList TransportAtDepartureTypeOfIdList => new CodeDescriptionPairList();

		public RefVesselCollection Vessels => new RefVesselCollection(Factory);

		public CodeDescriptionPairList OfficeCodeList => new CodeDescriptionPairList();

		public CodeDescriptionPairList AdditionalDeclarationTypeList => new CodeDescriptionPairList();

		public ICollection TOLCarrierIDList => new CodeDescriptionPairList();

		public ICollection TOLCarrierNationalityList => TOLCarrierNationalities;

		public ICollection ForeignDestPortCodes => new CodeDescriptionPairList();

		public ICollection PortOfPresentationCodes => new CodeDescriptionPairList();
	}
}
