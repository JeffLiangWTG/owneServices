using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class ComplianceDocumentBatch
	{
		public ZInt BatchNumber { get; set; }
		public ZGuid CompanyPK { get; set; }
		public ZInt Count => ComplianceDocumentHeaderDetails?.Length ?? 0;
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ComplianceDocumentHeaderDetail[] ComplianceDocumentHeaderDetails { get; set; }
	}
}
