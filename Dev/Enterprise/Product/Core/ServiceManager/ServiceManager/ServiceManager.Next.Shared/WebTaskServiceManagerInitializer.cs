using CargoWise.Data;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.ServiceManager.Next.Shared;

public class WebTaskServiceManagerInitializer : IServiceManagerInitializer
{
	readonly BaseExceptionReporter errorReporter;
	readonly INextSharedOptions nextSharedOptions;

	// these overloads are needed to find the right public constructor for DI 
	public WebTaskServiceManagerInitializer(BaseExceptionReporter errorReporter, INextRunnerOptions nextSharedOptions) : this(errorReporter, nextSharedOptions as INextSharedOptions) { }
	public WebTaskServiceManagerInitializer(BaseExceptionReporter errorReporter, INextLauncherOptions nextSharedOptions) : this(errorReporter, nextSharedOptions as INextSharedOptions) { }

	WebTaskServiceManagerInitializer(BaseExceptionReporter errorReporter, INextSharedOptions nextSharedOptions)
	{
		this.errorReporter = errorReporter;
		this.nextSharedOptions = nextSharedOptions;
	}

	public void InitializeApplication()
	{
		Globals.IsUserInteractive = false;
		DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceRunner;
		Initialiser.InitialiseServiceManager(errorReporter, usePooledConnection: true);
	}

	public void InitializeDatabase()
	{
		// if already set for unit tests, don't change it
		if (Db.DatabaseNameIsInitialized && Db.ServerNameIsInitialized)
		{
			return;
		}

		Db.InitializeDatabaseDetails(
			Db.GetMachineNameIfLocal(nextSharedOptions.ServerName),
			nextSharedOptions.DatabaseName,
			CargoWise.DataProtection.ApplicationType.Web);
		Db.Connection.EnsureIsOpen();
	}
}
