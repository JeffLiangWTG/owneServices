using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class NatveHandlerTest : TestCaseWithFactory
	{
		void AssertSetUserContext(string companyCode, string expectedLog, Guid expectedCompanyContext, Guid expectedBranchContext, bool isUserInteractive = false)
		{
			var dataContext = new DataContext
			{
				Company = new Company { Code = companyCode }
			};

			var logger = new MemoryLogger();

			using (Globals.SetIsUserInteractiveForTest(isUserInteractive))
			using (NativeHandler.SetUserContext(Factory, DataContextWrapper.New(dataContext), logger))
			{
				var expectedUser = isUserInteractive ? Env.CurrentUser.LoginName : User.InterchangeUserName;
				AssertEquals("Expected staff context", User.InterchangeUserName, Env.CurrentUser.LoginName);
				AssertEquals("Expected branch context", expectedBranchContext, Env.CurrentBranchPK);
				AssertEquals("Expected company context", expectedCompanyContext, Env.CurrentCompanyPK);
				AssertContains(expectedLog, string.Join("\r\n", logger.Buffer.Logs().Select(log => log.Message).ToArray()));
			}
		}

		public void TestUserContext()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var expectedLog = $"Targeting Branch '{branch.GB_Code}', Company '{company.GC_Code}' from Data Context";
			AssertSetUserContext(company.GC_Code, expectedLog, company.PK.ToGuid(), branch.PK.ToGuid());
		}

		public void TestUserContext_UserInteractive()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var expectedLog = $"Targeting Branch '{branch.GB_Code}', Company '{company.GC_Code}' from Data Context";
			AssertSetUserContext(company.GC_Code, expectedLog, company.PK.ToGuid(), branch.PK.ToGuid());
		}

		public void TestUserContext_InvalidCompany()
		{
			var expectedLog = $"Data Context Company 'XXX' does not exist";
			AssertSetUserContext("XXX", expectedLog, Env.CurrentCompanyPK, Env.CurrentBranchPK);
		}

		public void TestUserContext_NoActiveBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var expectedLog = $"Data Context Company '{company.GC_Code}' has no active Branches";
			AssertSetUserContext(company.GC_Code, expectedLog, Env.CurrentCompanyPK, Env.CurrentBranchPK);
		}

		public void TestUserContext_NonSystemUser()
		{
			var dataContext = SetLoginUserContext(out var staff);
			var logger = new MemoryLogger();
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (NativeHandler.SetUserContext(Factory, DataContextWrapper.New(dataContext), logger))
			{
				AssertEquals("Expected staff context", staff.GS_LoginName, Env.CurrentUser.LoginName);
			}
		}

		public void TestUserContext_HasSenderCode()
		{
			var dataContext = SetLoginUserContext(out var staff);
			var logger = new MemoryLogger();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_EI = SetInterchangeSenderProxyUsers(out var interchangeProxyUser).PK;

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (NativeHandler.SetUserContext(Factory, DataContextWrapper.New(dataContext), logger, message))
			{
				AssertEquals("Expected staff context", interchangeProxyUser.GS_LoginName, Env.CurrentUser.LoginName);
			}
		}

		public void TestUserContext_HasEDIClientSecurityProxy()
		{
			var dataContext = SetLoginUserContext(out var staff);
			var logger = new MemoryLogger();
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_EI = SetInterchangeSenderProxyUsers(out var _).PK;

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_LoginName = "Staff3";
			message.EM_ECC_CommunicationPartyConfig = CreatePartyConfig(staff3).PK;

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (NativeHandler.SetUserContext(Factory, DataContextWrapper.New(dataContext), logger, message))
			{
				AssertEquals(staff3.GS_LoginName, Env.CurrentUser.LoginName);
			}
		}

		DataContext SetLoginUserContext(out GlbStaff staff)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			var dataContext = new DataContext
			{
				Company = new Company { Code = company.GC_Code }
			};

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Staff1";
			Factory.Save();
			return dataContext;
		}

		EDIInterchange SetInterchangeSenderProxyUsers(out GlbStaff staff)
		{
			var senderCode = "THESENDER";
			var staffCode = "STF";

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = "Staff2";
			Factory.Save();

			var list = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value;
			var pair = list.AddNew();
			pair.Code = senderCode;
			pair.DescriptionValue = staffCode;
			eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = senderCode;
			return interchange;
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
			Factory.Save();
			return partyConfig;
		}
	}
}
