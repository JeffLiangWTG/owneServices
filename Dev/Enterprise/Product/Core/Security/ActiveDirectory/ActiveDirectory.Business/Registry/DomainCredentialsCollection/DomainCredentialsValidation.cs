using System;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public class DomainCredentialsValidation : ZValidation
	{
		public DomainCredentialsValidation(DomainCredentials parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DomainCredentials parent;

		#region Base Validation

		public void ValidateDomainName()
		{
			parent.DomainNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DomainNameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(parent.DomainNameInfo, (IMultilingualString)ResString.GetMultilingualString("48a0d1a6-e63a-4d61-93b6-b2f41c3ca113", "The domain name should be unique."));
		}

		public void ValidateDomainUserName()
		{
			parent.DomainUserNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DomainUserNameInfo);
		}

		public void ValidateDomainUserPassword()
		{
			parent.DomainUserPasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DomainUserPasswordInfo);
		}

		public void ValidateIsDefaultDomain()
		{
			parent.IsDefaultDomainInfo.ClearAllNotifications();

			foreach (var parentCollection in ((IBusinessObjectInternals)parent).ParentCollections)
			{
				var defaultDomainIsSet = false;
				foreach (var domainCredentials in parentCollection.OfType<DomainCredentials>())
				{
					if (domainCredentials.IsDefaultDomain)
					{
						if (defaultDomainIsSet)
						{
							parent.IsDefaultDomainInfo.AddError(Res.GetString("33f9de8b-1025-4980-9932-111a6b81b15f", "Only one domain can be set as Default Domain."));
						}
						defaultDomainIsSet = true;
					}
				}
				if (!defaultDomainIsSet)
				{
					parent.IsDefaultDomainInfo.AddError(Res.GetString("6b9e1c94-fcde-4da2-80d5-48d316a84cba", "One domain must be set as Default Domain."));
				}
			}
		}

		public void ValidateUserOrganisationalUnit()
		{
			parent.UserOrganisationalUnitInfo.ClearAllNotifications();
			MandatoryValidation.WarnIfNotEntered(parent.UserOrganisationalUnitInfo);
		}

		public void ValidateGroupOrganisationalUnit()
		{
			parent.GroupOrganisationalUnitInfo.ClearAllNotifications();
			if (ActiveDirectoryRegistry.Instance.EntitiesToSync == EntitiesToSync.UsersAndGroups)
			{
				MandatoryValidation.WarnIfNotEntered(parent.GroupOrganisationalUnitInfo);
			}
		}

		public void ValidateDefaultPassword()
		{
			parent.DefaultPasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.DefaultPasswordInfo);
		}

		public override void ValidateAll()
		{
			if (!parent.IsValidationSuspended && !IsValidatingLoginDetails())
			{
				ValidateDomainName();
				ValidateDomainUserName();
				ValidateDomainUserPassword();
				ValidateIsDefaultDomain();
				ValidateUserOrganisationalUnit();
				ValidateGroupOrganisationalUnit();
				ValidateDefaultPassword();
			}
		}

		public override Type AutoValidationType => typeof(DomainCredentialsValidation);

		#endregion

		#region Extra Validation (environment dependent)
		bool UsersLinkedOnDomain()
		{
			var factory = parent.Factory ?? new BusinessObjectFactory();
			return factory.Exists(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_DomainName, parent.DomainName));
		}

		bool GroupsLinkedOnDomain()
		{
			var factory = parent.Factory ?? new BusinessObjectFactory();
			return factory.Exists(typeof(GlbGroup), new ZQuery(GlbGroupSchema.GG_DomainName, parent.DomainName));
		}

		public bool IsCurrentlyInUse()
		{
			return UsersLinkedOnDomain() || GroupsLinkedOnDomain();
		}

		public bool AreDomainLoginDetailsValid()
		{
			if (!LoginDetailsAreFilled())
			{
				ValidateDomainName();
				ValidateDomainUserName();
				ValidateDomainUserPassword();
				return false;
			}
			else if (lastValidatedLoginDetails == null || !lastValidatedLoginDetails.IsSame(parent))
			{
				using (ValidatingLoginDetails())
				{
					var isValid = false;

					try
					{
						var searcher = new DirectorySearcherWrapper(parent.DomainUserName, parent.DomainUserPassword, parent.DomainName);
						if (!searcher.IsDomainNameFullyQualified(false))
						{
							parent.DomainNameInfo.AddError(Res.GetString("6fadd1f6-dab5-44fd-8c06-dad0736fbec1", "Please enter a Fully-Qualified Domain Name."));
						}
						else
						{
							isValid = true;
						}
					}
					catch (COMException e) when (e.ErrorCode == -2147023570)
					{
						//-2147023570 LDAP_INVALID_CREDENTIALS: The user name or password is incorrect. http://www.selfadsi.org/errorcodes.htm
						parent.DomainUserNameInfo.AddError(Res.GetString("de4fc3bc-1cc1-4cb1-8812-19cdf176c840", "The domain user name or password is incorrect."));
						parent.DomainUserPasswordInfo.AddError(Res.GetString("de4fc3bc-1cc1-4cb1-8812-19cdf176c840", "The domain user name or password is incorrect."));
					}
					catch (COMException e) when (e.ErrorCode == -2147016646)
					{
						// -2147016646 LDAP_SERVER_DOWN: The server is not operational. http://www.selfadsi.org/errorcodes.htm
						parent.DomainNameInfo.AddError(Res.GetString("f82a85e5-5c0d-4c3a-b84d-5f3c277b0a21", "The domain cannot be reached. Please check its availability or if its name is valid."));
					}
					catch (COMException e) when (e.ErrorCode == -2147016645)
					{
						// -2147016645: A local error has occurred. This is due to IS blocking NTLM authentication on the machine, and must use a fully-qualified user name such as username@domain or domain\username
						parent.DomainUserNameInfo.AddError(Res.GetString("66000CDE-7318-4B8D-B262-86034A0541EA", "Please enter a Fully-Qualified User Name, e.g. username@domain or domain\\username."));
					}
					finally
					{
						lastValidatedLoginDetails = new ValidatedLoginsDetails(parent, isValid);
					}
				}
			}
			return lastValidatedLoginDetails != null && lastValidatedLoginDetails.IsValid;
		}

		public bool IsOrganisationalUnitValid(OrganisationalUnitType ouType)
		{
			var isOUValid = false;

			if (AreDomainLoginDetailsValid())
			{
				ZPropertyInfo ouInfo;
				if (ouType == OrganisationalUnitType.User)
				{
					ValidateUserOrganisationalUnit();
					ouInfo = parent.UserOrganisationalUnitInfo;
				}
				else
				{
					ValidateGroupOrganisationalUnit();
					ouInfo = parent.GroupOrganisationalUnitInfo;
				}

				var organisationalUnit = (ZString)ouInfo.Value;
				if (!ouInfo.HasErrors())
				{
					var searcher = new DirectorySearcherWrapper(parent.DomainUserName, parent.DomainUserPassword, parent.DomainName);
					try
					{
						if (!searcher.CurrentUserHasSecurityOnOU(organisationalUnit) && !ADAccessRuleChecker.HasRequiredPermissionsToSync(ouType, searcher, organisationalUnit))
						{
							ouInfo.AddWarning(Res.GetString("97f15f27-59a9-4103-a0a7-d373cb5859af", "The domain user does not have write access to the selected Organizational Unit. Write access will be required for data to be synchronized to Active Directory."));
						}
					}
					catch (InvalidOUException)
					{
						ouInfo.AddError(Res.GetString("37F5171C-87AF-4224-B44B-A54BBB659287", "Please select a valid Organizational Unit."));
					}
				}

				isOUValid = !ouInfo.HasErrors();
			}

			return isOUValid;
		}

		bool LoginDetailsAreFilled()
		{
			return !parent.DomainName.IsEmpty && !parent.DomainUserName.IsEmpty && !parent.DomainUserPassword.IsEmpty;
		}

		IDisposable ValidatingLoginDetails()
		{
			return new DisposableAction(() => validatingLoginDetailsCount++, () => validatingLoginDetailsCount--);
		}
		int validatingLoginDetailsCount;

		bool IsValidatingLoginDetails() => validatingLoginDetailsCount > 0;

		public enum OrganisationalUnitType
		{
			User,
			Group
		}

		ValidatedLoginsDetails lastValidatedLoginDetails;

		class ValidatedLoginsDetails
		{
			public ValidatedLoginsDetails(DomainCredentials domainCredentials, bool isValid)
			{
				domainName = domainCredentials.DomainName;
				domainUserName = domainCredentials.DomainUserName;
				domainUserPassword = domainCredentials.DomainUserPassword;
				IsValid = isValid;
			}

			readonly string domainName;
			readonly string domainUserName;
			readonly string domainUserPassword;

			public bool IsValid { get; }

			public bool IsSame(DomainCredentials domainCredentials)
			{
				return domainName == domainCredentials.DomainName
					&& domainUserName == domainCredentials.DomainUserName
					&& domainUserPassword == domainCredentials.DomainUserPassword;
			}
		}

		#endregion
	}
}
