using System;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	class MetaTestCaseForTransactionedTestCase : TestCase
	{
		[ExpectNoExceptions]
		public void TestWellBehavedTest()
		{
			new MockWellBehavedTest().RunBare();
		}

		public void TestTransactionAlreadyOpen()
		{
			Db.Connection.BeginTransaction();

			// N.B. ExpectExceptionAttribute doesn't work here, because the exception we expect is an NUnit one.
			try
			{
				new MockWellBehavedTest().RunBare();
				throw new Exception("TransactionedTestCase should have failed this test because a transaction was already open");
			}
			catch (AssertionFailedError e)
			{
				Regex expected = new Regex(Regex.Escape(Html(TransactionedTestCase.TransactionLevelErrorMessages.Initial)));
				AssertMatch(expected, e.Message);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestTransactionLeftOpen()
		{
			// N.B. ExpectExceptionAttribute doesn't work here, because the exception we expect is an NUnit one.
			try
			{
				new MockLeavesDBTransactionOpenTest().RunBare();
				throw new Exception("TransactionedTestCase should have failed this test because it left a transaction open");
			}
			catch (AssertionFailedError e)
			{
				Regex expected = new Regex(Regex.Escape(Html(TransactionedTestCase.TransactionLevelErrorMessages.After)));
				AssertMatch(expected, e.Message);
			}
		}

		public void TestCommandCreatedOutsideTransactionShouldFailWhenUsedInsideTransaction()
		{
			bool caughtException = false;
			try
			{
				new CreatesCommandOutsideTransactionAndRunsCommandInsideTransactionTest().RunBare();
			}
			catch (Exception)
			{
				caughtException = true;
			}

			Assert("Should have thrown an exception - creating a command outside a transaction and trying to run it while its connection is inside a transaction is not allowed.", caughtException);
		}

		public void TestCommandCreatedOutsideTransactionShouldFailWhenUsedInsideTransactionWhenGlobalsIsWeb()
		{
			bool caughtException = false;
			try
			{
				new CreatesCommandOutsideTransactionAndRunsCommandInsideTransactionTestForWebEnvironment().RunBare();
			}
			catch (Exception)
			{
				caughtException = true;
			}

			Assert("Should have thrown an exception - creating a command outside a transaction and trying to run it while its connection is inside a transaction is not allowed.", caughtException);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AssertEquals("Db.Connection.IsInTransaction", false, Db.Connection.IsInTransaction);
			AssertEquals("InTransactionedTestCase", false, TransactionedTestCase.InTransactionedTestCase);
		}

		protected override void TearDown()
		{
			AssertEquals("Db.Connection.IsInTransaction", false, Db.Connection.IsInTransaction);
			AssertEquals("InTransactionedTestCase", false, TransactionedTestCase.InTransactionedTestCase);

			base.TearDown();
		}

		[DoNotAddToTestTree]
		class CreatesCommandOutsideTransactionAndRunsCommandInsideTransactionTest : TransactionedTestCase
		{
			public CreatesCommandOutsideTransactionAndRunsCommandInsideTransactionTest()
			{
				Name = "TestMock";
			}

			public void TestMock()
			{
				DbCommand command = Db.Connection.Command("--");

				Db.Connection.BeginTransaction();
				try
				{
					// The following would throw an exception in release code.
					// We want to make sure it also throws an exception in a TransactionedTestCase.
					command.ExecuteNonQuery();
					Assert(true);
				}
				finally
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}

		[DoNotAddToTestTree]
		class CreatesCommandOutsideTransactionAndRunsCommandInsideTransactionTestForWebEnvironment : TransactionedTestCase
		{
			public CreatesCommandOutsideTransactionAndRunsCommandInsideTransactionTestForWebEnvironment()
			{
				Name = "TestMock";
			}

			public void TestMock()
			{
				using (Globals.SetIsWebForTest(true))
				{
					DbCommand command = Db.Connection.Command("--");

					Db.Connection.BeginTransaction();
					try
					{
						// The following would throw an exception in release code.
						// We want to make sure it also throws an exception in a TransactionedTestCase.
						command.ExecuteNonQuery();
						Assert(true);
					}
					finally
					{
						Db.Connection.RollbackTransaction();
					}
				}
			}
		}

		[DoNotAddToTestTree]
		protected class MockWellBehavedTest : TransactionedTestCase
		{
			public MockWellBehavedTest()
			{
				Name = "TestMock";
			}

			public virtual void TestMock()
			{
				AssertEquals("InTransactionedTestCase", true, InTransactionedTestCase);
			}
		}

		[DoNotAddToTestTree]
		protected class MockLeavesDBTransactionOpenTest : MockWellBehavedTest
		{
			public override void TestMock()
			{
				base.TestMock();
				Db.Connection.BeginTransaction();
			}
		}
	}
}
