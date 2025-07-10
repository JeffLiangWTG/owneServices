using System;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#if WINZOR
using CargoWise.Common;
using CargoWise.Definitions;
using WinzorFramework;
#endif

namespace Enterprise.Environment
{
	public sealed class WinFormsDbEnvironment : BaseDbEnvironment
	{
		public override IDbConnectionGuiPlugin ConnectionGuiPlugin { get; } = new WinFormsAppDbConnectionGuiPlugin(new DatabaseUpgradedExceptionHandler(), new VersionUpgradedHandler(), new UpgradeCheckHelper(), new DbUpgradingWarningManager());
	}

	public partial class WinFormsAppDbConnectionGuiPlugin : IDbConnectionGuiPlugin
	{
		public WinFormsAppDbConnectionGuiPlugin(IDatabaseUpgradedExceptionHandler databaseUpgradedExceptionHandler, IVersionUpgradedHandler versionUpgradedHandler, IUpgradeCheckHelper upgradeCheckHelper, IDbUpgradingWarningManager dbUpgradingWarningManager)
		{
			this.databaseUpgradedExceptionHandler = databaseUpgradedExceptionHandler;
#if WINZOR
			this.versionUpgradedHandler = versionUpgradedHandler;
			this.upgradeCheckHelper = upgradeCheckHelper;
#endif
			this.dbUpgradingWarningManager = dbUpgradingWarningManager;
		}

		public IDisposable NewConnectingSplashFormManager()
		{
			return new ConnectingFormManager();
		}

		public bool GetUserConfirmation(string caption, string yesNoQuestion)
		{
			using (KForm.TemporarilyDisableQueryFormCaptionSuffix())
			{
				var result = ZArchitecture.Environment.Globals.Message.Show(
					yesNoQuestion, caption,
					ZMessageBoxButtons.YesNo,
					ZMessageBoxIcon.Question,
					ZDialogResult.No);

				return (result == ZDialogResult.Yes);
			}
		}

		public void CheckVersionUpgraded()
		{
#if WINZOR
			if (upgradeCheckHelper.HasBeenUpgraded())
			{
				versionUpgradedHandler.Exit(ExitCodes.VersionUpgraded);
			}
#endif
		}

		public void HandleDatabaseUpgradeException(DatabaseUpgradeException ex)
		{
			switch (ex)
			{
				case DatabaseUpgradedException _:
#if !WINZOR
					// Disable schema checks since Restart() can hit the registry, which is allowed even after a db upgrade.
					// Do it permanently since:
					// - the program is about to exit so permanently is not very long
					// - this method can be called recursively so we don't want to re-enable on return
					Db.DisableThreadSchemaVersionCheckPermanently();
					databaseUpgradedExceptionHandler.Restart();
#else
					databaseUpgradedExceptionHandler.Exit(ExitCodes.DatabaseUpgraded);
#endif
					return;
				case DatabaseUpgradeInProgressException _:
					dbUpgradingWarningManager.ShowWarningAndWait();

					NotifyDatabaseUpgradeInProgressExceptionIsHandled_ForTest();

					return;

				default:
					throw new ArgumentException("Unknown DatabaseUpgradeException type", ex);
			}
		}

		public void HandleDbConcurrencyException(Exception ex)
		{
			new ConcurrencyExceptionHandler(ex).NotifyUserAndDevelopersIfNotAlreadyProcessed();
		}

		readonly IDatabaseUpgradedExceptionHandler databaseUpgradedExceptionHandler;
#if WINZOR
		readonly IVersionUpgradedHandler versionUpgradedHandler;
		readonly IUpgradeCheckHelper upgradeCheckHelper;
#endif
		readonly IDbUpgradingWarningManager dbUpgradingWarningManager;

		partial void NotifyDatabaseUpgradeInProgressExceptionIsHandled_ForTest();
	}
}

#region Test
#if DEBUG

#region Partial Class

namespace Enterprise.Environment
{
	using CargoWise.Common;

	public partial class WinFormsAppDbConnectionGuiPlugin
	{
		partial void NotifyDatabaseUpgradeInProgressExceptionIsHandled_ForTest()
		{
			GetOverridableUpgradeInProgressHandled_ForTest.Value?.Invoke();
		}

		internal static Overridable<Action> GetOverridableUpgradeInProgressHandled_ForTest { get; set; } = new Overridable<Action>(null);
	}
}

#endregion // Partial Class
#endif
#endregion
