using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class AuxiliaryDbCreatorTest : TestCase
	{
		public void TestConnectionInheritsDefaultCommandTimeOutInSeconds()
		{
			var initialDefaultCommandTimeOut = Db.Connection.DefaultCommandTimeOutInSeconds;
			Db.Connection.DefaultCommandTimeOutInSeconds = 23;
			var actualTimeout = 0;
			IAuxiliaryDbCreator creator = new AuxiliaryDbCreatorForTest(conn => actualTimeout = conn.DefaultCommandTimeOutInSeconds);

			try
			{
				creator.CreateDropExisting();
			}
			finally
			{
				creator.Drop();
				Db.Connection.DefaultCommandTimeOutInSeconds = initialDefaultCommandTimeOut;
			}

			AssertEquals(23, actualTimeout);
		}

		class AuxiliaryDbCreatorForTest : AuxiliaryDbCreator
		{
			readonly Action<DbConnection> postCreationAction;

			public AuxiliaryDbCreatorForTest(Action<DbConnection> postCreationAction)
				: base("TestDBAuxiliaryDbCreatorTest")
			{
				this.postCreationAction = postCreationAction;
			}

			protected override void SetupDatabaseAfterCreation(DbConnection conn)
			{
				postCreationAction?.Invoke(conn);
			}
		}
	}
}
