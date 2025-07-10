using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteTemplateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAccessPropertiesValidation()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				StmNoteTemplate template = Factory.New<StmNoteTemplate>();
				template.Validation.ValidateAll();
				AssertNoErrors(template.IsPublishedInfo);
				AssertNoErrors(template.IsAllCompaniesInfo);

				template.IsPublished = true;
				AssertHasErrors(template.IsPublishedInfo);
				template.SecurityProvider.ClearAllCompaniesUserContext();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				StmNoteTemplate template = Factory.New<StmNoteTemplate>();
				template.Validation.ValidateAll();
				AssertNoErrors(template.IsPublishedInfo);
				AssertNoErrors(template.IsAllCompaniesInfo);

				template.IsPublished = true;
				AssertNoErrors(template.IsPublishedInfo);
				AssertNoErrors(template.IsAllCompaniesInfo);

				template.IsAllCompanies = true;
				AssertHasErrors(template.IsAllCompaniesInfo);
				template.SecurityProvider.ClearAllCompaniesUserContext();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffC.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				StmNoteTemplate template = Factory.New<StmNoteTemplate>();
				template.Validation.ValidateAll();
				AssertNoErrors(template.IsPublishedInfo);
				AssertNoErrors(template.IsAllCompaniesInfo);

				template.IsPublished = true;
				template.IsAllCompanies = true;
				AssertNoErrors(template.IsPublishedInfo);
				AssertNoErrors(template.IsAllCompaniesInfo);
			}
		}

		public void TestCheckDescriptionIsUnique()
		{
			IGlbCompany company2 = Factory.New<IGlbCompany>();
			IGlbBranch branch2 = Factory.New<IGlbBranch>();
			branch2.GB_GC = company2.PK;
			Factory.Save();

			StmNoteTemplate template;

			using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "banana";
				template.S8_Description = "float";
				template.IsPublished = true;
				template.Validation.ValidateAll();
				AssertNoErrors(template.S8_DescriptionInfo);

				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "boat";
				template.S8_Description = "float";
				template.IsPublished = true;
				template.Validation.ValidateAll();
				AssertNoErrors(template.S8_DescriptionInfo);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "banana";
				template.S8_Description = "yellow";
				AssertNoErrors(template.S8_DescriptionInfo);

				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "boat";
				template.S8_Description = "yellow";
				AssertNoErrors(template.S8_DescriptionInfo);

				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "banana";
				template.S8_Description = "tasty";
				AssertNoErrors(template.S8_DescriptionInfo);

				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "banana";
				template.S8_Description = "yellow";
				AssertHasErrors(template.S8_DescriptionInfo);

				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "boat";
				template.S8_Description = "float";
				AssertHasErrors(template.S8_DescriptionInfo);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "banana";
				template.S8_Description = "yellow";
				template.IsPublished = true;
				template.Validation.ValidateAll();
				AssertNoErrors(template.S8_DescriptionInfo);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffC.GS_LoginName, branch2.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "banana";
				template.S8_Description = "float";
				template.IsPublished = true;
				template.Validation.ValidateAll();
				AssertNoErrors(template.S8_DescriptionInfo);

				template = Factory.New<StmNoteTemplate>();
				template.S8_ContextID = "boat";
				template.S8_Description = "float";
				template.IsPublished = true;
				template.IsAllCompanies = true;
				template.Validation.ValidateAll();
				AssertHasErrors(template.S8_DescriptionInfo);
			}
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

		#endregion
	}
}
