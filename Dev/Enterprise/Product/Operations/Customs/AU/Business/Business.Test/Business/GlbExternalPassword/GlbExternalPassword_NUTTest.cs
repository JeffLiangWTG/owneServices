using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_NUT))]
	sealed class GlbExternalPassword_NUTTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<GlbExternalPassword_NUT>
	{
		public void TestHasValidCredential()
		{
			var nut = Factory.New<GlbExternalPassword_NUT>();
			nut.GP_PasswordStatus = ZString.Empty;
			Assert(!nut.HasValidCredential);
			nut.GP_PasswordStatus = Core.Constants.PasswordOK;
			Assert(nut.HasValidCredential);
			nut.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Assert(nut.HasValidCredential);
			nut.GP_PasswordStatus = "NO";
			Assert(!nut.HasValidCredential);
		}

		public void TestNUTAddToStaffCredential()
		{
			GlbExternalPassword.CurrentDecryptedPassword = "Test";
			GlbExternalPassword.GP_UserID = "ZAC";

			Factory.Save();
			var interchange = Factory.GetLatestEHubConfigurationInterchange();
			Configuration configuration;
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				configuration = reader.DeserializeToConfiguration();
			}

			AssertEquals("Configuration name should be set", "NEXDOCSUserLevel", configuration.Name);
			AssertEquals("There should be one group", 1, configuration.Group.Count);

			var systemGroup = configuration.Group[0];
			AssertEquals("Group type should be set", "System", systemGroup.Type);
			AssertEquals("Group reference should be set", "EDIDAT", systemGroup.Reference);
			AssertEquals("There should be one subgroup", 1, systemGroup.Items.Length);

			var companyGroup = (Group)systemGroup.Items[0];
			AssertEquals("Group type should be set", "Company", companyGroup.Type);
			AssertEquals("Group reference should be set", "EDI", companyGroup.Reference);
			AssertEquals("There should be one subgroup", 1, companyGroup.Items.Length);

			var group = (Group)companyGroup.Items[0];
			AssertEquals("Group type should be set", "Staff", group.Type);
			AssertEquals("Group reference should be set", "ZAC", group.Reference);
			AssertEquals("There should be one subgroup", 1, group.Items.Length);

			var subgroup = (Group)group.Items[0];
			AssertEquals("Group type should be set", "NUT", subgroup.Type);
			AssertEquals("Group status should be set", "VAL", subgroup.Status);
			AssertEquals("There should be one subgroup", 1, subgroup.Items.Length);

			var currentCredential = (Credential)subgroup.Items[0];
			AssertEquals("Credential name should be set", "Current", currentCredential.Name);
			AssertEquals("Credential user name should be set", ZString.Empty, currentCredential.UserName);
			AssertNotNull("Credential password should be set", currentCredential.Password);
		}
	}
}
