using System;

namespace CargoWise.Data
{
	public interface IDbConnectionFactory
	{
		/// <summary>
		/// Return a connection created by the factory, but owned by the caller. It's the caller's responsibility
		/// to manage the lifetime of this connection.
		/// </summary>
		/// <returns>A new connection, for the caller to manage.</returns>
		DbConnection SpawnNewConnection();
	}

	public interface IDbConnectionFactory<Connection> : IDbConnectionFactory where Connection : DbConnection
	{
		new Connection SpawnNewConnection();
	}

	class ConnectionFactoryAction<Connection> : IDbConnectionFactory<Connection> where Connection : DbConnection
	{
		readonly Func<Connection> SpawnConnection;

		public ConnectionFactoryAction(Func<Connection> spawnConnection)
		{
			SpawnConnection = spawnConnection;
		}

		public Connection SpawnNewConnection()
		{
			return SpawnConnection();
		}

		DbConnection IDbConnectionFactory.SpawnNewConnection()
		{
			return SpawnConnection();
		}
	}
}

