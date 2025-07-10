using System;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.PAVE.Common.Cache;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using SharedDistributedCache = CargoWise.PAVE.Common.Cache.DistributedCache;

namespace Enterprise.BufferManagement.Service.Cache
{
	public class DistributedCache : IDistributedCache
	{
		readonly SharedDistributedCache distributedCache;

		public DistributedCache(ISerializer serializer)
		{
			var sql = new SqlDistributedCache(new SystemClock(), new DBConnectionFactory());
			distributedCache = new SharedDistributedCache(sql, serializer);
		}

		public DistributedCache(JsonSerializerOptions jsonSerializeOptions)
		{
			var sql = new SqlDistributedCache(new SystemClock(), new DBConnectionFactory());
			distributedCache = new SharedDistributedCache(sql, jsonSerializeOptions);
		}

		public DistributedCache(IDBConnectionFactory dbConnectionFactory)
		{
			var sql = new SqlDistributedCache(new SystemClock(), dbConnectionFactory);
			distributedCache = new SharedDistributedCache(sql, new CargoWise.PAVE.Common.Cache.JsonSerializer(null));
		}

		internal DistributedCache(SqlDistributedCache sqlDistributedCache)
		{
			distributedCache = new SharedDistributedCache(sqlDistributedCache, new CargoWise.PAVE.Common.Cache.JsonSerializer(null));
		}

		public T Get<T>(string key) => Try(() => distributedCache.Get<T>(key), nameof(Get));

		public void Remove(string key) => Try(() => distributedCache.Remove(key), nameof(Remove));

		public void Set<T>(string key, T value, DistributedCacheOptions options, string userCode = null) => Try(() => distributedCache.Set(key, value, options, userCode ?? StaticCurrentFetcher.Instance.CurrentUserCode), nameof(Set));

		public T GetOrSet<T>(string key, Func<T> getValueFunc, DistributedCacheOptions options, string userCode = "E") => Try(() => distributedCache.GetOrSet(key, getValueFunc, options, userCode), nameof(GetOrSet));

		static void Try(Action func, string name)
		{
			_ = Try<object>(() =>
			{
				func();
				return null;
			}, name);
		}

		static T Try<T>(Func<T> func, string name)
		{
			T result = default;

			try
			{
				result = func();
			}
			catch (SqlException sqlException)
			{
				if (ZExceptionExtensions.IsInfrastructureDbError(sqlException) ||
					sqlException.IsSpecifiedError(DbErrorType.LockTimeoutExpired) ||
					sqlException.IsSpecifiedError(DbErrorType.TimeoutExpired))
				{
					//Oh NO!!. We're probably already closing the application and/or reporting exceptions elsewhere.
				}
				else
				{
					ErrorReporter.ReportOnce("1E87B641-C2E3-47CD-A3CC-B185E2687564", "SqlError to execute Cache operation:" + name, sqlException);
				}
			}
			catch (InvalidOperationException invalidOperationException)
			{
				if (invalidOperationException.Message != null && invalidOperationException.Message.Contains((NoResString)"The timeout period elapsed prior to obtaining a connection from the pool"))
				{
					//Ops!!. Sql can't handle any more connections :(, so probably the application are reporting exceptions elsewhere.
				}
				else
				{
					ErrorReporter.ReportOnce("1E87B641-C2E3-47CD-A3CC-B185E2687564", "Invalid Operation to execute Cache operation:" + name, invalidOperationException);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("D331325C-F324-4519-9F35-676C2893F4E7", "Error to execute Cache operation:" + name, ex);
			}

			return result;
		}
	}
}
