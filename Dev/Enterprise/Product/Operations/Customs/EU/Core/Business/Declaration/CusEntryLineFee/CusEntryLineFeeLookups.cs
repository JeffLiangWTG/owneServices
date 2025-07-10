using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent)
			: base(parent)
		{
		}

		public CusEntryLineFee EntryLineFee => Parent;

		protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		public virtual CodeDescriptionPairList NationalFeeTypeCodeList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public CodeDescriptionPairList ChargeTypeList => CommonLookupsHelper.TypeList;

		public override CodeDescriptionPairList RateOverrideReasonList => Factory.GetCachedValue<RateOverrideReasonList>();

		public override CodeDescriptionPairList MethodOfCalculationList
		{
			get
			{
				return Factory.GetCachedValue("MethodOfCalculationList_" + Parent.CountryCode, delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddRange(CommonLookupsHelper.BaseQuantityUQList);
					list.AddPair("%");
					list.Sort();
					return list;
				});
			}
		}

		public override CodeDescriptionPairList MethodOfPaymentList => CommonLookupsHelper.MOPList;

		#region CommonLookups

		protected TaxLookupsCommon CommonLookupsHelper
		{
			get
			{
				return Factory.GetCachedValue(this.GetType().FullName + ".CommonLookupsHelper_" + Parent.PK, () =>
				{
					return GetNewCommonLookupsHelper();
				});
			}
		}

		protected virtual TaxLookupsCommon GetNewCommonLookupsHelper() => new TaxLookupsCommon(EntryLineFee);

		#endregion
	}
}
