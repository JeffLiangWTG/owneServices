using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	abstract class MainDatabaseTestListenerTest : TestCase
	{
		protected abstract Action CodeToRun { get; }

		protected override void SetUp()
		{
			base.SetUp();
			mainDatabaseTestListener = new MainDatabaseTestListener();
			currentDbControl = Db.Connection;
		}

		public void TestFailsOnWrongDatabase()
		{
			// Arrange
			using (currentDbControl.UseDatabase(Db.SqlMasterDb))
			{
				// Act
				// Assert
				_ = AssertExceptionThrown<AssertionFailedError>(() => CodeToRun());
			}
		}

		public void TestPassesOnCorrectDatabase()
		{
			// Arrange
			using (currentDbControl.UseDatabase(Db.DatabaseName))
			{
				// Act
				// Assert
				AssertNoExceptionThrown(() => CodeToRun());
			}
		}

		ICurrentDbControl currentDbControl;
		MainDatabaseTestListener mainDatabaseTestListener;

		public class BeforeEachTest : MainDatabaseTestListenerTest
		{
			protected override Action CodeToRun => () => mainDatabaseTestListener.BeforeEachTest(DateTime.Now);
		}

		public class AfterEachTest : MainDatabaseTestListenerTest
		{
			protected override Action CodeToRun => () => mainDatabaseTestListener.AfterEachTest(DateTime.Now);
		}
	}
}
