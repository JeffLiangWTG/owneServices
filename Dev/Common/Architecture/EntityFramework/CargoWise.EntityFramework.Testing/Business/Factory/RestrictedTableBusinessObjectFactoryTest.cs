using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class RestrictedTableBusinessObjectFactoryTest : TestCaseWithFactory
	{
		public void TestLoadByPK_RestrictedType_ShouldReportError()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var restrictedFactory = new RestrictedTableBusinessObjectFactory(new[] { GlbStaffSchema.Constants.TableName });
			restrictedFactory.Load(dummy.TablePrefix, dummy.PK);

			AssertEquals("Records from table DummyBizo cannot be loaded with this factory", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			var staffPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GS_PK FROM dbo.GlbStaff");
			restrictedFactory.Load("GS", staffPK);

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestLoadByQuery_RestrictedType_ShouldReportError()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var restrictedFactory = new RestrictedTableBusinessObjectFactory(new[] { GlbStaffSchema.Constants.TableName });
			restrictedFactory.Load<DummyBusinessObject>(new ZQuery());

			AssertEquals("Records from table DummyBizo cannot be loaded with this factory", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLoadByQuery_NonRestrictedType_ShouldNotReportError()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var restrictedFactory = new RestrictedTableBusinessObjectFactory(new[] { DummyBizoSchema.Constants.TableName });

			restrictedFactory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.PK, dummy.PK));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestCreateNewFactory()
		{
			var restrictedFactory = new RestrictedTableBusinessObjectFactory(new[] { GlbStaffSchema.Constants.TableName });
			var newFactory = restrictedFactory.CreateNewFactory();

			AssertType<RestrictedTableBusinessObjectFactory>(newFactory);
			AssertCollectionContains(GlbStaffSchema.Constants.TableName, ((RestrictedTableBusinessObjectFactory)newFactory).AllowedTableNamesToLoad);
		}
	}
}
