using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments;

namespace Enterprise.DocumentWrappers.Testing.GenericDocument
{
	sealed class WrapperMapsTest : BaseRunDocumentsTest
	{
		public void TestDocumentEngineReferenceGuide()
		{
			RunDocument("Document Engine Reference Guide");
		}

		public void TestSummaryGenericFreightJobMap()
		{
			RunDocument("Summary Generic Freight Job Map");
		}

		public void TestFullGenericFreightJobMap()
		{
			RunDocument("Full Generic Freight Job Map");
		}

		public override BusinessObject GetBusinessObject
		{
			get { return DocumentMenuCustomisation.New(null, null, Factory); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DocumentCustomiser; }
		}
	}
}
