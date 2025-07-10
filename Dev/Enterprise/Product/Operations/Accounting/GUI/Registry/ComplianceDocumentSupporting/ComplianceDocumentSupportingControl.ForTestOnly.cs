#if DEBUG

namespace Enterprise.Accounting.GUI
{
	public partial class ComplianceDocumentSupportingControl
	{
		public ZArchitecture.ZGrid ComplianceDocumentSupportingGrid_ForTestOnly
		{
			get { return ComplianceDocumentSupportingGrid; }
			set { ComplianceDocumentSupportingGrid = value; }
		}
	}
}

#endif
