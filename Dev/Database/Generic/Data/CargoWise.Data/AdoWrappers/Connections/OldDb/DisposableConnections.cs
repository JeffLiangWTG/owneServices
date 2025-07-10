using System;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	//
	// This is part of the old Db class, responsible for a thread-static based thread pool,
	// mainly for spawning and ensuring the persistence of database connections.
	//
	// In the future, this should be abstracted away and/or deleted to provide a much more rigorous
	// public API for managing new database connections.
	//
	// Static dependencies from Db:
	// - NewExtraConnectionToMainDb()
	// - ShouldUseMainConnection
	//

	public partial class Db
	{
		[ThreadSafe]
		static readonly ThreadStaticConnectionFactory<DbConnection> extraConnectionFactory = new ThreadStaticConnectionFactory<DbConnection>(
			new ConnectionFactoryAction<DbConnection>(() => Db.NewExtraConnectionToMainDb())
		);

		static DbConnection GetDisposableExtraConnection()
		{
			return extraConnectionFactory.ProvideConnection();
		}

		public static IDisposable DisposableActionForDbConnection()
		{
			if (Instance.ShouldUseMainConnection)
			{
				return null;
			}

			return extraConnectionFactory.BeginThreadScope();
		}

		public static void DisposeThreadConnection()
		{
			extraConnectionFactory.DisposeThreadConnection();
		}

		public static bool IsDbConnectionDisposerMissing()
		{
			if (Instance.ShouldUseMainConnection)
			{
				return false;
			}

			return extraConnectionFactory.IsDbConnectionDisposerMissing();
		}
	}

#if DEBUG

	public partial class Db
	{
		public static void ResetAlreadyReported_ForTest() => extraConnectionFactory.ResetAlreadyReported();
	}
#endif
}
