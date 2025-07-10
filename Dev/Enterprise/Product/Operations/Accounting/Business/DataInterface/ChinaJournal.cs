using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;

namespace Enterprise.Accounting.Business.DataInterface
{
	public class ChinaJournal : Voucher
	{
		public ZDecimal OriginalAmount { get; set; }
		public ZString Currency { get; set; }
		public ZString ApprovedBy { get; set; }
		public ZString HandlingStaff { get; set; }
		public ZString Annotation { get; set; }
		public ZString PaymentNumber { get; set; }
		public ZString BusinessContacts { get; set; }
		public ZInt SerialNumber { get; set; }
		public ZString SystemModule { get; set; }
		public ZString BusinessDesc { get; set; }
		public ZString ReferenceInformation { get; set; }
		public ZString GLAccountNumAndDescription { get; set; }
	}
}