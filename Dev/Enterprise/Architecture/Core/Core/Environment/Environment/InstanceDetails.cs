using System;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Environment;

public class InstanceDetails
{
	public InstanceDetails(string domain, string instance, string serverName, string databaseName)
	{
		Domain = domain;
		Instance = instance;
		ServerName = serverName;
		DatabaseName = databaseName;
	}

	public string Domain { get; }
	public string Instance { get; }
	public string ServerName { get; }
	public string DatabaseName { get; }

	public static InstanceDetails Current => current.Value;

	public static bool ShouldAddDatabaseInfoToUrls
	{
		get
		{
			return !Db.Connection.DatabaseUpgradedExceptionHasBeenThrown && DataRegistry.Instance.AddDatabaseInfoToEdientUrls;
		}
	}

#if DEBUG

	public static IDisposable SetUpCurrentForTest(InstanceDetails currentValue = null)
	{
		current.Value = currentValue;
		return new DisposableAction(current.ResetValue);
	}

	internal static IDisposable ReCalculateForTest()
	{
		return SetUpCurrentForTest(Calculate());
	}

#endif

	static InstanceDetails Calculate()
	{
		try
		{
			using var domain = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain();
			if (domain != null)
			{
				var instanceClass = ObjectFactory.Get<ICargoWiseOneInstanceClass>();
				if (instanceClass.ExistsInCurrentSchema())
				{
					var instance = instanceClass.FindInstanceByDatabase(Db.ServerName, Db.DatabaseName, options: new DirectorySearchOptions { Scope = DirectorySearchScope.Forest });
					if (instance != null)
					{
						return new InstanceDetails(domain.Name, instance.Name, Db.ServerName, Db.DatabaseName);
					}
				}
			}
		}
		catch (ActiveDirectoryOperationException) { } // This exception is only going to happen if the machine running the code is not actually logged into that domain
		catch (ActiveDirectoryServerDownException e) when (e.InnerException is COMException && ShouldIgnoreCOMException((COMException)e.InnerException)) { }
#if !WINZOR
		catch (AuthenticationException e) when (e.InnerException is COMException && ShouldIgnoreCOMException((COMException)e.InnerException)) { }
#endif
		catch (COMException e) when (ShouldIgnoreCOMException(e)) { }
		catch (PlatformNotSupportedException e)
		{
			if (EnvProxy.Instance.IsProductionSystem)
			{
				ErrorReporter.ReportOnce("InstanceDetailsCalculate", "Exception in InstanceDetails.Calculate().", e);
			}
		}

		return new InstanceDetails(null, null, Db.ServerName, Db.DatabaseName);
	}

	static bool ShouldIgnoreCOMException(COMException e)
	{
		// Here we can ingore COMException with following error code and message ref. http://www.selfadsi.org/errorcodes.htm
		// ErrorCode -2147023570 LDAP_INVALID_CREDENTIALS:
		// The user name or password is incorrect.
		// Logon failure: unknown user name or bad password.
		// ErrorCode -2147016646 LDAP_SERVER_DOWN:
		// The server is not operational.
		return
			e.ErrorCode == -2147023570
			|| e.ErrorCode == -2147016646
			|| e.ErrorCode == -2147016689 // The directory service is unavailable.
			;
	}

	static readonly LazyOverridable<InstanceDetails> current = new LazyOverridable<InstanceDetails>(Calculate, LazyThreadSafetyMode.PublicationOnly);
}
