using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationLookups : JobDeclarationLookups
	{
		public EMCSJobDeclarationLookups(EMCSJobDeclaration parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<DefermentMethodList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<EDIMessageStatusList>();

		public override ICodeDescriptionPairList DeclarantTypeList => Factory.GetCachedValue<EMCSEntryTypeList>();

		public override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<EMCSDestinationTypeList>();

		public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<EntryStatusList>();

		public override CodeDescriptionPairList TransportTypeList
		{
			get
			{
				return Factory.GetCachedValue("EU.EMCSJobDeclarationLookups.TransportTypeList", () =>
				{
					var list = new TransportTypeList();
					list.RemoveCode(Customs.Business.TransportTypeList.Codes.OwnPropulsion);
					list.Add(new CodeDescriptionPair(Core.Constants.TransportModes.Other, Core.Constants.TransportModeDescriptions.Other));
					list.Sort();
					return list;
				});
			}
		}

		public override CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				return Factory.GetCachedValue("EU.EMCSJobDeclarationLookups.ApplicationCodeList", () =>
				{
					var list = new CodeDescriptionPairList();
					list.Add(new CodeDescriptionPair(EMCSJobDeclaration.EMCSApplicationCode, EMCSJobDeclaration.EMCSApplicationCode));
					return list;
				});
			}
		}

		public CodeDescriptionPairList JourneyTimeUnitList => Factory.GetCachedValue<JourneyTimeUnitList>();

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public ConsigneeCollection ConsigneesList => new ConsigneeCollection(Factory);

		public OrganisationsFindBoxCollection OwnersList => new OrganisationsFindBoxCollection(Factory);

		public OrganisationsFindBoxCollection CarrierAgentList => new ShippingProviderCollection(Factory);

		public OrganisationsFindBoxCollection TransporterList => new ShippingProviderCollection(Factory);

		public OrganisationsFindBoxCollection DispatchWarehouseList => new WarehouseClientCollection(Factory);

		public OrganisationsFindBoxCollection DestinationWarehouseList => new WarehouseClientCollection(Factory);
	}
}
