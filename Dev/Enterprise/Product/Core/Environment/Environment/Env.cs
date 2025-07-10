using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment
{
	public static class Env
	{
		#region Instance/Delegate Members

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static EnvProvider fProvider;

		public static BaseEnvironment Instance
		{
			get
			{
				Argument.NotNull(Provider, "Env.Provider");
				Argument.NotNull(Provider.Instance, "Env.Provider.Instance");
				return Provider.Instance;
			}
		}

		internal static EnvProvider Provider
		{
			get
			{
#if DEBUG
				if (fProvider == null)
				{
					return new Enterprise.Environment.Testing.NullEnvProvider();
				}
#endif
				return fProvider;
			}
			set
			{
				fProvider = value;
			}
		}

		public static EnvProvider GetCurrentProvider()
		{
			return Provider;
		}

		#endregion Instance/Delegate Member

		#region Static Accessors to Delegate

		public static string TempPath
		{
			get { return Instance.TempPath; }
		}

		public static TimeFactory Time
		{
			get { return Instance.Time; }
		}

		public static ICompany CurrentCompany
		{
			get { return Instance.CurrentCompany; }
		}

		public static IBranch CurrentBranch
		{
			get { return Instance.CurrentBranch; }
		}

		public static IDepartment CurrentDepartment
		{
			get { return Instance.CurrentDepartment; }
		}

		public static Guid CurrentBranchPK { get { return Instance.CurrentBranchPK; } }
		public static Guid CurrentDepartmentPK { get { return Instance.CurrentDepartmentPK; } }
		public static Guid CurrentCompanyPK { get { return Instance.CurrentCompanyPK; } }
		public static Guid CurrentUserPK { get { return Instance.CurrentUserPK; } }

		public static IOutgoingMailManager OutgoingMailManager
		{
			get { return Instance.OutgoingMailManager; }
		}

		public static IOutgoingCustomsMailManager OutgoingCustomsMailManager
		{
			get { return Instance.OutgoingCustomsMailManager; }
		}

		public static bool IsLoggedIn => Instance.IsLoggedIn;
		public static bool IsAuthenticated => Instance.IsAuthenticated;
		public static bool IsValidLogon => Instance.IsValidLogon;

		public static DataRegistry Registry
		{
			get { return Instance.Registry; }
		}

		public static IUser CurrentUser
		{
			get { return Instance.CurrentUser; }
		}

		public static IUserLoginController LoginController
		{
			get { return Instance.LoginController; }
		}

		public static NumberFountains NumberFountains
		{
			get { return Instance.NumberFountains; }
		}

		public static SecurityCore Security
		{
			get { return Instance.Security; }
		}

		public static Licences Licence
		{
			get { return Instance.Licence; }
		}

		public static string GetTempFileName()
		{
			return Instance.GetTempFileName();
		}

		public static string GetTempFileName(string directoryName)
		{
			return Instance.GetTempFileName(directoryName);
		}

		public static string GetTempFileName(string directoryName, string extension)
		{
			return Instance.GetTempFileName(directoryName, extension);
		}

		public static string GlobalFormTopCaption
		{
			get { return Instance.GlobalFormTopCaption; }
		}

		public static UserContext CurrentUserContext
		{
			get { return (UserContext)Instance.CurrentUserContext; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		public static void SetUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			Instance.SetUserContext(userContext, callerFilePath, callerMemberName, callerLineNumber);
		}

		public static void ClearUserContext(
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			Instance.ClearUserContext(callerFilePath, callerMemberName, callerLineNumber);
		}

		public static IDisposable SetTemporaryUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return Instance.SetTemporaryUserContext(userContext, callerFilePath, callerMemberName, callerLineNumber);
		}

		public static IDisposable SetTemporaryUserContext(
			string staffLoginName,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return Instance.SetTemporaryUserContext(staffLoginName, branchPK, departmentPK, callerFilePath, callerMemberName, callerLineNumber);
		}

		public static IDisposable SetTemporaryUserContext(
			Guid staffPK,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return Instance.SetTemporaryUserContext(staffPK, branchPK, departmentPK, callerFilePath, callerMemberName, callerLineNumber);
		}

		public static void ExitApplication()
		{
			Instance.ExitApplication();
		}

		public static string ApplicationStartupPath
		{
			get { return Instance.ApplicationStartupPath; }
		}

		public static bool IsCargoWiseDomain(string domainName)
		{
			switch (domainName)
			{
				case "corporate.cargowise.com":
				case "dev.corporate.cargowise.com":
				case "wtg.zone":
				case "sand.wtg.zone":
					return true;
				default:
					return false;
			}
		}

		#endregion

		#region Debug Internal Accessors

#if DEBUG
		public static IDisposable SetTemporarySecurityInstanceForTest(SecurityCore securityInstance)
		{
			return Instance.SetTemporarySecurityInstanceForTest(securityInstance);
		}

		public static void ClearAllEmailsCreated()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public static IEnumerable<EmailDef> AllEmailsCreated
		{
			get
			{
				foreach (var email in Env.OutgoingMailManager.EmailsCreated)
				{
					yield return email;
				}
				foreach (var customsEmail in Env.OutgoingCustomsMailManager.EmailsCreated)
				{
					yield return customsEmail;
				}
			}
		}

		public static void SetupDataBeforeTest()
		{
			providerBeforeTest = Provider;
			Instance.SetupDataBeforeTest();
		}

		internal static EnvProvider providerBeforeTest;

		public static IDisposable TemporarilySetNullEnvironmentInstanceForTesting()
		{
			return SetEnvProviderTemporarily(new NullEnvironmentInstanceEnvProvider());
		}

		public static IDisposable SetEnvProviderTemporarily(EnvProvider provider)
		{
			var savedProvider = fProvider;
			fProvider = provider;

			return new DisposableAction(() =>
			{
				fProvider.Dispose();
				fProvider = savedProvider;
			});
		}

		public static bool IsDotNet45
		{
			get { return new ClientDotNetRetriever().GetDotNetVersion().Version >= new Version(4, 0, 30319, 17929); }
		}

		public sealed class NullEnvironmentInstanceEnvProvider : EnvProvider, Enterprise.Integration.Environment.INullEnvProvider
		{
			public override BaseEnvironment Instance
			{
				get { return null; }
			}

			protected override IDbEnvironment GetDbEnvironmentInstance()
			{
				return new BaseDbEnvironment();
			}
		}
#endif
		#endregion

		public static IDisposable StartContextSwitchTrace(ILogContextSwitches logger)
		{
			if (Instance.UserContextLogger != null)
			{
				return null;
			}

			Instance.UserContextLogger = logger;

			void HookUserContextChanges(object sender, IUserContextChangingEventArgs args)
			{
				logger.Log(new ContextSwitchInfo(new StackTrace(), args.OldUserContext, args.NewUserContext, !args.IsRevert));
			}

			Instance.UserContextChanging += HookUserContextChanges;

			bool disposed = false;
			return new DisposableAction(() =>
			{
				if (disposed)
				{
					ErrorReporter.ReportOnce("Double dispose of Context switch trace");
					return;
				}

				disposed = true;
				Instance.UserContextChanging -= HookUserContextChanges;
				Instance.UserContextLogger = null;
			});
		}
	}

	public class UserContextSwitchLogger : ILogContextSwitches
	{
		public void Log(ContextSwitchInfo contextSwitchInfo)
		{
			Logs.Enqueue(contextSwitchInfo);
		}

		public ConcurrentQueue<ContextSwitchInfo> Logs { get; } = new ConcurrentQueue<ContextSwitchInfo>();
	}

	public interface ILogContextSwitches
	{
		void Log(ContextSwitchInfo contextSwitchInfo);
	}

	public class ContextSwitchInfo
	{
		public ContextSwitchInfo(StackTrace trace, IUserContext oldUserContext, IUserContext newUserContext, bool start)
		{
			this.Trace = trace;
			this.Start = start;

			UserContextInfo? CreateContextInfo(IUserContext context)
			{
				if (context == null)
				{
					return null;
				}

				return new UserContextInfo(
					context.Branch?.Code,
					context.Company?.Code,
					context.User?.LoginName);
			}

			this.OldUserContext = CreateContextInfo(oldUserContext);
			this.NewUserContext = CreateContextInfo(newUserContext);
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			result.Append(FormattableString.Invariant($@"OldUserContext Company: {OldUserContext?.CompanyCode}, Branch: {OldUserContext?.BranchCode}, User: {OldUserContext?.UserName}"));
			result.Append(FormattableString.Invariant($@"NewUserContext Company: {NewUserContext?.CompanyCode}, Branch: {NewUserContext?.BranchCode}, User: {NewUserContext?.UserName}"));
			result.Append(Trace);
			return result.ToString();
		}

		public StackTrace Trace { get; }
		public UserContextInfo? OldUserContext { get; }
		public UserContextInfo? NewUserContext { get; }
		public bool Start { get; }
	}
}
