using System;
using CargoWise.Common;

namespace Enterprise.Semaphores.Common
{
	class SemaphoreHandle : ISemaphoreHandle, ISemaphoreDisposal
	{
		SemaphoreHandle(Guid heartbeatUniqueId, ISemaphoreType semaphore, bool loadExisting, int attemptsToHandleDeadlocks)
		{
			Argument.NotNull(semaphore, nameof(semaphore));
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			if (!loadExisting)
			{
				InitializeSemaphoreHandle(heartbeatUniqueId, semaphore, attemptsToHandleDeadlocks);
			}
			else
			{
				LoadSemaphoreHandle(heartbeatUniqueId, semaphore);
			}
		}

		SemaphoreHandle(Guid heartbeatUniqueId, ISemaphoreType semaphore, int attemptsToHandleDeadlocks)
			: this(heartbeatUniqueId, semaphore, false, attemptsToHandleDeadlocks)
		{
			Argument.NotNull(semaphore, nameof(semaphore));
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}
		}

		internal static SemaphoreHandle New(Guid heartbeatUniqueId, ISemaphoreType semaphore, int attemptsToHandleDeadlocks)
		{
			Argument.NotNull(semaphore, nameof(semaphore));
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			return new SemaphoreHandle(heartbeatUniqueId, semaphore, attemptsToHandleDeadlocks);
		}

		internal static SemaphoreHandle Load(Guid heartbeatUniqueId, ISemaphoreType semaphore, int attemptsToHandleDeadlocks)
		{
			Argument.NotNull(semaphore, nameof(semaphore));
			if (heartbeatUniqueId == Guid.Empty)
			{
				throw new ArgumentException("Invalid argument.", nameof(heartbeatUniqueId));
			}

			return new SemaphoreHandle(heartbeatUniqueId, semaphore, true, attemptsToHandleDeadlocks);
		}

		#region SemaphoreHandle Initialization

		void InitializeSemaphoreHandle(Guid heartbeatUniqueId, ISemaphoreType semaphore, int attemptsToHandleDeadlocks)
		{
			Argument.NotNull(semaphore, nameof(semaphore));

			try
			{
				this.handleUniqueId = CreateSemaphoreHandleWithAttemptToHandleDeadlocks(heartbeatUniqueId, semaphore, attemptsToHandleDeadlocks);
				this.success = true;
			}
			catch (SqlException ex)
			{
				this.exception = GetFormattedException(ex, heartbeatUniqueId);
				this.success = false;
			}

			this.semaphoreType = semaphore;
		}

		void LoadSemaphoreHandle(Guid heartbeatUniqueId, ISemaphoreType semaphore)
		{
			Argument.NotNull(semaphore, nameof(semaphore));

			handleUniqueId = dbManager.LoadSemaphoreHandle(heartbeatUniqueId, semaphore.LockInfo, semaphore.Category);
			if (handleUniqueId != Guid.Empty)
			{
				success = true;
				semaphoreType = semaphore;
			}
		}

		Guid CreateSemaphoreHandleWithAttemptToHandleDeadlocks(Guid heartbeatUniqueId, ISemaphoreType semaphore, int attemptCount)
		{
			Argument.NotNull(semaphore, nameof(semaphore));

			Guid handleId = Guid.Empty;

			try
			{
				handleId = dbManager.CreateSemaphoreHandleInTransaction(heartbeatUniqueId, semaphore.LockInfo, semaphore.Category, semaphore.MaxConcurrentHandles);
			}
			catch (SqlException ex)
			{
				if (SemaphoreHandleException.NewException(ex) is SemaphoreTransactionDeadlockException && attemptCount > 0)
				{
					handleId = CreateSemaphoreHandleWithAttemptToHandleDeadlocks(heartbeatUniqueId, semaphore, --attemptCount);
				}
				else
				{
					throw;
				}
			}

			return handleId;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		Exception GetFormattedException(SqlException ex, Guid heartbeatUniqueId)
		{
			Argument.NotNull(ex, nameof(ex));

			Exception result = SemaphoreHandleException.NewException(ex);

			if (result is SemaphoreInvalidSessionIdException)
			{
				string message = String.Format(
					"{0}\r\nExpected Session ID: [{1}]",
					result.Message, heartbeatUniqueId.ToString());

				result = new Exception(message, result);
			}

			return result;
		}

		#endregion

		#region ISemaphoreHandle Members

		ISemaphoreType ISemaphoreHandle.Semaphore
		{
			get { return semaphoreType; }
		}

		ISemaphoreType semaphoreType;

		bool ISemaphoreHandle.Success
		{
			get { return success; }
		}

		bool success;

		Exception ISemaphoreHandle.CreateException
		{
			get { return exception; }
		}

		Exception exception;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);

			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		~SemaphoreHandle()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					CloseHandleIfActive();
					onDisposed?.Invoke(this);
				}

				disposed = true;
			}
		}

		bool disposed;

		void CloseHandleIfActive()
		{
			try
			{
				if (success && handleUniqueId != Guid.Empty)
				{
					dbManager.DeleteSemaphoreHandleFromDatabase(handleUniqueId);
				}
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("Error occurred while deleting semaphore handle from database.", ex);
			}
		}

		#endregion

		#region ISemaphoreDisposal Members

		OnSemaphoreDisposedDelegate ISemaphoreDisposal.OnSemaphoreDisposed
		{
			set
			{
				if (onDisposed != null)
				{
					throw new InvalidOperationException("Semaphore on disposed cannot be ovewriten once already set.");
				}

				onDisposed = value;
			}
		}

		OnSemaphoreDisposedDelegate onDisposed;

		#endregion

		readonly SemaphoreDbManager dbManager = new SemaphoreDbManager();
		Guid handleUniqueId;
	}
}
