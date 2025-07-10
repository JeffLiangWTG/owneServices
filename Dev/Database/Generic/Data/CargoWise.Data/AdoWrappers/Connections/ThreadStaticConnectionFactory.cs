using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;

namespace CargoWise.Data
{
	// A factory for thread-static disposable connections
	//
	// NOTE: This class is not scaleable, and shouldn't be instanced many times due to its use of AsyncLocal.
	// Too many AsyncLocal instances cause performance issues.
	public class ThreadStaticConnectionFactory<Connection> : IDbConnectionProvider<Connection> where Connection : DbConnection
	{
		IDbConnectionFactory<Connection> ConnectionFactory { get; }

		readonly object lockObj = new object();

		// A flag to reduce error spam. It's not supposed to be accurate, just as a simple limiter, hence it being thread static.
		readonly ThreadLocal<bool> alreadyReported = new ThreadLocal<bool>();

		readonly ThreadLocal<Connection> extraConnectionInstanceForThread = new ThreadLocal<Connection>();

		readonly AsyncLocal<int?> initialTaskId = new AsyncLocal<int?>();

		readonly Dictionary<(int ThreadId, int? TaskId), ConnectionDisposer> connectionDictionary = new Dictionary<(int ThreadId, int? TaskId), ConnectionDisposer>();
		public int ConnectionCount
		{
			get
			{
				lock (lockObj)
				{
					return connectionDictionary.Count;
				}
			}
		}

		public ThreadStaticConnectionFactory(IDbConnectionFactory<Connection> factory)
		{
			ConnectionFactory = factory;
		}

		public Connection ProvideConnection()
		{
			var reportUseWithoutDisposableAction = false;
			try
			{
				lock (lockObj)
				{
					var targetThread = Thread.CurrentThread;
					var targetTaskId = GetTargetTaskId(targetThread);

					if (!connectionDictionary.TryGetValue((targetThread.ManagedThreadId, targetTaskId), out var disposer))
					{
						if (targetThread.IsThreadPoolThread && initialTaskId.Value != null && Task.CurrentId == null)
						{
							disposer = new ConnectionDisposer(ConnectionFactory, true);
							connectionDictionary.Add((targetThread.ManagedThreadId, targetTaskId), disposer);
						}
						else
						{
							if (!GetAlreadyReported())
							{
								reportUseWithoutDisposableAction = true;
								SetAlreadyReported(true);
							}
							
							try
							{
								return extraConnectionInstanceForThread.Value ?? (extraConnectionInstanceForThread.Value = ConnectionFactory.SpawnNewConnection());
							}
							catch (ObjectDisposedException)
							{
								return ConnectionFactory.SpawnNewConnection();
							}
						}
					}

					return disposer.DbConnection;
				}
			}
			finally
			{
				if (reportUseWithoutDisposableAction)
				{
					ErrorReporter.ReportDeveloperExceptionOnce(
						ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction,
						new InvalidOperationException(ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction));
				}
			}
		}

		public IDisposable BeginThreadScope()
		{
			return BeginThreadScopeAndGiveDisposer(out _);
		}

		public IDisposable BeginThreadScopeAndGiveDisposer(out ConnectionDisposer disposer)
		{
			disposer = null;
			lock (lockObj)
			{
				if (GetAlreadyReported())
				{
					return null;
				}

				if (Task.CurrentId != null)
				{
					initialTaskId.Value = Task.CurrentId;
				}

				var targetThread = Thread.CurrentThread;
				var isThreadPooled = targetThread.IsThreadPoolThread;
				var targetThreadId = targetThread.ManagedThreadId;
				var targetTaskId = GetTargetTaskId(targetThread);

				if (connectionDictionary.TryGetValue((targetThreadId, targetTaskId), out disposer))
				{
					disposer.Subscribe();
				}
				else
				{
					disposer = new ConnectionDisposer(ConnectionFactory, isAutoCreated: false);
					connectionDictionary.Add((targetThreadId, targetTaskId), disposer);
				}

				return new DisposableAction(() => DisposeExtraConnectionForThread(forceDispose: false, targetThreadId, isThreadPooled, targetTaskId));
			}
		}

		DbConnection IDbConnectionProvider.ProvideConnection()
		{
			return ProvideConnection();
		}

		public Connection SpawnNewConnection()
		{
			return ConnectionFactory.SpawnNewConnection();
		}

		DbConnection IDbConnectionFactory.SpawnNewConnection()
		{
			return ConnectionFactory.SpawnNewConnection();
		}

		int? GetTargetTaskId(Thread currentThread)
		{
			return currentThread.IsThreadPoolThread
				? Task.CurrentId ?? initialTaskId.Value
				: null;
		}

		public void DisposeThreadConnection()
		{
			var targetThread = Thread.CurrentThread;
			var targetTaskId = GetTargetTaskId(targetThread);

			DisposeExtraConnectionForThread(true, targetThread.ManagedThreadId, targetThread.IsThreadPoolThread, targetTaskId);
		}

		void DisposeExtraConnectionForThread(bool forceDispose, int threadId, bool isThreadPooled, int? taskId)
		{
			lock (lockObj)
			{
				if (connectionDictionary.TryGetValue((threadId, taskId), out var disposer))
				{
					if (forceDispose)
					{
						disposer.ClearSubscribers();
					}
					else
					{
						disposer.Unsubscribe();
					}

					if (disposer.Subscribers == 0)
					{
						disposer.Dispose();
						connectionDictionary.Remove((threadId, taskId));

						if (isThreadPooled && initialTaskId.Value != null)
						{
							var autoCreatedDisposableActions = connectionDictionary
								.Where(p => p.Key.TaskId == initialTaskId.Value)
								.Where(pair => pair.Value.IsAutoCreated)
								.ToArray();
							foreach (var pair in autoCreatedDisposableActions)
							{
								pair.Value.Dispose();
								connectionDictionary.Remove(pair.Key);
							}
						}
					}
				}

				if (forceDispose)
				{
					try
					{
						extraConnectionInstanceForThread.Value?.Dispose();
						extraConnectionInstanceForThread.Value = null;
					}
					catch (ObjectDisposedException) { }
				}
			}
		}

		public bool IsDbConnectionDisposerMissing()
		{
			lock (lockObj)
			{
				var targetThread = Thread.CurrentThread;
				var targetThreadId = targetThread.ManagedThreadId;
				var targetTaskId = GetTargetTaskId(targetThread);

				return !connectionDictionary.ContainsKey((targetThreadId, targetTaskId));
			}
		}

		bool GetAlreadyReported(bool defaultValue = false)
		{
			try
			{
				return alreadyReported.Value;
			}
			catch (ObjectDisposedException)
			{
				return defaultValue;
			}
		}

		void SetAlreadyReported(bool value)
		{
			try
			{
				alreadyReported.Value = value;
			}
			catch (ObjectDisposedException) { }
		}

		internal void ResetAlreadyReported()
		{
			SetAlreadyReported(false);
		}

		public class ConnectionDisposer
		{
			public int Subscribers { get; private set; }
			public event Action<Connection> OnDispose;

			IDbConnectionFactory<Connection> ConnectionFactory { get; init; }
			public Connection dbConnection;
			public Connection DbConnection => dbConnection ?? (dbConnection = ConnectionFactory.SpawnNewConnection());

			public bool IsAutoCreated { get; }

			public ConnectionDisposer(IDbConnectionFactory<Connection> factory, bool isAutoCreated)
			{
				ConnectionFactory = factory;
				IsAutoCreated = isAutoCreated;
				Subscribers = 1;
			}

			public void Subscribe()
			{
				Subscribers++;
			}

			public void Unsubscribe()
			{
				if (Subscribers > 0)
				{
					Subscribers--;
				}
			}

			public void ClearSubscribers()
			{
				Subscribers = 0;
			}

			public void Dispose()
			{
				if (dbConnection != null)
				{
					OnDispose?.Invoke(dbConnection);

					using (((Async.ThreadSentry)dbConnection.ThreadSentry).ForcefullyBorrowThreadOwnership())
					{
						dbConnection.Dispose();
					}

					dbConnection = null;
				}
			}
		}
	}

	public static class ThreadStaticConnectionFactory
	{
		public const string AttemptToUseConnectionWithoutDisposableAction = "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()\r\nPlease add this to the highest position in the call stack that this would make sense.\r\nie. the call after a Form.WndProc()"; // Development constant, should not use Res.GetString
	}
}
