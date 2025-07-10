using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageBillLookups : AsycudaBillLookups
	{
		public TemporaryStorageBillLookups(AutoAsycudaBill parent) : base(parent)
		{
		}

		public new TemporaryStorageBill Parent => (TemporaryStorageBill)base.Parent;

		public TemporaryStorageBillKindList BillKindList => new TemporaryStorageBillKindList();

		public ZZRefCusCodeListCombinedCollection BillTypeList
		{
			get
			{
				var dataGrouping = Parent.Header?.DataGrouping ?? ZString.Empty;
				var date = ZDateTime.Today;
				return Factory.GetCachedValue(string.Join("|", "EU.Business.TemporaryStorageBillLookups.BillTypeList", dataGrouping, date.ToShortDateString()), () =>
				{
					var list = new ZZRefCusCodeListCombinedCollection(Factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, date);
					list.Load();
					list.Sort(ZZRefCusCodeListCombinedSchema.ZZD_Code.Name);
					return list;
				});
			}
		}

		public CodeDescriptionPairList GrossWeightUnitList => Factory.GetWeightUQList();

		public OrgHeaderCollection ConsignorOrganizationList => new ConsignorCollection(Factory);

		public OrgHeaderCollection ConsigneeOrganizationList => new ConsigneeCollection(Factory);

		public OrgHeaderCollection NotifyPartyOrganizationList => new OrganisationsFindBoxCollection(Factory);

		public CodeDescriptionPairList TypeOfPersonList => Factory.GetCachedValue<TypeOfPersonList>();

		public CodeDescriptionPairList ShipperState_List => GetState_List(Parent.ABL_RN_NKShipperCountryInfo);

		public CodeDescriptionPairList ConsigneeState_List => GetState_List(Parent.ABL_RN_NKConsigneeCountryInfo);

		public CodeDescriptionPairList NotifyPartyState_List => GetState_List(Parent.ABL_RN_NKNotifyPartyCountryInfo);

		CodeDescriptionPairList GetState_List(ZPropertyInfo info)
		{
			return Factory.GetStateList((ZString)info.Value, info.HasErrors());
		}
	}
}
