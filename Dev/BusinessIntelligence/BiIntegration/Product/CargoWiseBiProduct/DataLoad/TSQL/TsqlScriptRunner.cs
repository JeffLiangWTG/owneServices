using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Maintenance;
using CargoWise.Bi.Product.DataLoad.Helper;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.DataLoad
{
	public abstract class TsqlScriptRunner
	{
		protected TsqlScriptRunner(DbConnection biConnection, ILogger logger)
		{
			this.logger = logger;
			this.biConnection = biConnection;
		}
		protected readonly ILogger logger;
		protected readonly DbConnection biConnection;

		public void Run()
		{
			bool retry = false;
			try
			{
				RunUnsafe();
			}
			catch (Exception ex) when (IsLinkedServerCreationRequired(ex))
			{
				logger.Log(LogType.Debug, $"Linked server creation required: " + ex.Message);
				retry = CreateLinkedServerAndRetry();
			}
			catch (SqlException ex) when (InsufficientSystemMemoryException(ex))
			{
				if (!EnvProxy.IsHostedWithCargowise)
				{
					throw new HostedServiceException(ex.Message, ex) { LogException = true };
				}
				else
				{
					throw;
				}
			}

			if (retry)
			{
				RunUnsafe();
			}
		}

		public bool CreateLinkedServerAndRetry()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			if (registration.IsWiseTechGlobalInternalEDISystem() || !registration.IsWiseTechGlobalInternalSystem())
			{
				using (var biAdminConnection = Db.NewAdminConnection(biConnection.ServerName, Db.SqlMasterDb))
				{
					logger.Log(LogType.Debug, $"Creating linked server [{LinkedServerName}]");
					var creator = new LinkedServerCreator(biAdminConnection, LinkedServerName, logger);
					creator.CreateLinkedServer();
				}
				return true;
			}
			else
			{
				logger.Log(LogType.Debug, $"Skip linked server creation for WTG internal systems");
				return false; // Do not retry for WTG inernal system
			}
		}

		public bool InsufficientSystemMemoryException(SqlException ex)
		{
			var exceptionType = new DbErrorMatch(ex).ExceptionType;
			return exceptionType == DbErrorType.InsufficientSystemMemoryToRunQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging message")]
		public bool IsLinkedServerCreationRequired(Exception ex)
		{
			var exceptionType = DbErrorType.NotInitialised;
			if (ex is EtlExecutionException etlException)
			{
				exceptionType = etlException.ExceptionType;
			}
			else if (ex is SqlException sqlException)
			{
				exceptionType = new DbErrorHandler(sqlException, biConnection).ExceptionType;
			}

			if (exceptionType == DbErrorType.CouldNotFindLinkedServer
				|| exceptionType == DbErrorType.CannotInitOleDbDataSourceObjForLinkedServer
				|| exceptionType == DbErrorType.NoLoginMappingExistsOnRemoteServer
				|| exceptionType == DbErrorType.ServerIsNotConfigured)
			{
				return true;
			}

			if (exceptionType != DbErrorType.GeneralNetworkError)
			{
				return false;
			}

			var serverInfo = LinkedServerCreator.GetLinkedServerDataSource(LinkedServerName);
			if (serverInfo is null)
			{
				return true;
			}

			string port;
			if (!LinkedServerCreator.TryParseServerPortNumber(serverInfo, out port) || !port.Equals(LinkedServerCreator.GetMainDBServerPortNumber()))
			{
				logger.Warning("Linked server recreation required");
				return true;
			}

			return false;
		}

		public abstract bool IsInitialLoad();

		public abstract bool ShouldRunEtl();

		protected virtual string LinkedServerName
		{
			get { return Db.ServerName; }
		}

		/// <summary>
		/// To be removed once relevant stored procedures use brackets around the linked server name
		/// </summary>
		public string LinkedServerNameWithinBrackets
		{
			get { return "[" + LinkedServerName + "]"; }
		}

		protected abstract IEnumerable<string> GetErrorList();

		/// <summary>
		/// Execute until it succeeds, up to 5 times.
		/// </summary>
		protected T ExecuteEtlStepWithRetry<T>(Func<T> executeStep, Func<T, bool> shouldRetry)
		{
			using (new SqlMessagePassthroughLogger(biConnection, logger))
			{
				return ExecuteEtlStepWithRetryRecursive(executeStep, shouldRetry, 0);
			}
		}

		T ExecuteEtlStepWithRetryRecursive<T>(Func<T> executeStep, Func<T, bool> shouldRetry, int retryCount = 0)
		{
			var result = executeStep();

			return (shouldRetry(result) && retryCount < 5)
				? ExecuteEtlStepWithRetryRecursive(executeStep, shouldRetry, retryCount + 1)
				: result;
		}

		public abstract string ScriptName { get; }
		public abstract string BiDatabaseName { get; }
		protected abstract void RunUnsafe();
		public string BiDatabaseType => BiDatabaseName?.Replace(Db.DatabaseName, "");

		public class EtlError
		{
			public EtlError(string tableName, string errorMsg, ZDateTime? utcTime)
			{
				this.TableName = tableName;
				this.ErrorMessage = errorMsg;
				this.UtcTime = utcTime;
			}

			public string TableName { get; private set; }
			public string ErrorMessage { get; private set; }
			public ZDateTime? UtcTime { get; private set; }

			public override string ToString()
			{
				string errorDateTime = "";
				if (UtcTime != null)
				{
					errorDateTime = new ZDateTime(Env.Time.GetLocalTimeFromUtc(UtcTime.Value.ToDateTime())).ToBestReadableDateTimeString();
				}
				return string.Format(CultureInfo.InvariantCulture, "[{0}] - {1}{2}", TableName, (!string.IsNullOrEmpty(errorDateTime) ? $"{errorDateTime} : " : ""), ErrorMessage); // Date time logging
			}
		}
	}
}
