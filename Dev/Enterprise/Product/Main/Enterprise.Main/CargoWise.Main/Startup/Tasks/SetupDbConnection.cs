using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.Startup
{
	public partial class SetupDbConnection : AbstractApplicationStartupTask
	{
		public override string TaskDescription => (NoResString)"Connecting to database";

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return true;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override bool DoExecute(CommandLineArguments arguments)
		{
			SetServerAndDatabaseNames(arguments);

			using (Db.DisableSchemaVersionCheck()) // SetupDbConnection is run before database upgrade startup task
			{
				try
				{
					DataUtils.ValidateMainDatabaseName(Db.DatabaseName);

					try
					{
						try
						{
							Db.Connection.EnsureIsOpen();
							if (DbLockout.HasLockout(Db.Connection))
							{
								HandleLockoutError(arguments);
							}
						}
						catch (DatabaseUpgradeException)
						{
							return HandleLockoutError(arguments);
						}
						catch (SqlException sqlEx)
						{
							if (Db.IsUpgradeLockoutError(sqlEx))
							{
								return HandleLockoutError(arguments);
							}
							else
							{
								throw;
							}
						}
					}
					catch (SqlException sqlEx) when (Db.HandleDbConnectionSetupErrors<RestrictedWriterLoginCredentials>(sqlEx, Db.ServerName, Db.DatabaseName, Db.ServerName))
					{
					}
					catch (SqlException sqlEx) when (new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.TlsCertificateError)
					{
						DbErrorMatch dbErrorMatch = new DbErrorMatch(sqlEx);
						StartupNotification.ShowError(dbErrorMatch.GetUserFriendlyMessage(Db.Connection), BrandingFactory.Instance.ProductName);
						return false;
					}
				}
				catch (SqlException sqlEx)
				{
					if (!SqlProxyClientProvider.IsHttpEnabled)
					{
						try
						{
							using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
							{
								if (IsRunningAnOldVersionOfSqlServer(adminConnection.ServerVersionNumber))
								{
									ShowUnsupportedSqlVersionPopUp();
									return false;
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
						}
					}

					string errorMessage =
						string.Format((NoResString)"Failed to connect to database {3}\\{0}. (Error Number: {2})\r\nPlease report the message below to the Systems Administrator.\r\n\r\n{1}",
							Db.DatabaseName,
							sqlEx.Message,
							sqlEx.Number.ToString(),
							Db.ServerName);
					StartupNotification.ShowError(errorMessage, BrandingFactory.Instance.ProductName);
					return false;
				}
				catch (Exception ex)
				{
					StartupNotification.ShowError(ex.Message, BrandingFactory.Instance.ProductName);
					return false;
				}
			}

			if (!SqlProxyClientProvider.IsHttpEnabled && IsRunningAnOldVersionOfSqlServer(Db.Connection.ServerVersionNumber))
			{
				ShowUnsupportedSqlVersionPopUp();
				return false;
			}

			return true;
		}

		bool IsRunningAnOldVersionOfSqlServer(SqlServerVersionNumber sqlServerVersion)
		{
			return overridableGetIsBelowMinSupportedSqlMajorVersion.Value.Invoke(sqlServerVersion);
		}

		void SetServerAndDatabaseNames(CommandLineArguments arguments)
		{
			string tempServerName = (arguments.ServerName == null) ? "." : arguments.ServerName.Trim();

			var serverName = Db.GetMachineNameIfLocal(tempServerName);
			var databaseName = arguments.DatabaseName ?? (NoResString)"Odyssey";

			Db.InitializeDatabaseDetails(serverName, databaseName);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "res is not available yet")]
		void ShowUnsupportedSqlVersionPopUp()
		{
			const string formTitle = "Application is closing";
			const string firstMessage = "Critical {0} Upgrade to MS {1}";
			StartupNotification.ShowError(SqlCutOverHelper.UpgradeToSupportedSqlVersionAction, formTitle,
				() => ShowRichTextEmailDisplayForm(formTitle, firstMessage, SqlCutOverHelper.UpgradeToSupportedSqlVersionAction, SqlServerVersionNumber.SqlMinimumSupportedGenerationEdition));
		}

		void ShowRichTextEmailDisplayForm(string title, string firstMessage, string context, string supportedSqlVersion)
		{
			using (var form = new RichTextEmailDisplayZForm(FormatMessage(firstMessage, context, supportedSqlVersion), title))
			{
				ControlDpiScalingHelper.SetHeight(form, 400, true);
				ControlDpiScalingHelper.SetWidth(form, 637, true);
				form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
				form.TopMost = true;
				form.BackColor = Color.White;
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		string FormatMessage(string firstMessage, string context, string supportedSqlVersion)
		{
			var detailMessage = context
				.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => Invariant($@"{x}\par{System.Environment.NewLine}"))
				.Aggregate((x, y) => x + y);
			var messageFrame = LoadFrameFromRtfFile();

			return string.Format(CultureInfo.InvariantCulture,
				messageFrame,
				string.Format(firstMessage, Enterprise.Core.Constants.ProductName, supportedSqlVersion),
				detailMessage);

			string LoadFrameFromRtfFile()
			{
				using (var rtfStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Startup.Tasks.SqlServerVersionDiscontinuedSupport.rtf"))
				using (var contentsReader = new StreamReader(rtfStream))
				{
					return contentsReader.ReadToEnd();
				}
			}
		}

		bool HandleLockoutError(CommandLineArguments arguments)
		{
			IDisposable adminConnectionElevation = (Db.Instance as IDbUpgradeSupport).ElevateToAdminConnectionForUpgrade();
			Db.AdminConnection.EnsureIsOpen();
			try
			{
				DbLockoutState state = Db.AdminConnection.CheckLockoutState();
				if (state == DbLockoutState.InvalidLockout)
				{
					if (arguments[ApplicationArguments.OptionUpgrade] != null)
					{
						neverDisposeAdminConnectionElevation = adminConnectionElevation;
						return true;
					}
					else
					{
#if DEBUG
						if ((bool)arguments[ApplicationArguments.OptionTestAdapter])
						{
							DbLockoutState restResult = Db.AdminConnection.ResetLockout();
							if (restResult != DbLockoutState.ResetLockout)
							{
								throw new Exception("Could not reset lockout");
							}
							adminConnectionElevation.Dispose();
							Db.Connection.EnsureIsOpen();
							return true;
						}
#endif
						if (!Globals.CanShowDialogs)
						{
							return false;
						}
						using (LoginForResolveLockoutForm loginForm = new LoginForResolveLockoutForm())
						{
							DialogResult dialogResult = ZFormModaliser.ShowDialogWithoutDispose(loginForm);
							if (dialogResult == DialogResult.OK)
							{
								if (loginForm.SelectedAction == LoginForResolveLockoutForm.ResolveLockoutAction.KeepWithUpgrade)
								{
									ShowUpgradeModule(loginForm.UserNameTextBox.Text);
									return false;
								}
								else
								{
									DbLockoutState resetResult = Db.ResetLockout();
									if (resetResult == DbLockoutState.ResetLockout)
									{
										StartupNotification.Show(
											(NoResString)"Application access to the database has been unlocked.",
											BrandingFactory.Instance.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
										return true;
									}
									else
									{
										StartupNotification.ShowError(
											(NoResString)"Could not unlock database access to the database.",
											BrandingFactory.Instance.ProductName);
									}
								}
							}
						}
					}
				}
				else if (state == DbLockoutState.ValidLockout)
				{
					adminConnectionElevation.Dispose();
					var exception = new DatabaseUpgradeInProgressException();
					DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(exception);
					return true;
				}
				else
				{
					// no lockout state, repair main user permissions and try again
					((IDbLoginRepair)Db.AdminConnection).EnsureWriterDbLoginCorrectlyMappedToAllDatabases();
					adminConnectionElevation.Dispose();
					Db.Connection.EnsureIsOpen();
					return true;
				}
			}
			finally
			{
				if (adminConnectionElevation != neverDisposeAdminConnectionElevation && Db.AdminConnection != null)
				{
					adminConnectionElevation.Dispose();
				}
			}
			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IDisposable neverDisposeAdminConnectionElevation;

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "User is being logged in")]
		void ShowUpgradeModule(string loginName)
		{
#if !WINZOR
			try
			{
				Env.Instance.SetUserContext(new UserContext(loginName, AnyBranch, AnyDepartment));

				BusinessObjectFactory factory = new BusinessObjectFactory(Db.AdminConnection);
				using (StmUpgradeForm form = new StmUpgradeForm(new StmUpgradeCollectionContainer(factory)))
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
			catch (Exception ex)
			{
				throw new ShowUpgradeModuleException(ex);
			}
#endif
		}

#if !WINZOR
		static Guid AnyBranch
		{
			get { return Utilities.GetGuidFromTopRowInTable(GlbBranchSchema.Constants.PK, GlbBranchSchema.Constants.TableName); }
		}

		static Guid AnyDepartment
		{
			get { return Utilities.GetGuidFromTopRowInTable(GlbDepartmentSchema.Constants.PK, GlbDepartmentSchema.Constants.TableName); }
		}
#endif
		public override int FailureExitCode => ExitCodes.SetupDbConnectionError;

		[Serializable]
		class ShowUpgradeModuleException : Exception
		{
			public ShowUpgradeModuleException(Exception innerException)
				: base(innerException.Message, innerException)
			{ }

#if NETFRAMEWORK
			protected ShowUpgradeModuleException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		readonly Overridable<Func<SqlServerVersionNumber, bool>> overridableGetIsBelowMinSupportedSqlMajorVersion = new Overridable<Func<SqlServerVersionNumber, bool>>(x => !x.IsMinimumRequiredVersionOrAbove);
	}
}

#if DEBUG

namespace Enterprise.Startup
{
	public partial class SetupDbConnection
	{
		public IDisposable OverrideIsBelowMinSupportedSqlMajorVersion(Func<SqlServerVersionNumber, bool> testFunc)
		{
			overridableGetIsBelowMinSupportedSqlMajorVersion.Value = testFunc;
			return new DisposableAction(() => overridableGetIsBelowMinSupportedSqlMajorVersion.ResetValue());
		}

		public string firstMessage { get; set; }
		public string context { get; set; }

		public string FormatMessage_ExposedForTest(string firstMessage, string context, string supportedSqlGeneration)
		{
			return GetFormatTextFromFormatMessage(FormatMessage(firstMessage, context, supportedSqlGeneration));

			string GetFormatTextFromFormatMessage(string formatMessage)
			{
				using (var box = new KRichTextBox())
				{
#if !WINZOR
					box.Rtf = formatMessage;
#else
					box.Html = formatMessage;
#endif
					return box.Text;
				}
			}
		}
	}
}
#endif
