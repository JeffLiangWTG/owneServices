using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DailyNoticeDetailsWrapper : NonPersistentBusinessObject
	{
		public DailyNoticeDetailsWrapper(CusStatementLine line, BusinessObjectFactory factory) : base(factory)
		{
			this.line = line;
		}

		readonly CusStatementLine line;

		#region Properties

		public ZDateTime ReleaseDate => line.B3_EntryDate;

		public ZDateTime AccountingDate => line.B3_ScheduledProcessDate;

		public ZString TransactionType => line.B3_EntryType;

		public ZString TransactionDescription => line.CARMTransactionDescription;

		public ZString ReferenceNumber => line.B3_AssociatedEntry;

		public ZString CADNumber => line.B3_EntryNum;

		public ZString CADVersion => line.CARMCADVersion;

		public ZString SubmittedBy => line.CARMSubmittedBy;

		public ZString AccountingStatus => line.CARMStatus;

		public ZString PortNumber => line.CARMPort;

		public ZDecimal CustomsDuty => line.B4_CARMDNChargeAmount_Duties;

		public ZDecimal ExciseTax => line.B4_CARMDNChargeAmount_ExciseTax;

		public ZDecimal ExciseDuties => line.B4_CARMDNChargeAmount_ExciseDuties;

		public ZDecimal SIMA => line.B4_CARMDNChargeAmount_SIMA;

		public ZDecimal GST => line.B4_CARMDNChargeAmount_GST;

		public ZDecimal HST => line.B4_CARMDNChargeAmount_HST;

		public ZDecimal PST => line.B4_CARMDNChargeAmount_PST;

		public ZDecimal Interest => line.B4_CARMDNChargeAmount_Interests;

		public ZDecimal Penalties => line.B4_CARMDNChargeAmount_Penalties;

		public ZDecimal Payments => line.B4_CARMDNChargeAmount_Payments;

		public ZDateTime PaymentDueDate => (ZDateTime)line.B3_DueDate;

		public ZDecimal Others => line.B4_CARMDNChargeAmount_Others;

		public ZDecimal TotalAmount => line.CARMTotal;

		#endregion

	}
}
