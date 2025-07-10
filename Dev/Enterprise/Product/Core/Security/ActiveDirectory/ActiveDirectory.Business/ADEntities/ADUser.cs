using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADUser : ADEntity<GlbStaff, IUserDirectoryEntry>, IADUser
	{
		public ADUser(GlbStaff staff)
			: base(staff)
		{
		}

		#region Properties

		[ResourceStringData("ADUser.UserPrincipalName", Caption = "User Principal Name", ShortCaption = "UPN")]
		public string UserPrincipalName
		{
			get { return RequireDirectoryEntry().UserPrincipalName ?? string.Empty; }
			set { RequireDirectoryEntry(true).UserPrincipalName = value; }
		}

		public string SAMAccountName
		{
			get { return (string)RequireDirectoryEntry()[ADAttributes.SAMAccountName] ?? string.Empty; }
			set { RequireDirectoryEntry(true)[ADAttributes.SAMAccountName] = value; }
		}

		[ResourceStringData("ADUser.LoginName", Caption = "Login Name")]
		public string LoginName
		{
			get { return SearcherFilter.UserNameComponent(UserPrincipalName); }
		}

		[ResourceStringData("ADUser.FullName", Caption = "Full Name", ShortCaption = "Name")]
		public string FullName
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_FullName); }
			set
			{
				if (AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName) == ADAttributes.Name)
				{
					newNameAttribute = value;
				}
				else
				{
					RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_FullName, value);
				}
			}
		}

		[ResourceStringData("ADUser.StreetAddress", Caption = "Street Address", ShortCaption = "Address")]
		public string StreetAddress
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_UserAddress1); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_UserAddress1, value); }
		}

		[ResourceStringData("ADUser.City", Caption = "City")]
		public string City
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_City); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_City, value); }
		}

		[ResourceStringData("ADUser.Title", Caption = "Title")]
		public string Title
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_Title); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_Title, value); }
		}

		[ResourceStringData("ADUser.State", Caption = "State")]
		public string State
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_State); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_State, value); }
		}

		[ResourceStringData("ADUser.Postcode", Caption = "Postcode")]
		public string Postcode
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_Postcode); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_Postcode, value); }
		}

		[ResourceStringData("ADUser.WorkPhone", Caption = "Work Phone")]
		public string WorkPhone
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_WorkPhone); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_WorkPhone, value); }
		}

		[ResourceStringData("ADUser.WorkExtension", Caption = "Work Extension")]
		public string WorkExtension
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_WorkExtension); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_WorkExtension, value); }
		}

		[ResourceStringData("ADUser.FaxNum", Caption = "Fax Number")]
		public string FaxNum
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_FaxNum); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_FaxNum, value); }
		}

		[ResourceStringData("ADUser.HomePhone", Caption = "Home Phone")]
		public string HomePhone
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_HomePhone); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_HomePhone, value); }
		}

		[ResourceStringData("ADUser.MobilePhone", Caption = "Mobile Phone")]
		public string MobilePhone
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_MobilePhone); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_MobilePhone, value); }
		}

		[ResourceStringData("ADUser.PreferredLanguage", Caption = "Preferred Language")]
		public string PreferredLanguage
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_WorkingLanguage); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_WorkingLanguage, value); }
		}

		[ResourceStringData("ADUser.EmailAddress", Caption = "Email Address")]
		public string EmailAddress
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_EmailAddress); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_EmailAddress, value); }
		}

		[ResourceStringData("ADUser.Pager", Caption = "Pager")]
		public string Pager
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_Pager); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_Pager, value); }
		}

		public byte[] ThumbnailImage
		{
			get { return (byte[])RequireDirectoryEntry().GetValue(GlbStaffSchema.GS_ProfilePhoto) ?? Array.Empty<byte>(); }
			set { RequireDirectoryEntry(true).SetValue(GlbStaffSchema.GS_ProfilePhoto, value); }
		}

		public DateTime PasswordLastSet
		{
			get { return RequireDirectoryEntry().PasswordLastSet; }
		}

		[ResourceStringData("ADUser.Manager", Caption = "Manager")]
		public GlbStaff Manager
		{
			get
			{
				var managerEntry = RequireDirectoryEntry().Manager;
				if (managerEntry != null)
				{
					return EnterpriseEntity.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, managerEntry.Guid));
				}
				return null;
			}
			set
			{
				IUserDirectoryEntry managerEntry = null;
				if (value != null)
				{
					if (value.IsADLinked)
					{
						var searcher = GetDirectorySearcher(false);
						managerEntry = searcher.FindUser(value.GS_ActiveDirectoryObjectGuid.ToGuid());
					}
				}
				RequireDirectoryEntry(true).Manager = managerEntry;
			}
		}
		#endregion

		#region ADEntity overrides

		protected override IUserDirectoryEntry FindDirectoryEntryCore(IDirectorySearcher directorySearcher, Guid guid, string rootOU)
		{
			return guid != Guid.Empty ? directorySearcher.FindUser(guid, rootOU) : null;
		}

		protected override IUserDirectoryEntry FindDirectoryEntryCore(IDirectorySearcher directorySearcher, string enterpriseIdentity, string rootOU)
		{
			return directorySearcher.FindUser(enterpriseIdentity, rootOU);
		}

		public override string EnterpriseIdentity
		{
			get { return (string)EnterpriseEntity.GS_LoginName ?? string.Empty; }
		}

		string GetValidAndUniqueCommonName(string name) => GetValidAndUniqueName(name, IsValidCommonName, GetValidCommonName, IsCommonNameAlreadyExistInTheSameOU);

		string GetValidAndUniqueSAMAccountName(string name) => GetValidAndUniqueName(name, IsValidSAMAccountName, GetValidSAMAccountName, IsSAMAccountNameAlreadyExist);

		bool requiredUpdateCommonName;

		string newNameAttribute;

		public void RenameLoginName(string newName)
		{
			var currentUPNDomainNameComponent = SearcherFilter.DomainNameComponent(UserPrincipalName);
			UserPrincipalName = currentUPNDomainNameComponent.IsNullOrEmpty() ? newName : newName + '@' + currentUPNDomainNameComponent;

			var validAndUniqueUserName = GetValidAndUniqueSAMAccountName(newName);
			if (IsValidSAMAccountName(validAndUniqueUserName))
			{
				// We only update SAMAccountName when the uniqueTruncatedUserName can be found
				SAMAccountName = validAndUniqueUserName;
			}
			requiredUpdateCommonName = true;
		}

		void UpdateCommonNameIfRequired()
		{
			if (requiredUpdateCommonName && IsSynchronisedSuccessfully)
			{
				var validAndUniqueCommonName = GetValidAndUniqueCommonName(LoginName);
				if (IsValidCommonName(validAndUniqueCommonName))
				{
					RequireDirectoryEntry(true).Rename(validAndUniqueCommonName);
				}
				requiredUpdateCommonName = false;
			}
		}

		void UpdateNameAttributeIfRequired()
		{
			if (!string.IsNullOrEmpty(newNameAttribute) && IsSynchronisedSuccessfully)
			{
				RequireDirectoryEntry(true).Rename(newNameAttribute);
			}
			newNameAttribute = null;
		}

		bool IsValidSAMAccountName(ZString uniqueUserName) => uniqueUserName.Length <= 20 &&
																!uniqueUserName.EndsWith(".", StringComparison.OrdinalIgnoreCase) &&
																!uniqueUserName.Contains("@", StringComparison.OrdinalIgnoreCase);

		bool IsValidCommonName(ZString uniqueUserName) => uniqueUserName.Length <= 64;

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

		ZString GetValidCommonName(ZString commonName)
		{
			if (commonName.IsEmpty)
			{
				return commonName;
			}

			// Truncate to 64
			commonName = commonName.SubstringSafe(0, 64);
			return commonName;
		}

		string GetValidAndUniqueName(ZString name, Func<ZString, bool> isValidName, Func<ZString, ZString> getValidName, Func<string, IDirectorySearcher, bool> isNameAlreadyExists)
		{
			if (name.IsEmpty || isValidName(name))
			{
				return name;
			}

			var validAndUniqueUsername = getValidName(name);
			var directorySearcher = GetDirectorySearcher(false);
			for (int i = 1; i < 100; i++)
			{
				if (!isNameAlreadyExists(validAndUniqueUsername, directorySearcher))
				{
					return validAndUniqueUsername;
				}

				var numOfPadding = validAndUniqueUsername.Length <= 2 ? 1 : 2;
				validAndUniqueUsername = validAndUniqueUsername.SubstringSafe(0, validAndUniqueUsername.Length - numOfPadding) + i.ToString(CultureInfo.InvariantCulture).PadLeft(numOfPadding, '0');
			}

			// if no unique and valid username can be found, return the original username
			return name;
		}

		bool IsSAMAccountNameAlreadyExist(string username, IDirectorySearcher directorySearcher)
		{
			var directoryEntryByIdentity = FindDirectoryEntryCore(directorySearcher, username, string.Empty);
			return directoryEntryByIdentity != null && directoryEntryByIdentity.Guid != (ZGuid)GuidPropertyInfo.Value;
		}

		bool IsCommonNameAlreadyExistInTheSameOU(string commonName, IDirectorySearcher directorySearcher)
		{
			var directoryEntryByIdentity = directorySearcher.FindUserByCommonName(commonName, OUPathForEntity(DomainCredentials));
			return directoryEntryByIdentity != null && directoryEntryByIdentity.Guid != (ZGuid)GuidPropertyInfo.Value;
		}

		public override ZPropertyInfo GuidPropertyInfo
		{
			get { return EnterpriseEntity.GS_ActiveDirectoryObjectGuidInfo; }
		}

		protected override string OUPathForEntity(IDomainCredentials domainCredentials) => domainCredentials?.UserOrganisationalUnit ?? "";

		protected override DirectoryObjectType EntityType
		{
			get { return DirectoryObjectType.User; }
		}

		protected override IUserDirectoryEntry CreateNewDirectoryEntryCore(IOrganisationalUnit organisationalUnitEntity, IDirectorySearcher directorySearcherWithWritePrivileges)
		{
			return (IUserDirectoryEntry)organisationalUnitEntity.CreateNewChild(
				GetValidAndUniqueCommonName(EnterpriseIdentity),
				EnterpriseIdentity,
				GetValidAndUniqueSAMAccountName(EnterpriseIdentity),
				EntityType,
				directorySearcherWithWritePrivileges
			);
		}

		protected override IUserDirectoryEntry CreateNewDirectoryEntry()
		{
			var directoryEntry = base.CreateNewDirectoryEntry();
			if (directoryEntry != null)
			{
				try
				{
					directoryEntry.SetPassword(PasswordToUse);
					directoryEntry.PasswordMustChangeAtNextLogon = true;
					directoryEntry.PasswordNotRequired = false;
					directoryEntry.CommitChanges();
					AddToWiseCloudAccessSecurityGroupIfRequired(directoryEntry);
				}
				catch (PasswordDoesNotMatchPolicyException)
				{
					directoryEntry.Delete();
					HandlePasswordDoesNotMatchPolicyException();
					throw;
				}
			}

			return directoryEntry;
		}

		protected virtual void HandlePasswordDoesNotMatchPolicyException()
		{
			if (DomainCredentials != null)
			{
				DomainCredentials.DefaultPasswordFailsToMeetDomainPolicy = true;
			}
		}

		protected virtual string PasswordToUse => DomainCredentials?.DefaultPassword ?? Enterprise.Security.ActiveDirectory.DomainCredentials.DefaultPasswordValue;
		protected virtual string[] SecurityGroupsToUse => ActiveDirectoryRegistry.Instance.WiseCloudAccessSecurityGroup.Value;

		protected override string GetObjectAlreadyExistsErrorMessageCore()
		{
			return System.Environment.NewLine + string.Format(CultureInfo.InvariantCulture, @"{1}={0}
{2}={0}",
					EnterpriseIdentity,
					"UserPrincipleName",
					"SAMAccountName");
		}

		const int ErrorCodeInvalidAccountName = unchecked((int)0x8007001F); //-2147024865

		protected override void CommitChangesCore()
		{
			try
			{
				base.CommitChangesCore();
				// CN rename should only be done after commit
				UpdateCommonNameIfRequired();
				UpdateNameAttributeIfRequired();
			}
			catch (DirectoryServicesException ex)
			{
				if (ex.InnerException is DirectoryServicesCOMException innerEx)
				{
					if (innerEx.ErrorCode == ErrorCodeInvalidAccountName)
					{
						throw new DirectoryServicesException(Res.GetString("7D18C375-CEE1-4163-B8B5-90D2BFE28126", @"Could not save user with username '{0}'.
Please ensure that the username is not longer than 20 characters and does not contain any of these symbols: {1}", SAMAccountName, @""" / \ [ ] : ; | = , + * ? < >"));
					}
				}

				throw;
			}
		}

		void AddToWiseCloudAccessSecurityGroupIfRequired(IDirectoryEntry directoryEntry)
		{
			if (!EnvProxy.IsHostedWithCargowise)
			{
				return;
			}

			var securityGroups = SecurityGroupsToUse;
			var directorySearcher = GetDirectorySearcher(true);
			var listofNonExistanceGroup = new List<string>();

			foreach (var group in securityGroups)
			{
				var securityGroup = directorySearcher.FindGroup(group);
				if (securityGroup != null)
				{
					securityGroup.AddMember(directoryEntry);
					securityGroup.CommitChanges();
				}
				else
				{
					listofNonExistanceGroup.Add(group);
				}
			}

			if (listofNonExistanceGroup.Count > 0)
			{
				var message = Res.GetString("8E2C40EE-C47A-45AD-B513-8B146B351CA7", "Could not find the following WiseCloud Access Security group(s) in domain {0}: {1}",
					DomainCredentialDomainName, string.Join(", ", listofNonExistanceGroup));
				ADNotification.ShowError(message);
			}
		}

		#endregion

		#region IADUser members

		public bool PasswordExpired => RequireDirectoryEntry().PasswordExpired;

		public bool PasswordMustChangeAtNextLogon
		{
			get { return RequireDirectoryEntry().PasswordMustChangeAtNextLogon; }
			set { RequireDirectoryEntry(true).PasswordMustChangeAtNextLogon = value; }
		}

		public bool PasswordDoesntExpire => RequireDirectoryEntry().PasswordDoesntExpire;

		public bool PasswordDoesntExpireUserAttribute
		{
			get { return RequireDirectoryEntry().PasswordDoesntExpireUserAttribute; }
			set { RequireDirectoryEntry(true).PasswordDoesntExpireUserAttribute = value; }
		}

		public virtual int GetNumberOfDaysTillPasswordExpiry()
		{
			var expiryDate = (ZDateTime)RequireDirectoryEntry().PasswordExpirationDate;
			if (PasswordDoesntExpire)
			{
				return int.MaxValue;
			}
			else if (!expiryDate.IsValid)
			{
				return 0;
			}
			else
			{
				return Math.Max((expiryDate - ZDateTime.Now).Days, 0);
			}
		}

		public void SetPassword(string newPassword)
		{
			CommitPasswordAction(d => d.SetPassword(newPassword, true), true);
		}

		public void ChangePassword(string oldPassword, string newPassword)
		{
			CommitPasswordAction(d => d.ChangePassword(oldPassword, newPassword), false);
		}

		void CommitPasswordAction(Action<IUserDirectoryEntry> passwordAction, bool requireDomainWritePrivilege)
		{
			try
			{
				var directoryEntry = RequireDirectoryEntry(requireDomainWritePrivilege);
				if (directoryEntry.PasswordCannotChange)
				{
					throw new DirectoryServicesException("Cannot change password as the account in Active Directory is set to refuse password changes.");
				}
				passwordAction(directoryEntry);
				directoryEntry.CommitChanges();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!DirectoryExceptionHandler.TryHandleDirectoryException(ex, EnterpriseIdentity))
				{
					throw;
				}
			}
		}

		public bool IsPasswordValid(string currentPassword)
		{
			Argument.NotNullOrEmpty(currentPassword, "currentPassword");

			var loginName = LoginName;
			return !string.IsNullOrEmpty(loginName) && !string.IsNullOrEmpty(currentPassword) && Validation.ValidateCredentials(loginName, currentPassword);
		}

		public void UnlockAccount()
		{
			RequireDirectoryEntry(true).IsActive = true;
		}

		public bool LockedOut => RequireDirectoryEntry().LockedOut;

		public string DomainNetBiosName => GetDirectorySearcher(false).DomainNetBiosName;

		#region Explicit

		void IADUser.ChangePassword(string oldPassword, string newPassword)
		{
			DirectoryExceptionHandler.ExecuteWithExceptionHandling(() => ChangePassword(oldPassword, newPassword), EnterpriseIdentity);
		}

		void IADUser.UnlockAccount()
		{
			DirectoryExceptionHandler.ExecuteWithExceptionHandling(UnlockAccount, EnterpriseIdentity);
		}

		#endregion

		#endregion

		IValidationPrincipalContextProvider Validation
		{
			get { return validation ?? (validation = new ValidationPrincipalContextProvider(GetDirectorySearcher(false))); }
		}
		IValidationPrincipalContextProvider validation;

		protected override void SetGuid(Guid value)
		{
			if (GlbStaffValidationReal.AreDifferentAccent(LoginName, EnterpriseIdentity))
			{
				var message = Res.GetString("08ECEA6C-3E31-4D24-9172-974EC11F6AD4",
					@"Cannot synchronize '{0}' with the matching Active Directory User '{1}' because the {2} has different diacritic marks or ligature (e.g. {3} and {4}).
Please change {2} '{0}' to match with AD user's logon name '{1}' and try again.",
					EnterpriseIdentity,
					LoginName,
					EnterpriseEntity.GS_LoginNameInfo.HumanReadableName,
					"à", "a"
					);

				throw new DirectoryServicesException(message);
			}

			GuidPropertyInfo.Value = new ZGuid(value);
		}
	}
}
