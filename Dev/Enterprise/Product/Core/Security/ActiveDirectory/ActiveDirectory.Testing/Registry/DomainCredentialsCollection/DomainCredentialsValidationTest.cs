using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class DomainCredentialsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDomainName()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = domainCredentialsCollection.AddNew();
			domainCredentials.Validation.ValidateDomainName();
			AssertHasError(domainCredentials.DomainNameInfo, "Please enter a Domain Name.");

			domainCredentials.DomainName = "fake.domain";
			AssertNoErrors(domainCredentials.DomainNameInfo);
		}

		public void TestValidateDomainUserName()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = domainCredentialsCollection.AddNew();
			domainCredentials.Validation.ValidateDomainUserName();
			AssertHasError(domainCredentials.DomainUserNameInfo, "Please enter a Domain User Name.");

			domainCredentials.DomainUserName = "Dexter";
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
		}

		public void TestValidateDomainUserPassword()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = domainCredentialsCollection.AddNew();
			domainCredentials.Validation.ValidateDomainUserPassword();
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "Please enter a Domain User Password.");

			domainCredentials.DomainUserPassword = "DasIstEinPasswort";
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);
		}

		public void TestValidateIsDefaultDomain()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials1 = domainCredentialsCollection.AddNew();
			domainCredentials1.Validation.ValidateIsDefaultDomain();
			AssertHasError(domainCredentials1.IsDefaultDomainInfo, "One domain must be set as Default Domain.");

			domainCredentials1.IsDefaultDomain = true;
			AssertNoErrors(domainCredentials1.IsDefaultDomainInfo);

			var domainCredentials2 = domainCredentialsCollection.AddNew();
			domainCredentials2.Validation.ValidateIsDefaultDomain();
			AssertNoErrors(domainCredentials2.IsDefaultDomainInfo);

			domainCredentials2.IsDefaultDomain = true;
			AssertHasError(domainCredentials2.IsDefaultDomainInfo, "Only one domain can be set as Default Domain.");
		}

		public void TestValidateUserOrganisationalUnit()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = domainCredentialsCollection.AddNew();
			domainCredentials.Validation.ValidateUserOrganisationalUnit();
			AssertHasWarning(domainCredentials.UserOrganisationalUnitInfo, "You have not entered an Users' Organizational Unit.");

			domainCredentials.UserOrganisationalUnit = "/this/is/definitely/a/real/users/ou";
			AssertNoErrors(domainCredentials.UserOrganisationalUnitInfo);
		}

		public void TestValidateGroupOrganisationalUnit()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = domainCredentialsCollection.AddNew();
			domainCredentials.Validation.ValidateGroupOrganisationalUnit();
			AssertHasWarning(domainCredentials.GroupOrganisationalUnitInfo, "You have not entered a Groups' Organizational Unit.");

			domainCredentials.GroupOrganisationalUnit = "/this/is/definitely/a/real/groups/ou";
			AssertNoErrors(domainCredentials.GroupOrganisationalUnitInfo);

			domainCredentials.GroupOrganisationalUnit = string.Empty;
			AssertHasWarning(domainCredentials.GroupOrganisationalUnitInfo, "You have not entered a Groups' Organizational Unit.");

			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
			domainCredentials.Validation.ValidateGroupOrganisationalUnit();
			AssertHasWarning(domainCredentials.GroupOrganisationalUnitInfo, "You have not entered a Groups' Organizational Unit.");

			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;
			domainCredentials.Validation.ValidateGroupOrganisationalUnit();
			AssertNoErrors(domainCredentials.GroupOrganisationalUnitInfo);
		}

		public void TestValidateDefaultPassword()
		{
			var domainCredentialsCollection = new DomainCredentialsCollection();
			var domainCredentials = domainCredentialsCollection.AddNew();
			domainCredentials.DefaultPassword = string.Empty; //DefaultPassword defaults to Changeme1234. 
			AssertHasError(domainCredentials.DefaultPasswordInfo, "Please enter a Default Password.");

			domainCredentials.DefaultPassword = "DasIstEinPasswort";
			AssertNoErrors(domainCredentials.DefaultPasswordInfo);
		}

		public void TestAutoValidationType()
		{
			var domainCredentials = new DomainCredentials();
			AssertEquals(typeof(DomainCredentialsValidation), domainCredentials.Validation.AutoValidationType);
		}

		public void TestValidateAll()
		{
			var invalidDomainCredentialsCollection = new DomainCredentialsCollection();
			var invalidDomainCredentials = invalidDomainCredentialsCollection.AddNew();
			invalidDomainCredentials.DefaultPassword = string.Empty; //DefaultPassword defaults to Changeme1234. 
			invalidDomainCredentials.Validation.ValidateAll();
			AssertHasError(invalidDomainCredentials.DomainNameInfo, "Please enter a Domain Name.");
			AssertHasError(invalidDomainCredentials.DomainUserNameInfo, "Please enter a Domain User Name.");
			AssertHasError(invalidDomainCredentials.DomainUserPasswordInfo, "Please enter a Domain User Password.");
			AssertHasError(invalidDomainCredentials.IsDefaultDomainInfo, "One domain must be set as Default Domain.");
			AssertHasWarning(invalidDomainCredentials.UserOrganisationalUnitInfo, "You have not entered an Users' Organizational Unit.");
			AssertHasWarning(invalidDomainCredentials.GroupOrganisationalUnitInfo, "You have not entered a Groups' Organizational Unit.");
			AssertHasError(invalidDomainCredentials.DefaultPasswordInfo, "Please enter a Default Password.");

			var validDomainCredentialsCollection = new DomainCredentialsCollection();
			var validDomainCredentials = GetValidDomainCredentials();
			validDomainCredentialsCollection.Add(validDomainCredentials);
			validDomainCredentials.Validation.ValidateAll();
			AssertNoErrors(validDomainCredentials.DomainNameInfo);
			AssertNoErrors(validDomainCredentials.DomainUserNameInfo);
			AssertNoErrors(validDomainCredentials.DomainUserPasswordInfo);
			AssertNoErrors(validDomainCredentials.IsDefaultDomainInfo);
			AssertNoErrors(validDomainCredentials.UserOrganisationalUnitInfo);
			AssertNoErrors(validDomainCredentials.GroupOrganisationalUnitInfo);
			AssertNoErrors(validDomainCredentials.DefaultPasswordInfo);
		}

		public void TestAreDomainLoginDetailsValid()
		{
			var domainCredentials = new DomainCredentials();
			AssertEquals(false, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertHasError(domainCredentials.DomainNameInfo, "Please enter a Domain Name.");
			AssertHasError(domainCredentials.DomainUserNameInfo, "Please enter a Domain User Name.");
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "Please enter a Domain User Password.");

			domainCredentials.DomainName = "fake@domain";
			AssertEquals(false, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertHasError(domainCredentials.DomainUserNameInfo, "Please enter a Domain User Name.");
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "Please enter a Domain User Password.");

			domainCredentials.DomainUserName = "dexter@domain";
			domainCredentials.DomainUserPassword = "morgan";
			AssertEquals(false, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertHasError(domainCredentials.DomainNameInfo, "The domain cannot be reached. Please check its availability or if its name is valid.");
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);

			domainCredentials.DomainName = TestConstants.Domain;
			AssertEquals(false, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertHasError(domainCredentials.DomainUserNameInfo, "The domain user name or password is incorrect.");
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "The domain user name or password is incorrect.");

			domainCredentials.DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain;
			AssertEquals(false, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertHasError(domainCredentials.DomainUserNameInfo, "The domain user name or password is incorrect.");
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "The domain user name or password is incorrect.");

			domainCredentials.DomainUserName = "dexter@domain";
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccount.Password;
			AssertEquals(false, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertHasError(domainCredentials.DomainUserNameInfo, "The domain user name or password is incorrect.");
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "The domain user name or password is incorrect.");

			domainCredentials.DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccount.Password;
			AssertEquals(true, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);

			domainCredentials.DomainName = TestConstants.DomainPreWin2000;
			AssertEquals(false, domainCredentials.Validation.AreDomainLoginDetailsValid());
			AssertHasError(domainCredentials.DomainNameInfo, "Please enter a Fully-Qualified Domain Name.");
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);
		}

		public void TestIsOrganisationalUnitValid()
		{
			var domainCredentials = new DomainCredentials();
			AssertEquals(false, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.User));
			AssertEquals(false, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.Group));
			AssertHasError(domainCredentials.DomainNameInfo, "Please enter a Domain Name.");
			AssertHasError(domainCredentials.DomainUserNameInfo, "Please enter a Domain User Name.");
			AssertHasError(domainCredentials.DomainUserPasswordInfo, "Please enter a Domain User Password.");
			AssertNoErrors(domainCredentials.UserOrganisationalUnitInfo);
			AssertNoErrors(domainCredentials.GroupOrganisationalUnitInfo);

			domainCredentials.DomainName = TestConstants.Domain;
			domainCredentials.DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccount.Password;
			AssertEquals(true, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.User));
			AssertEquals(true, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.Group));
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);
			AssertHasWarning(domainCredentials.UserOrganisationalUnitInfo, "You have not entered an Users' Organizational Unit.");
			AssertHasWarning(domainCredentials.GroupOrganisationalUnitInfo, "You have not entered a Groups' Organizational Unit.");
			AssertHasWarning(domainCredentials.UserOrganisationalUnitInfo, "The domain user does not have write access to the selected Organizational Unit. Write access will be required for data to be synchronized to Active Directory.");
			AssertHasWarning(domainCredentials.GroupOrganisationalUnitInfo, "The domain user does not have write access to the selected Organizational Unit. Write access will be required for data to be synchronized to Active Directory.");

			domainCredentials.UserOrganisationalUnit = TestConstants.InvalidOU;
			domainCredentials.GroupOrganisationalUnit = TestConstants.InvalidOU;
			AssertEquals(false, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.User));
			AssertEquals(false, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.Group));
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);
			AssertHasError(domainCredentials.UserOrganisationalUnitInfo, "Please select a valid Organizational Unit.");
			AssertHasError(domainCredentials.GroupOrganisationalUnitInfo, "Please select a valid Organizational Unit.");

			domainCredentials.UserOrganisationalUnit = TestConstants.ValidOU;
			domainCredentials.GroupOrganisationalUnit = TestConstants.ValidOU;
			AssertEquals(true, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.User));
			AssertEquals(true, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.Group));
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);
			AssertNoErrors(domainCredentials.UserOrganisationalUnitInfo);
			AssertNoErrors(domainCredentials.GroupOrganisationalUnitInfo);

			domainCredentials.DomainUserName = TestConstants.ADTestUserAccountNoOURight.NameWithDomain;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccountNoOURight.Password;
			AssertEquals(true, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.User));
			AssertEquals(true, domainCredentials.Validation.IsOrganisationalUnitValid(DomainCredentialsValidation.OrganisationalUnitType.Group));
			AssertNoErrors(domainCredentials.DomainNameInfo);
			AssertNoErrors(domainCredentials.DomainUserNameInfo);
			AssertNoErrors(domainCredentials.DomainUserPasswordInfo);
			AssertHasWarning(domainCredentials.UserOrganisationalUnitInfo, "The domain user does not have write access to the selected Organizational Unit. Write access will be required for data to be synchronized to Active Directory.");
			AssertHasWarning(domainCredentials.GroupOrganisationalUnitInfo, "The domain user does not have write access to the selected Organizational Unit. Write access will be required for data to be synchronized to Active Directory.");
		}

		DomainCredentials GetValidDomainCredentials()
		{
			return new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				IsDefaultDomain = true,
				UserOrganisationalUnit = TestConstants.ValidOU,
				GroupOrganisationalUnit = TestConstants.ValidOU,
				DefaultPassword = "Changeme1234"
			};
		}
	}
}
