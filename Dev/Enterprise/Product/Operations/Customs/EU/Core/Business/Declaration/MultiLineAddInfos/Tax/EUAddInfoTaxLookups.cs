//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEUAddInfoTaxLookups
//
//    This class should be used for overriding collections in AutoEUAddInfoTaxLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class EUAddInfoTaxLookups : AutoEUAddInfoTaxLookups
	{
		public EUAddInfoTaxLookups(AutoEUAddInfoTax parent) : base(parent)
		{
		}

		protected TaxLookupsCommon CommonLookupsHelper
		{
			get
			{
				return Factory.GetCachedValue(this.GetType().FullName + "CommonLookupsHelper_" + Parent.PK, () =>
				{
					return GetNewCommonLookupsHelper();
				});
			}
		}

		public new Tax_CusAddInfoOnlyForPIVOT Parent => (Tax_CusAddInfoOnlyForPIVOT)base.Parent;

		protected virtual TaxLookupsCommon GetNewCommonLookupsHelper()
		{
			return new TaxLookupsCommon(Parent);
		}

		public CodeDescriptionPairList RateDutyList => CommonLookupsHelper.RateDutyList;
		public CodeDescriptionPairList MOPList => CommonLookupsHelper.MOPList;
		public CodeDescriptionPairList TypeList => CommonLookupsHelper.TypeList;
	}
}
