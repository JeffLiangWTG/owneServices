using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Logging;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Retrieve.Retrievers
{
	public class PartialRetrieverTest : TransactionedTestCase
	{
		public void TestPartialRetrieve_Success()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "FullName";
			pair.Value = "A PLUS INFO CORPORATION";

			var results = retriever.Retrieve(definition, new[] { pair });

			AssertEquals("One record found", 1, results.Count());
		}

		public void TestPartialRetrieve_NoMatches()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "FullName";
			pair.Value = "ZUBIN TEST ORG";

			var results = retriever.Retrieve(definition, new[] { pair });

			AssertEquals("No Record should be found", 0, results.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();
			session = new AncillaryImportServices();
			logger = session.Logger as MemoryLogger;
			logger.Clear();
			retriever = RetrieverFactory.GetRetriever(RetrieveType.Partial, session);
			definition = TestUtil.GetEntitySetDefinition("Organization");
		}
		AncillaryImportServices session;
		MemoryLogger logger;
		Retriever retriever;
		EntitySetDefinition definition;
	}
}
