using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Accounting.GUI.InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD;

namespace Enterprise.Accounting.GUI.Testing
{
	class ARCreditNoteApprovalAlternativeCredentialsTest : TestCaseWithFactory
	{
		readonly SecurityCheckpoint creditNoteApprovalFirstLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval;
		readonly SecurityCheckpoint creditNoteApprovalSecondLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval;
		readonly SecurityCheckpoint creditNoteApprovalThirdLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalThirdLevelApproval;
		readonly SecurityCheckpoint creditNoteApprovalFourthLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalFourthLevelApproval;
		readonly SecurityCheckpoint creditNoteApprovalFifthLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalFifthLevelApproval;
		readonly SecurityCheckpoint creditNoteApprovalSixthLevelApproval = Env.Security.CreditAdjustmentNotePostingApprovalSixthLevelApproval;
		List<BranchDepartmentPair> firstLevelBranchDepartmentPairs;
		List<BranchDepartmentPair> secondLevelBranchDepartmentPairs;
		List<BranchDepartmentPair> thirdLevelBranchDepartmentPairs;
		List<BranchDepartmentPair> fourthLevelBranchDepartmentPairs;
		List<BranchDepartmentPair> fifthLevelBranchDepartmentPairs;
		List<BranchDepartmentPair> sixthLevelBranchDepartmentPairs;

		public void TestValidLogin_Password()
		{
			TestValidLogin("testuser", "password");
		}

		public void TestValidLogin_SecurityOverrideToken()
		{
			ValidateSecurityOverrideToken(() => { TestValidLogin("testuser", "token1"); });
		}

		void TestValidLogin(string username, string passwordOrToken)
		{
			var securityLogin = new ARCreditNoteApprovalAlternativeCredentials(username, passwordOrToken,
																	firstLevelBranchDepartmentPairs, secondLevelBranchDepartmentPairs,
																	thirdLevelBranchDepartmentPairs, fourthLevelBranchDepartmentPairs,
																	fifthLevelBranchDepartmentPairs, sixthLevelBranchDepartmentPairs);
			AssertNotNull(securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalFirstLevelApproval));
			AssertNotNull(securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalSecondLevelApproval));
			AssertNotNull(securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalThirdLevelApproval));
			AssertNotNull(securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalFourthLevelApproval));
			AssertNotNull(securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalFifthLevelApproval));
			AssertNotNull(securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalSixthLevelApproval));
		}

		public void TestInvalidLogin_Password()
		{
			TestInvalidLogin("invaliduserid", "");
		}

		public void TestInvalidLogin_SecurityOverrideToken()
		{
			ValidateSecurityOverrideToken(() => { TestInvalidLogin("testuser", "password"); });
		}

		void TestInvalidLogin(string username, string passwordOrToken)
		{
			var securityLogin = new ARCreditNoteApprovalAlternativeCredentials(username, passwordOrToken,
																	firstLevelBranchDepartmentPairs, secondLevelBranchDepartmentPairs,
																	thirdLevelBranchDepartmentPairs, fourthLevelBranchDepartmentPairs,
																	fifthLevelBranchDepartmentPairs, sixthLevelBranchDepartmentPairs);
			AssertEquals("User Securities for First Level should be 0", 0, securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalFirstLevelApproval).Length);
			AssertEquals("User Securities for Second Level should be 0", 0, securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalSecondLevelApproval).Length);
			AssertEquals("User Securities for Third Level should be 0", 0, securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalThirdLevelApproval).Length);
			AssertEquals("User Securities for Fourth Level should be 0", 0, securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalFourthLevelApproval).Length);
			AssertEquals("User Securities for Fifth Level should be 0", 0, securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalFifthLevelApproval).Length);
			AssertEquals("User Securities for Sixth Level should be 0", 0, securityLogin.GetUserSecuritiesCheckForBranchDepartment(creditNoteApprovalSixthLevelApproval).Length);
		}

		void SetUpUserAndBranchDepartment()
		{
			SecurityTestObject.CreateTestUser(true, "", "tst", "testuser", "password");
			var testObjectCreator = new TestObjectCreator(Factory);

			firstLevelBranchDepartmentPairs = new List<BranchDepartmentPair>();
			firstLevelBranchDepartmentPairs.Add(new BranchDepartmentPair(Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			secondLevelBranchDepartmentPairs = new List<BranchDepartmentPair>();
			secondLevelBranchDepartmentPairs.Add(new BranchDepartmentPair(testObjectCreator.NonCurrentBranch.PK, testObjectCreator.NonCurrentBranch.PK));
			thirdLevelBranchDepartmentPairs = new List<BranchDepartmentPair>();
			thirdLevelBranchDepartmentPairs.Add(new BranchDepartmentPair(testObjectCreator.NonCurrentBranch.PK, testObjectCreator.NonCurrentBranch.PK));
			fourthLevelBranchDepartmentPairs = new List<BranchDepartmentPair>();
			fourthLevelBranchDepartmentPairs.Add(new BranchDepartmentPair(testObjectCreator.NonCurrentBranch.PK, testObjectCreator.NonCurrentBranch.PK));
			fifthLevelBranchDepartmentPairs = new List<BranchDepartmentPair>();
			fifthLevelBranchDepartmentPairs.Add(new BranchDepartmentPair(testObjectCreator.NonCurrentBranch.PK, testObjectCreator.NonCurrentBranch.PK));
			sixthLevelBranchDepartmentPairs = new List<BranchDepartmentPair>();
			sixthLevelBranchDepartmentPairs.Add(new BranchDepartmentPair(testObjectCreator.NonCurrentBranch.PK, testObjectCreator.NonCurrentBranch.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpUserAndBranchDepartment();
		}

		void ValidateSecurityOverrideToken(Action action)
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "tst"));
			var mockIOIDCConfig = new Mock<IOIDCConfig>();
			mockIOIDCConfig.Setup(m => m.IsOIDCEnabled).Returns(true);
			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var stmAccessToken = Factory.New<StmAccessToken>();
				stmAccessToken.SAT_Type = AccessTokenTypes.SecurityOverrideToken;
				stmAccessToken.SAT_ParentTableCode = GlbStaffSchema.Constants.Prefix;
				stmAccessToken.SAT_ParentId = staff.PK;
				stmAccessToken.SAT_ExpiresAt = ZDateTime.Now.AddMinutes(10);
				stmAccessToken.SAT_RemainingUseCount = 1;
				stmAccessToken.SAT_Token = "token1";
				Factory.Save();

				action();
			}
		}
	}
}
