using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderLookups : CusInBondHeaderLookups
	{
		public NctsHeaderLookups(NctsHeader parent)
			: base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		public virtual CodeDescriptionPairList EventFlagList
		{
			get
			{
				return Factory.GetCachedValue("NctsHeaderLookups.EventFlagList", () =>
				{
					var list = new EventFlagList();

					if (Parent.IsPhase5)
					{
						list.RemoveCode(Business.EventFlagList.Codes.Cancelled);
					}
					return list;
				});
			}
		}

		public RefCountryCollection Countries => new RefCountryCollection(Factory);

		public CodeDescriptionPairList NctsMovementTypeList => Factory.GetCachedValue<NctsMovementType>();

		public CodeDescriptionPairList CountryOfDispatchList => Factory.GetCachedCountryNC008List(Parent.DefaultDataGroupingCode);

		public RefUNLOCOCollection PortOfDispatchList
		{
			get
			{
				var result = new RefUNLOCOCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.BH_RL_NKImportLoadPort));
				return result;
			}
		}

		public CusEntryHeadersToAttachCollection CusEntryHeadersToAttach => new CusEntryHeadersToAttachCollection(Parent);

		public CodeDescriptionPairList CountryOfDestinationList => Factory.GetCachedCountryNC008List(Parent.DefaultDataGroupingCode);

		public new ShippingProviderCollection Carriers => new ShippingProviderCollection(Factory);

		public ConsigneeCollection Consignees => new ConsigneeCollection(Factory);

		public ConsignorCollection Consignors => new ConsignorCollection(Factory);

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		public CodeDescriptionPairList ModeOfTransportList => Factory.GetCachedValue<ModeOfTransportList>();

		public CustomsOfficeCodeCollection DestinationCustomsOfficeCodeList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Parent.GetDataGroupsForDestinationOfficeLookup(), Parent.GetRolesForDestinationOfficeLookup());

		public virtual CodeDescriptionPairList NctsTransitStatusList => Factory.GetCachedValue<NctsTransitStatusList>();

		public virtual CodeDescriptionPairList NctsMessageStatusList => Factory.GetCachedValue<LogicalStatusList>();

		public CodeDescriptionPairList UnloadedMeansOfTransportAtDepartureNationalityList
		{
			get
			{
				return Factory.GetCachedValue("EU|NCTS|UnloadedMeansOfTransportAtDepartureNationalityList", () =>
				{
					var countryList = new CodeDescriptionPairList();
					countryList.AddRange(new RefCountryCollection(Factory, new ZQuery(RefCountrySchema.RN_IsActive, true)));
					countryList.SortByDescription();
					return countryList;
				});
			}
		}

		public virtual CodeDescriptionPairList CommunicationLanguageList => new CodeDescriptionPairList();

		public CodeDescriptionPairList EUCommunityCountryCodesList => Factory.GetEUCommunityCountryCodesList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
	}
}
