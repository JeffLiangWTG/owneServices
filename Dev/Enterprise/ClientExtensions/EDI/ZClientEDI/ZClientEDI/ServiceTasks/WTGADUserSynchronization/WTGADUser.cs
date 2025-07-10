using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.WTGADUserSynchronization
{
	class WTGADUser
	{
		public WTGADUser(EDIGlbStaff ediStaff, IDirectorySearcher directorySearcher, ILogger logger)
		{
			Argument.NotNull(ediStaff, "ediStaff");
			Argument.NotNull(directorySearcher, "directorySearcher");
			Argument.NotNull(logger, "logger");

			this.ediStaff = ediStaff;
			this.directorySearcher = directorySearcher;
			this.logger = logger;
		}

		readonly EDIGlbStaff ediStaff;
		readonly IDirectorySearcher directorySearcher;
		readonly ILogger logger;

		string StaffLoginName => ediStaff.GS_LoginName;

		string StaffActiveDirectoryName => string.IsNullOrEmpty(ediStaff.GS_LoginName) ? string.Empty : FormattableString.Invariant($"WTG.{ediStaff.GS_LoginName}");

		ZGuid StaffActiveDirectoryObjectId => ediStaff.StaffEx.GS9_EdiActiveDirectoryObjectGuid;

		string OUForUser => EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.Value.OrganizationalUnitPath;

		public void Synchronise()
		{
			var creatingADUser = false;
			IUserDirectoryEntry userDirectoryEntry;
			if (!StaffActiveDirectoryObjectId.IsEmpty && StaffActiveDirectoryObjectId.IsValid)
			{
				userDirectoryEntry = GetLinkedUserDirectoryEntry();
			}
			else
			{
				creatingADUser = true;
				userDirectoryEntry = CreateADUserIfNotExist();

				ediStaff.StaffEx.GS9_EdiActiveDirectoryObjectGuid = userDirectoryEntry.Guid;
			}

			if (userDirectoryEntry != null)
			{
				try
				{
					SynchronizeAttributes(userDirectoryEntry);

					if (creatingADUser)
					{
						ediStaff.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
						ediStaff.Person?.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
						ediStaff.Factory.Save();
					}
				}
				catch (Exception)
				{
					if (creatingADUser)
					{
						userDirectoryEntry.Delete();
					}

					throw;
				}
			}
		}

		IUserDirectoryEntry GetLinkedUserDirectoryEntry()
		{
			var userDirectoryEntry = directorySearcher.FindUser(StaffActiveDirectoryObjectId.ToGuid());

			if (userDirectoryEntry != null)
			{
				if (!userDirectoryEntry.OrganisationalUnit.Equals(OUForUser, StringComparison.InvariantCultureIgnoreCase))
				{
					var message = FormattableString.Invariant($"The staff '{StaffLoginName}' is linked to an AD user '{userDirectoryEntry.UserPrincipalName}' in OU '{userDirectoryEntry.OrganisationalUnit}' instead of OU '{OUForUser}'");

					if (userDirectoryEntry.IsActive || ediStaff.GS_IsActive)
					{
						logger.Warning(message);
						SendNotificationEmail(message);
					}
					else
					{
						logger.Information(message);
					}

					return null;
				}
			}
			else
			{
				var message = FormattableString.Invariant($"The staff '{StaffLoginName}' is linked to a AD user with object id '{StaffActiveDirectoryObjectId}', but it doesn't exist in AD. If '{StaffLoginName}' was created or activated recently please try again later.");
				SendNotificationEmail(message);
			}

			return userDirectoryEntry;
		}

		void SendNotificationEmail(string body)
		{
			var emailDef = new EmailDef();
			emailDef.Subject = FormattableString.Invariant($"{SynchronizeEDIStaffToActiveDirectoryTask.Code} - {SynchronizeEDIStaffToActiveDirectoryTask.Description}");
			emailDef.Body = body;
			Env.OutgoingMailManager.CreateAndSave(emailDef, EDIDataRegistry.Instance.NotificationGroupForADETask.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.NotificationGroupForADETask));
		}

		IUserDirectoryEntry CreateADUserIfNotExist()
		{
			var existedUser = directorySearcher.FindUser(StaffActiveDirectoryName);
			if (existedUser != null)
			{
				logger.Warning($"Skip creating because there is AD user with same login name '{StaffActiveDirectoryName}' in OU '{existedUser.OrganisationalUnit}'");

				return existedUser;
			}

			var organisationalUnitEntity = directorySearcher.FindOrganisationalUnit(OUForUser);
			var newUser = organisationalUnitEntity.CreateNewChild(StaffActiveDirectoryName, DirectoryObjectType.User, directorySearcher) as IUserDirectoryEntry;

			return newUser;
		}

		void SynchronizeAttributes(IUserDirectoryEntry user)
		{
			RenameADUser(user);

			user.IsActive = ediStaff.GS_IsActive;
			user.CommitChanges();

			AddUserToWiseCloudAccessSecurityGroup(user);
		}

		void RenameADUser(IUserDirectoryEntry user)
		{
			var validUserPrincipalName = FormattableString.Invariant($"{StaffActiveDirectoryName}@{directorySearcher.DomainName}");
			var validSAMAccountName = GetValidAndUniqueSAMAccountName(StaffActiveDirectoryName);

			user.Rename(StaffActiveDirectoryName);
			user.UserPrincipalName = validUserPrincipalName;
			user[ADAttributes.SAMAccountName] = validSAMAccountName;
		}

		void AddUserToWiseCloudAccessSecurityGroup(IUserDirectoryEntry user)
		{
			var missedGroups = new List<string>();

			foreach (var groupName in EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.Value)
			{
				var group = directorySearcher.FindGroup(groupName);
				if (group != null)
				{
					group.AddMember(user);
					group.CommitChanges();
				}
				else
				{
					missedGroups.Add(groupName);
				}
			}

			if (missedGroups.Count > 0)
			{
				logger.Warning($"Cannot find following security group(s): {string.Join(",", missedGroups)}");
			}
		}

		string GetValidAndUniqueSAMAccountName(ZString newName)
		{
			if (newName.IsEmpty || IsValidSAMAccountName(newName))
			{
				return newName;
			}

			var validAndUniqueUsername = GetValidSAMAccountName(newName);
			for (var i = 1; i < 100; i++)
			{
				var userWithName = directorySearcher.FindUser(validAndUniqueUsername);
				if (userWithName == null || userWithName.Guid == StaffActiveDirectoryObjectId)
				{
					return validAndUniqueUsername;
				}

				var numOfPadding = validAndUniqueUsername.Length <= 2 ? 1 : 2;
				validAndUniqueUsername = validAndUniqueUsername.SubstringSafe(0, validAndUniqueUsername.Length - numOfPadding) + i.ToString(CultureInfo.InvariantCulture).PadLeft(numOfPadding, '0');
			}

			// if no unique and valid username can be found, return the original username
			return newName;

			bool IsValidSAMAccountName(ZString uniqueUserName) =>
				uniqueUserName.Length <= 20
				&& !uniqueUserName.EndsWith(".", StringComparison.OrdinalIgnoreCase)
				&& !uniqueUserName.Contains("@", StringComparison.OrdinalIgnoreCase);

			ZString GetValidSAMAccountName(ZString username)
			{
				if (username.IsEmpty)
				{
					return username;
				}

				// Truncate to 20 characters
				username = username.SubstringSafe(0, 20);

				const char replacementChar = '_';
				// Replace ending . with _
				if (username.EndsWith(".", StringComparison.OrdinalIgnoreCase))
				{
					username = username.SubstringSafe(0, username.Length - 1) + replacementChar;
				}

				// Replace @ with _
				username = username.Replace('@', replacementChar);

				return username;
			}
		}
	}
}
