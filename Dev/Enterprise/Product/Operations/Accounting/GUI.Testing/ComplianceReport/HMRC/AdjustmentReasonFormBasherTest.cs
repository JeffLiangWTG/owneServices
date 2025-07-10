using System.Windows.Forms;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC.Testing
{
	[TestedType(typeof(AdjustmentReasonForm))]
	public class AdjustmentReasonFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			MTDSubmissionDataColumns columns = new MTDSubmissionDataColumns(Factory, Factory.NewWithValidTestData<AccComplianceReport>());
			return new AdjustmentReasonForm(new MTDSubmissionDataRow(Factory, 1, "Test", columns));
		}
	}
}
