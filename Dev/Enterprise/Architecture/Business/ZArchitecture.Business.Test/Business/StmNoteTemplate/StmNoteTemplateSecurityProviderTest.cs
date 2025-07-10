using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteTemplateSecurityProviderTest : TestCaseWithFactory
	{
		public void TestCacheCurrentUserCanPublishAcrossAllCompanies()
		{
			var template = Factory.New<StmNoteTemplate>();
			var changedCount = 0;
			try
			{
				EnvProxy.Instance.UserContextChanging += Instance_UserContextChanging;

				using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					Assert("staffA !CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
					AssertEquals(3, changedCount);

					using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
					{
						Assert("staffB !CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
						AssertEquals(6, changedCount);

						using (EnvProxy.Instance.SetTemporaryUserContext(staffC.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
						{
							Assert("staffC CurrentUserCanPublishAccrossAllCompanies", template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
							AssertEquals(9, changedCount);

							Assert("staffC CurrentUserCanPublishAccrossAllCompanies", template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
							AssertEquals("Should not change as it's cached.", 9, changedCount);
						}

						Assert("staffB !CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
					}

					Assert("staffA !CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				}
			}
			finally
			{
				EnvProxy.Instance.UserContextChanging -= Instance_UserContextChanging;
			}

			void Instance_UserContextChanging(object sender, IUserContextChangingEventArgs e)
			{
				changedCount++;
			}
		}

		public void TestAccessProperties()
		{
			StmNoteTemplate template = Factory.New<StmNoteTemplate>();
			AssertEquals(false, template.IsPublished);
			AssertEquals(false, template.IsAllCompanies);
			AssertEquals(EnvProxy.Instance.CurrentUser.Initials, template.S8_GS_NKStaff);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, template.S8_GC);

			template.IsPublished = true;
			AssertEquals(true, template.IsPublished);
			AssertEquals(false, template.IsAllCompanies);
			AssertEquals(ZString.Empty, template.S8_GS_NKStaff);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, template.S8_GC);

			using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Assert("Default CurrentUserCanPublish", !template.SecurityProvider.CurrentUserCanPublish);
				Assert("Default CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				Assert("Should not mark note template as read only - using validation instead", !template.ReadOnly);
				template.SecurityProvider.ClearAllCompaniesUserContext();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Assert("CurrentUserCanPublish", template.SecurityProvider.CurrentUserCanPublish);
				Assert("!CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				Assert("Should not mark note template as read only - using validation instead", !template.ReadOnly);
			}

			template.IsAllCompanies = true;
			AssertEquals(true, template.IsPublished);
			AssertEquals(true, template.IsAllCompanies);
			AssertEquals(ZString.Empty, template.S8_GS_NKStaff);
			AssertEquals(ZGuid.Empty, template.S8_GC);

			template.IsAllCompanies = false;
			AssertEquals(true, template.IsPublished);
			AssertEquals(false, template.IsAllCompanies);
			AssertEquals(ZString.Empty, template.S8_GS_NKStaff);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, template.S8_GC);

			template.IsPublished = false;
			AssertEquals(false, template.IsPublished);
			AssertEquals(false, template.IsAllCompanies);
			AssertEquals(EnvProxy.Instance.CurrentUser.Initials, template.S8_GS_NKStaff);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, template.S8_GC);
		}

		public void TestAccessPropertiesForNonEnglishUsers()
		{
			staffA.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.ChineseSimplified;
			staffB.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.ChineseSimplified;
			Factory.Save();

			var template = Factory.New<StmNoteTemplate>();
			using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Assert("Default CurrentUserCanPublish", !template.SecurityProvider.CurrentUserCanPublish);
				Assert("Default CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				Assert("Should not mark note template as read only - using validation instead", !template.ReadOnly);
				template.SecurityProvider.ClearAllCompaniesUserContext();
			}

			template = Factory.New<StmNoteTemplate>();
			using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Assert("CurrentUserCanPublish", template.SecurityProvider.CurrentUserCanPublish);
				Assert("!CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				Assert("Should not mark note template as read only - using validation instead", !template.ReadOnly);
			}
		}

		public void TestIsTemplateEditableByUser()
		{
			StmNoteTemplate template = Factory.New<StmNoteTemplate>();
			var securityProvider = new StmNoteTemplateSecurityProvider();

			using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Assert("Default CurrentUserCanPublish", !template.SecurityProvider.CurrentUserCanPublish);
				Assert("Default CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				Assert("Should not mark note template as read only - using validation instead", !template.ReadOnly);

				template.IsPublished = true;
				template.IsAllCompanies = false;
				Assert(!securityProvider.IsTemplateEditableByUser(template));
				template.IsPublished = false;
				template.IsAllCompanies = false;
				Assert(securityProvider.IsTemplateEditableByUser(template));

				securityProvider.ClearAllCompaniesUserContext();
				template.SecurityProvider.ClearAllCompaniesUserContext();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Assert("CurrentUserCanPublish", template.SecurityProvider.CurrentUserCanPublish);
				Assert("!CurrentUserCanPublishAccrossAllCompanies", !template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				Assert("Should not mark note template as read only - using validation instead", !template.ReadOnly);

				template.IsPublished = false;
				template.IsAllCompanies = false;
				Assert(securityProvider.IsTemplateEditableByUser(template));

				template.IsPublished = false;
				template.IsAllCompanies = true;
				Assert(!securityProvider.IsTemplateEditableByUser(template));

				securityProvider.ClearAllCompaniesUserContext();
				template.SecurityProvider.ClearAllCompaniesUserContext();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffC.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Assert("CurrentUserCanPublish", template.SecurityProvider.CurrentUserCanPublish);
				Assert("CurrentUserCanPublishAccrossAllCompanies", template.SecurityProvider.CurrentUserCanPublishAcrossAllCompanies);
				Assert("Should not mark note template as read only - using validation instead", !template.ReadOnly);

				template.IsPublished = false;
				template.IsAllCompanies = false;
				Assert(securityProvider.IsTemplateEditableByUser(template));

				template.IsPublished = true;
				template.IsAllCompanies = true;
				Assert(securityProvider.IsTemplateEditableByUser(template));
			}
		}

		static void SetupSecurity(BusinessObjectFactory factory, out IGlbStaff staffA, out IGlbStaff staffB, out IGlbStaff staffD)
		{
			staffA = factory.New<IGlbStaff>();
			staffA.GS_Code = "AAA";
			staffA.GS_LoginName = "alpha";
			staffA.GS_FullName = "Alpha Albert Anaheim";

			staffB = factory.New<IGlbStaff>();
			staffB.GS_Code = "BBB";
			staffB.GS_LoginName = "beta";
			staffB.GS_FullName = "Beta Brett Boston";

			IGlbSecurity security = factory.New<IGlbSecurity>();
			security.GU_GS = staffB.PK;
			security.GU_GC = EnvProxy.Instance.CurrentCompany.PK;
			security.GU_SecurityRight = StmNoteTemplateSecurityProvider.SecurityCheckpointForPublish.Code;
			security.GU_SecurityItemIsAllowed = ZBool.True;

			staffD = factory.New<IGlbStaff>();
			staffD.GS_Code = "DDD";
			staffD.GS_LoginName = "delta";
			staffD.GS_FullName = "Delta Dan Detriot";

			security = factory.New<IGlbSecurity>();
			security.GU_GS = staffD.PK;
			security.GU_SecurityRight = StmNoteTemplateSecurityProvider.SecurityCheckpointForPublish.Code;
			security.GU_SecurityItemIsAllowed = ZBool.True;

			factory.Save();
		}

		#region Implementation

		IGlbStaff staffA;
		IGlbStaff staffB;
		IGlbStaff staffC;

		protected override void SetUp()
		{
			base.SetUp();
			SetupSecurity(Factory, out staffA, out staffB, out staffC);
		}

		#endregion

	}
}
