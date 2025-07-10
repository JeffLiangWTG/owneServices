using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Business.ComplianceReport;
	using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
	using NUnit.Framework;
	using Constants = Core.Constants;

	[TestedType(typeof(AccComplianceReportDocumentSupporter))]
	public class AccComplianceReportDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			Assert("DataContext.ComplianceReport is supported", DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Constants.DataContext.ComplianceReport))));
		}

		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext", BusinessContext.ComplianceReport, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var creator = new TestObjectCreator(Factory);
			var result = Factory.NewWithValidTestData<AccComplianceReport>();
			var invoice = creator.CreateAPInvoice<APInvoice>("I0001", creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AT = creator.GST1.PK;
			Factory.Save();

			creator.CreateComplianceReportTransactionPivot(result, line);
			creator.CreateConfigurationForComplianceReport(result);
			return result;
		}

		AccComplianceReportDocumentSupporter DocumentSupporter
		{
			get { return AccComplianceReportDocumentSupporter.New((AccComplianceReport)GetDocumentSupportableBusinessObject()); }
		}

		#endregion
	}
}
