using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class FlagManagedCharge : CustomsChargeCode
	{
		public FlagManagedCharge(string code, MultilingualString description) : base(code, description)
		{
		}

		public MessageChargeKey MessageChargeKey
		{
			get { return new MessageChargeKey(Code, IsDutiable, IsVATible, IsIncludedInITOT, IsStatisticalValueApplicable); }
		}

		public bool IsIncludedInITOT { get; set; }

		public bool IsIncludedInInvoice { get; set; }

		public bool IsIncludedInInvoiceDeemedForThisCharge { get; set; }
	}
}
