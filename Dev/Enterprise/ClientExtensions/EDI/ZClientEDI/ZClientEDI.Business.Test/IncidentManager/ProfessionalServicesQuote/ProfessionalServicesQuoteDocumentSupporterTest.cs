using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[TestedType(typeof(ProfessionalServicesQuote.ProfessionalServicesQuoteDocumentSupporter))]
	class ProfessionalServicesQuoteDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext.Organisation is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob))));
		}

		public void TestGetDocBusinessObject()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, null);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			var quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			AssertEquals(BusinessContext.ProfessionalSrvQuote, quote.DocumentSupporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("Security checkpoint", EDISecurityCheckpoints.ProfessionalServicesQuote, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			base.TestRunningDocumentsShouldNotCauseException();
			Assert(true);
		}

		#region Implementation

		ProfessionalServicesQuote.ProfessionalServicesQuoteDocumentSupporter documentSupporter;
		ProfessionalServicesQuote.ProfessionalServicesQuoteDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = documentSupporter = new ProfessionalServicesQuote.ProfessionalServicesQuoteDocumentSupporter(Factory.NewWithValidTestData<ProfessionalServicesQuote>())); }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<ProfessionalServicesQuote>();
		}

		#endregion
	}
}
