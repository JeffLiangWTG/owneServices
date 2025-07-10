using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class CARMSOASummaryByDayDocumentWrapper : NonPersistentBusinessObject
	{
		public CARMSOASummaryByDayDocumentWrapper(CusStatementLine line, BusinessObjectFactory factory) : base(factory)
		{
			this.line = line;
		}

		readonly CusStatementLine line;

		#region Properties

		public ZDateTime ReleaseDate => line.B3_DueDate;

		public ZDateTime AccountingDate => line.B3_ScheduledProcessDate;

		public ZDecimal Duties => line.B4_CARMDNChargeAmount_Duties;

		public ZDecimal Excise => line.B4_CARMDNChargeAmount_ExciseTax;

		public ZDecimal ExciseDuties => line.B4_CARMDNChargeAmount_ExciseDuties;

		public ZDecimal SIMA => line.B4_CARMDNChargeAmount_SIMA;

		public ZDecimal GST => line.B4_CARMDNChargeAmount_GST;

		public ZDecimal HST => line.B4_CARMDNChargeAmount_HST;

		public ZDecimal PST => line.B4_CARMDNChargeAmount_PST;

		public ZDecimal Payments => line.B4_CARMDNChargeAmount_Payments;

		public ZDecimal Others => line.B4_CARMDNChargeAmount_Others;

		public ZDecimal TotalAmount => line.B4_CARMSOAChargeAmount_Totals;

		#endregion
	}
}
