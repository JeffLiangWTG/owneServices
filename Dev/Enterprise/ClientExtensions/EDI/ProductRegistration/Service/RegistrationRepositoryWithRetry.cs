using System.Threading.Tasks;
using CargoWise.Licensing;
using Enterprise.ProductRegistration.Common;

namespace CargoWise.ProductRegistration.Service
{
	public sealed class RegistrationRepositoryWithRetry : IRegistrationRepository
	{
		public RegistrationRepositoryWithRetry(IRegistrationRepository repository)
		{
			this.repository = repository;
		}

		readonly IRegistrationRepository repository;

		public async Task<DbStatus> GetStatus(string productKey)
		{
			int tryCount = 2;

			do
			{
				try
				{
					return await repository.GetStatus(productKey);
				}
				catch (System.Data.Common.DbException ex)
				{
					if (--tryCount <= 0 || !HandleException(ex))
					{
						throw;
					}
				}
#if DEBUG
#else
				await Task.Delay(50);
#endif
			} while (true);
		}

		public async Task<DbRegisterResult> Register(RegisterRequest request, string passwordHash)
		{
			int tryCount = 2;

			do
			{
				try
				{
					return await repository.Register(request, passwordHash);
				}
				catch (System.Data.Common.DbException ex)
				{
					if (--tryCount <= 0 || !HandleException(ex))
					{
						throw;
					}
				}
#if DEBUG
#else
				await Task.Delay(50);
#endif
			} while (true);
		}

		public async Task<DbRegisterResult> Verify(int databaseNumber, string passwordHash, VerifyRequest request)
		{
			int tryCount = 2;

			do
			{
				try
				{
					return await repository.Verify(databaseNumber, passwordHash, request);
				}
				catch (System.Data.Common.DbException ex)
				{
					if (--tryCount <= 0 || !HandleException(ex))
					{
						throw;
					}
				}
#if DEBUG
#else
				await Task.Delay(50);
#endif
			} while (true);
		}

		public async Task<int> Unregister(int databaseNumber, string passwordHash)
		{
			int tryCount = 2;

			do
			{
				try
				{
					return await repository.Unregister(databaseNumber, passwordHash);
				}
				catch (System.Data.Common.DbException ex)
				{
					if (--tryCount <= 0 || !HandleException(ex))
					{
						throw;
					}
				}
#if DEBUG
#else
				await Task.Delay(50);
#endif
			} while (true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed to identify SqlClient types")]
		bool HandleException(System.Data.Common.DbException ex)
		{
			int err = 0;
			if (ex is System.Data.SqlClient.SqlException sqlExceptionSys)
			{
				err = sqlExceptionSys.Number;
			}
#if NET
			else if (ex is Microsoft.Data.SqlClient.SqlException sqlExceptionMS)
			{
				err = sqlExceptionMS.Number;
			}
#endif

			if (err == 18470 // Login failed for user. Reason: The account is disabled
				|| err == 18401 // Login failed for user. Reason: Server is in script upgrade mode. Only administrator can connect at this time.
				|| err == 18456 // Login failed for user
				|| err == 615  // Could not find database ID %d, name '%.*ls'. The database may be offline. Wait a few minutes and try again.
				|| err == 911  // Database '%.*ls' does not exist. Make sure that the name is entered correctly.
				|| err == 913  // Could not find database ID %d. Database may not be activated yet or may be in transition.
				|| err == 916  // The server principal "%.*ls" is not able to access the database "%.*ls" under the current security context.
				|| err == 922  // Database '%.*ls' is being recovered. Waiting until recovery is finished.
				|| err == 924  // Database '%.*ls' is already open and can only have one user at a time.
				|| err == 926  // Database '%.*ls' cannot be opened. It has been marked SUSPECT by recovery. See the SQL Server errorlog for more information.
				|| err == 927  // Database '%.*ls' cannot be opened. It is in the middle of a restore.
				|| err == 942 // Database '%.*ls' cannot be opened because it is offline.
				|| err == 952 // Database '%.*ls' is in transition. Try the statement later.
				|| err == 983  // Unable to access database '%.*ls' because its replica role is RESOLVING which does not allow connections. Try the operation again later.
				|| err == 1205 // Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim.
				)
			{
				Connection.Reset();
				return true;
			}

			return false;
		}

		public void Dispose()
		{
			repository.Dispose();
		}

		public async Task<DatabaseUniqueKey> GetDatabaseUniqueKey(int databaseNumber)
		{
			try
			{
				return await repository.GetDatabaseUniqueKey(databaseNumber);
			}
			catch (System.Data.Common.DbException)
			{
			}
			return null;
		}

		public IConnectionManager Connection
		{
			get
			{
				return repository.Connection;
			}
		}
	}
}
