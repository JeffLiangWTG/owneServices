using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Module
{
	public class ForeignOperatorLookups : CommonFilterLookups
	{
		public ForeignOperatorLookups(FilterStripBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<BRMessageStatusList>();

		public CodeDescriptionPairList CustomsStatusTypeList => Factory.GetCachedValue<ForeignOperatorCustomsStatusTypeList>();

		public virtual RefCountryCollection Countries
		{
			get
			{
				return Factory.GetCachedValue("RefCountryCollection", delegate
				{
					return new RefCountryCollection(Factory);
				});
			}
		}
	}
}
