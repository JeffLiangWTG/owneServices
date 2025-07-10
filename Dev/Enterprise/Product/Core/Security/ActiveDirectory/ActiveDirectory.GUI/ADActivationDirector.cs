using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class ADActivationDirector : IADActivationDirector
	{
		public ADActivationDirector()
		{
			factory = new BusinessObjectFactory { NameForDebugging = nameof(ADActivationDirector) };
		}

		readonly BusinessObjectFactory factory;

		#region Enable/Disable

		public void EnableIntegration(EntitiesToSync entitiesToSync)
		{
			var results = new ActiveDirectoryStabilityChecker().Check(true);
			if (results.Length > 0)
			{
				var message = new ZStringBuilder(Res.GetString("ac4556b6-8cb7-415a-aa1b-fe550e580733", "Integration cannot be enabled."));
				message.Append(Res.GetString("754ae9a8-40f3-4672-bc39-c52d3834da1e", "Please disable integration, correct the issues below and save your changes before retrying to enable the integration:"));
				foreach (var result in results)
				{
					message.Append("- " + result.Description);
				}
				throw new DirectoryServicesException(message.ToStringWithNewLineBetweenAppends());
			}

			if (!ShowWarningBeforeProceeding(EnableWarning, (NoResString)"Enable", () => EnableIntegrationCore(entitiesToSync)))
			{
				throw new DirectoryServicesException(Res.GetString("6777790a-902f-49ea-a770-86fdb19c61ab", "Enabling integration canceled"));
			}

			HasChanges = true;
		}

		bool EnableIntegrationCore(EntitiesToSync entitiesToSync)
		{
			return ConfirmChanges(Activator.EnableIntegration(entitiesToSync));
		}

		bool ConfirmChanges(IEnumerable<EntitySynchronisedEventArgs> syncHistories)
		{
			var excelFile = ObjectFactory.Get<IActivationReportBuilder>().BuildReport(syncHistories);
			try
			{
				excelFile.PreviewInXl();

				if (!HasOneActiveNonOperationalOrControllerUser(syncHistories))
				{
					var errorMessage = Res.GetString("8DDC537D-6907-45B7-AA34-6FE3AF7B9CE5", @"AD Integration cannot be enabled.
Please ensure at least one active non-operational or controller staff can be synchronized with Active Directory before proceeding.");
					Globals.Message.ShowError(errorMessage);

					return false;
				}

				var message = Res.GetString("8e106b99-3451-42cd-9264-b768e748e1ad", @"Please thoroughly review the changes about to be made to your system before proceeding.");
				var caption = Res.GetString("53206409-055a-42f9-904b-bc24405e4dc0", "Confirm Changes");
				var result = UserNotification.ShowConfirmation(message, caption, (NoResString)"Continue", MessageBoxIcon.Question);

				return result == DialogResult.OK;
			}
			finally
			{
				excelFile.Dispose();
			}
		}

		bool HasOneActiveNonOperationalOrControllerUser(IEnumerable<EntitySynchronisedEventArgs> syncHistories)
		{
			foreach (var history in syncHistories)
			{
				if (history.Entity.EnterpriseEntity is GlbStaff user)
				{
					if (user.GS_IsActive && !user.GS_IsSystemAccount && (user.GS_IsController || !user.GS_IsOperational))
					{
						return true;
					}
				}
			}

			return false;
		}

		public void DisableIntegration(bool disableGroupOnly = false)
		{
			if (!ShowWarningBeforeProceeding(GetDisableWarning(disableGroupOnly), (NoResString)"Disable", () => Activator.DisableIntegration(disableGroupOnly)))
			{
				throw new DirectoryServicesException(Res.GetString("39dfdc8e-d581-4103-bfe4-7ef00b33003a", "Disabling integration canceled"));
			}
			HasChanges = true;
		}

		bool ShowWarningBeforeProceeding(string warningMessage, string confirmationString, Func<bool> action)
		{
			var confirmationResult = UserNotification.ShowConfirmation(warningMessage, Res.GetString("0501c844-b4cd-4f62-a761-e6db0ee32d84", "{0} Integration", confirmationString), confirmationString, MessageBoxIcon.Exclamation);
			return confirmationResult == DialogResult.OK
				&& action();
		}

		#endregion

		#region Dependencies

		public IADIntegrationActivator Activator
		{
			get { return activator ?? (activator = new ADIntegrationActivator(factory)); }
			set { activator = value; }
		}
		IADIntegrationActivator activator;

		public IUserNotification UserNotification
		{
			get { return userNotification ?? Globals.Message; }
			set { userNotification = value; }
		}
		IUserNotification userNotification;

		#endregion

		#region Warning messages

		public static string EnableWarning
		{
			get
			{
				return ResString.GetMultilingualString("2bd9f39c-cc38-4cfe-b318-4cdf65e89792", @"You are about to activate integration with Active Directory. Before proceeding, please ensure the following:

1. You have read the Active Directory Integration update note.
2. You have access to a user account with domain read/write privileges.
3. All user and group details are up-to-date in either {0} or Active Directory. Once integration is enabled you can choose which of these will be used as the primary source when performing the initial synchronization.
4. You are aware that any staff and groups that are not matched to Active Directory counterparts will be deactivated until they are matched by correcting their staff login name or group name.
5. The system requires at least one active Controller and one active Operational staff, you are aware that at least one of each staff must be successfully matched to Active Directory in order to meet this requirement.", BrandingFactory.Instance.ProductName);
			}
		}

		public static string GetDisableWarning(bool disableGroupOnly = false)
		{
			if (disableGroupOnly)
			{
				return ResString.GetMultilingualString("ac1f2832-ab91-4b42-a102-44276c88e3c8", @"You are about to disable integration with Active Directory for Group only. Before proceeding, please be aware that all existing group records will be disconnected from Active Directory and their details will no longer be synced.");
			}
			else
			{
				return ResString.GetMultilingualString("96a95f3a-c8d3-4fc8-995b-23130be35cc8", @"You are about to disable integration with Active Directory. Before proceeding, please be aware of the following:

1. All existing staff and group records will be disconnected from Active Directory and their details will no longer be synced.
2. The next time each staff member logs into {0} they will be required to change their password. The default intermediate password is defined in the registry item '{1}'.", BrandingFactory.Instance.ProductName, ((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual);
			}
		}

		#endregion

		#region SaveChanges

		public bool HasChanges { get; private set; }

		public void SaveChanges()
		{
			try
			{
				Activator.SaveChanges();
				HasChanges = false;
			}
			catch (ZCannotSaveException ex)
			{
				throw new RegistryValidationException(ex.Message);
			}
		}

		#endregion
	}
}
