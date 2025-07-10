using System.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class UniversalObjectFactoryExtensionsTest : TestCaseWithUniversalObjectFactory
	{
		#region TestDeleteRowAndSetHasChanges

		public void TestDeleteRowAndSetHasChanges()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			Factory.SaveForTesting();

			var dummyBizORow = Factory.RowFactory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummyBizO.PK);
			AssertEquals("Precondition", DataRowState.Unchanged, dummyBizORow.RowState);
			AssertEquals("Precondition", false, dummyBizO.HasChanges);

			var columnIndexer = DataObjectReader.GetColumnIndexerFromRow(dummyBizORow);
			Factory.DeleteRowAndSetHasChanges<DummyBusinessObject>(columnIndexer, DummyBizoSchema.PK);
			AssertEquals("Data Row should be deleted.", DataRowState.Deleted, dummyBizORow.RowState);
			AssertEquals("BizO must have changes.", true, dummyBizO.HasChanges);

			Factory.SaveForTesting();
			AssertNull("BizO must be deleted.", new UniversalObjectFactory().Load<DummyBusinessObject>(dummyBizO.PK));
		}

		#endregion

		#region TestDeleteRowAndSetHasChanges_ForRow

		public void TestDeleteRowAndSetHasChanges_ForRow()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			Factory.SaveForTesting();

			var dummyBizORow = Factory.RowFactory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummyBizO.PK);
			AssertEquals("Precondition", DataRowState.Unchanged, dummyBizORow.RowState);
			AssertEquals("Precondition", false, dummyBizO.HasChanges);

			var columnIndexer = DataObjectReader.GetColumnIndexerFromRow(dummyBizORow);
			columnIndexer.DeleteRowAndSetHasChanges(dummyBizO);
			AssertEquals("Data Row should be deleted.", DataRowState.Deleted, dummyBizORow.RowState);
			AssertEquals("BizO must have changes.", true, dummyBizO.HasChanges);

			Factory.SaveForTesting();
			AssertNull("BizO must be deleted.", new UniversalObjectFactory().Load<DummyBusinessObject>(dummyBizO.PK));
		}

		#endregion
	}
}

