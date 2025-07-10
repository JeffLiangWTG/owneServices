using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ComplianceReportDataContextManager))]
	public class ComplianceReportDataContextManagerTest : DataContextManagerTestCase<ComplianceReportDataContextManager, AccComplianceReport>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("ComplianceReport doesn't have any JobNumber", true);
		}

		#region Implementation

		protected override AccComplianceReport GetNewBusinessObjectForTesting()
		{
			var result = Factory.NewWithValidTestData<AccComplianceReport>();

			var creator = new TestObjectCreator(Factory.BOFactory);
			var invoice = creator.CreateAPInvoice<APInvoice>("I0001", creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AT = creator.GST1.PK;
			Factory.SaveForTesting();

			creator.CreateComplianceReportTransactionPivot(result, line);
			creator.CreateConfigurationForComplianceReport(result);

			return result;
		}

		#endregion
	}
}
