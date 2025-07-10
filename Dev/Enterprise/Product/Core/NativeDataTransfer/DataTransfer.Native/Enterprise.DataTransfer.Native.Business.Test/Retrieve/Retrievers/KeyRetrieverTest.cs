using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Retrieve.Retrievers
{
	public class KeyRetrieverTest : TransactionedTestCase
	{
		public void TestKeyRetrieve_PK()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "PK";
			pair.Value = "C3F842EF-3BE5-448C-BED3-0017B232C624";

			var results = retriever.Retrieve(definition, new[] { pair });

			AssertEquals(1, results.Count());
		}

		public void TestKeyRetrieve_CandidateKey()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "Code";
			pair.Value = "ABIGAS";

			var results = retriever.Retrieve(definition, new[] { pair });

			AssertEquals(1, results.Count());
		}

		public void TestKeyRetrieve_NoneFound()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "PK";
			pair.Value = "E3F842EF-3BE5-448C-BED3-0017B232C624";

			var results = retriever.Retrieve(definition, new[] { pair });

			AssertEquals(0, results.Count());
		}

		public void TestCheckCriteria_MultipleCriteria()
		{
			var pairs = new List<EntityCriteria>();

			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "PK";
			pair.Value = "C3F842EF-3BE5-448C-BED3-0017B232C624";

			var pair2 = new EntityCriteria();
			pair2.EntityName = "OrgHeader";
			pair2.PropertyName = "PK";
			pair2.Value = "C3F842EF-3BE5-448C-BED3-0017B232C624";

			pairs.Add(pair);
			pairs.Add(pair2);

			retriever.CheckCriterias(definition, new[] { pair, pair2 });

			AssertEquals(LogType.Warning, logger.Buffer.Logs().First().Type);
			AssertEquals("Only a single criteria item can be passed for Key based retrieve.", logger.Buffer.Logs().First().Message);
		}

		public void TestCheckCriteria_ValidPrimaryKey()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "PK";
			pair.Value = "C3F842EF-3BE5-448C-BED3-0017B232C624";

			retriever.CheckCriterias(definition, new[] { pair });
			Assert("Check should be passed for valid primary key", true);
		}

		public void TestCheckCriteria_CandidateKey()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "Code";
			pair.Value = "ABC";

			retriever.CheckCriterias(definition, new[] { pair });
			Assert("Check should be passed for valid Candidate Key", true);
		}

		public void TestCheckCriteria_NonPKOrCandidateKey()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "FullName";
			pair.Value = "Some Organisation Name";

			AssertExceptionThrown(
				"Only Primary Key or CandidateKey could be allowed",
				typeof(NativeXMLUserVisibleException),
				() => retriever.CheckCriterias(definition, new[] { pair }));
		}

		protected override void SetUp()
		{
			base.SetUp();
			session = new AncillaryImportServices();
			logger = session.Logger as MemoryLogger;
			retriever = RetrieverFactory.GetRetriever(RetrieveType.Key, session);
			definition = TestUtil.GetEntitySetDefinition("Organization");
		}
		AncillaryImportServices session;
		MemoryLogger logger;
		Retriever retriever;
		EntitySetDefinition definition;
	}
}
