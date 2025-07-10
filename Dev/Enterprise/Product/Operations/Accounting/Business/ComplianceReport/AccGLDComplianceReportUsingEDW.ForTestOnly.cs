#if DEBUG

using CargoWise.Data;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public partial class AccGLDComplianceReportUsingEDW
	{
		public DbConnection EDWConnectionForTest
		{
			get { return edwConnection; }
			set { edwConnection = value; }
		}
	}
}

#endif
