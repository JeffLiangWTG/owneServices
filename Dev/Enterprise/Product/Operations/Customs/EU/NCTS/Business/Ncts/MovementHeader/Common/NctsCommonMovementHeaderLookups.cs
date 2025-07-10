using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsCommonMovementHeaderLookups : CusInBondMoveHeaderLookups
	{
		public NctsCommonMovementHeaderLookups(NctsCommonMovementHeader parent) : base(parent)
		{
		}

		protected new NctsCommonMovementHeader Parent => (NctsCommonMovementHeader)base.Parent;

		public virtual CodeDescriptionPairList DeclarationTypeList => RefCusCodeListTypes.GetCachedList(Factory, Parent.DefaultDataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, ZDateTime.Today);

		public CodeDescriptionPairList NctsMoveHeaderTypeList => Factory.GetCachedValue<Common.EU.NctsMoveHeaderType>();

		public RefUNLOCOCollection PortsOfUnloading => new RefUNLOCOCollection(Factory);

		public CodeDescriptionPairList CountryOfDestinationList => CountryList;

		public CodeDescriptionPairList NctsTransitStatusList => NctsTransitStatusListCore;

		protected virtual CodeDescriptionPairList NctsTransitStatusListCore => Parent.Header.Lookups.NctsTransitStatusList;

		public CodeDescriptionPairList DischargeTypeList => Factory.GetCachedValue<DischargeTypeList>();

		public CodeDescriptionPairList CarnetTotalPagesList => Factory.GetCachedValue<CarnetTotalPagesList>();

		public ZZRefCusCodeListCombinedCollection TransportNationalityList => Factory.GetNCNATCountryList();

		public CodeDescriptionPairList CountryOfDispatchList => CountryList;

		public CodeDescriptionPairList NctsMovementHeaderTransactionStatusList => NctsMovementHeaderTransactionStatusListCore;

		protected virtual CodeDescriptionPairList NctsMovementHeaderTransactionStatusListCore => Factory.GetCachedValue<NctsMovementHeaderTransactionStatusList>();

		CodeDescriptionPairList CountryList => Factory.GetCachedCountryNC008List(Parent.DefaultDataGroupingCode);

		public CustomsOfficeCodeCollection DestinationCustomsOfficeCodeList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Parent.GetDataGroupsForDestinationOfficeLookup(), Parent.GetRolesForDestinationOfficeLookup());

		public CodeDescriptionPairList WeightUnitList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}
