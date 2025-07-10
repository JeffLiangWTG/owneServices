#if DEBUG

namespace Enterprise.Accounting.Business
{
	public partial class ARComplianceDocumentHeader
	{
		public bool ADH_ReportingPeriod_ReadOnly_ForTestOnly => ADH_ReportingPeriod_ReadOnly;

		public bool ADH_ComplianceSubType_ReadOnly_ForTestOnly => ADH_ComplianceSubType_ReadOnly;

		public bool ADH_XD_ComplianceBook_ReadOnly_ForTestOnly => ADH_XD_ComplianceBook_ReadOnly;

		public bool ADH_DocumentDate_ReadOnly_ForTestOnly => ADH_DocumentDate_ReadOnly;

		public bool ADH_DocumentNumber_ReadOnly_ForTestOnly => ADH_DocumentNumber_ReadOnly;
	}
}

#endif
