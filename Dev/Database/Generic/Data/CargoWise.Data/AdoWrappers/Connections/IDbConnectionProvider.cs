using System;

namespace CargoWise.Data
{
	public interface IDbConnectionProvider : IDbConnectionFactory
	{
		/// <summary>
		/// Notify the connection provider that we are beginning a thread/task scope.
		///
		/// This may let the connection provider decide to allow new connections to be spawned, which
		/// will automatically be disposed when the thread/task scope ends.
		///
		/// This is for compatibility with DisposableActionForDbConnection in the old code.
		/// </summary>
		/// <returns>A disposable for ending the thread/task scope.</returns>
		IDisposable BeginThreadScope();

		/// <summary>
		/// Return a connection for the current environment, owned by the provider. It's the provider's responsibility
		/// to manage the lifetime of this connection.
		/// </summary>
		/// <returns>The current connection, as decided by the provider.</returns>
		DbConnection ProvideConnection();
	}

	public interface IDbConnectionProvider<Connection> : IDbConnectionProvider, IDbConnectionFactory<Connection> where Connection : DbConnection
	{
		new Connection ProvideConnection();
	}
}

