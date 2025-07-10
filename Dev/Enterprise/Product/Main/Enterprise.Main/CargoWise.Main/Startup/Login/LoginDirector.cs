using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Main.Startup.Login;
using CargoWise.Windows.UI.Controls.Internal;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.Security;
using Enterprise.Semaphores.Common;
using Enterprise.Upgrades;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class LoginDirector : AbstractApplicationStartupTask
	{
		public LoginDirector()
		{
		}

		public override string TaskDescription => Res.GetString("10ee23b3-7577-4d77-8858-5045d3f9e3f3", "Checking Security Credentials");

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return true;
		}

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			ReadArguments(arguments);
			if (!WaitForUrlAuthenticationIfRequired())
			{
				LoginAutomatically(arguments);
			}
			return true;
		}

		public static LoginDirector Instance
		{
			get { return instance; }
#if DEBUG
			set { instance = value; }
#endif
		}
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static LoginDirector instance = new LoginDirector();

#if DEBUG
		public static IDisposable UseTestInstance(LoginDirector testInstance = null)
		{
			var initialInstance = Instance;
			instance = testInstance ?? new LoginDirector();
			return new DisposableAction(() => instance = initialInstance);
		}
#endif

		bool WaitForUrlAuthenticationIfRequired()
		{
			var isLoginSuccesful = false;

#if !WINZOR

			if (GetIsUrlAuthenticationRequired())
			{
				var loginAction = UrlHandlerServiceUtils.GetLoginAction();
				if (loginAction != null)
				{
					if (isLoginSuccesful = loginAction())
					{
						HideUI = true;
						LoggedInLocation = true;
						MainForm.InitializeAfterLogin();
					}
				}
			}

#endif

			return isLoginSuccesful;
		}

#if !WINZOR

		bool GetIsUrlAuthenticationRequired()
		{
			var channel = EnterpriseChannel.Instance;
			return channel == null || !channel.IsConnected
				? UrlAuthenticationMutex.IsUrlAuthenticationRequired
				: InitializationMessageHandler.IsUrlAuthenticationSupported
					&& channel.SendMessage<bool>(EnterpriseChannelMessageTypes.UrlAuthenticationRequired, Array.Empty<byte>());
		}

#endif

		void LoginAutomatically(CommandLineArguments arguments)
		{
			var oidcConfig = ObjectFactory.Get<IOIDCConfig>();
			if (oidcConfig.IsOIDCEnabled)
			{
				var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(90));

				AuthenticatedUser = OIDCUserLogin.SingleSignOnLoginWithRefreshToken(oidcConfig, WebUrlLauncher.Launch, cancellationTokenSource.Token);
				AuthenticatedUser.TakeThreadOwnershipForUser();
			}
			else
			{
				bool retry;
				do
				{
					AuthenticatedUser = Env.LoginController.LoginUserSingleSignOn(support2FA: true);
					retry = HandleLoginFailureForRetry(null);
				}
				while (retry);
			}

#if DEBUG
			if (!AuthenticatedUser.LoginValidated && !(bool)arguments[ApplicationArguments.OptionShowLogin])
			{
				AuthenticatedUser = Env.LoginController.LoginUserDeveloper();
			}
#endif
			if (AuthenticatedUser.LoginValidated)
			{
				TryLoginDefaultLocation();
			}
		}

#if DEBUG
		internal void LoginAutomatically()
		{
			LoginAutomatically(new ApplicationArguments(Array.Empty<string>()));
		}
#endif

		bool TryLoginDefaultLocation()
		{
			if (!string.IsNullOrEmpty(ForceBranch) || !string.IsNullOrEmpty(ForceDepartment))
			{
				LoggedInLocation = Env.LoginController.LoginLocation(AuthenticatedUser, ForceBranch, ForceDepartment);
			}
			else
			{
				LoggedInLocation = Env.LoginController.LoginLocationAutomatically(AuthenticatedUser);
			}
			return LoggedInLocation;
		}

		public virtual string EmailTemporaryPassword(string username) => Env.LoginController.GenerateAndSendTemporaryPassword(username);

		/// <summary>
		/// Called from Login UI
		/// </summary>
		public virtual LoginAuthenticationInfo LoginUserInteractive(string username, string password, string twoFactorAuthenticationCode = null, bool support2FA = false)
		{
			LoginAuthenticationInfo result;

			bool retry;
			do
			{
				AuthenticatedUser = IsSSOWith2FA() ?
					Env.LoginController.LoginUserSingleSignOn(twoFactorAuthenticationCode, support2FA) :
					Env.LoginController.LoginUser(username, password, twoFactorAuthenticationCode: twoFactorAuthenticationCode, support2FA: support2FA);
				retry = HandleLoginFailureForRetry(username);
			}
			while (retry);

			result = AuthenticatedUser;

			if (!LoginWithTempPassword() && AuthenticatedUser.LoginValidated && !TwoFactorAuthenticationRequired() && (IsSSOWith2FA() || DoExpiredPasswordCheck(username)))
			{
				if (TryLoginDefaultLocation())
				{
					MainForm.InitializeAfterLogin();
				}
				else
				{
					StartupOpenMainFormTask.MainFormInstance.ShowLoginLocationControl();
				}
			}
			return result;

			bool IsSSOWith2FA() => string.IsNullOrEmpty(password) && twoFactorAuthenticationCode != null;

			bool TwoFactorAuthenticationRequired() => AuthenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired
				|| AuthenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationFailed
				|| AuthenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationFieldMissing;
		}

		bool LoginWithTempPassword() => AuthenticatedUser.State == LoginAuthenticationInfo.Status.TempPasswordRequired
									|| AuthenticatedUser.State == LoginAuthenticationInfo.Status.TempPasswordLoginSuccessfully;

		public virtual bool ResetLocalPassword(Guid staffPK)
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var staff = factory.Load<GlbStaff>(staffPK);

				var shouldSave = PasswordDialogs.Instance.ShowResetPasswordDialog(staff);
				if (shouldSave)
				{
					staff.GS_ChangePasswordAtNextLogin = false;

					try
					{
						factory.Save();
					}
					catch (ZSaveConcurrencyException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
						factory.Save();
					}
					return true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// disregard any non exception and return false
			}
			return false;
		}

		/// <summary>
		/// Called from Login UI
		/// </summary>
		internal virtual LoginAuthenticationInfo LoginLocationInteractive(string branchCode, string departmentCode)
		{
			LoginAuthenticationInfo result;
			bool retry;
			do
			{
				retry = false;
				result = Env.LoginController.LoginLocationEx(AuthenticatedUser, branchCode, departmentCode);
				if (result.State == LoginAuthenticationInfo.Status.OneMachinePerUserLimitExceeded)
				{
					retry = DoOneMachinePerUserLimitExceededCheck(result.LoginOnAnotherMachine);
					// If they don't want to force the remote logoff then should we give them the option
					// of changing to another user?
				}
			} while (retry);

			return result;
		}

		bool HandleLoginFailureForRetry(string username)
		{
			bool retry = false;

			if (username != null && !LoginWithTempPassword() &&
				(
					AuthenticatedUser.State == LoginAuthenticationInfo.Status.PasswordExpired
					||
					AuthenticatedUser.User != null && (AuthenticatedUser.User as GlbStaff).ChangePasswordAtNextLogin
				))
			{
				// If the new password is set sucessfully, we don't need to retry, otherwise return fail password error
				if (!DoExpiredPasswordCheck(username))
				{
					AuthenticatedUser = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.PasswordExpired, null);
				}
			}

			return retry;
		}

		internal bool HideUI { get; set; }

		void ReadArguments(CommandLineArguments arguments)
		{
			if (arguments[ApplicationArguments.OptionBranch] is string)
			{
				ForceBranch = (string)arguments[ApplicationArguments.OptionBranch];
			}

			if (arguments[ApplicationArguments.OptionDepartment] is string)
			{
				ForceDepartment = (string)arguments[ApplicationArguments.OptionDepartment];
			}

			if (arguments[ApplicationArguments.OptionVerboseLoginArgument] is string)
			{
				Env.LoginController.VerboseLoginFilename = (string)arguments[ApplicationArguments.OptionVerboseLoginArgument];
			}
		}

		/// <summary>
		/// Update the LoginDirector.Instance state when managing login state from outside the LoginDirector.
		/// </summary>
		/// <param name="authenticationInfo">Authentication result for user that tried to authenticate.</param>
		/// <param name="isLocationLoggedIn">Indicates if the user was successfully logged into a location or not.</param>
		public static void UpdateLoginState(LoginAuthenticationInfo authenticationInfo, bool isLocationLoggedIn)
		{
			Instance.AuthenticatedUser = authenticationInfo;
			Instance.LoggedInLocation = isLocationLoggedIn;
		}

		#region Standard Login

		bool DoOneMachinePerUserLimitExceededCheck(ISemaphoreInfo remoteLoginSemaphoreInfo)
		{
			string msg = Res.GetString("DEA1B556-5111-408e-B139-57F1D3A281CD",
				"This user is already logged in from machine {0} (since {1}).\r\nA user can be only logged in from one computer or terminal session at a time.\r\nYou may force the other login to exit. They will have {2} seconds to save their work.\r\n\r\nDo you want to force the other user to log off?",
				remoteLoginSemaphoreInfo.OwnerSession.HostName,
				remoteLoginSemaphoreInfo.CreateTimeUtc.ToLocalTime(),
				SelfLogoffForm.LogoffDelayInSeconds);

			bool result = DialogResult.Yes == Globals.Message.Show(msg, Res.GetString("a0d5f642-23dd-47ff-bb38-432b37a7ef2f", "User already logged in"), MessageBoxButtons.YesNo, MessageBoxIcon.Stop, DialogResult.No);

			if (result)
			{
				Env.LoginController.ForceRemoteLogoff(remoteLoginSemaphoreInfo);
			}

			return result;
		}

		internal bool DoExpiredPasswordCheck(string userName = null)
		{
			var result = (DialogResult)Env.LoginController.PromptPasswordChange(out var mustChange, userName);

			if (result == DialogResult.OK)
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Expired password" };
				var staff = userName != null
					? GlbStaff.LoadFromLoginName(factory, userName)
					: factory.Load<GlbStaff>(Env.CurrentUser.PK);
				factory.ReloadAllSafe<GlbStaff>();

				if (staff.AllowPasswordChange)
				{
					result = ChangePasswordCore(staff);
					if (result == DialogResult.OK)
					{
						using (GetUserContextForExpiredPasswordChange(staff))
						{
							try
							{
								factory.Save();
							}
							catch (ZSaveConcurrencyException ex)
							{
								result = DialogResult.No;
								ZExceptionReporting.HandleSaveException(ex);
								factory.Save();
							}
						}
					}
					return result == DialogResult.OK || !mustChange;
				}
			}

			return result == DialogResult.No;
		}

		protected virtual DialogResult ChangePasswordCore(GlbStaff staff) =>
			PasswordDialogs.Instance.ShowChangePasswordDialog(staff) ? DialogResult.OK : DialogResult.Cancel;

		IDisposable GetUserContextForExpiredPasswordChange(GlbStaff staff)
		{
			if (Env.CurrentUser != null)
			{
				return null;
			}

			var branch = Env.CurrentBranchPK;
			if (branch == Guid.Empty)
			{
				branch = staff.GS_GB_HomeBranch != Guid.Empty
					? staff.GS_GB_HomeBranch.ToGuid()
					: EnvProxy.GetAnyBranch(staff.Factory);
			}

			var department = Env.CurrentDepartmentPK;
			if (department == Guid.Empty)
			{
				department = staff.GS_GE_HomeDepartment != Guid.Empty
					? staff.GS_GE_HomeDepartment.ToGuid()
					: EnvProxy.GetAnyDepartment(staff.Factory);
			}

			return Env.SetTemporaryUserContext(new UserContext(staff, branch, department));
		}

		#endregion

		public LoginAuthenticationInfo AuthenticatedUser
		{
			get { return authenticatedUser ?? (authenticatedUser = LoginAuthenticationInfo.NewFailedLogin(null)); }
			set
			{
				authenticatedUser = value;
				if (value != null && value.IsOK)
				{
					Db.OnMainConnectionOpened -= CheckUserStatusAndTerminateClient;		// Preventing add this callback twice.
					Db.OnMainConnectionOpened += CheckUserStatusAndTerminateClient;
				}
				else
				{
					Db.OnMainConnectionOpened -= CheckUserStatusAndTerminateClient;
				}
			}
		}
		LoginAuthenticationInfo authenticatedUser;

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
#if DEBUG
		internal 
#endif
		void CheckUserStatusAndTerminateClient(object sender, EventArgs e)
		{
			if (DbLockout.HasLockout(Db.Connection))
			{
#if DEBUG
				CheckCount++;
#endif
				if (!CheckLockWorker.IsBusy)
				{
					CheckLockWorker.RunWorkerAsync();
				}
				return;
			}

			var sql = $@"SELECT GS_IsActive FROM dbo.GlbStaff WHERE GS_PK = @UserPK";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@UserPK", SqlDbType.UniqueIdentifier, AuthenticatedUser.User.PK);

				var isActive = command.ExecuteScalar();
				if (isActive == null || !(bool)isActive)
				{
					var terminateMessage = Res.GetString("cbc83619-6132-46d1-b5e4-8053ecf698d5",
						"Your account has been deactivated and the application will now terminate.");
					try
					{
						Globals.Message.ShowWarning(terminateMessage,
							Res.GetString("fa396353-6595-4b6b-9597-60a782ce0820", "Account Deactivated"));
					}
					catch
					{
						WindowsEventLogger.LogError(terminateMessage);
						Console.Error.Write(terminateMessage);
					}

					ExceptionReporter.SuppressGui();
					GCTracker.StopTracking();
					ApplicationDispatcher.Current?.BeginInvoke(new Action(Application.Exit));
					Thread.Sleep(10000);
					Process.GetCurrentProcess().Kill();
				}
			}
		}

		BackgroundWorker checkLockWorker;
		BackgroundWorker CheckLockWorker
		{
			get
			{
				if (checkLockWorker == null)
				{
					checkLockWorker = new BackgroundWorker();
					checkLockWorker.DoWork += new DoWorkEventHandler(StartDbLockCheck);
					checkLockWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnStartDbLockCheck);
				}
				return checkLockWorker;
			}
		}

#if DEBUG
		internal int CheckCount;
#endif

		void StartDbLockCheck(object sender, DoWorkEventArgs doWorkEventArgs)
		{
			Thread.Sleep(5 * 1000);
		}

		void OnStartDbLockCheck(object sender, RunWorkerCompletedEventArgs e)
		{
			CheckUserStatusAndTerminateClient(null, null);
		}

		public bool LoggedInLocation
		{
			get;
			private set;
		}

		public string ForceBranch
		{
			get;
			private set;
		}

		public string ForceDepartment
		{
			get;
			private set;
		}

		public override int FailureExitCode => ExitCodes.LoginDirectorError;
	}
}
