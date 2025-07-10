using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccGLDComplianceReport))]
	public class AccGLDComplianceReportTest : AccComplianceReportTest
	{
		[TestDate(2023, 01, 1)]
		[SuspendCriticalValidation]
		public void TestGLDReportLinesExportWhenDataNotInEDW_GLDComplianceReportUsingEDWEnabled()
		{
			TestGLDReportLinesExport(true, false, false);
		}

		[TestDate(2023, 01, 1)]
		[SuspendCriticalValidation]
		public void TestGLDReportLinesExportWhenDataNotInEDW_GLDComplianceReportUsingEDWDisabled()
		{
			TestGLDReportLinesExport(false, false, false);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AccGLDComplianceReport>();
		}

		protected override Type BusinessObjectType => typeof(AccGLDComplianceReport);
	}
}
