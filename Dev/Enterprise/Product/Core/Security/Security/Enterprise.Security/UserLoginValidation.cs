using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.Security
{
	class UserLoginValidation
	{
		public UserLoginValidation(string loginName, string password, string twoFactorAuthenticationCode = null, Dictionary<Guid, string> twoFactorAuthenticationCodeTable = null, Guid? activeDirectoryObjectGuid = null, bool support2FA = false)
		{
			this.loginName = loginName;
			this.password = password;
			this.support2FA = support2FA;
			this.twoFactorAuthenticationCode = twoFactorAuthenticationCode;
			this.twoFactorAuthenticationCodeTable = twoFactorAuthenticationCodeTable;
			this.activeDirectoryObjectGuid = activeDirectoryObjectGuid;
		}

		bool ValidateWithSingleSignOn => ADRegistry.IsSingleSignOnEnabled && activeDirectoryObjectGuid.HasValue;

		internal string StaffCodeForUpgrade { get; private set; }

		internal IUser User { get; private set; }

		readonly string loginName;
		readonly string password;
		readonly Guid? activeDirectoryObjectGuid;
		readonly string twoFactorAuthenticationCode;
		readonly bool support2FA;
		CWSupportTokenValidationResult tokenValidationResult;
		readonly Dictionary<Guid, string> twoFactorAuthenticationCodeTable;

		IUser GetUser(out LoginAuthenticationInfo loginAuthenticationInfo)
		{
			loginAuthenticationInfo = null;
			GlbStaff staff = null;
			if (activeDirectoryObjectGuid.HasValue)
			{
				// For single sign-on
				staff = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, (ZGuid)activeDirectoryObjectGuid.Value);
			}
			else if (!string.IsNullOrEmpty(loginName))
			{
				try
				{
					staff = GlbStaff.LoadFromLoginName(new BusinessObjectFactory(), loginName);
				}
				catch (UserExistsInMultipleDomainsException e)
				{
					loginAuthenticationInfo = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserExistsInMultipleDomains, e.Message);
					return null;
				}
			}
			staff?.ReloadSafe(); // To ensure the user has the latest data from the DB
			return staff;
		}

		public LoginAuthenticationInfo Validate(bool isDeviceLoginAttempt = false, TemporaryPassword temporaryPasswordCache = null)
		{
			LoginAuthenticationInfo result;

			var clientHook = ClientHookLoader.Instance.ClientHook;
			if (clientHook != null && !clientHook.IsValidLogin(loginName, password))
			{
				result = LoginAuthenticationInfo.NewFailedLogin(clientHook.LoginDeniedMessage);
			}
			else
			{
				User = GetUser(out result);
				if (User == null && result != null)
				{
					return result;
				}
				else if (User == null || User.IsResource)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, LoginFailMsg);
				}
				else if (!User.CanLogin)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, CLUserLoginFailMsg);
				}
				else if (User.IsDeviceOnly && !isDeviceLoginAttempt)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, LoginIsForDeviceOnlyMsg);
				}
				else if (User.IsWebUser && !Globals.IsWeb)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.WebUserCannotLogin, LoginFailMsg);
				}
				else if (User.IsSysAdmin && !IsSysAdminLogonAllowed())
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.DisallowedSysAdminLogon, InvalidSysadminLogonMsg);
				}
				else if (!User.IsSysAdmin && !User.IsActive)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserInactive, InactiveLoginMsg);
				}
				else if (User.IsLockedOut)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserLockedOut, LockedoutLoginMsg);
				}
				else if (LocalPasswordMustBeReset())
				{
					result = LoginWithTemporaryPassword(temporaryPasswordCache);
				}
				else
				{
					var loginToken = new LoginToken();
					User.LoginToken = loginToken;
					var validateCredentials = IsLoginDetailsCorrect(User.PK, User, User.IsSystemAccount, loginToken);
					if (validateCredentials == ValidateCredentialsResult.OK)
					{
						var restriction = CheckIPAddressRestriction(User);
						if (!string.IsNullOrEmpty(restriction))
						{
							result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, restriction);
						}
						else
						{
							result = ValidateUsingTwoFactorIfRequired();
						}
					}
					else if (validateCredentials == ValidateCredentialsResult.ADRecordNotCreated)
					{
						result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.ADRecordNotFound, UserNotInAD);
					}
					else if (validateCredentials == ValidateCredentialsResult.ADRecordNotFound)
					{
						result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.ADRecordInvalid, UserInvalidInAD);
					}
					else if (validateCredentials == ValidateCredentialsResult.LockedOut)
					{
						result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserLockedOut, LockedoutLoginMsg);
					}
					else if (validateCredentials == ValidateCredentialsResult.ADError)
					{
						result = LoginAuthenticationInfo.NewFailedLogin(ADErrorMsg);
					}
					else
					{
						LoginAuthenticationInfo.Status validateResult = validateCredentials == ValidateCredentialsResult.PasswordExpired
							? LoginAuthenticationInfo.Status.PasswordExpired
							: LoginAuthenticationInfo.Status.PasswordInvalid;
						var message = User.IsSupportUser ? tokenValidationResult?.FailedReason : LoginFailMsg;
						result = LoginAuthenticationInfo.NewFailedLogin(validateResult, message);
					}
				}
			}

			return result;

			bool LocalPasswordMustBeReset() => !User.IsSystemAccount
				&& !CWSupportLoginToken.IsValidToken(password)
				&& !ADRegistry.IsIntegrationEnabled
				&& User.LocalPasswordMustBeReset;
		}

		LoginAuthenticationInfo LoginWithTemporaryPassword(TemporaryPassword temporaryPasswordCache)
		{
			if (temporaryPasswordCache == null)
			{
				return LoginAuthenticationInfo.NewTempPasswordRequired(User);
			}
			else
			{
				var result = temporaryPasswordCache.Validate(loginName, password);
				switch (result)
				{
					case TemporaryPasswordValidationResult.OK:
						return LoginAuthenticationInfo.NewTempPasswordLoginSuccessfully(User);
					case TemporaryPasswordValidationResult.Expired:
						return LoginAuthenticationInfo.NewTempPasswordRequired(User);
					default:
						return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.TempPasswordRequired, TempPasswordInvalidtMsg);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry item key")]
		LoginAuthenticationInfo ValidateUsingTwoFactorIfRequired()
		{
			{
				var twoFactorAuthenticationType = SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				if (twoFactorAuthenticationType != "None" && User.IsTwoFactorAuthenticationEnabled)
				{
					if (twoFactorAuthenticationCode == null)
					{
						if (support2FA)
						{
							// Only request 2FA if 2FA code is not provided in the login and support2FA is set to true
							ITwoFactorAuthenticationEngine codeSender;
							switch (twoFactorAuthenticationType)
							{
								case "Email":
									if (string.IsNullOrWhiteSpace(User.EmailAddress))
									{
										return LoginAuthenticationInfo.NewTwoFactorAuthenticationFieldMissing(User);
									}
									codeSender = ObjectFactory.Get<ITwoFactorAuthenticationEngine>("TwoFactorAuthEmailSender");
									break;

								//case "SMS":

								default:
									throw new InvalidOperationException($"Current setting \"{twoFactorAuthenticationType}\" for two factor authentication is not supported or valid");
							}

							twoFactorAuthenticationCodeTable[User.PK] = codeSender.SendTwoFactorAuthenticationCode(User);
							return LoginAuthenticationInfo.NewTwoFactorAuthenticationLogin(User); // LoginValidated but not IsOK (status == TwoFactorAuthenticationRequired)
						}
					}
					else
					{
						// If 2FA code is provided as part of the login, always verify the 2FA code regardless of support2FA
						if (IsTwoFactorAuthenticationCodeCorrect(User.PK, twoFactorAuthenticationCode) != ValidateCredentialsResult.OK)
						{
							return LoginAuthenticationInfo.NewTwoFactorAuthenticationFailedLogin(User); // LoginValidated and not IsOK (status == TwoFactorAuthenticationFailed)
						}
					}
				}
			}

			// otherwise, it is s successful login (LoginValidate and IsOK)
			return LoginAuthenticationInfo.NewSuccessfulLogin(User);
		}

		ValidateCredentialsResult IsTwoFactorAuthenticationCodeCorrect(Guid pK, string twoFactorAuthenCode)
		{
			Argument.NotNull(twoFactorAuthenticationCodeTable, nameof(twoFactorAuthenticationCodeTable));
			if (!twoFactorAuthenticationCodeTable.TryGetValue(pK, out var correctCode))
			{
				throw new ArgumentException("The system did not generate a code for this user. This should never happen.");
			}
			if (correctCode == twoFactorAuthenCode)
			{
				return ValidateCredentialsResult.OK;
			}
			else
			{
				return ValidateCredentialsResult.Invalid;
			}
		}

		bool IsSysAdminLogonAllowed()
		{
			return (
				(!ControllerOrNonOperationalUserExistsAndCanLogin())
				|| ObjectFactory.Get<IProductRegistration>().LocalVerify() == ProductRegistrationVerifyResult.Unregistered
			);
		}

		bool ControllerOrNonOperationalUserExistsAndCanLogin()
		{
			var isControllerOrNonOperationalQuery = new ZQuery(GlbStaffSchema.GS_IsController, 1);
			isControllerOrNonOperationalQuery.AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_IsOperational, 0);

			var mainQuery = new ZQuery(GlbStaffSchema.GS_IsActive, 1);
			mainQuery.AddToFilter(GlbStaffSchema.GS_CanLogin, 1);

			var passwordHashNotEmptyQuery = new ZQuery(GlbStaffSchema.GS_PasswordHash, SQLComparisonOperator.NotEqual, null);
			passwordHashNotEmptyQuery.AddToFilter(GlbStaffSchema.GS_PasswordSalt, SQLComparisonOperator.NotEqual, null);
			passwordHashNotEmptyQuery.AddToFilter(GlbStaffSchema.GS_PasswordHashIterations, SQLComparisonOperator.NotEqual, 0);

			mainQuery.AddToFilter(passwordHashNotEmptyQuery);
			mainQuery.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, 0);
			mainQuery.AddToFilter(isControllerOrNonOperationalQuery, JoinCondition.And);

			mainQuery.ReLoadExistingRows = true;
			return new BusinessObjectFactory().Exists(typeof(GlbStaff), mainQuery);
		}

		internal static ISemaphoreInfo GetLoginOnAnotherMachine(Guid userPk)
		{
			ISemaphoreInfo remoteLoginSemaphoreInfo = null;
			var activeHandles = EnvProxy.Instance.SemaphoreProvider.GetRemoteActiveSemaphoreHandles(Globals.IsWinzor ? new EnterpriseWinzorActiveLoginSemaphore() : new EnterpriseActiveLoginSemaphore(), userPk, Globals.ClientIdentifier);
			if (activeHandles.Length > 0)
			{
				DateTime earliestLogin = DateTime.MaxValue;
				foreach (var handle in activeHandles)
				{
					if (earliestLogin > handle.CreateTimeUtc)
					{
						earliestLogin = handle.CreateTimeUtc;
						remoteLoginSemaphoreInfo = handle;
					}
				}
			}
			return remoteLoginSemaphoreInfo;
		}

		public static string CheckIPAddressRestriction(IUser loggedInUser)
		{
			string restriction = null;
			if (loggedInUser.IsOperational && !loggedInUser.IsSupportUser)
			{
				var clientIpRestriction = RawDataRegistry.Instance.ClientIPAddressRestriction.Value;
				if (!string.IsNullOrWhiteSpace(clientIpRestriction))
				{
					var terminalService = ObjectFactory.Get<TerminalService>();
					if (!terminalService.IsRemoteAppSession)
					{
						if (!clientIpRestriction.Contains((NoResString)"localhost", StringComparison.OrdinalIgnoreCase))
						{
							restriction = Res.GetString("22958A80-7B37-4590-96DF-E0391F07334C", "Access from the hosting server is not permitted. To enable add \"LOCALHOST\" in the IP Address Restriction list.");
						}
					}
					else if (!EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal.Contains(EnterpriseChannelMessageTypes.GetClientIPAddress))
					{
						restriction = Res.GetString("C1A2460D-7C6B-4995-A7A4-0001818C5164", "IP Address Restriction is not supported because {0} Service Plug-in is not installed.", terminalService.IsCitrixICA ? (NoResString)"Citrix" : (NoResString)"Remote Desktop");
					}
					else
					{
						try
						{
							var productKey = ObjectFactory.Get<IProductRegistration>().Key;
							var clientIpAddress = IPAddress.Parse(ObjectFactory.Get<IWiseCloudSecurityClient>().GetClientIPAddress(productKey.EnterpriseCode + productKey.ServerCode, loggedInUser.LoginName));
							if (!IPAddressRanges.Parse(clientIpRestriction).IsInRange(clientIpAddress))
							{
								restriction = Res.GetString("EBF60C62-09E9-4C3E-AE50-FA74C5BA2537", "{0} Accessing from IP: {1}", RawDataRegistry.Instance.ClientIPAddressRestrictedMessage.Value, clientIpAddress.ToString());
							}
						}
						catch (FormatException ex)
						{
							restriction = Res.GetString("35360990-370A-44E6-AAB3-3906869A82DE", "IP address format was invalid: {0}", ex.Message);
						}
						catch (IOException ex)
						{
							restriction = Res.GetString("040A2650-22C8-4DF1-9539-8189DA1B4B0B", "Failed to find path: {0}", ex.Message);
						}
						catch (Exception ex) when (ex.InnerException is RDPRemoteException remoteException)
						{
							restriction = Res.GetString("EEE2141C-B6AE-4639-89C7-42B3891F66F0", "An error occurred while verifying your location due to a Remote Desktop issue: {0}", remoteException.RemoteExceptionMessage);
						}
					}
				}
			}
			return restriction;
		}

		public enum ValidateForUpgradeResult
		{
			OK,
			Invalid,
			NotController,
		}

		/// <summary>
		/// Validates the login for upgrade purposes (without actually logging in to the application):
		///	  1. validates the credentials
		///	  2. checks if it is a controller
		///	 Called before the DB is upgraded. Cannot use BizObj or anything which references the latest schema.
		/// </summary>
		public ValidateForUpgradeResult ValidateForUpgrade()
		{
			ValidateForUpgradeResult result = ValidateForUpgradeResult.Invalid;
			if (loginName != null)
			{
				string sqlText = @"EXEC sp_executesql N'SELECT GS_PK, GS_Code, GS_PasswordHashIterations, GS_PasswordSalt, GS_PasswordHash, GS_IsController, GS_IsSystemAccount FROM dbo.GlbStaff WHERE CAST(GS_IsActive AS CHAR(1)) in (''Y'', ''1'') AND GS_LoginName = @LoginName', N'@LoginName nvarchar(200)', @LoginName = @LoginName";
				Guid staffPK = Guid.Empty;
				PasswordStoredForUpgrade storedPwd = null;
				var isController = false;
				var isSystemAccount = false;

				using (DbCommand command = Db.Connection.Command(sqlText))
				{
					command.AddParameter("@LoginName", SqlDbType.Char, loginName);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							staffPK = reader.GetGuid(reader.GetOrdinal(GlbStaffSchema.Constants.PK));
							StaffCodeForUpgrade = reader.GetString(reader.GetOrdinal(GlbStaffSchema.Constants.GS_Code)).TrimEnd();
							string isControllerString = reader.GetValue(reader.GetOrdinal(GlbStaffSchema.Constants.GS_IsController)).ToString();
							isController = isControllerString == "Y" || isControllerString == true.ToString();
							string isSystemAccountString = reader.GetValue(reader.GetOrdinal(GlbStaffSchema.Constants.GS_IsSystemAccount)).ToString();
							isSystemAccount = isSystemAccountString == "Y" || isSystemAccountString == true.ToString();

							storedPwd = new PasswordStoredForUpgrade
							{
								StaffPK = staffPK
							};

							if (reader.HasColumn(GlbStaffSchema.Constants.GS_PasswordHash))
							{
								storedPwd.PasswordHash = reader.ReadAllBytes(reader.GetOrdinal(GlbStaffSchema.Constants.GS_PasswordHash));
								storedPwd.PasswordSalt = reader.ReadAllBytes(reader.GetOrdinal(GlbStaffSchema.Constants.GS_PasswordSalt));
								storedPwd.PasswordHashIterations = reader.GetInt32(reader.GetOrdinal(GlbStaffSchema.Constants.GS_PasswordHashIterations));
							}
						}
					}
				}

				if (staffPK != Guid.Empty &&
					ValidateCredentialsResult.OK == IsLoginDetailsCorrect(staffPK, storedPwd, isSystemAccount, new LoginToken()))
				{
					if (!isController)
					{
						result = ValidateForUpgradeResult.NotController;
					}
					else
					{
						result = ValidateForUpgradeResult.OK;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Common method for validating login and password.
		/// May be called before the DB is upgraded. Cannot use BizObj or anything which references the latest schema.
		/// </summary>
		ValidateCredentialsResult IsLoginDetailsCorrect(Guid userPK, IPasswordStored passwordStored, bool isSystemAccount, LoginToken loginToken)
		{
			ValidateCredentialsResult result;

			if (ADRegistry.IsIntegrationEnabled && !isSystemAccount)
			{
				bool isADObjectGuidValid;
				if (activeDirectoryObjectGuid.HasValue)
				{
					isADObjectGuidValid = true;
				}
				else
				{
					isADObjectGuidValid = false;

					string sqlText = @"IF EXISTS (SELECT * FROM information_Schema.COLUMNS WHERE TABLE_NAME = 'GlbStaff' AND COLUMN_NAME = 'GS_ActiveDirectoryObjectGuid')
						SELECT GS_ActiveDirectoryObjectGuid FROM dbo.GlbStaff WHERE GS_PK = @UserPK";

					using (var command = Db.Connection.Command(sqlText))
					{
						command.AddParameter("@UserPK", SqlDbType.UniqueIdentifier, userPK);
						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								var value = reader.GetValue(0);
								if (value != DBNull.Value)
								{
									isADObjectGuidValid = new ZGuid(value).IsValid;
								}
							}
							else
							{
								isADObjectGuidValid = true;
							}
						}
					}
				}

				if (!isADObjectGuidValid)
				{
					result = ValidateCredentialsResult.ADRecordNotCreated;
				}
				else if (IfLoginNameIsCWSupport() && CWSupportLoginToken.IsValidToken(password, out tokenValidationResult))
				{
					result = ValidateCredentialsResult.OK;
					loginToken.LoggedInWithSupportToken = true;
				}
				else if (ValidateWithSingleSignOn)
				{
					result = WindowsAuthenticationProvider.ValidateCurrentUserSession(activeDirectoryObjectGuid.Value);
				}
				else
				{
					result = WindowsAuthenticationProvider.ValidateCredentials(userPK, password);
				}
			}
			else
			{
				var loginCorrect = passwordStored?.VerifyPassword(password) ?? false;

				if (!loginCorrect && IfLoginNameIsCWSupport() && CWSupportLoginToken.IsValidToken(password, out tokenValidationResult))
				{
					loginCorrect = true;
					loginToken.LoggedInWithSupportToken = true;
					loginToken.SupportTokenUserCode = tokenValidationResult.UserCode;
					loginToken.SupportTokenIncident = tokenValidationResult.Incident;
					loginToken.SupportTokenUserName = tokenValidationResult.UserName;
				}

				result = loginCorrect ? ValidateCredentialsResult.OK : ValidateCredentialsResult.Invalid;
			}

			return result;
		}

		bool IfLoginNameIsCWSupport() => ZArchitecture.Environment.User.SupportUserName.Equals(loginName, StringComparison.InvariantCultureIgnoreCase);

		public IWindowsUserAuthenticationProvider WindowsAuthenticationProvider
		{
			get { return windowsAuthenticationProvider ?? (windowsAuthenticationProvider = ObjectFactory.Get<IWindowsUserAuthenticationProvider>()); }
			set { windowsAuthenticationProvider = value; }
		}
		IWindowsUserAuthenticationProvider windowsAuthenticationProvider;

		public IADRegistry ADRegistry
		{
			get { return adIntegrationModeProvider ?? (adIntegrationModeProvider = ObjectFactory.Get<IADRegistry>()); }
			set { adIntegrationModeProvider = value; }
		}
		IADRegistry adIntegrationModeProvider;

		internal static string LoginFailMsg => Res.GetString("69df04ce-7175-49e7-ac23-656d2b7f577f", "You must enter a correct user name and/or password. Please try again.");
		internal static string InactiveLoginMsg => Res.GetString("54b8c706-813a-4925-9452-c856b8137e4a", "Your login has been disabled. Please see your system administrator to re-activate your login.");
		internal static string InvalidSysadminLogonMsg => Res.GetString("9b1717a5-8b97-418e-b499-1c2464e0976f", "sysadmin can only be used when there are no other controller or non-operational users.");
		internal static string LockedoutLoginMsg => Res.GetString("fd991f03-e155-4d0a-a9a7-cc4d5e3a840b", "Your login is currently locked out due to a previous failed login. Please try again later or see your system administrator.");
		internal static string OneMachinePerUserLimitExceededMsg => Res.GetString("c9cfc4c2-8ebd-4e91-94d0-402054172504", "This user is already logged in from another machine");
		internal static string UserNotInAD => Res.GetString("c2bf7d09-b3d6-48da-8dc7-69740b5c0973", "This staff record does not have an AD User created or linked to it yet. Please try again later.");
		internal static string UserInvalidInAD => Res.GetString("62213996-494d-439f-b602-2a6f766f26dd", "Your Active Directory account is not configured correctly. Please try again later or see your system administrator.");
		internal static string LoginIsForDeviceOnlyMsg => Res.GetString("66390F13-C8B5-4CBE-B1DB-F9B4ED56644D", "You do not have the appropriate settings to login, this login is for device only.");
		internal static string CLUserLoginFailMsg => Res.GetString("22BB3ECC-2C9B-45AF-B52A-5FD9548E0F27", "This account has been set as a “Cannot Login User” and cannot be used to login to {0}.", Enterprise.Core.Constants.ProductName);
		internal static string TempPasswordInvalidtMsg => Res.GetString("48029ECF-B909-4A1E-A38A-2D3D2B1AC954", "The password is invalid or has expired. Verify that you have entered the password correctly or request a new temporary password.");
		internal static string ADErrorMsg => Res.GetString("680BAD1F-322B-4D78-B32F-552CDEED939F", "The service is currently unavailable, please try again later.");
		internal static string UserNotInIdp => Res.GetString("855bd663-be0d-4d79-8e4f-3a0b64d9b22e", "This staff record has not been synchronized into Identity Provider yet. Please try again later.");
		internal static string UserNotVerified => Res.GetString("df5fb0a6-b66f-4429-946f-9053ec8347a1", "The sign-up process has not been completed at the Identity Provider, user has not been verified. Please check email.");
	}

	class LoginToken : ILoginToken
	{
		public bool IsDeveloper { get; internal set; }
		public bool LoggedInWithSupportToken { get; internal set; }
		public string SupportTokenUserCode { get; set; }
		internal string SupportTokenIncident { get; set; }
		public bool ForcedRemoteLogoff { get; set; }
		public string SupportTokenUserName { get; set; }
		public string DummyTokenOriginalUserName { get; set; }
		public string DummyTokenOriginalUserEmail { get; set; }
	}

	class PasswordStoredForUpgrade : IPasswordStored
	{
		public Guid StaffPK { get; set; }

		public UserSecretHashAlgorithm PasswordAlgorithm => UserSecretHashAlgorithm.Pbkdf2HmacSha1;

		public int PasswordHashIterations { get; set; }

		public ZBlob PasswordHash { get; set; }

		public ZBlob PasswordSalt { get; set; }

		public ZGuid Identifier => StaffPK;

		bool IPasswordStored.VerifyPassword(IUserSecretsContext userSecretsContext, string password)
		{
			if (password != null)
			{
				if (!PasswordHash.IsEmpty && !PasswordSalt.IsEmpty)
				{
					return userSecretsContext.IsMatchingSecret(password, new PasswordStoredAdapter(this));
				}
			}
			return false;
		}
	}

	struct PasswordStoredAdapter : IUserSecretHashComponents
	{
		public PasswordStoredAdapter(PasswordStoredForUpgrade record)
		{
			this.record = record;
		}

		readonly PasswordStoredForUpgrade record;

		public UserSecretHashAlgorithm Algorithm => record.PasswordAlgorithm;
		public byte[] Hash => record.PasswordHash;
		public byte[] Salt => record.PasswordSalt;
		public int IterationsCount => record.PasswordHashIterations;
		public void Update(UserSecretHashAlgorithm algorithm, byte[] hash, byte[] salt, int iterationsCount) => throw new NotImplementedException();
	}
}
