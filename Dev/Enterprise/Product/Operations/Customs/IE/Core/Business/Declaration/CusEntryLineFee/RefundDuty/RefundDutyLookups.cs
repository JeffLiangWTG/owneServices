using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class RefundDutyLookups : ZLookups
	{
		public RefundDutyLookups(RefundDuty parent) : base(parent)
		{
		}

		public new RefundDuty Parent => (RefundDuty)base.Parent;

		const string TaxTypesCacheKey = "IERefundDutyTypes";
		public CodeDescriptionPairList TaxTypes => Factory.GetCachedValue<CodeDescriptionPairList>(TaxTypesCacheKey, () => new TaxLookupsCommon(Parent).TypeList);
	}
}
