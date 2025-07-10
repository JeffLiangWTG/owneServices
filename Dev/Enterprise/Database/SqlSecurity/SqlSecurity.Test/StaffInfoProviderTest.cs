using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test
{
	[UseSnapshotProtection]
	class StaffInfoProviderTest : TestCase
	{
		public void TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRoleSelfHostedLocked()
		{
			TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRole(DatabaseTestMode.SelfHostedLocked);
		}

		public void TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRoleSelfHostedOpen()
		{
			TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRole(DatabaseTestMode.SelfHostedOpen);
		}

		public void TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRoleHostedInWiseCloudSharedServer()
		{
			TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRole(DatabaseTestMode.HostedInWiseCloudSharedServer);
		}

		public void TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRoleHostedInWiseCloudDedicatedServer()
		{
			TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRole(DatabaseTestMode.HostedInWiseCloudDedicatedServer);
		}

		public void TestRetrievesSqlLoginInfoForStaffMembersWithDatabaseRolesWhenADIntegrationIsDisabled()
		{
			// Arrange
			var staffInfoProvider = new StaffInfoProvider(cancellationTokenSource.Token, Mock.Of<IntegrationLogging.ILogger>());
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), It.IsAny<IDomainCredentials>())))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staffDbBackUpOperatorReaderAndDeveloper = Factory.New<GlbStaff>();
				staffDbBackUpOperatorReaderAndDeveloper.GS_LoginName = "BobBackUpOperatorReaderAndDeveloper";
				staffDbBackUpOperatorReaderAndDeveloper.GS_FullName = "Bob BackUpOperatorReaderAndDeveloper";
				staffDbBackUpOperatorReaderAndDeveloper.IsDatabaseDeveloper = true;
				staffDbBackUpOperatorReaderAndDeveloper.IsReadOnlyDBUser = true;
				staffDbBackUpOperatorReaderAndDeveloper.IsBackupOperator = true;

				var staffReaderAndBackupOperator = Factory.New<GlbStaff>();
				staffReaderAndBackupOperator.GS_LoginName = "BobReaderAndBackupOperator";
				staffReaderAndBackupOperator.GS_FullName = "Bob ReaderAndBackupOperator";
				staffReaderAndBackupOperator.IsDatabaseDeveloper = false;
				staffReaderAndBackupOperator.IsReadOnlyDBUser = true;
				staffReaderAndBackupOperator.IsBackupOperator = true;

				var staffDeveloperAndBackupOperator = Factory.New<GlbStaff>();
				staffDeveloperAndBackupOperator.GS_LoginName = "BobDeveloperAndBackupOperator";
				staffDeveloperAndBackupOperator.GS_FullName = "Bob DeveloperAndBackupOperator";
				staffDeveloperAndBackupOperator.IsDatabaseDeveloper = true;
				staffDeveloperAndBackupOperator.IsReadOnlyDBUser = false;
				staffDeveloperAndBackupOperator.IsBackupOperator = true;

				var staffReaderAndDeveloper = Factory.New<GlbStaff>();
				staffReaderAndDeveloper.GS_LoginName = "BobReaderAndDeveloper";
				staffReaderAndDeveloper.GS_FullName = "Bob ReaderAndDeveloper";
				staffReaderAndDeveloper.IsDatabaseDeveloper = true;
				staffReaderAndDeveloper.IsReadOnlyDBUser = true;
				staffReaderAndDeveloper.IsBackupOperator = false;

				var staffDbDeveloper = Factory.New<GlbStaff>();
				staffDbDeveloper.GS_LoginName = "BobDeveloper";
				staffDbDeveloper.GS_FullName = "Bob Developer";
				staffDbDeveloper.IsDatabaseDeveloper = true;
				staffDbDeveloper.IsReadOnlyDBUser = false;
				staffDbDeveloper.IsBackupOperator = false;

				var staffDbReader = Factory.New<GlbStaff>();
				staffDbReader.GS_LoginName = "BobReader";
				staffDbReader.GS_FullName = "Bob Reader";
				staffDbReader.IsDatabaseDeveloper = false;
				staffDbReader.IsReadOnlyDBUser = true;
				staffDbReader.IsBackupOperator = false;

				var staffBackupOperator = Factory.New<GlbStaff>();
				staffBackupOperator.GS_LoginName = "BobBackupOperator";
				staffBackupOperator.GS_FullName = "Bob BackupOperator";
				staffBackupOperator.IsDatabaseDeveloper = false;
				staffBackupOperator.IsReadOnlyDBUser = false;
				staffBackupOperator.IsBackupOperator = true;

				var hrmGroup = Factory.New<GlbGroup>();
				hrmGroup.GG_Code = "HRM";
				var hrmRole = hrmGroup.Roles.AddNew();
				hrmRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

				var staffHrmUser = Factory.New<GlbStaff>();
				staffHrmUser.GS_LoginName = "BobHrmUser";
				staffHrmUser.GS_FullName = "Bob HrmUser";
				staffHrmUser.IsDatabaseDeveloper = false;
				staffHrmUser.IsReadOnlyDBUser = false;
				staffHrmUser.IsBackupOperator = false;

				var hrmGroupLink = Factory.New<GlbGroupLink>();
				hrmGroupLink.GK_GS = staffHrmUser.PK;
				hrmGroupLink.GK_GG = hrmGroup.PK;

				var expectedStaffLoginsInfoDictionary = new Dictionary<string, DbUserManager.StaffLoginInfo>()
				{
					{
						Helper.GetEnterpriseLoginFullName(staffDbBackUpOperatorReaderAndDeveloper.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffDbBackUpOperatorReaderAndDeveloper.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(staffReaderAndBackupOperator.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffReaderAndBackupOperator.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles =  new HashSet<string> { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(staffDeveloperAndBackupOperator.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffDeveloperAndBackupOperator.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles =  new HashSet<string> { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(staffReaderAndDeveloper.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffReaderAndDeveloper.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(staffDbDeveloper.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffDbDeveloper.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbDataWriterRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(staffDbReader.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffDbReader.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwRestrictedReaderRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(staffBackupOperator.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffBackupOperator.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.DbBackupOperatorRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(staffHrmUser.GS_LoginName, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							LoginName = Helper.GetEnterpriseLoginFullName(staffHrmUser.GS_LoginName, Db.DatabaseName),
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwHRMStaffRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
				};

				new DbUserManager().SetPasswordForStaff(staffDbBackUpOperatorReaderAndDeveloper, "Pa$$w0rd!");
				new DbUserManager().SetPasswordForStaff(staffReaderAndBackupOperator, "Pa$$w0rd!");
				new DbUserManager().SetPasswordForStaff(staffDeveloperAndBackupOperator, "Pa$$w0rd!");
				new DbUserManager().SetPasswordForStaff(staffReaderAndDeveloper, "Pa$$w0rd!");
				new DbUserManager().SetPasswordForStaff(staffDbDeveloper, "Pa$$w0rd!");
				new DbUserManager().SetPasswordForStaff(staffDbReader, "Pa$$w0rd!");
				new DbUserManager().SetPasswordForStaff(staffBackupOperator, "Pa$$w0rd!");

				Factory.Save();

				var factoryForHrm = new BusinessObjectFactory(Db.Connection);
				new DbUserManager().SetPasswordForStaff(factoryForHrm.Load<GlbStaff>(staffHrmUser.PK), "Pa$$w0rd!");
				factoryForHrm.Save();

				// Act
				var actualStaffLoginsInfoList = staffInfoProvider.GetStaffLoginsInfo();

				// Assert
				AssertEquals("Actual and expected staff login info list should contain the same number or elements.", expectedStaffLoginsInfoDictionary.Count, actualStaffLoginsInfoList.Count());

				foreach (var actualStaffLoginInfo in actualStaffLoginsInfoList)
				{
					AssertEquals($"Actual staff login info for '{actualStaffLoginInfo.LoginName}' is not present in the expected list.", expectedStaffLoginsInfoDictionary.ContainsKey(actualStaffLoginInfo.LoginName), true);
					var expectedStaffLoginInfo = expectedStaffLoginsInfoDictionary[actualStaffLoginInfo.LoginName];

					AssertionsHelper.AssertCollectionsEquivalent("Actual staff login info database access group roles do not match expected.", actualStaffLoginInfo.StaffDatabaseAccessGroupRoles, expectedStaffLoginInfo.StaffDatabaseAccessGroupRoles);
					AssertEquals("Actual staff login info DbAuthenticationMode doest not match expected.", actualStaffLoginInfo.DbAuthenticationMode, expectedStaffLoginInfo.DbAuthenticationMode);
				}
			}
		}

		public void TestRetrievesOnlyWindowsLoginInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInSelfHostedLockedEnvironment()
		{
			TestRetrievesOnlyWindowsLoginInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInSelfHostedEnvironment(DatabaseTestMode.SelfHostedLocked);
		}

		public void TestRetrievesOnlyWindowsLoginInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInSelfHostedOpenEnvironment()
		{
			TestRetrievesOnlyWindowsLoginInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInSelfHostedEnvironment(DatabaseTestMode.SelfHostedOpen);
		}

		public void TestRetrievesWindowsAndSqlLoginsInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInWiseCloudHostedSharedEnvironment()
		{
			TestRetrievesWindowsAndSqlLoginsInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInWiseCloudHostedEnvironment(DatabaseTestMode.HostedInWiseCloudSharedServer);
		}

		public void TestRetrievesWindowsAndSqlLoginsInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInWiseCloudHostedDedicatedEnvironment()
		{
			TestRetrievesWindowsAndSqlLoginsInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInWiseCloudHostedEnvironment(DatabaseTestMode.HostedInWiseCloudDedicatedServer);
		}

		#region Implementation

		void TestGetStaffLoginsInfoDoesNotRetrieveStaffMemberWithRandomRole(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var provider = new StaffInfoProvider(cancellationTokenSource.Token, Mock.Of<IntegrationLogging.ILogger>());

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staffWithRandomRole = Factory.New<GlbStaff>();
				staffWithRandomRole.GS_LoginName = "BobWithRandomRole";
				staffWithRandomRole.GS_FullName = "Bob WithRandomRole";
				staffWithRandomRole.IsDatabaseDeveloper = false;
				staffWithRandomRole.IsReadOnlyDBUser = false;
				staffWithRandomRole.IsBackupOperator = false;

				var randomGroup = Factory.New<GlbGroup>();
				randomGroup.GG_Code = "RG1";
				var randomRole = randomGroup.Roles.AddNew();
				randomRole.GGR_RoleName = "TheRandomRole";

				var randomGroupLink = Factory.New<GlbGroupLink>();
				randomGroupLink.GK_GS = staffWithRandomRole.PK;
				randomGroupLink.GK_GG = randomGroup.PK;

				Factory.Save();

				// Act
				var actualStaffLoginsInfoList = provider.GetStaffLoginsInfo();

				// Assert
				AssertEquals("No staff that require logins should be retrieved", actualStaffLoginsInfoList?.Count() ?? 0, 0);
			}
		}

		void TestRetrievesOnlyWindowsLoginInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInSelfHostedEnvironment(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var provider = new StaffInfoProvider(cancellationTokenSource.Token, Mock.Of<IntegrationLogging.ILogger>());

				var expectedStaffLoginInfo =
				new DbUserManager.StaffLoginInfo()
				{
					LoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000,
					DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
					StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwHRMStaffRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole },
					HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
				};

				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = TestConstants.ADTestUserAccount.Name;
				staff.GS_FullName = TestConstants.ADTestUserAccount.Name;
				staff.IsDatabaseDeveloper = true;
				staff.IsReadOnlyDBUser = true;
				staff.IsBackupOperator = true;
				staff.GS_ActiveDirectoryObjectGuid = System.Guid.Parse(TestConstants.ADTestUserAccount.Guid);

				var group = Factory.New<GlbGroup>();
				var hrmRole = group.Roles.AddNew();
				hrmRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

				var link = Factory.New<GlbGroupLink>();
				link.GK_GS = staff.PK;
				link.GK_GG = group.PK;

				using (ObjectFactory.Substitute(Helper.GetADRegistryMock(true, It.IsAny<string>(), Helper.TestDomainCredentials)))
				using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
				{
					Factory.Save();

					// Act
					var actualStaffLoginsInfoList = provider.GetStaffLoginsInfo();

					// Assert
					AssertEquals("There should be only one actual staff login info.", actualStaffLoginsInfoList.Count(), 1);
					AssertEquals("Actual staff login info DbAuthenticationMode should be Windows.", actualStaffLoginsInfoList.First().DbAuthenticationMode, DbUserManager.DatabaseAuthenticationMode.Windows);
					AssertionsHelper.AssertCollectionsEquivalent("Actual staff login info database access group roles should match expected", actualStaffLoginsInfoList.First().StaffDatabaseAccessGroupRoles, expectedStaffLoginInfo.StaffDatabaseAccessGroupRoles);
				}
			}
		}

		void TestRetrievesWindowsAndSqlLoginsInfoForStaffMemberWithDatabaseRolesWhenAdIntegrationIsEnabledInWiseCloudHostedEnvironment(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				var provider = new StaffInfoProvider(cancellationTokenSource.Token, Mock.Of<IntegrationLogging.ILogger>());

				var expectedStaffLoginsInfoDictionary = new Dictionary<string, DbUserManager.StaffLoginInfo>()
				{
					{
						TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000,
						new DbUserManager.StaffLoginInfo()
						{
							LoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000,
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Windows,
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwHRMStaffRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
					{
						Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName),
						new DbUserManager.StaffLoginInfo()
						{
							LoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName),
							DbAuthenticationMode = DbUserManager.DatabaseAuthenticationMode.Sql,
							StaffDatabaseAccessGroupRoles = new HashSet<string> { DbRoleTypes.CwHRMStaffRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole },
							HashedPassword = "0x0200D45ED6CE8ABEC39D51C8ED8D58B01AC9407D89DF269E9F5DA8423C363A5167CBDFA9B8A58AA446EA51E2B4CA53E3EF2A1E04836D2CDD7F11FC538E066F5BF5255AE1BA7E",
						}
					},
				};

				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = TestConstants.ADTestUserAccount.Name;
				staff.GS_FullName = TestConstants.ADTestUserAccount.Name;
				staff.IsDatabaseDeveloper = true;
				staff.IsReadOnlyDBUser = true;
				staff.IsBackupOperator = true;
				staff.GS_ActiveDirectoryObjectGuid = System.Guid.Parse(TestConstants.ADTestUserAccount.Guid);

				var group = Factory.New<GlbGroup>();
				var hrmRole = group.Roles.AddNew();
				hrmRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

				var link = Factory.New<GlbGroupLink>();
				link.GK_GS = staff.PK;
				link.GK_GG = group.PK;

				using (ObjectFactory.Substitute(Helper.GetADRegistryMock(true, It.IsAny<string>(), Helper.TestDomainCredentials)))
				using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					new DbUserManager().SetPasswordForStaff(staff, "Pa$$w0rd!");
					Factory.Save();

					// Act
					var actualStaffLoginsInfoList = provider.GetStaffLoginsInfo();

					// Assert
					AssertEquals("Actual and expected staff login info list should contain the same number or elements.", actualStaffLoginsInfoList.Count(), expectedStaffLoginsInfoDictionary.Count);

					foreach (var actualStaffLoginInfo in actualStaffLoginsInfoList)
					{
						AssertEquals($"Actual staff login info for '{actualStaffLoginInfo.LoginName}' is not present in the expected list.", expectedStaffLoginsInfoDictionary.ContainsKey(actualStaffLoginInfo.LoginName), true);
						var expectedStaffLoginInfo = expectedStaffLoginsInfoDictionary[actualStaffLoginInfo.LoginName];

						AssertionsHelper.AssertCollectionsEquivalent("Actual staff login info database access group roles do not match expected.", actualStaffLoginInfo.StaffDatabaseAccessGroupRoles, expectedStaffLoginInfo.StaffDatabaseAccessGroupRoles);
						AssertEquals("Actual staff login info DbAuthenticationMode doest not match expected.", actualStaffLoginInfo.DbAuthenticationMode, expectedStaffLoginInfo.DbAuthenticationMode);
					}
				}
			}
		}

		BusinessObjectFactory factory;

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory(Db.Connection);
				}

				return factory;
			}
		}

		IDisposable dropUserRepository;
		CancellationTokenSource cancellationTokenSource;

		protected override void SetUp()
		{
			base.SetUp();
			using (var connection = Db.NewAdminConnection())
			{
				// make sure we have all databases created and that we may need for testing BEFORE transaction scope open
				var userRepositoryDb = $"{Db.DatabaseName}{DbUserRepository.RepositoryDbSuffix}";
				if (!connection.DatabaseExists(userRepositoryDb))
				{
					dropUserRepository = new DisposableAction(() =>
					{
						using (var adminConnection = Db.NewAdminConnection())
						{
							AdoTestUtils.DropDbIfExists(adminConnection, userRepositoryDb);
						}
					});

					(new DbUserRepository()).CreateRepositoryDatabase();
				}
			}

			cancellationTokenSource = new CancellationTokenSource(180000);
		}

		protected override void TearDown()
		{
			base.TearDown();
			factory = null;

			dropUserRepository?.Dispose();
		}

		#endregion Implementation
	}
}
