using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class CWSupportLoginTokenTest : TransactionedTestCase
	{
		public void TestExternalSystem()
		{
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				var mockProductReg = MockProductRegistration("WTG", "TST");

				using (ObjectFactory.Substitute(mockProductReg))
				{
					var signedToken = CWSupportLoginToken.GenerateTokenForTest("BAS", "INC000001", "WTGTST", ZDateTime.UtcNow);

					var validateResult = CWSupportLoginToken.Validate(signedToken);
					Assert("Should be valid token.", validateResult.IsValid);
					AssertEquals("INC000001", validateResult.Incident);
					AssertEquals("BAS", validateResult.UserCode);

					validateResult = CWSupportLoginToken.Validate(string.Empty);
					Assert("Should be invalid because token is invalid.", !validateResult.IsValid);
					AssertEquals("The CWSupport token provided is invalid.", validateResult.FailedReason);

					signedToken = CWSupportLoginToken.GenerateTokenForTest("BAS", "INC000001", "UP3PRD", ZDateTime.UtcNow);

					validateResult = CWSupportLoginToken.Validate(signedToken);
					Assert("Should be invalid because the system code is not matched.", !validateResult.IsValid);
					AssertEquals("The CWSupport token has the incorrect system code: UP3PRD.", validateResult.FailedReason);
				}
			}
		}

		public void TestInternalSystem()
		{
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				var mockProductReg = MockProductRegistration("WTG", "TST", true);

				using (ObjectFactory.Substitute(mockProductReg))
				{
					var signedToken = CWSupportLoginToken.GenerateTokenForTest("BAS", "INC000001", "WTGTST", ZDateTime.UtcNow, true, new ZGuid(), "Elliott");

					var validateResult = CWSupportLoginToken.Validate(signedToken);
					Assert("Should be valid token.", validateResult.IsValid);
					AssertEquals("INC000001", validateResult.Incident);
					AssertEquals("BAS", validateResult.UserCode);
					AssertEquals("Elliott", validateResult.UserName);

					signedToken = CWSupportLoginToken.GenerateTokenForTest("E", "", "", ZDateTime.UtcNow);

					validateResult = CWSupportLoginToken.Validate(signedToken);
					Assert("Should be valid because internal system doesn't validate the system code.", validateResult.IsValid);
					AssertEquals("", validateResult.Incident);
					AssertEquals("E", validateResult.UserCode);
				}
			}
		}

		public void TestExpired()
		{
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				var mockProductReg = MockProductRegistration("WTG", "TST");
				var signedToken = CWSupportLoginToken.GenerateTokenForTest("BAS", "INC000001", "WTGTST", new ZDateTime(2023, 1, 1));

				using (ObjectFactory.Substitute(mockProductReg))
				{
					var validateResult = CWSupportLoginToken.Validate(signedToken);
					Assert("Should be invalid.", !validateResult.IsValid);
					AssertEquals("The CWSupport token is expired.", validateResult.FailedReason);
				}
			}
		}

		public void TestCanNotLoginInternalSystemByUsingExternalRoles()
		{
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				var mockProductReg = MockProductRegistration("WTG", "TST", true);

				using (ObjectFactory.Substitute(mockProductReg))
				{
					var signedToken = CWSupportLoginToken.GenerateTokenForTest("BAS", "INC000001", "WTGTST", ZDateTime.UtcNow, false);

					var validateResult = CWSupportLoginToken.Validate(signedToken);
					Assert("Should be invalid.", !validateResult.IsValid);
					AssertEquals("The CWSupport token can not login internal system.", validateResult.FailedReason);
				}
			}
		}

		IProductRegistration MockProductRegistration(string enterpriseCode, string serverCode, bool internalSystem = false)
		{
			var mockProductReg = new Mock<IProductRegistration>();
			var mockProductRegKey = new Mock<IProductRegistrationKey>();
			mockProductReg.Setup(reg => reg.IsWiseTechGlobalInternalSystem()).Returns(internalSystem);
			mockProductReg.Setup(reg => reg.Key).Returns(mockProductRegKey.Object);
			mockProductRegKey.Setup(regKey => regKey.EnterpriseCode).Returns(enterpriseCode);
			mockProductRegKey.Setup(regKey => regKey.ServerCode).Returns(serverCode);
			return mockProductReg.Object;
		}
	}
}
