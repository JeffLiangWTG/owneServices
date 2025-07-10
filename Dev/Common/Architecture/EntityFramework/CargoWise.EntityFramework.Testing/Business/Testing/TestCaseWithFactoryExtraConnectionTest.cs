using System;
using CargoWise.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TestCaseWithFactoryExtraConnectionTest : TestCaseWithFactoryBaseTest
	{
		DbConnection extraConnection;

		protected override void SetUp()
		{
			base.SetUp();
			extraConnection = Db.NewExtraConnectionToMainDb();
		}

		protected override void TearDown()
		{
			extraConnection.Dispose();
			base.TearDown();
		}

		protected override Type ExpectedConnectionType
		{
			get { return typeof(DbConnection); }
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory(extraConnection);
		}
	}
}
