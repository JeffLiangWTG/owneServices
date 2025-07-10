using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	public abstract class SecurityOverrideProviderTestCase : TransactionedTestCase
	{
		protected abstract SecurityOverrideProvider GetSecurityOverrideProvider();

		[GuiTest]
		[RequiresSTA]
		public void TestCurrentLoginSecurityRights()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;
			Env.Security.ReopenJob.IsAllowed = false;
			SecurityOverrideProvider testProvider = GetSecurityOverrideProvider();
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);

			Env.Security.ReopenJob.IsAllowed = true;
			AssertEquals(true, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		class SecurityOverrideProviderTest : SecurityOverrideProviderTestCase
		{
			public void TestPromptForTemporaryAccess_Granted_NoneSecurityCheckPoint()
			{
				PromptForTemporaryAccess_Granted(Env.Security.None);
			}

			public void TestPromptForTemporaryAccess_Granted_ReopenJobSecurityCheckPoint()
			{
				PromptForTemporaryAccess_Granted(Env.Security.ReopenJob);
			}

			void PromptForTemporaryAccess_Granted(SecurityCheckpoint checkPoint)
			{
				SecurityOverrideProvider testProvider = GetSecurityOverrideProvider();
				SecurityCore userSecurityWithAccess = CreateTestSecurity(true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				MockSecurityOverrideProvider
					.Protected()
					.Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>())
					.Returns(userSecurityWithAccess);
				((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(checkPoint);
				MockSecurityOverrideProvider.VerifyAll();
				MockSecurityOverrideProvider.Reset();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(userSecurityWithAccess, testProvider.UserSecurityOverride);
			}

			public void TestPromptForTemporaryAccess_InvalidCredentials()
			{
				SecurityOverrideProvider testProvider = GetSecurityOverrideProvider();
				SecurityCore invalidUserSecurity = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				MockSecurityOverrideProvider
					.Protected()
					.Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>())
					.Returns(invalidUserSecurity);
				((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(Env.Security.ReopenJob);
				MockSecurityOverrideProvider.VerifyAll();
				MockSecurityOverrideProvider.Reset();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(testProvider.UserSecurityOverride);
			}

			public void TestPromptForTemporaryAccess_Denied()
			{
				Env.Security.ReopenJob.IsAllowed = false;
				SecurityOverrideProvider testProvider = GetSecurityOverrideProvider();
				SecurityCore userSecurityWithNoAccess = CreateTestSecurity(false);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				MockSecurityOverrideProvider
					.Protected()
					.Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>())
					.Returns(userSecurityWithNoAccess);
				((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(Env.Security.ReopenJob);
				MockSecurityOverrideProvider.VerifyAll();
				MockSecurityOverrideProvider.Reset();

				AssertEquals(SecurityOverrideProvider.DeniedSecurityRightsErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			public void TestPromptForTemporaryAccess_InvalidCheckpoint()
			{
				SecurityOverrideProvider testProvider = GetSecurityOverrideProvider();
				SecurityCore userSecurity = CreateTestSecurity(true);
				SecurityCore userSecurityWithoutTestCheckpoint = CreateTestSecurity(true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				SecurityCheckpoint checkPoint = new SecurityCheckpoint("tst", (NoResString)"test", null, ((SecurityForTest)userSecurity).ZSecurityInstance);
				MockSecurityOverrideProvider
					.Protected()
					.Setup<SecurityCore>("RequestLoginCredentials", ItExpr.IsAny<SecurityCheckpoint>())
					.Returns(userSecurityWithoutTestCheckpoint);
				((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(checkPoint);

				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				Assert(ExceptionReporterTestListener.Instance[0] is DeveloperNotificationException);
				ExceptionReporterTestListener.Instance.Clear();
			}

			protected override SecurityOverrideProvider GetSecurityOverrideProvider()
			{
				return MockSecurityOverrideProvider.Object;
			}

			protected override void SetUp()
			{
				MockSecurityOverrideProvider = new Mock<SecurityOverrideProvider>();
				MockSecurityOverrideProvider.CallBase = true;
				base.SetUp();
			}

			SecurityCore CreateTestSecurity(bool isAllowed)
			{
				SecurityForTest testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
				testSecurity.CachingEnabled = false;
				testSecurity.ReopenJob.IsAllowed = isAllowed;

				return testSecurity;
			}

			Mock<SecurityOverrideProvider> MockSecurityOverrideProvider;
		}
	}
}
