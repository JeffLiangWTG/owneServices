using System;
using Enterprise.DataTransfer.Native.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Retrieve.Retrievers
{
	public class ScoreRetrieverTest : TransactionedTestCase
	{
		public void TestScoreRetrieve()
		{
			var pair = new EntityCriteria();
			pair.EntityName = "OrgHeader";
			pair.PropertyName = "Code";
			pair.Value = "ABC";

			var retriever = RetrieverFactory.GetRetriever(RetrieveType.Score, new AncillaryImportServices());
			var definition = TestUtil.GetEntitySetDefinition("Organization");

			AssertExceptionThrown(
				"Score Retriever has not be implemented yet",
				typeof(NotImplementedException),
				() => retriever.Retrieve(definition, new[] { pair }));
		}
	}
}
