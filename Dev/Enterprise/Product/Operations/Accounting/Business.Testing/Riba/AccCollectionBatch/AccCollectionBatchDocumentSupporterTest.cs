using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccCollectionBatchDocumentSupporter))]
	public class AccCollectionBatchDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporter()
		{
			AccCollectionBatch batch = Factory.NewWithValidTestData<AccCollectionBatch>();
			AssertEquals("Document Supporter should be of type", typeof(AccCollectionBatchDocumentSupporter), batch.DocumentSupporter.GetType());
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext CollectionBatch is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.CollectionBatch))));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CollectionBatch, DocumentSupporter.BusinessContext);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<AccCollectionBatch>();
		}

		AccCollectionBatchDocumentSupporter DocumentSupporter
		{
			get
			{
				return new AccCollectionBatchDocumentSupporter(Factory.NewWithValidTestData<AccCollectionBatch>());
			}
		}
	}
}
