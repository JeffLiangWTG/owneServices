using System.Windows.Forms;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC.Testing
{
	[TestedType(typeof(MTDDeclarationForm))]
	public class DeclarationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MTDDeclarationForm(new MTDSubmissionDataColumns(Factory, Factory.NewWithValidTestData<AccComplianceReport>()));
		}
	}
}
