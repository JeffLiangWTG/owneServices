using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using JC = CargoWise.EntityFramework.JoinCondition;

namespace CargoWise.EntityFramework.Testing
{
	[UseSnapshotProtection]
	sealed class ZDBOnlyQueryNonTransactionedTestCase : TestCase
	{
		public void TestBitField()
		{
			var factory = new BusinessObjectFactory();
			var b1 = factory.New<DummyBusinessObject>();
			b1.Z0_BitFalse = false;
			var b2 = factory.New<DummyBusinessObject>();
			b2.Z0_BitFalse = true;
			factory.Save();

			var factory2 = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_BitFalse, false);
			var collection = factory2.Load<DummyBusinessObject>(query);
			AssertEquals(1, collection.Length);
			AssertEquals(b1.PK, collection[0].PK);
		}

		public void TestNoLockWithSubQueryOnDB()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = factory.New<DummyBusinessObject>();
			DummyDependantBusinessObject kid = factory.New<DummyDependantBusinessObject>();
			kid.ZD1_Z0 = dummy.PK;

			factory.Save();

			using (DbConnection extraConnection = Db.NewExtraConnectionToMainDb())
			{
				extraConnection.BeginTransaction();

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
				query.IsNoLock = true;
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject),
					DummyDependentBizoSchema.ZD1_Z0);
				subQuery.AddToFilter(DummyDependentBizoSchema.PK, kid.PK);
				query.AddSubQuery(subQuery, JC.And);
				AssertNotNull("Should find dummy", new BusinessObjectFactory().LoadTop1(typeof(DummyBusinessObject), query));

				try
				{
					extraConnection.ExecuteNonQuery("delete from " + DummyDependantBusinessObject.Schema.TableName);
					extraConnection.ExecuteNonQuery("delete from " + DummyBusinessObject.Schema.TableName);
					AssertNull("Should not find dummy since its been deleted on another connection",
						new BusinessObjectFactory().LoadTop1(typeof(DummyBusinessObject), query));
				}
				finally
				{
					extraConnection.RollbackTransaction();
				}
			}
		}
	}
}
