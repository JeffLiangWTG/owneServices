using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class ComplianceDocumentHeaderDetail
	{
		public ZGuid HeaderPK { get; set; }
		public ZGuid OriginalHeaderPK { get; set; }
		public ZString TransactionType { get; set; }
		public ZString DocumentNumber { get; set; }
		public ZDateTime DocumentDate { get; set; }
		public ZDateTime? OriginalDocumentDate { get; set; }
		public ZDateTime? VoidDate { get; set; }
		public ZString SystemVATRegistrationNum { get; set; }
		public OrgHeaderDetail OrgHeaderDetail { get; set; }
		public ZString OrgHeaderCategory { get; set; }
		public ZString BarCode { get; set; }
		public ZString Description { get; set; }
		public ZString InternalReference { get; set; }
		public ZDecimal Amount => ComplianceDocumentLineDetails?.Sum(x => x.Amount) ?? 0M;
		public ZDecimal TaxAmount => ComplianceDocumentLineDetails?.Sum(x => x.TaxAmount) ?? 0M;
		public ZDecimal TotalAmount => Amount + TaxAmount;
		public ZString VoidingReason { get; set; }
		public ZString ApprovalNumber { get; set; }
		public bool IsSpecialVoiding { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ComplianceDocumentLineDetail[] ComplianceDocumentLineDetails { get; set; }
	}
}
