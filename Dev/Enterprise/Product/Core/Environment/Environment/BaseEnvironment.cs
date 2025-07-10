using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Environment
{
	/// <summary>
	/// As at Mar 2018:
	/// For the WinForms application, there is one instance of BaseEnvironment shared across all threads.
	/// Originally, the WinForms app was not significantly multi-threaded though.
	/// 
	/// For the Web application, there is an instance for each request.
	/// If the request has a session, the instance is shared across all requests in the session.
	/// Since a request has it's own thread, no thread locking is needed.
	/// If a request thread spawns another thread, that thread will not have an HttpContext and will get a new BaseEnvironment.
	/// 
	/// A special case is a web service only application that uses WebServiceEnvironmentProvider.
	/// It has a single BaseEnvironment (WebEnvironment) for all threads.
	/// </summary>
	public abstract class BaseEnvironment : Disposable, IEnvironment
#if DEBUG
		, IEnvironmentForTest
#endif
	{
		protected BaseEnvironment(IUserContextManager contextManager)
		{
			userContextManager = contextManager;

			Thread.CurrentThread.CurrentUICulture = Culture.Default;
			CultureInfo.DefaultThreadCurrentCulture = Culture.Default;
		}

		internal readonly IUserContextManager userContextManager;
		[ThreadStatic] static ILogContextSwitches logger;

		public ILogContextSwitches UserContextLogger
		{
			get => logger;
			set
			{
				if (value != null && logger != null)
				{
					throw new InvalidOperationException("Logger was already set");
				}

				logger = value;
			}
		}

		#region Outgoing Mail Manager

		public IOutgoingMailManager OutgoingMailManager
		{
			get
			{
				if (fOutgoingMailManager == null)
				{
					Type outgoingManagerType = ObjectFactory.GetType<IOutgoingMailManager>();
					fOutgoingMailManager = (IOutgoingMailManager)outgoingManagerType.InvokeMember("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField | BindingFlags.FlattenHierarchy, null, null, null);
				}
				return fOutgoingMailManager;
			}
		}
		IOutgoingMailManager fOutgoingMailManager;

		#endregion

		#region Outgoing Mail Manager

		public IOutgoingCustomsMailManager OutgoingCustomsMailManager
		{
			get
			{
				if (outgoingCustomsMailManager == null)
				{
					Type outgoingCustomsManagerType = ObjectFactory.GetType<IOutgoingCustomsMailManager>();
					outgoingCustomsMailManager = (IOutgoingCustomsMailManager)outgoingCustomsManagerType.InvokeMember("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField | BindingFlags.FlattenHierarchy, null, null, null);
				}
				return outgoingCustomsMailManager;
			}
		}
		IOutgoingCustomsMailManager outgoingCustomsMailManager;

		#endregion

		#region DataRegistry

		public DataRegistry Registry
		{
			get
			{
				return DataRegistry.Instance;
			}
		}

		#endregion

		#region Temp Path

		public string TempPath
		{
			get { return Temp.TempPath; }
		}

		public abstract string ApplicationStartupPath { get; }

		public string GetTempFileName()
		{
			return Temp.GetTempFileName();
		}

		public string GetTempFileName(string directoryName)
		{
			return Temp.GetTempFileName(directoryName);
		}

		public string GetTempFileName(string directoryName, string extension)
		{
			return Temp.GetTempFileName(directoryName, extension);
		}

		#endregion

		#region IsWeb

		public virtual bool IsWeb => false;

		public virtual bool IsWebService => false;

		#endregion

		#region IsProductionSystem

		public bool IsProductionSystem
		{
			get { return ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production; }
		}

		#endregion

		#region Time

		public TimeFactory Time
		{
			get { return TimeFactory.Instance; }
		}

		#endregion

		#region Number fountain

		public NumberFountains NumberFountains
		{
			get { return new NumberFountains(new NumberFountain.NumberFountains()); }
		}

		#endregion

		#region Service Task

		public string ServiceTaskCode
		{
			get => serviceTaskCode.Value;
			private set
			{
				if (Globals.IsUserInteractive && !Globals.IsTest)
				{
					ErrorReporter.ReportOnce("Service tasks are not user interactive you numpty.");
				}
				serviceTaskCode.Value = value;
			}
		}

		readonly Overridable<string> serviceTaskCode = new Overridable<string>(string.Empty);

		#endregion

		#region Current User Context

		public Licences Licence => (CurrentUserContext != null) ? ((UserContext)CurrentUserContext).Licence : null;
		public ICompany CurrentCompany => CurrentUserContext?.Company;

		public IBranch CurrentBranch
		{
			get
			{
				if (implicitUserContextAccessReportingRefCount > 0 && settingUserContextRefCount.Value == 0 && !userContextManager.CurrentThreadContextIsOverriden)
				{
					var message = (NoResString)"Service Task: " + ServiceTaskCode + (NoResString)" accesses environment current branch without setting the environment first.\r\n";
					message += (NoResString)"Direct access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.\r\n";

					ErrorReporter.ReportDeveloperExceptionOnce(message, new BranchAccessedWithoutConfiguredEnvironmentException($"Service Task: {ServiceTaskCode} accesses environment current branch without setting the environment first."));
				}

				return CurrentUserContext?.Branch;
			}
		}

		public IDepartment CurrentDepartment => CurrentUserContext?.Department;
		public IUser CurrentUser => CurrentUserContext?.User;
		IUserContext CurrentUserContextForReadOnly => userContextManager.ContextForReadOnly;
		public string CurrentNKUNLOCO => CurrentUserContextForReadOnly.Branch?.NKUNLOCO;
		public Guid CurrentUserPK => CurrentUserContextForReadOnly.User?.PK ?? Guid.Empty;
		public Guid CurrentDepartmentPK => CurrentUserContextForReadOnly.Department?.PK ?? Guid.Empty;
		public Guid CurrentBranchPK => CurrentUserContextForReadOnly.Branch?.PK ?? Guid.Empty;
		public Guid CurrentCompanyPK => CurrentUserContextForReadOnly.Company?.PK ?? Guid.Empty;
		public bool IsLoggedIn => CurrentUserContextForReadOnly.User != null;

		public bool IsAuthenticated
		{
			get
			{
				var context = CurrentUserContextForReadOnly;
				return context?.User != null
						&&
						(
							context.LoginAuthenticationInfo?.IsOK ?? false
						);
			}
		}

		public bool IsValidLogon
		{
			get
			{
				var context = CurrentUserContextForReadOnly;
				return context != null
					&& context.Branch != null
					&& context.Department != null
					&& context.Company != null;
			}
		}

		public void SetUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			var language = GetLanguageForContext(userContext);
			SetUserContextCore(userContext, setCurrentThreadContext: false, isRevert: false, contextLanguage: language, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber, isTempSwitch: false);
		}

		protected void SetCurrentThreadUserContext(
			IUserContext userContext,
			bool isRevert,
			string language,
			string callerFilePath,
			string callerMemberName,
			int callerLineNumber,
			bool isTempSwitch)
		{
			SetUserContextCore(userContext, setCurrentThreadContext: true, isRevert: isRevert, contextLanguage: language, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber, isTempSwitch: isTempSwitch);
		}

		public class UserContextChangingEventArgs : EventArgs, IUserContextChangingEventArgs
		{
			public UserContextChangingEventArgs(
				IUserContext oldUserContext,
				IUserContext newUserContext,
				bool setCurrentThreadContext,
				bool isRevert,
				string callerFilePath,
				string callerMemberName,
				int callerLineNumber)
			{
				OldUserContext = oldUserContext;
				NewUserContext = newUserContext;
				SetCurrentThreadContext = setCurrentThreadContext;
				IsRevert = isRevert;
				CallerFilePath = callerFilePath;
				CallerMemberName = callerMemberName;
				CallerLineNumber = callerLineNumber;
			}

			public IUserContext OldUserContext { get; }

			public IUserContext NewUserContext { get; }

			public bool SetCurrentThreadContext { get; }

			public bool IsRevert { get; }

			public string CallerFilePath { get; }

			public string CallerMemberName { get; }

			public int CallerLineNumber { get; }
		}

		readonly ThreadLocal<EventHandler<IUserContextChangingEventArgs>> userContextChanging = new ThreadLocal<EventHandler<IUserContextChangingEventArgs>>();

		public event EventHandler<IUserContextChangingEventArgs> UserContextChanging
		{
			add
			{
				userContextChanging.Value += value;
			}
			remove
			{
				userContextChanging.Value -= value;
			}
		}

		readonly ThreadLocal<EventHandler<IUserContextChangingEventArgs>> userContextChanged = new ThreadLocal<EventHandler<IUserContextChangingEventArgs>>();

		public event EventHandler<IUserContextChangingEventArgs> UserContextChanged
		{
			add
			{
				userContextChanged.Value += value;
			}
			remove
			{
				userContextChanged.Value -= value;
			}
		}

		protected virtual void PreUserContextSwitch(
			IUserContext userContext,
			string contextLanguage,
			bool setCurrentThreadContext,
			bool isRevert)
		{
			if (!CanUseExistingLanguageLicense(CurrentUserContext, userContext))
			{
				SetLanguageForContext(userContext, contextLanguage);
			}
		}

		void SetLanguageForContext(IUserContext userContext, string language)
		{
			if (usingLanguageLicense != null)
			{
				usingLanguageLicense.Dispose();
				usingLanguageLicense = null;
			}

			var contextForGettingLicense = userContext ?? new UserContext();
			if (!Res.IsEnglish(language))
			{
				usingLanguageLicense = contextForGettingLicense.Licence.UseLanguageLicense(ref language, LanguageUsageType.GUI);
			}
			ObjectFactory.Get<IResourceStrings>().CurrentLanguage = language;
		}

		string GetLanguageForContext(IUserContext userContext)
		{
			var language = userContext?.User?.Language;
			if (string.IsNullOrEmpty(language) || language == Res.DefaultLanguage || !LanguageHelper.ActiveLanguageExistsForOLookUpEditType(language))
			{
				language = DataRegistry.Instance.EnglishSpellingForCompany(userContext?.Company);
			}

			return language;
		}

		bool CanUseExistingLanguageLicense(IUserContext context1, IUserContext context2)
		{
			return context1?.User != null && context2?.User != null && context1.User.LoginName == context2.User.LoginName && context1.User.Language == context2.User.Language
				&& (context1.Company == null || context2.Company == null || context1.Company.PK == context2.Company.PK);
		}

		IDisposable usingLanguageLicense;

		protected virtual void PostUserContextSwitch(
			IUserContext userContext,
			bool setCurrentThreadContext,
			bool isRevert)
		{
		}

		void SetUserContextCore(IUserContext userContext,
			bool setCurrentThreadContext,
			bool isRevert,
			string contextLanguage,
			string callerFilePath,
			string callerMemberName,
			int callerLineNumber,
			bool isTempSwitch)
		{
			Exception exception = null;
			try
			{
				CheckContextSwitchIsReasonable(userContext, isRevert);
				Db.SetCurrentUser(userContext?.User?.Initials);

				using (SuspendBranchAccessError())
				{
					var oldUserContext = userContextManager.ContextForReadOnly;
					userContextChanging.Value?.Invoke(
						this,
						new UserContextChangingEventArgs(
							oldUserContext,
							userContext,
							setCurrentThreadContext,
							isRevert,
							callerFilePath,
							callerMemberName,
							callerLineNumber));

					PreUserContextSwitch(
						userContext,
						contextLanguage,
						setCurrentThreadContext,
						isRevert);

					if (setCurrentThreadContext)
					{
						userContextManager.SetCurrentThreadUserContext(userContext, isRevert);
					}
					else
					{
						userContextManager.SetMasterUserContext(userContext);
					}

					userContextChanged.Value?.Invoke(
						this,
						new UserContextChangingEventArgs(
							oldUserContext,
							userContext,
							setCurrentThreadContext,
							isRevert,
							callerFilePath,
							callerMemberName,
							callerLineNumber));

					PostUserContextSwitch(
						userContext,
						setCurrentThreadContext,
						isRevert);

					Culture.Set(Culture.Default);
				}
			}
			catch (Exception ex)
			{
				exception = ex;
				if (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("SwitchingUserContextError", ex);
				}
				throw;
			}
			finally
			{
				if (!isTempSwitch)
				{
					var glowUserDataManager = GetGlowUserDataManager();
					glowUserDataManager?.ClearUserData();
				}

				if (isRevert)
				{
					if (!Equals(userContext, CurrentUserContext))
					{
						// If we have been unable to reset the user context (due to an exception or similar)
						// it is a critical failure as continuing would cause processing in the wrong user context
						// and potential data corruption
						throw new UserContextLostException(userContext, CurrentUserContext, exception);
					}
				}
				else
				{
					if (exception != null && Equals(userContext, CurrentUserContext))
					{
						throw new UserContextLostException(userContext, CurrentUserContext, exception);
					}
				}
			}
		}

		#region Context Switch Check

		readonly ThreadLocal<bool> isSwitchContextCheckSuppressed = new ThreadLocal<bool>();

		public IDisposable SuppressSwitchContextCheck(bool ensureContextIsRestoredAfterSuppression = true)
		{
			var restoreContextCheck = ensureContextIsRestoredAfterSuppression ? EnsureUserContextIsRestoredAfterThis() : null;

			if (isSwitchContextCheckSuppressed.Value)
			{
				return restoreContextCheck;
			}

			isSwitchContextCheckSuppressed.Value = true;

			return new DisposableAction(() =>
			{
				isSwitchContextCheckSuppressed.Value = false;
				restoreContextCheck?.Dispose();
			});
		}

		public const string ContextSwitchMessageKey = "ContextSwitchMessageKey";

		void CheckContextSwitchIsReasonable(IUserContext userContext, bool isRevert)
		{
			if (!isSwitchContextCheckSuppressed.Value
				&& !isRevert
				&& CurrentUser != null
				&& CurrentUser.LoginName == User.WebUserName
				&& (userContext == null || userContext.User == null
					|| userContext.User.LoginName != User.WebUserName
				))
			{
				ErrorReporter.ReportOnce(ContextSwitchMessageKey, $@"Unexpected context switch for the web user. The new context:
User: {userContext?.User?.LoginName}
Branch: {userContext?.Branch?.Code}
Department: {userContext?.Department?.Code}

If you are sure the context switching is correct, wrap your code with context switching with {nameof(SuppressSwitchContextCheck)}.");
			}
		}

		#endregion

		/// <summary>
		/// This should only be used in limited circumstances, such as error reporter and performance statistics collector
		/// Correct behaviour is to actually initialise the branch appropriately
		/// </summary>
		/// <returns></returns>
		public IDisposable SuspendBranchAccessError()
		{
			++settingUserContextRefCount.Value;
			return new DisposableAction(() => --settingUserContextRefCount.Value);
		}

		readonly ThreadLocal<int> settingUserContextRefCount = new ThreadLocal<int>();

		int implicitUserContextAccessReportingRefCount;

		public IDisposable TemporaryServiceTaskContext(
			string code,
			bool canRunInAnyBranch,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			var previousCode = ServiceTaskCode;
			ServiceTaskCode = code;

			if (canRunInAnyBranch)
			{
				Interlocked.Increment(ref implicitUserContextAccessReportingRefCount);
			}

			return new DisposableAction(() =>
			{
				if (canRunInAnyBranch)
				{
					Interlocked.Decrement(ref implicitUserContextAccessReportingRefCount);
				}

				ServiceTaskCode = previousCode;
			});
		}

		/// <summary>
		/// Clear the user AND any security
		/// </summary>
		public void ClearUserContext(
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			SetUserContext(null, callerFilePath, callerMemberName, callerLineNumber);
		}

		public IUserContext CurrentUserContext => userContextManager.Context;

		public SecurityCore Security => userContextManager.Security;

		public IUserContext NewUserContext(string staffLoginName, Guid branchPK, Guid departmentPK)
		{
			var user = (IUser)new BusinessObjectFactory().LoadFromNaturalKey(ObjectFactory.GetType("IGlbStaff"), GlbStaffSchema.GS_LoginName, staffLoginName);
			return new UserContext(user, branchPK, departmentPK);
		}

		/// <summary>
		/// Call from background threads to release any thread local objects for garbage collection.
		/// </summary>
		public void CleanupUserContextOnCurrentThread() => userContextManager.ClearCurrentThread();

		#endregion

		#region

		public IDisposable EnsureUserContextIsRestoredAfterThis()
		{
			var userContext = CurrentUserContext;

			return new DisposableAction(() => {
				if (!Equals(userContext, CurrentUserContext))
				{
					// If we have been unable to reset the user context (due to an exception or similar)
					// it is a critical failure as continuing would cause processing in the wrong user context
					// and potential data corruption
					throw new UserContextLostException(userContext, CurrentUserContext);
				}
			});
		}

		#endregion

		#region GlobalFormTopCaption

		public string GlobalFormTopCaption
		{
			get
			{
				string result = "";

				try
				{
					result = AddToGlobalFormTopCaption(result, true, Registry.GlobalFormTopCaption);
					result = AddToGlobalFormTopCaption(result, Registry.ShowDatabaseName, Res.GetString("5bcda11b-60f8-45d3-845a-f89afc880873", "DB: {0}", CargoWise.Data.Db.DatabaseName));

					using (SuspendBranchAccessError()) // No need to report error 
					{
						if (CurrentBranch != null)
						{
							result = AddToGlobalFormTopCaption(result, Registry.ShowBranchName, Res.GetString("8d3f7326-2374-4737-a0ea-a3d0874ea001", "Branch: {0}", CurrentBranch.Name));
						}
					}

					if (CurrentCompany != null)
					{
						result = AddToGlobalFormTopCaption(result, Registry.ShowCompanyName, Res.GetString("2ccc58a1-364a-48b1-9d85-1c5162337f93", "Company: {0}", CurrentCompany.Name));
					}
					if (CurrentDepartment != null)
					{
						result = AddToGlobalFormTopCaption(result, Registry.ShowDepartmentName, Res.GetString("802d4111-2fed-446b-b8a7-7b4afa1d6f92", "Department: {0}", CurrentDepartment.Description));
					}
					if (CurrentUser != null)
					{
						result = AddToGlobalFormTopCaption(result, Registry.ShowUserName, Res.GetString("32e2f739-4a34-4b7F-875e-8a9c0c4c98ea", "User: {0}", CurrentUser.FullName));
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result = AddToGlobalFormTopCaption(result, true, Res.GetString("a6344dec-2a3f-4c49-b065-a44a06183710", "(Exception: {0})", e.Message));
				}

				return result;
			}
		}

		string AddToGlobalFormTopCaption(string wholeCaption, bool shouldWeAddThisString, string stringToAdd)
		{
			if (shouldWeAddThisString && !string.IsNullOrEmpty(stringToAdd))
			{
				var separator = string.IsNullOrEmpty(wholeCaption) ? string.Empty : " - ";
				wholeCaption += separator + stringToAdd;
			}
			return wholeCaption;
		}

		#endregion

		#region Set Temporary User Context

		public IDisposable SetTemporaryUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			using (SuspendBranchAccessError())
			{
				var oldInstance = RecentItemManager.Instance;
				var oldUserContext = CurrentUserContext;

				var oldLanguage = GetLanguageForContext(oldUserContext);
				var newLanguage = GetLanguageForContext(userContext);
				var glowUserDataManager = GetGlowUserDataManager();
				var removeTempUserContext = glowUserDataManager.IncreaseTempUserCount();

				var disposableAction = new DisposableAction(() =>
				{
					SetCurrentThreadUserContext(oldUserContext, isRevert: true, oldLanguage, callerFilePath, callerMemberName, callerLineNumber, isTempSwitch: true);
					RecentItemManager.Instance = oldInstance;
					removeTempUserContext.Dispose();
				});

				SetCurrentThreadUserContext(userContext, isRevert: false, newLanguage, callerFilePath, callerMemberName, callerLineNumber, isTempSwitch: true);
				return disposableAction;
			}
		}

		public IDisposable SetTemporaryMasterUserContext(
			string staffLoginName,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			using (SuspendBranchAccessError())
			{
				var userContext = UserContext.SetupAndReturnUserContextForTemporarySwitch(staffLoginName, branchPK, departmentPK);
				var newContextLanguage = GetLanguageForContext(userContext);
				var oldInstance = RecentItemManager.Instance;
				var oldUserContext = CurrentUserContext;
				var oldContextLanguage = GetLanguageForContext(oldUserContext);

				var disposableAction = new DisposableAction(() =>
				{
					SetUserContextCore(oldUserContext, setCurrentThreadContext: false, isRevert: true, contextLanguage: oldContextLanguage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber, isTempSwitch: false);
					RecentItemManager.Instance = oldInstance;
				});

				SetUserContextCore(userContext, setCurrentThreadContext: false, isRevert: false, contextLanguage: newContextLanguage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber, isTempSwitch: false);
				return disposableAction;
			}
		}

		public IDisposable SetTemporaryUserContext(
			string staffLoginName,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return SetTemporaryUserContext(
				UserContext.SetupAndReturnUserContextForTemporarySwitch(staffLoginName, branchPK, departmentPK),
				callerFilePath,
				callerMemberName,
				callerLineNumber);
		}

		public IDisposable SetTemporaryUserContext(
			Guid staffPK,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return SetTemporaryUserContext(
				UserContext.SetupAndReturnUserContextForTemporarySwitch(staffPK, branchPK, departmentPK),
				callerFilePath,
				callerMemberName,
				callerLineNumber);
		}

		#endregion

		#region For Testing

#if DEBUG
		public IDisposable SetTemporarySecurityInstanceForTest(SecurityCore securityInstance)
		{
			var oldSecurityInstance = userContextManager.Security;
			var disposableAction = new DisposableAction(delegate
			{
				userContextManager.SetSecurityForTest(oldSecurityInstance);
			});

			userContextManager.SetSecurityForTest(securityInstance);
			return disposableAction;
		}

		public IDisposable SetTemporaryMasterUserContext(IUserContext userContext, string callerFilePath = "",
			string callerMemberName = "", int callerLineNumber = -1)
		{
			using (SuspendBranchAccessError())
			{
				var oldInstance = RecentItemManager.Instance;
				var oldUserContext = CurrentUserContext;

				var oldLanguage = GetLanguageForContext(oldUserContext);
				var newLanguage = GetLanguageForContext(userContext);

				var disposableAction = new DisposableAction(() =>
				{
					SetUserContextCore(oldUserContext, setCurrentThreadContext: false, isRevert: true, contextLanguage: oldLanguage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber, isTempSwitch: false);
					RecentItemManager.Instance = oldInstance;
				});
				SetUserContextCore(userContext, setCurrentThreadContext: false, isRevert: false, contextLanguage: newLanguage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber, isTempSwitch: false);
				return disposableAction;
			}
		}

		/// <summary>
		/// Allows a test to determine if security was created, i.e., a SecurityCore constructed
		/// </summary>
		public bool IsSecurityCreatedForTest => userContextManager.IsSecurityCreatedForTest;

		/// <summary>
		/// Reset the security to allow tests to determine if any later code accesses security (via IsSecurityCreatedForTest)
		/// </summary>
		public void ResetSecurityForTest()
		{
			userContextManager.ResetSecurityForTest();
		}

		public void SetupDataBeforeTest()
		{
			if (Globals.IsTest)
			{
				userContextBeforeTest = userContextManager.ContextForReadOnly;
			}
		}
		internal IUserContext userContextBeforeTest;

		public IUserContextManager UserContextManagerForTesting => userContextManager;

#endif
		#endregion

		#region IEnvironment Members

		ILicenceProxy IEnvironment.Licence
		{
			get { return CurrentUserContext?.Licence; }
		}

		ISecurityProxy IEnvironment.Security
		{
			get { return Security; }
		}

		#region Enterprise Semaphore Provider

		ISemaphoreProvider IEnvironment.SemaphoreProvider
		{
			get { return EnvironmentSpecificSemaphoreProvider; }
		}

		public virtual IDbUpgradeCaptions DbUpgradeCaptions { get; }

		protected abstract ISemaphoreProvider EnvironmentSpecificSemaphoreProvider { get; }

		#endregion

		#endregion

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				settingUserContextRefCount.Dispose();
				userContextManager.Dispose();
			}
		}

		public abstract void ExitApplication();

		IGlowUserDataManager GetGlowUserDataManager()
		{
			return ObjectFactory.Get(name: "IGlowServiceClientFactory") as IGlowUserDataManager;
		}
		public abstract IUserLoginController LoginController { get; }
	}
}
