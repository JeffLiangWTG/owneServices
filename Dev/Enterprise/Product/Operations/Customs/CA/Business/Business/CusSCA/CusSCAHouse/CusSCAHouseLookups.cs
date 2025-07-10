using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAHouseLookups : Customs.Business.CusSCAHouseLookups
	{
		public CusSCAHouseLookups(CusSCAHouse parent)
			: base(parent)
		{
		}

		new CusSCAHouse Parent
		{
			get { return (CusSCAHouse)base.Parent; }
		}

		public CodeDescriptionPairList InTransitCodeList
		{
			get { return Factory.GetCachedValue<InTransitCodeList>(); }
		}

		public CodeDescriptionPairList ConsigneeState_List_For_Country
		{
			get { return GetList(Parent.CA_RN_NKConsigneeCountryCode); }
		}

		public CodeDescriptionPairList ConsignorState_List_For_Country
		{
			get { return GetList(Parent.CA_RN_NKConsignorCountryCode); }
		}

		public CodeDescriptionPairList NotifyState_List_For_Country
		{
			get { return GetList(Parent.CA_RN_NKNotifyCountryCode); }
		}

		public CodeDescriptionPairList DeliveryState_List_For_Country
		{
			get { return GetList(Parent.CA_RN_NKDeliveryCountryCode); }
		}

		CodeDescriptionPairList GetList(ZString countryCode)
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			return country != null ? new OrgCodeLists().State_List(country) : new CodeDescriptionPairList();
		}
	}
}
