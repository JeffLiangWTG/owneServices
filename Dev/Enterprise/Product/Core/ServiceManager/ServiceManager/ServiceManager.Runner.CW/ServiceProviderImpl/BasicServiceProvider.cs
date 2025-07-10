using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Runner
{
	public abstract class BasicServiceProvider : IServiceTaskHandler
	{
		readonly IWebRequestDefaultProxyWrapper proxyWrapper;

		internal const string GlbStaffTypeName = "Enterprise.MasterFiles.Business.GlbStaff, Enterprise.MasterFiles.Business";

		#region Constructors

		protected internal BasicServiceProvider(
			BusinessObjectFactory? factory,
			IWebRequestDefaultProxyWrapper proxyWrapper,
			ILoggerFactory loggerFactory,
			IServiceTaskLoader taskLoader)
		{
			this.factory = factory;
			this.proxyWrapper = proxyWrapper;
			this.taskLoader = taskLoader;
			LoggerFactory = loggerFactory;
		}

		#endregion

		protected abstract void RunTask(CancellationToken cancellationToken);

		#region IServiceTaskHandler Members

		void InitializeCommon(string dbServer, string dbName)
		{
			Db.InitializeDatabaseDetails(dbServer, dbName);
			DbEnv.Instance.DeadlockPriority = (int)DeadlockPriority.Low;

			if (DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				BrandingFactory.Configure(BrandingFactory.BrandingType.ProductivityWise);
			}
			else
			{
				BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseNext);
			}

			if (SystemDataRegistry.Instance.ShowQueryStackTraceInProcessControllerEnabled.Value)
			{
				QueryStackTraceRecorder.Instance.Enabled = true;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		void InitializeProcessRunningEnvironment()
		{
			SetUserContextOnceForTheWholeProcess();

			InitializeProxyForTheWholeProcessOncePerProcessLifetime();

			void SetUserContextOnceForTheWholeProcess()
			{
				if (Env.CurrentUser == null || !DefaultBranchExists())
				{
					SetUserContext();
				}

				bool DefaultBranchExists()
				{
					var branchCheckSql = $@"SELECT COUNT(*)
FROM dbo.GlbBranch AS branch
LEFT JOIN dbo.GlbCompany AS company
 ON (branch.GB_GC = company.GC_PK)
WHERE branch.GB_IsActive = 1
 AND branch.GB_PK = @taskBranchPK
 AND company.GC_IsActive = 1";

					var activeBranchPkCount = Db.Connection.ExecuteScalar<int>(branchCheckSql, cmd =>
					{
						cmd.AddParameter("@taskBranchPK", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
					});

					return activeBranchPkCount > 0;
				}

				void SetUserContext()
				{
					var user = (IUser)Factory.LoadFromNaturalKey(Type.GetType(GlbStaffTypeName, throwOnError: true), GlbStaffSchema.GS_LoginName, User.ServiceUserName);

					Env.SetUserContext(new UserContext(user, GetBranchPk(), GetDepartmentPk()));
					HostLogger!.Log(LogLevel.Debug, $"Environment initialised: user [{Env.CurrentUser?.LoginName ?? "(null)"}], branch [{Env.CurrentBranch.Code}], department [{Env.CurrentDepartment.Code}].");

					var clientHook = ClientHookLoader.Instance.ClientHook;
					if (clientHook != null && !clientHook.IsInitialised)
					{
						clientHook.Initialise(true);
					}
				}
			}

			void InitializeProxyForTheWholeProcessOncePerProcessLifetime()
			{
				if (proxyWrapper.DefaultWebProxy == null)
				{
					var hostname = ServiceManagerHelper.GetHostName();
					var host = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hostname));
					if (host != null && !host.SH_ProxyAutoDetect)
					{
						HostLogger!.Log(LogLevel.Debug, $"Proxy autodetect for {hostname} is not set. Proxy settings will be set based on host configuration.");
						proxyWrapper.SetDefaultProxyToHostProxy(host);
					}
					else
					{
						if (host == null)
						{
							HostLogger!.Log(LogLevel.Warning, $"Unable to find host record with hostname {hostname} to retrieve proxy settings. Defaulting to system proxy settings.");
						}
						else
						{
							HostLogger!.Log(LogLevel.Debug, $"Proxy autodetect for {hostname} is set. Defaulting to system proxy settings.");
						}

						proxyWrapper.SetDefaultProxyToSystemProxy();
					}
				}
			}
		}

		public void InitializeRunningEnvironment(IHostedServiceAttribute hostedServiceAttribute, string taskConfigString, Microsoft.Extensions.Logging.ILogger logger)
		{
			HostLogger = logger ?? throw new ArgumentNullException(nameof(logger));
			HostedServiceAttribute = hostedServiceAttribute ?? throw new ArgumentNullException(nameof(hostedServiceAttribute));
			Logger = LoggerFactory.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, hostedServiceAttribute.Code);

			InitializeCommon(Db.ServerName, Db.DatabaseName);
			InitializeProcessRunningEnvironment();

			var configUser = GetServiceTaskConfigurationUser(this);
			if (configUser != null)
			{
				configUser.ConfigString = taskConfigString;
			}
		}

		static IServiceTaskConfigurationUser? GetServiceTaskConfigurationUser(IServiceTaskHandler service)
		{
			switch (service)
			{
				case IServiceTaskConfigurationUser serviceTaskConfigurationUser:
					return serviceTaskConfigurationUser;

				case IServiceTaskConfigurationUserProvider serviceTaskConfigurationUserProvider:
					return serviceTaskConfigurationUserProvider.GetServiceTaskConfigurationUser();

				default:
					return null;
			}
		}

		void IServiceTaskHandler.Run(CancellationToken cancellationToken)
		{
			BackgroundAppDomainWorker.Error += BackgroundAppDomainWorker_Error;
			try
			{
				var task = GetTaskSchedule()
					?? throw new HostedServiceException("Attempt to run a task without initialization");
				CheckRunningEnvironment(task);

				using (TemporaryContext(task))
				using (Env.Instance.TemporaryServiceTaskContext(HostedServiceAttribute!.Code, canRunInAnyBranch: false))
				{
					RunTask(cancellationToken);
				}

				if (backgroundAppDomainWorkerExceptions.Count > 0)
				{
					throw new HostedServiceException("Exception in BackgroundAppDomainWorker", backgroundAppDomainWorkerExceptions[0]) { LogException = false };
				}
			}
			finally
			{
				BackgroundAppDomainWorker.Error -= BackgroundAppDomainWorker_Error;
			}

			IDisposable? TemporaryContext(IServiceTask task)
			{
				var branchPK = FindSuitableBranch(task);
				var context = branchPK == Guid.Empty || branchPK == Env.CurrentBranchPK
					? null
					: Env.Instance.SetTemporaryMasterUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartmentPK);

				HostLogger!.Log(LogLevel.Debug, $"Using environment: user [{Env.CurrentUser?.LoginName ?? "(null)"}], branch [{Env.CurrentBranch.Code}], department [{Env.CurrentDepartment.Code}].");
				return context;
			}

			void CheckRunningEnvironment(IServiceTask task)
			{
				if (Env.CurrentUser == null || proxyWrapper.DefaultWebProxy == null)
				{
					throw new HostedServiceException("Attempt to run a task without pre-initialization");
				}

				task.PreRunValidation();
				var branchError = task.BranchErrorMessage;
				if (!string.IsNullOrEmpty(branchError))
				{
					Logger!.Log(LogType.Error, $"There are validation errors on specified Branch in service task: {branchError}");
				}
			}
		}

		public void HandleException(Exception exception, string serviceTaskCode)
		{
			if (exception.FlattenInnerExceptions().Any(x => x is DatabaseUpgradeException))
			{
				try
				{
					Logger!.Log(LogType.Warning, "Service task was interrupted due to a database upgrade.");
				}
				catch
				{
					Console.Error.WriteLine("Service task was interrupted due to a database upgrade.");
				}
			}
			else if (exception.IsCriticalException())
			{
				try
				{
					Logger!.Log(LogType.Error, "Unhandled exception in the Service Task", exception);
				}
				catch (Exception logException)
				{
					Console.Error.WriteLine($"Unhandled exception in the Service Task [{serviceTaskCode}]:");
					Console.Error.WriteLine(exception);
					Console.Error.WriteLine("Logging exception:");
					Console.Error.WriteLine(logException);
				}
			}
			else
			{
				ErrorReporter.ReportOnce(ExceptionConstants.RunnerExceptionLocation, exception);
			}

			try
			{
				HostLogger!.Log(LogLevel.Warning, $"{serviceTaskCode} threw an unhandled exception {exception.GetType()}: {exception.Message}{System.Environment.NewLine}For more information please check service task logs.");
			}
			catch (Exception logException)
			{
				Console.Error.WriteLine("Logging exception:");
				Console.Error.WriteLine(logException);
			}
		}

		#endregion

		#region Implementation

		Guid FindSuitableBranch(IServiceTask task)
		{
			if (!HostedServiceAttribute!.CanRunInAnyBranch)
			{
				return task.BranchPk;
			}

			if (HostedServiceAttribute.RequiresCompanyInCountry.IsNullOrEmpty())
			{
				return Guid.Empty;
			}

			var sql = $@"
    DECLARE @Countries NVARCHAR(MAX) = '{HostedServiceAttribute.RequiresCompanyInCountry}';
    DECLARE @BranchPK uniqueidentifier;

    SELECT TOP(1) @BranchPK = branch.GB_PK
    FROM dbo.GlbBranch AS branch
    LEFT JOIN dbo.GlbCompany AS company
        ON (branch.GB_GC = company.GC_PK)
    WHERE branch.GB_IsActive = 1
        AND company.GC_IsActive = 1
        AND company.GC_RN_NKCountryCode IN (
            SELECT VALUE FROM STRING_SPLIT(@Countries, ',')
        )
    ORDER BY
        company.GC_SystemCreateTimeUtc ASC,
        branch.GB_SystemCreateTimeUtc ASC,
        branch.GB_PK DESC

    IF (@BranchPK IS NULL)
    BEGIN
        SELECT TOP(1) @BranchPK = branch.GB_PK
        FROM dbo.GlbBranch AS branch
        LEFT JOIN dbo.RefUNLOCO AS unloco
            ON (branch.GB_RL_NKHomePort = unloco.RL_Code)
        WHERE branch.GB_IsActive = 1
            AND unloco.RL_IsActive = 1
            AND unloco.RL_RN_NKCountryCode IN (
                SELECT VALUE FROM STRING_SPLIT(@Countries, ',')
            )
        ORDER BY
            branch.GB_SystemCreateTimeUtc ASC,
            branch.GB_PK DESC
    END

    SELECT @BranchPK AS BranchPK;
    ";

			var branchPK = Db.Connection.ExecuteScalar(sql);
			if (branchPK == DBNull.Value || branchPK == null)
			{
				branchPK = Guid.Empty;
			}

			return (Guid)branchPK;
		}

		IServiceTask? GetTaskSchedule()
		{
			return taskLoader.Load(HostedServiceAttribute!.Code);
		}

		static Guid GetBranchPk()
		{
			var sqlText = $@"
    SELECT TOP 1 {GlbBranchSchema.Constants.PK}
    FROM {GlbBranchSchema.Constants.SqlSchemaName}.{GlbBranchSchema.Constants.TableName}
    WHERE
        {GlbBranchSchema.Constants.GB_IsActive} = 1
        AND {GlbBranchSchema.Constants.GB_GC} IN
        (
            SELECT {GlbCompanySchema.Constants.PK}
            FROM {GlbCompanySchema.Constants.SqlSchemaName}.{GlbCompanySchema.Constants.TableName}
            WHERE {GlbCompanySchema.Constants.GC_IsActive} = 1
        )
    ORDER BY
        {GlbBranchSchema.Constants.GB_SystemCreateTimeUtc} ASC,
        {GlbBranchSchema.Constants.PK} DESC
    ";
			var branchPk = Db.Connection.ExecuteScalar<Guid>(sqlText);
			return branchPk;
		}

		static Guid GetDepartmentPk()
		{
			var sqlText = $@"
    SELECT TOP 1 {GlbDepartmentSchema.Constants.PK}
    FROM {GlbDepartmentSchema.Constants.SqlSchemaName}.{GlbDepartmentSchema.Constants.TableName}
    WHERE
        {GlbDepartmentSchema.Constants.GE_IsActive} = 1
    ORDER BY
        {GlbDepartmentSchema.Constants.GE_SystemCreateTimeUtc} ASC,
        {GlbDepartmentSchema.Constants.PK} DESC
    ";
			var departmentPk = Db.Connection.ExecuteScalar<Guid>(sqlText);
			return departmentPk;
		}

		public Integration.ILogger? Logger { get; private set; }
		public Microsoft.Extensions.Logging.ILogger? HostLogger { get; private set; }
		protected ILoggerFactory LoggerFactory { get; }
		readonly IServiceTaskLoader taskLoader;

		BusinessObjectFactory Factory
		{
			get => factory ??= new BusinessObjectFactory { RefreshEnabled = false };
		}

		public IHostedServiceAttribute? HostedServiceAttribute { get; private set; }

		void BackgroundAppDomainWorker_Error(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex)
			{
				backgroundAppDomainWorkerExceptions.Add(ex);
				Logger!.Log(LogType.Error, ex.Message, ex);
			}
		}

		readonly List<Exception> backgroundAppDomainWorkerExceptions = new List<Exception>();
		BusinessObjectFactory? factory;

		#endregion
	}
}
