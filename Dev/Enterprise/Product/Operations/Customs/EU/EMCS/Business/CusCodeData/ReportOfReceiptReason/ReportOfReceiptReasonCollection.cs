using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptReasonCollection : CusCodeDataCollection<ReportOfReceiptReason>
	{
		public ReportOfReceiptReasonCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.ReportOfReceiptReason)
		{
			MaxCountValidationEnable(maxCount);
		}

		protected override bool AllowNewCore => Count < maxCount;
		const int maxCount = 9;
	}
}
