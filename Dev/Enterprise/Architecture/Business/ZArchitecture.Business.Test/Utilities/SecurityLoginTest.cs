using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(SecurityLogin))]
	sealed class SecurityLoginTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorCopiesSecurityList()
		{
			var securityList = new List<Func<SecurityCore, SecurityCheckpoint>>
			{
				(s) => s.ReceivablesOnCreditHoldController,
				(s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr
			};
			var loginBisObject = new SecurityLogin(securityList);
			loginBisObject.SecurityCheckpoints.Clear();
			AssertEquals("login bizo checkpoints", 0, loginBisObject.SecurityCheckpoints.Count);
			AssertEquals("original list of checkpoints", 2, securityList.Count);
		}

		#region TestLogin

		public void TestLogin()
		{
			SecurityLogin loginBisObject = new SecurityLogin((s) => s.ReceivablesOnCreditHoldController);
			AssertEquals("Login is empty", ZString.Empty, loginBisObject.Login);

			loginBisObject.ValidateLogin();
			Assert("Login has an error", loginBisObject.LoginInfo.HasErrors());

			loginBisObject.Login = "Login Name";
			AssertEquals("Login is not empty", "Login Name", loginBisObject.Login);

			loginBisObject.ValidateLogin();
			Assert("Login has does not have an error", !loginBisObject.LoginInfo.HasErrors());
		}

		#endregion

		#region TestPassword

		public void TestPassword()
		{
			SecurityLogin loginBisObject = new SecurityLogin((s) => s.ReceivablesOnCreditHoldController);
			AssertEquals("Password is empty", ZString.Empty, loginBisObject.Password);

			loginBisObject.ValidatePassword();
			Assert("Password has an error", loginBisObject.PasswordInfo.HasErrors());

			loginBisObject.Password = "Password";
			AssertEquals("Password is not empty", "Password", loginBisObject.Password);

			loginBisObject.ValidatePassword();
			Assert("Password has does not have an error", !loginBisObject.PasswordInfo.HasErrors());
		}

		#endregion

		#region TestMessageToShowWhenNotPrinting

		public void TestMessageToShowWhenNotPrinting()
		{
			SecurityLogin loginBisObject = new SecurityLogin((s) => s.ReceivablesOnCreditHoldController);
			AssertEquals("MessageToShowWhenNotPrinting", ZString.Empty, loginBisObject.MessageToShowWhenNotPrinting);

			loginBisObject.MessageToShowWhenNotPrinting = "Testing the message";
			AssertEquals("MessageToShowWhenNotPrinting", "Testing the message", loginBisObject.MessageToShowWhenNotPrinting);
		}

		#endregion

		#region TestCheckIfValidLoginForDocumentPrinting

		public void TestCheckIfValidLoginForDocumentPrinting()
		{
			SecurityLogin loginBisObject = new SecurityLogin((s) => s.ReceivablesOnCreditHoldController);
			IUserLoginController loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());

			loginBisObject.Login = "test";
			loginBisObject.Password = "password";
			Assert("Login should be incorrect", !loginController.ValidateUserLoginAndPassword(loginBisObject.Login, loginBisObject.Password).LoginValidated);
			Assert("!CheckIfValidLoginForDocumentPrinting", !loginBisObject.CheckIfValidLoginForDocumentPrinting());

			loginBisObject.Login = ZArchitecture.Environment.User.SupportUserName;
			loginBisObject.Password = ZArchitecture.Environment.User.MasterPassword;
			Assert("Login should be correct", loginController.ValidateUserLoginAndPassword(loginBisObject.Login, loginBisObject.Password).LoginValidated);
			Assert("CheckIfValidLoginForDocumentPrinting (valid login and valid rights)", loginBisObject.CheckIfValidLoginForDocumentPrinting());

			loginBisObject = new SecurityLogin((s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr);
			loginBisObject.Login = ZArchitecture.Environment.User.SupportUserName;
			loginBisObject.Password = ZArchitecture.Environment.User.MasterPassword;
			Assert("Login should be correct", loginController.ValidateUserLoginAndPassword(loginBisObject.Login, loginBisObject.Password).LoginValidated);
			Assert("CheckIfValidLoginForDocumentPrinting (valid login and valid rights)", loginBisObject.CheckIfValidLoginForDocumentPrinting());
		}

		public void TestCheckIfValidLoginForDocumentPrinting_ExpiredPassword()
		{
			var loginBisObject = new SecurityLogin((s) => s.ReceivablesOnCreditHoldController);
			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());

			var staff = Factory.New<IGlbStaff>();
			staff.GS_LoginName = "testUser";
			staff.GS_Code = "XXX";
			staff.StaffPlainTextPassword = "testPassword";
			staff.GS_ChangePasswordAtNextLogin = true;
			staff.GS_IsSystemAccount = true;
			Factory.Save();

			loginBisObject.Login = "testUser";
			loginBisObject.Password = "testPassword";

			Assert("Login should be incorrect", !loginController.ValidateUserLoginAndPasswordAndExpiredPassword(loginBisObject.Login, loginBisObject.Password).LoginValidated);
			Assert("!CheckIfValidLoginForDocumentPrinting", !loginBisObject.CheckIfValidLoginForDocumentPrinting());

			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();
			Assert("Login should be correct", loginController.ValidateUserLoginAndPasswordAndExpiredPassword(loginBisObject.Login, loginBisObject.Password).LoginValidated);
			Assert("CheckIfValidLoginForDocumentPrinting (valid login and valid rights)", loginBisObject.CheckIfValidLoginForDocumentPrinting());
		}

		#endregion

		#region TestIsRestrictedByCheckpoint

		public void TestIsRestrictedByCheckpoint_ListContainsTargetCheckpoint_ReturnsTrue()
		{
			var securityCore = GetTestSecurityCore();
			var targetCheckpointFunc = (Func<SecurityCore, SecurityCheckpoint>)(s => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr);
			var targetLookupKey = targetCheckpointFunc(securityCore).LookupKey;

			var checkpointList = new List<Func<SecurityCore, SecurityCheckpoint>>
			{
				s => s.ReceivablesOnCreditHoldController,
				targetCheckpointFunc
			};
			var loginBisObject = new SecurityLogin(checkpointList);

			var result = loginBisObject.IsRestrictedByCheckpoint(securityCore, targetLookupKey);

			Assert("Should return true when the target checkpoint is in the list", result);

			var multiTargetCheckpointFunc = new List<Func<SecurityCore, SecurityCheckpoint>>
			{
				s => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr,
				s => s.ReceivablesOnCreditHoldController
			};
			var multiTargetLookupKey = multiTargetCheckpointFunc.Select(u => u(securityCore).LookupKey);
			result = loginBisObject.IsRestrictedByCheckpoint(securityCore, multiTargetLookupKey.ToArray());

			Assert("Should return true when one of the target checkpoint is in the list", result);
		}

		public void TestIsRestrictedByCheckpoint_ListDoesNotContainTargetCheckpoint_ReturnsFalse()
		{
			var securityCore = GetTestSecurityCore();
			var targetLookupKey = securityCore.UserAdmin.LookupKey;

			var checkpointList = new List<Func<SecurityCore, SecurityCheckpoint>>
			{
				s => s.ReceivablesOnCreditHoldController,
				s => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr
			};
			var loginBisObject = new SecurityLogin(checkpointList);

			var result = loginBisObject.IsRestrictedByCheckpoint(securityCore, targetLookupKey);

			Assert("Should return false when the target checkpoint is not in the list", !result);

			var multiTargetCheckpointFunc = new List<Func<SecurityCore, SecurityCheckpoint>>
			{
				s => s.BookingsCommercialInvoiceEdit,
				s => s.OrgDeniedPartyScreening
			};
			var multiTargetLookupKey = multiTargetCheckpointFunc.Select(u => u(securityCore).LookupKey);
			result = loginBisObject.IsRestrictedByCheckpoint(securityCore, multiTargetLookupKey.ToArray());

			Assert("Should return false when none of the target checkpoint is in the list", !result);
		}

		SecurityCore GetTestSecurityCore()
		{
			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			var security = loginController.GetSecurityForUser(ZArchitecture.Environment.User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) ?? throw new InvalidOperationException("Failed to get SecurityCore for testing. Check test environment setup.");
			return (SecurityCore)security;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SecurityLogin((s) => s.ReceivablesOnCreditHoldController);
		}

		#endregion
	}
}
