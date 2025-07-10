using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.Testing
{
	class UserContextExtractorTest : TestCaseWithFactory
	{
		public void TestGetUserForEnvironmentReturnsMessageSecurityProxy()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Staff1";

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ECC_CommunicationPartyConfig = CreatePartyConfig(staff).PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var user = UserContextExtractor.GetUserForEnvironment(message);
				AssertEquals(staff.GS_LoginName, user.LoginName);
			}
		}

		public void TestGetUserForEnvironmentDoesNotReturnMessageSecurityProxyIfNotSystemContext()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Staff1";

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ECC_CommunicationPartyConfig = CreatePartyConfig(staff1).PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "Staff2";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff2.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var user = UserContextExtractor.GetUserForEnvironment(message);
				AssertEquals(staff2.GS_LoginName, user.LoginName);
			}
		}

		public void TestGetUserForEnvironmentReturnsCurrentNonSystemUser()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Staff1";

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ECC_CommunicationPartyConfig = CreatePartyConfig(staff1).PK;

			message.EM_EI = SetInterchangeSenderProxyUsers(out var _).PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "Staff2";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff2.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var user = UserContextExtractor.GetUserForEnvironment(message);
				AssertEquals(staff2.GS_LoginName, user.LoginName);
			}
		}

		public void TestGetUserForEnvironmentReturnsInterchangeSenderProxyUser_IfNoSecurityProxy()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ECC_CommunicationPartyConfig = CreatePartyConfig(null).PK;
			message.EM_EI = SetInterchangeSenderProxyUsers(out var interchangeProxyUser).PK;

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var user = UserContextExtractor.GetUserForEnvironment(message);
				AssertEquals(interchangeProxyUser.GS_LoginName, user.LoginName);
			}
		}

		public void TestGetUserForEnvironmentReturnsInterchangeSenderProxyUser()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_EI = SetInterchangeSenderProxyUsers(out var interchangeProxyUser).PK;

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var user = UserContextExtractor.GetUserForEnvironment(message);
				AssertEquals(interchangeProxyUser.GS_LoginName, user.LoginName);
			}
		}

		public void TestGetUserForEnvironmentWithInterchangeSenderProxyUserReturnsMessageSecurityProxy()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Staff1";
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ECC_CommunicationPartyConfig = CreatePartyConfig(staff1).PK;
			message.EM_EI = SetInterchangeSenderProxyUsers(out var _).PK;

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var user = UserContextExtractor.GetUserForEnvironment(message);
				AssertEquals(staff1.GS_LoginName, user.LoginName);
			}
		}

		EDICommunicationPartyConfig CreatePartyConfig(GlbStaff staff)
		{
			var partyConfig = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			partyConfig.ECC_ECP_Party = party.PK;
			if (staff != null)
			{
				party.ECP_GS_SecurityProxy = staff.PK;
			}
			return partyConfig;
		}

		EDIInterchange SetInterchangeSenderProxyUsers(out GlbStaff staff)
		{
			var senderCode = "THESENDER";
			var staffCode = "STF";

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			Factory.Save();
			// Populate the registry item so it will use our bespoke staff member
			var list = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value;
			var pair = list.AddNew();
			pair.Code = senderCode;
			pair.DescriptionValue = staffCode;
			eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = senderCode;
			return interchange;
		}
	}
}
