using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.Testing
{
	/// <summary>
	/// Common logic for Esterometro tests.
	/// </summary>
	public abstract class EsterometroTestCaseWithFactory : TestCaseWithFactory
	{
		protected void AssertXMLForBranch(ZGuid branchPK, string expectedXML, ComplianceReportXmlBuilder xmlBuilder)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = ObjectCreator.CreateComplianceReport(AccComplianceReport.ReportTypes.Esterometro);
				this.AssertXMLEqualsByDiff(expectedXML, xmlBuilder.BuildXml(report).ToString());
			}
		}
		protected void AssertXMLForBranch(ZGuid branchPK, string errorMessage, string expectedXML, ComplianceReportXmlBuilder xmlBuilder)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = ObjectCreator.CreateComplianceReport(AccComplianceReport.ReportTypes.Esterometro);
				this.AssertXMLEqualsByDiff(errorMessage, expectedXML, xmlBuilder.BuildXml(report).ToString());
			}
		}

		protected TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
