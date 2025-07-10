using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public static class DbHelper
	{
		public static DbConnection NewConnection()
		{
#if DEBUG

			if (Globals.IsTest)
			{
				if (ExceptionToThrowInTest != null)
				{
					throw ExceptionToThrowInTest;
				}

				if (DbConnectionForTest != null)
				{
					return DbConnectionForTest;
				}

				if (NumberOfConnectionsForTest > -1)
				{
					NumberOfConnectionsForTest++;
				}
			}
#endif

			var result = Db.NewExtraConnectionToMainDb();
			try
			{
				result.EnsureIsOpen();
				return result;
			}
			catch (Exception ex)
			{
				result.Dispose();
				throw RemotePrintingDbConnectionException.New(ex);
			}
		}

#if DEBUG
		public static IDisposable StartConnectionCounter()
		{
			NumberOfConnectionsForTest = 0;
			return new DisposableAction(CleanUpConnectionCounter);
		}

		static void CleanUpConnectionCounter() => NumberOfConnectionsForTest = -1;

		public static int NumberOfConnectionsForTest { get; private set; }

		static Exception ExceptionToThrowInTest { get; set; }

		public static IDisposable SetExceptionToThrowInTest(Exception ex)
		{
			if (ExceptionToThrowInTest != null)
			{
				throw new ApplicationException("Test exception is already set.");
			}
			ExceptionToThrowInTest = ex;
			return new DisposableAction(() => ExceptionToThrowInTest = null);
		}

		static DbConnection DbConnectionForTest { get; set; }

		public static IDisposable OverrideConnectionForTest(DbConnection connection)
		{
			if (DbConnectionForTest != null)
			{
				throw new ApplicationException("Db connection for test is already overridden.");
			}
			DbConnectionForTest = connection;
			return new DisposableAction(() => DbConnectionForTest = null);
		}

		#endif
	}
}
