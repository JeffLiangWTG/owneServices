using System;
using System.Globalization;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADPasswordSettingsManager : NonPersistentBusinessObject
	{
		public ADPasswordSettingsManager() : base()
		{
			BizO = new ADPasswordSettings();
		}

		public ADPasswordSettings BizO { get; }

		public bool SaveToAD()
		{
			try
			{
				if (BizO.OverrideDomainPasswordPolicy)
				{
					var adPso = LoadOrCreateADPasswordSettingsObject();
					if (adPso != null)
					{
						WriteMaximumPasswordAge(BizO, adPso);
						WriteMinimumPasswordAge(BizO, adPso);
						WriteMinimumPasswordLength(BizO, adPso);
						WritePasswordHistoryLength(BizO, adPso);
						WritePasswordComplexityEnabled(BizO, adPso);
						WritePasswordLockoutPolicy(BizO, adPso);

						adPso.CommitChanges();
						return true;
					}
				}
				else
				{
					return DeleteADPasswordSettingsObject();
				}
			}
			catch (DirectoryServicesException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (COMException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			return false;
		}

		public bool LoadFromAD()
		{
			try
			{
				var searcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials, false);
				var adPso = searcher.FindPasswordSettings(BizO.ADPasswordSettingsObjectName);
				if (adPso == null)
				{
					adPso = searcher.FindPasswordSettings(GetOldADPasswordSettingsObjectName());
					if (adPso != null)
					{
						adPso.Rename(BizO.ADPasswordSettingsObjectName);
					}
				}

				BizO.OverrideDomainPasswordPolicy = adPso != null;
				if (adPso != null)
				{
					ReadMaximumPasswordAge(BizO, adPso);
					ReadMinimumPasswordAge(BizO, adPso);
					ReadMinimumPasswordLength(BizO, adPso);
					ReadPasswordHistoryLength(BizO, adPso);
					ReadPasswordComplexityEnabled(BizO, adPso);
					ReadPasswordLockoutPolicy(BizO, adPso);
					BizO.HasChanges = false;
				}
				return true;
			}
			catch (DirectoryServicesException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (COMException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			return false;
		}

		public ZString GetOldADPasswordSettingsObjectName()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return registrationKey.EnterpriseCode + EnvProxy.Instance.CurrentCompany.Code + registrationKey.ServerCode + "_PasswordPolicy";
		}

		public bool DeleteADPasswordSettingsObject()
		{
			try
			{
				var searcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials, true);

				var policyObject = searcher.FindPasswordSettings(BizO.ADPasswordSettingsObjectName);
				if (policyObject != null)
				{
					policyObject.Delete();
					return true;
				}
			}
			catch (DirectoryServicesException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (COMException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			return false;
		}

		#region Save Bizo properties to AD Password Settings Object

		void WriteMaximumPasswordAge(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (bizo.EnforceMaximumPasswordAge)
			{
				adPso.MaximumPasswordAge = TimeSpan.FromDays(bizo.MaximumPasswordAge);
			}
			else
			{
				adPso.MaximumPasswordAge = TimeSpan.FromTicks(long.MinValue); // The 'not set' value of MaximumPasswordAge is long.MinValue ticks
			}
		}

		void WriteMinimumPasswordAge(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (bizo.EnforceMinimumPasswordAge)
			{
				adPso.MinimumPasswordAge = TimeSpan.FromDays(bizo.MinimumPasswordAge);
			}
			else
			{
				adPso.MinimumPasswordAge = TimeSpan.FromDays(0); // The 'not set' value of MinimumPasswordAge is 0 days
			}
		}

		void WriteMinimumPasswordLength(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (bizo.EnforceMinimumPasswordLength)
			{
				adPso.MinimumPasswordLength = bizo.MinimumPasswordLength;
			}
			else
			{
				adPso.MinimumPasswordLength = 0;
			}
		}

		void WritePasswordHistoryLength(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (bizo.EnforcePasswordHistoryLength)
			{
				adPso.PasswordHistoryLength = bizo.PasswordHistoryLength;
			}
			else
			{
				adPso.PasswordHistoryLength = 0;
			}
		}

		void WritePasswordComplexityEnabled(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			adPso.PasswordComplexityEnabled = bizo.PasswordComplexityEnabled;
		}

		void WritePasswordLockoutPolicy(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (bizo.EnforcePasswordLockoutPolicy)
			{
				adPso.LockoutThreshold = bizo.LockoutThreshold;
				adPso.LockoutObservationWindow = TimeSpan.FromMinutes(bizo.LockoutObservationWindow);
				if (bizo.RequireAdminUnlock)
				{
					adPso.LockoutDuration = TimeSpan.FromTicks(long.MinValue);
				}
				else
				{
					adPso.LockoutDuration = TimeSpan.FromMinutes(bizo.LockoutDuration);
				}
			}
			else
			{
				adPso.LockoutThreshold = 0;
				// default value is 30 mins when there are not set
				adPso.LockoutObservationWindow = TimeSpan.FromMinutes(30);
				adPso.LockoutDuration = TimeSpan.FromMinutes(30);
			}
		}

		#endregion

		#region Load Bizo properties from AD Password Settings Object

		public void ReadMaximumPasswordAge(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			var maxPasswordAgeDays = (ZInt)adPso.MaximumPasswordAge.TotalDays;
			if (maxPasswordAgeDays > 0)
			{
				bizo.EnforceMaximumPasswordAge = true;
				bizo.MaximumPasswordAge = maxPasswordAgeDays;
			}
			else
			{
				bizo.EnforceMaximumPasswordAge = false;
			}
		}

		void ReadMinimumPasswordAge(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			var minPasswordAgeDays = (ZInt)adPso.MinimumPasswordAge.TotalDays;
			if (minPasswordAgeDays > 0)
			{
				bizo.EnforceMinimumPasswordAge = true;
				bizo.MinimumPasswordAge = minPasswordAgeDays;
			}
			else
			{
				bizo.EnforceMinimumPasswordAge = false;
			}
		}

		void ReadMinimumPasswordLength(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (adPso.MinimumPasswordLength > 0)
			{
				bizo.EnforceMinimumPasswordLength = true;
				bizo.MinimumPasswordLength = adPso.MinimumPasswordLength;
			}
			else
			{
				bizo.EnforceMinimumPasswordLength = false;
			}
		}

		void ReadPasswordHistoryLength(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (adPso.PasswordHistoryLength > 0)
			{
				bizo.EnforcePasswordHistoryLength = true;
				bizo.PasswordHistoryLength = adPso.PasswordHistoryLength;
			}
			else
			{
				bizo.EnforcePasswordHistoryLength = false;
			}
		}

		void ReadPasswordComplexityEnabled(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			bizo.PasswordComplexityEnabled = adPso.PasswordComplexityEnabled;
		}

		void ReadPasswordLockoutPolicy(ADPasswordSettings bizo, IPasswordSettings adPso)
		{
			if (adPso.LockoutThreshold > 0)
			{
				bizo.EnforcePasswordLockoutPolicy = true;
				bizo.LockoutThreshold = adPso.LockoutThreshold;
				bizo.LockoutObservationWindow = (ZInt)adPso.LockoutObservationWindow.TotalMinutes;

				if (adPso.LockoutDuration.Ticks == long.MinValue)
				{
					bizo.RequireAdminUnlock = true;
				}
				else
				{
					bizo.RequireAdminUnlock = false;
					bizo.LockoutDuration = (ZInt)adPso.LockoutDuration.TotalMinutes;
				}
			}
			else
			{
				bizo.EnforcePasswordLockoutPolicy = false;
			}
		}

		#endregion

		#region AD Password Settings Object and operations

		IPasswordSettings LoadOrCreateADPasswordSettingsObject()
		{
			IPasswordSettings result = null;

			try
			{
				if (EnvProxy.IsHostedWithCargowise)
				{
					var searcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials, true);

					result = searcher.FindPasswordSettings(BizO.ADPasswordSettingsObjectName);
					if (result == null)
					{
						result = CreateNewADPasswordSettingsObject(searcher);
					}
				}
				else
				{
					Globals.Message.ShowError(GetErrorMessageOfNonHosted());
				}
			}
			catch (COMException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (DirectoryServicesException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}

			return result;
		}

		string GetErrorMessageOfNonHosted() => Res.GetString("E52029F0-5964-417E-972B-B021AE3A41E3", "Only WiseCloud hosted customers can access this registry setting");

		string GetErrorMessageOfDomainUserSecurity(string userName, string domainName) => Res.GetString("79479CC0-797E-4F74-9860-15CBA784F09E", "Domain user {0} does not have write privileges to the Password Settings Container for domain {1} set in the registry '{2}'. Please contact your system administrator.",
					userName,
					domainName,
					((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.ADPasswordSettings).LocationMultilingual);

		string GetErrorMessageOfWCAGroupNotSet() => Res.GetString("43D07B9A-31D1-4C39-8DDB-73D48D80304E", "WiseCloud Access Security Group is not set, please contact customer support.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "AD object description")]
		IPasswordSettings CreateNewADPasswordSettingsObject(IDirectorySearcher searcher)
		{
			IPasswordSettings result = null;
			var container = searcher.GetPasswordSettingsContainer();
			if (container == null)
			{
				Globals.Message.ShowError(GetErrorMessageOfDomainUserSecurity(searcher.UserName, searcher.DomainName));
				return null;
			}
			if (!IsWiseCloudAccessSecurityGroupSet())
			{
				// Check if WCA Security Group is not defined, no point of creating a password policy as it won't be applied to any group or user
				Globals.Message.ShowError(GetErrorMessageOfWCAGroupNotSet());
				return null;
			}

			result = container.CreateNewChild(BizO.ADPasswordSettingsObjectName, searcher); // Note: this method creates and commits straight away to AD
			var psoDescription = "Created by {0} registry '{1}' for client with license code {2} on {3} (UTC).";
#if DEBUG
			psoDescription += " (This is from a DEBUG build and not for a real client)";
#endif
			result.Description = string.Format(CultureInfo.InvariantCulture, psoDescription,
					BrandingFactory.Instance.ProductName,
					((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.ADPasswordSettings).LocationMultilingual,
					EnvProxy.Instance.CurrentCompany.GetLicenceCode(),
					DateTime.UtcNow);

			AddWiseCloudSecurityGroups(searcher, result);

			if (result.Count == 0)
			{
				Globals.Message.ShowError(
					Res.GetString("F0747874-D8C0-4676-834F-35A98EEEA7A9", "The WiseCloud Access Security Group set in registry '{0}' does not exist in the domain，please contact customer support.",
						((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.WiseCloudAccessSecurityGroup).LocationMultilingual
					));
				result.Delete();
				return null;
			}
			result.CommitChanges(); // Commit to ensure base value and group are applied
			return result;
		}

		bool IsWiseCloudAccessSecurityGroupSet() => ActiveDirectoryRegistry.Instance.WiseCloudAccessSecurityGroup.Value.Length > 0;

		void AddWiseCloudSecurityGroups(IDirectorySearcher searcher, IPasswordSettings policyObject)
		{
			foreach (var group in ActiveDirectoryRegistry.Instance.WiseCloudAccessSecurityGroup.Value)
			{
				var groupEntry = searcher.FindGroup(group);
				if (groupEntry != null)
				{
					policyObject.Add(groupEntry);
				}
			}
		}

		#endregion
	}
}
