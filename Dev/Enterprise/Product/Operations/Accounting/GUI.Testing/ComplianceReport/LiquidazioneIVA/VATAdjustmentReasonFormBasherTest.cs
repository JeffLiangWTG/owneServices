using System.Windows.Forms;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA.Testing
{
	[TestedType(typeof(VATAdjustmentReasonForm))]
	public class VATAdjustmentReasonFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			LIQSubmissionDataColumns columns = new LIQSubmissionDataColumns(Factory, Factory.NewWithValidTestData<AccComplianceReport>());
			return new VATAdjustmentReasonForm(new LIQSubmissionDataRow(Factory, RowType.TotalVatBaseReceivables, "Test", columns));
		}
	}
}
