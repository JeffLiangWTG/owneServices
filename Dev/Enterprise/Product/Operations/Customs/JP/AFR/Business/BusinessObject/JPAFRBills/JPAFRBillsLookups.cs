//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRBillsLookups
//
//    This class should be used for overriding collections in AutoJPAFRBillsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRBillsLookups : AutoJPAFRBillsLookups
	{
		public JPAFRBillsLookups(AutoJPAFRBills parent)
			: base(parent)
		{
		}

		public ConsignorCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public ICodeDescriptionPairList ManifestUnitList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Japan, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, ZDateTime.Today);

		public ICodeDescriptionPairList WeightUnitList
		{
			get { return Factory.GetCachedValue<WeightUnitCodeList>(); }
		}

		public ICodeDescriptionPairList VolumeUnitList
		{
			get { return Factory.GetCachedValue<VolumeUnitCodeList>(); }
		}

		public RefCurrencyCollection GoodsValueCurrencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public ICodeDescriptionPairList TemporaryLandingReasonCodeList
		{
			get { return Factory.GetCachedValue<TemporaryLandingReasonCodeList>(); }
		}

		public ICodeDescriptionPairList TransportModeList
		{
			get { return Factory.GetCachedValue<TransportModeList>(); }
		}

		public AFRBillCustomsStatusList BillCustomsStatusList
		{
			get { return Factory.GetCachedValue<AFRBillCustomsStatusList>(); }
		}

		public MessageStatusList MessageStatusList
		{
			get { return Factory.GetCachedValue<MessageStatusList>(); }
		}

		ZZRefCusCodeListCombinedCollection specialCargoCodes;
		public ZZRefCusCodeListCombinedCollection SpecialCargoCodes
		{
			get
			{
				if (specialCargoCodes == null)
				{
					specialCargoCodes = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, ZDateTime.Today);
					specialCargoCodes.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.Japan)));
				}
				return specialCargoCodes;
			}
		}

		#region UNDGSubstances

		public virtual UNDGSubstanceCollection UNDGSubstances
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}

		#endregion
	}
}
