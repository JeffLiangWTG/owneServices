using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class DataLayerImportExtensionsTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestNewRowWithPK()
		{
			var dummy = Factory.RowFactory.NewRowWithPK(DummyBizoSchema.Instance);
			AssertEquals("Should have given the row a PK.", false, new ZGuid(dummy[DummyBizoSchema.Constants.PK]).IsEmpty);
			AssertEquals("Should have added the Row to the Table.", DataRowState.Added, ((DataRow)dummy).RowState);
		}

		public void TestDeleteAll()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			AssertEquals("Precondition", false, dummy1.IsDeleted);
			AssertEquals("Precondition", false, dummy2.IsDeleted);

			new[] { ((IBusinessObjectInternals)dummy1).Row, ((IBusinessObjectInternals)dummy2).Row }.DeleteAll();
			AssertEquals(true, dummy1.IsDeleted);
			AssertEquals(true, dummy2.IsDeleted);
		}

		public void TestLoadAndDeleteRows()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			AssertEquals("Precondition", false, dummy1.IsDeleted);
			AssertEquals("Precondition", false, dummy2.IsDeleted);

			var query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.PK, dummy1.PK);
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.PK, dummy2.PK);
			Factory.RowFactory.LoadAndDeleteRows(DummyBizoSchema.Constants.TableName, query);
			AssertEquals(true, dummy1.IsDeleted);
			AssertEquals(true, dummy2.IsDeleted);
		}

		public void TestDeleteAllJobDocAddresses()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var jobDocAddress1 = Factory.New<JobDocAddress>();
			var jobDocAddress2 = Factory.New<JobDocAddress>();
			jobDocAddress1.E2_ParentID = dummy.PK;
			jobDocAddress2.E2_ParentID = dummy.PK;
			AssertEquals("Precondition", false, jobDocAddress1.IsDeleted);
			AssertEquals("Precondition", false, jobDocAddress2.IsDeleted);

			dummy.Row().DeleteAllJobDocAddresses(Factory.RowFactory, DummyBizoSchema.PK);
			AssertEquals(true, jobDocAddress1.IsDeleted);
			AssertEquals(true, jobDocAddress2.IsDeleted);
		}
	}
}
