using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test
{
	class NullifyRelationshipsTest : TestCaseWithFactory
	{
		public void TestFkToJobHeader()
		{
			var schemaGuidColumns = NullifyRelationships.FkToJobHeader;

			AssertEquals(3, schemaGuidColumns.Count);

			CombineAssertions("Schema columns did not include FK(s)", () =>
			{
				AssertCollectionContains(AccTransactionHeaderSchema.AH_JH, schemaGuidColumns);
				AssertCollectionContains(AccTransactionLinesSchema.AL_JH, schemaGuidColumns);
				AssertCollectionContains(AccHotChequeSchema.AQ_JH, schemaGuidColumns);
			});
		}
	}
}
