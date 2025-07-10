using System;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(DomainCredentialsCollectionRegistryDataType))]
	class DomainCredentialsCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DomainCredentialsCollectionRegistryDataType>
	{
		#region Validation

		public void TestValidate_WithInvalidDomainName_ShouldThrowValidationException()
		{
			AssertValidationErrorThrown("The domains' credentials are not valid.", new DomainCredentialsCollection
			{
				new DomainCredentials
				{
					DomainName = "fake.domain",
					DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
					DomainUserPassword = TestConstants.ADTestUserAccount.Password,
					IsDefaultDomain = true,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234"
				}
			});
		}

		public void TestValidate_WithInvalidUserName_ShouldThrowValidationException()
		{
			AssertValidationErrorThrown("The domains' credentials are not valid.", new DomainCredentialsCollection
			{
				new DomainCredentials
				{
					DomainName = TestConstants.Domain,
					DomainUserName = "fake.user@domain",
					DomainUserPassword = TestConstants.ADTestUserAccount.Password,
					IsDefaultDomain = true,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234"
				}
			});
		}

		public void TestValidate_WithInvalidUserPassword_ShouldThrowValidationException()
		{
			AssertValidationErrorThrown("The domains' credentials are not valid.", new DomainCredentialsCollection
			{
				new DomainCredentials
				{
					DomainName = TestConstants.Domain,
					DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
					DomainUserPassword = "ThisIsNotTheRightPassword",
					IsDefaultDomain = true,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234"
				}
			});
		}

		public void TestValidate_MultipleDomains_OneWithInvalidUserDomain_ShouldThrowValidationException()
		{
			AssertValidationErrorThrown(@"The domains' credentials are not valid.", new DomainCredentialsCollection
			{
				GetValidDomainCredentials(),
				new DomainCredentials
				{
					DomainName = "fake.domain",
					DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
					DomainUserPassword = TestConstants.ADTestUserAccount.Password,
					IsDefaultDomain = false,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234",
				}
			});
		}

		public void TestValidate_WithValidDomain()
		{
			AssertNoValidationErrorThrown(new DomainCredentialsCollection
			{
				new DomainCredentials
				{
					DomainName = TestConstants.Domain,
					DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
					DomainUserPassword = TestConstants.ADTestUserAccount.Password,
					IsDefaultDomain = true,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234",
				}
			});

			AssertNoValidationErrorThrown(new DomainCredentialsCollection());
		}

		public void TestValidate_WithValidDomain_ButCancellingUserNotification_ShouldThrowValidationException()
		{
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);

			AssertValidationErrorThrown("Canceled saving this registry value", new DomainCredentialsCollection
			{
				GetValidDomainCredentials()
			});

			var credentials = ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value;
			AssertEquals(0, credentials.Count);
		}

		public void TestValidate_RemovingADomainThatStillHasUsers_ShouldThrowValidationException()
		{
			var domains = ADTestHelper.CreateDomainCredentialsCollection();

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "ValidateTestUser1";
			staff.GS_DomainName = domains[0].DomainName;

			factory.Save();

			var registryItem = GetNewRegistryItem(domains);

			AssertValidationErrorThrownCore("You cannot rename or remove the following domains as they are still in use: sand.wtg.zone", new DomainCredentialsCollection(), registryItem);
		}

		public void TestValidate_RemovingADomainThatStillHasGroups_ShouldThrowValidationException()
		{
			var domains = ADTestHelper.CreateDomainCredentialsCollection();

			var factory = new BusinessObjectFactory();
			var group = factory.New<GlbGroup>();
			group.GG_Desc = "ValidateTestGroup1";
			group.GG_Code = "1TG";
			group.GG_DomainName = domains[0].DomainName;

			factory.Save();

			var registryItem = GetNewRegistryItem(domains);

			AssertValidationErrorThrownCore("You cannot rename or remove the following domains as they are still in use: sand.wtg.zone", new DomainCredentialsCollection(), registryItem);
		}

		public void TestValidate_RenamingADomainThatStillHasUsers_ShouldThrowValidationException()
		{
			var domains = new DomainCredentialsCollection {
				new DomainCredentials() { DomainName = "NewDomainName" }
			};

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "ValidateTestUser";
			staff.GS_DomainName = domains[0].DomainName;

			factory.Save();

			var registryItem = GetNewRegistryItem(domains);

			AssertValidationErrorThrownCore("You cannot rename or remove the following domains as they are still in use: NewDomainName", ADTestHelper.CreateDomainCredentialsCollection(), registryItem);
		}

		public void TestValidate_RenamingADomainThatStillHasGroups_ShouldThrowValidationException()
		{
			var domains = new DomainCredentialsCollection {
				new DomainCredentials() { DomainName = "NewDomainName" }
			};

			var factory = new BusinessObjectFactory();
			var group = factory.New<GlbGroup>();
			group.GG_Desc = "ValidateTestGroup";
			group.GG_Code = "TG";
			group.GG_DomainName = domains[0].DomainName;

			factory.Save();

			var registryItem = GetNewRegistryItem(domains);

			AssertValidationErrorThrownCore("You cannot rename or remove the following domains as they are still in use: NewDomainName", ADTestHelper.CreateDomainCredentialsCollection(), registryItem);
		}

		public void TestValidate_RemovingLastDomainCredentials_ShouldThrowValidationException()
		{
			var domains = new DomainCredentialsCollection()
			{
				new DomainCredentials() {
					DomainName = "domainNameForGroupsTest"
				}
			};
			var registryItem = GetNewRegistryItem(domains);

			var oldValue = ActiveDirectoryRegistry.Instance.IsIntegrationEnabled;
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			try
			{
				AssertValidationErrorThrownCore("You need to have at least one domain set in this registry while AD Integration is enabled.", new DomainCredentialsCollection(), registryItem);
			}
			finally
			{
				ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = oldValue;
			}
		}

		public void TestValidate_RenamingADomainThatStillHasUsers_ShouldWorkAfterMovingTheUserToOtherDomain()
		{
			var domains = new DomainCredentialsCollection {
				ADTestHelper.CreateDomainCredentials(),
				new DomainCredentials() { DomainName = "DomainName1" }
			};

			var factory = new BusinessObjectFactory();
			var staff1 = factory.New<GlbStaff>();
			staff1.GS_LoginName = "ValidateTestUser1";
			staff1.GS_DomainName = domains[0].DomainName;

			var staff2 = factory.New<GlbStaff>();
			staff2.GS_LoginName = "ValidateTestUser2";
			staff2.GS_DomainName = domains[1].DomainName;

			factory.Save();

			var registryItem = GetNewRegistryItem(domains);
			var newDomains = new DomainCredentialsCollection { domains[0] };

			AssertValidationErrorThrownCore("You cannot rename or remove the following domains as they are still in use: DomainName1", newDomains, registryItem);

			staff2.GS_DomainName = domains[0].DomainName;

			factory.Save();

			AssertValidationErrorThrownCore(null, newDomains, registryItem);
		}
		#endregion

		#region Assertions

		void AssertValidationErrorThrown(string expectedValidationError, DomainCredentialsCollection proposedValue)
		{
			AssertValidationErrorThrownCore(expectedValidationError, proposedValue);
		}

		void AssertNoValidationErrorThrown(DomainCredentialsCollection proposedValue)
		{
			AssertValidationErrorThrownCore(null, proposedValue);
		}

		void AssertValidationErrorThrownCore(string expectedValidationError, DomainCredentialsCollection proposedValue, IRegistryItem currentRegistryItem = null)
		{
			var registryItem = currentRegistryItem ?? GetNewRegistryItem(new DomainCredentialsCollection());
			var validate = new AnonymousMethod(() => DataType.ValidateBeforeRegistryFormSave(registryItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));

			if (expectedValidationError == null)
			{
				AssertNoExceptionThrown(validate);
				var expectedWarning = string.Format($@"Changing this registry item may cause users and groups to be created in the new domains for existing staff and group records in {Core.Constants.ProductName}.
Please enter the text 'CHANGEDOMAIN' to continue. It is advisable to restart {Core.Constants.ProductName} after changing this registry item's value.");
				AssertEquals(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("CHANGEDOMAIN", UnitTestUserNotification.Instance.LastConfirmationStringShown);
			}
			else
			{
				var ex = AssertExceptionThrown<RegistryValidationException>(validate);
				AssertMultilineASCIIEquals("", expectedValidationError, ex.Message);
			}
		}

		#endregion

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

		#region Implementation

		protected override string ExpectedEditorName => "DomainCredentialsCollectionRegistryEditor";

		protected override DomainCredentialsCollectionRegistryDataType GetNewDataType()
		{
			return new DomainCredentialsCollectionRegistryDataType(new DomainCredentialsCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var credentials1 = new DomainCredentialsCollection();

			var credentials2 = new DomainCredentialsCollection();
			credentials2.Add(new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				IsDefaultDomain = true,
				UserOrganisationalUnit = TestConstants.ValidOU,
				GroupOrganisationalUnit = TestConstants.ValidOU,
				DefaultPassword = "Changeme1234"
			});

			var credentials3 = new DomainCredentialsCollection
			{
				new DomainCredentials
				{
					DomainName = TestConstants.Domain,
					DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
					DomainUserPassword = TestConstants.ADTestUserAccount.Password,
					IsDefaultDomain = true,
					UserOrganisationalUnit = TestConstants.ValidOU,
					GroupOrganisationalUnit = TestConstants.ValidOU,
					DefaultPassword = "Changeme1234"
				},
				new DomainCredentials
				{
					DomainName = "fake.domain",
					DomainUserName = "fake.user",
					DomainUserPassword = "fakepassword",
					IsDefaultDomain = false,
					UserOrganisationalUnit = "fake/users/ou",
					GroupOrganisationalUnit = "fake/groups/ou",
					DefaultPassword = "Changeme1234"
				}
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(credentials1, DataType.Serialise(credentials1)),
				new ValidSampleAndBinaryValueInDB(credentials2, DataType.Serialise(credentials2)),
				new ValidSampleAndBinaryValueInDB(credentials3, DataType.Serialise(credentials3)),
			};
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;
		#endregion
	}
}
