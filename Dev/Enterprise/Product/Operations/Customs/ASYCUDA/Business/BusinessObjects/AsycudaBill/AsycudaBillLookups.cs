using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public partial class AsycudaBillLookups : ManifestBase.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public static CodeDescriptionPairList GetCargoStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CargoStatusList>();
		}

		public CodeDescriptionPairList CargoStatusList => CargoStatusListCore;

		protected virtual CodeDescriptionPairList CargoStatusListCore
		{
			get { return GetCargoStatusList(Factory); }
		}

		public CodeDescriptionPairList PackageTypeList => PackageTypeListCore;

		protected virtual CodeDescriptionPairList PackageTypeListCore
		{
			get { return Factory.GetPackageTypeList(); }
		}

		public CodeDescriptionPairList WeightUQList => WeightUQListCore;

		protected virtual CodeDescriptionPairList WeightUQListCore
		{
			get { return Factory.GetWeightUQList(); }
		}

		public CodeDescriptionPairList VolumeUQList
		{
			get { return Factory.GetVolumeUQList(); }
		}

		public CodeDescriptionPairList ShipmentTypesList
		{
			get { return Factory.GetCachedValue<ShipmentTypeList>(); }
		}

		public CodeDescriptionPairList PrepaidCollectList => GetPrepaidCollectListCore();

		protected virtual CodeDescriptionPairList GetPrepaidCollectListCore()
		{
			return Factory.GetCachedValue("AsycudaBillLookups.PrepaidCollectList", delegate
			{
				var prepaidCollectList = new CodeDescriptionPairList();
				prepaidCollectList.AddPair(Core.Constants.DomesticPaymentTerms.Collect, Res.GetString("11111111-f8a9-48a9-85a5-fa515bfee776", "Collect"));
				prepaidCollectList.AddPair(Core.Constants.DomesticPaymentTerms.Prepaid, Res.GetString("11111111-8597-427e-86fe-7929a12c0226", "Prepaid"));
				return prepaidCollectList;
			});
		}

		public RefCurrencyCollection DiscountValueCurrencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public RefCurrencyCollection OtherChargesValueCurrencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public new ConsigneeCollection Consignees
		{
			get { return new AsycudaBillConsigneeCollection(Factory, Parent); }
		}

		#region Consignee State List

		public virtual CodeDescriptionPairList ConsigneeState_List => GetState_List(Parent.ABL_RN_NKConsigneeCountryInfo);

		#endregion

		#region Seller State List

		public virtual CodeDescriptionPairList SellerState_List => GetState_List(Parent.ABL_RN_NKSellerCountryInfo);

		#endregion

		public ConsignorCollection Consignors
		{
			get { return new AsycudaBillConsignorCollection(Factory, Parent); }
		}

		#region Shipper State List

		public virtual CodeDescriptionPairList ShipperState_List => GetState_List(Parent.ABL_RN_NKShipperCountryInfo);

		#endregion

		#region NotifyParty State List

		public CodeDescriptionPairList NotifyPartyState_List => GetState_List(Parent.ABL_RN_NKNotifyPartyCountryInfo);

		#endregion

		public CodeDescriptionPairList BuyerState_List => GetState_List(Parent.ABL_RN_NKBuyerCountryInfo);

		public OrgHeaderCollection Organisations
		{
			get { return new AsycudaBillNotifyPartyCollection(Factory, Parent); }
		}

		public new OrgHeaderCollection Buyers
		{
			get { return new AsycudaBillBuyerCollection(Factory, Parent); }
		}

		public OrgHeaderCollection SellersOrgList
		{
			get { return new AsycudaBillSellerCollection(Factory, Parent); }
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public static CodeDescriptionPairList GetBolTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AsycudaBillLookups.BolTypes", delegate
			{
				var prepaidCollectList = new CodeDescriptionPairList();
				prepaidCollectList.AddPair(Core.Constants.ShipmentTypes.StandardHouse, Core.Constants.ShipmentTypeDescriptions.StandardHouse);
				prepaidCollectList.AddPair(Core.Constants.ShipmentTypes.CoLoadMaster, Core.Constants.ShipmentTypeDescriptions.CoLoadMaster);
				return prepaidCollectList;
			});
		}

		public CodeDescriptionPairList BolTypes
		{
			get
			{
				return GetBolTypes(Factory);
			}
		}

		public ICollection CustomsLoadingPortList => Parent.Header.Lookups.CustomsLoadingPortList;
		public ICollection CustomsDischargePortList => Parent.Header.Lookups.CustomsDischargePortList;

		#region Implementation

		protected CodeDescriptionPairList GetState_List(ZPropertyInfo info)
		{
			return Factory.GetStateList((ZString)info.Value, info.HasErrors());
		}

		#endregion

		#region Reg. No. Type List

		public CodeDescriptionPairList ShipperRegistrationNoTypeList
		{
			get { return GetRegistrationNoTypeList(Parent.ShipperRegNoTypes()); }
		}

		public CodeDescriptionPairList ConsigneeRegistrationNoTypeList
		{
			get { return GetRegistrationNoTypeList(Parent.ConsigneeRegNoTypes()); }
		}

		public virtual CodeDescriptionPairList SellerRegistrationNoTypeList
		{
			get { return GetRegistrationNoTypeList(Parent.SellerRegNoTypes()); }
		}

		public CodeDescriptionPairList NotifyPartyRegistrationNoTypeList
		{
			get { return GetRegistrationNoTypeList(Parent.NotifyPartyRegNoTypes()); }
		}

		protected CodeDescriptionPairList GetRegistrationNoTypeList(ZString[] regNoTypeList)
		{
			var result = new CodeDescriptionPairList();

			var pairListForGettingDescription = new OrgCodeLists().CustomsCodes_List(Parent.CountryCode);

			foreach (var item in regNoTypeList)
			{
				result.AddPair(item, pairListForGettingDescription.GetDescriptionFromCode(item));
			}

			return result;
		}

		#endregion

		#region Incoterm List
		public virtual CodeDescriptionPairList IncotermList => Factory.GetCachedValue("AsycudaBillLookups.IncotermCodeList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.IncoTerms2010));

		#endregion

		public virtual ICodeDescriptionPairList Procedures => new CodeDescriptionPairList();

		public virtual OrgHeaderCollection GoodsLocationsOrgHeaderCollection => new OrgHeaderCollection(Factory);
	}
}
