using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptReasonLookups : CusCodeDataLookups
	{
		public ReportOfReceiptReasonLookups(ReportOfReceiptReason officeCode) : base(officeCode)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<EMCSReceiptReasonCodeList>();

		protected new ReportOfReceiptReason Parent => (ReportOfReceiptReason)base.Parent;
	}
}
