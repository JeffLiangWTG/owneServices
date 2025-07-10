using System;
using Enterprise.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	sealed class SecurityCertificateHashtableTest : TransactionedTestCase
	{
		public void TestPromptForGrantedConfirmation()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;

			var mockSecurityOverrideProvider = new Mock<ISecurityOverrideProvider>();
			var testHashtable = new SecurityCertificateHashtable(mockSecurityOverrideProvider.Object);

			Env.Security.ReopenJob.IsAllowed = true;
			mockSecurityOverrideProvider
				.Setup(m => m.ShouldPromptForGrantedConfirmation)
				.Returns(false);
			mockSecurityOverrideProvider
				.Setup(m => m.IsUserInitiatorAndNotAllowedToApprove)
				.Returns(false);
			mockSecurityOverrideProvider
				.Verify(m => m.PromptForAccessGrantConfirmation(It.IsAny<SecurityCheckpoint>()), Times.Never());
			AssertEquals(true, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider.VerifyAll();

			mockSecurityOverrideProvider.Reset();

			mockSecurityOverrideProvider
				.Setup(m => m.ShouldPromptForGrantedConfirmation)
				.Returns(true);
			mockSecurityOverrideProvider
				.Setup(m => m.PromptForAccessGrantConfirmation(It.IsAny<SecurityCheckpoint>()))
				.Returns(SecurityCertificate.Granted);
			AssertEquals(true, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider.VerifyAll();

			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		public void TestPromptAndGrantAccess()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;

			var mockSecurityOverrideProvider = new Mock<ISecurityOverrideProvider>();
			SecurityCertificateHashtable testHashtable = new SecurityCertificateHashtable(mockSecurityOverrideProvider.Object);

			//IsAllowed = true, Should NOT call PromptForTemporaryAccess
			Env.Security.ReopenJob.IsAllowed = true;
			mockSecurityOverrideProvider
				.Setup(m => m.ShouldPromptForGrantedConfirmation)
				.Returns(false);
			mockSecurityOverrideProvider
				.Setup(m => m.IsUserInitiatorAndNotAllowedToApprove)
				.Returns(false);
			AssertEquals(true, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider
				.Verify(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()), Times.Never());
			mockSecurityOverrideProvider.Reset();

			//IsAllowed = false, Should call PromptForTemporaryAccess
			Env.Security.ReopenJob.IsAllowed = false;
			mockSecurityOverrideProvider
				.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()))
				.Returns(SecurityCertificate.Granted);
			AssertEquals(true, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider.VerifyAll();
			mockSecurityOverrideProvider.Invocations.Clear();

			//Access already granted, Should NOT call PromptForTemporaryAccess
			mockSecurityOverrideProvider
				.Setup(m => m.ShouldPromptForGrantedConfirmation)
				.Returns(false);
			mockSecurityOverrideProvider
				.Setup(m => m.IsUserInitiatorAndNotAllowedToApprove)
				.Returns(false);
			AssertEquals(true, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider.Verify(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()), Times.Never());
			mockSecurityOverrideProvider.Reset();
			mockSecurityOverrideProvider
				.Setup(m => m.ShouldPromptForGrantedConfirmation)
				.Returns(false);
			mockSecurityOverrideProvider
				.Setup(m => m.IsUserInitiatorAndNotAllowedToApprove)
				.Returns(false);

			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		public void TestPromptDifferentCheckPointAndDenyAccess()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;
			bool isAllowedCommodity = Env.Security.Commodity.IsAllowed;

			var mockSecurityOverrideProvider = new Mock<ISecurityOverrideProvider>();
			var testHashtable = new SecurityCertificateHashtable(mockSecurityOverrideProvider.Object);

			//ReopenJob.IsAllowed = false, Should call PromptForTemporaryAccess
			Env.Security.ReopenJob.IsAllowed = false;
			mockSecurityOverrideProvider
				.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()))
				.Returns(SecurityCertificate.Denied);
			AssertEquals(false, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider.VerifyAll();

			//Should call PromptForTemporaryAccess again
			mockSecurityOverrideProvider
				.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()))
				.Returns(SecurityCertificate.Denied);
			AssertEquals(false, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider.VerifyAll();
			mockSecurityOverrideProvider.Invocations.Clear();

			//Commodity.IsAllowed = true, Should NOT call PromptForTemporaryAccess
			Env.Security.Commodity.IsAllowed = true;
			mockSecurityOverrideProvider
				.Setup(m => m.ShouldPromptForGrantedConfirmation)
				.Returns(false);
			mockSecurityOverrideProvider
				.Setup(m => m.IsUserInitiatorAndNotAllowedToApprove)
				.Returns(false);
			Assert(testHashtable[Env.Security.Commodity].IsAllowed);
			mockSecurityOverrideProvider
				.Verify(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()), Times.Never());
			mockSecurityOverrideProvider.Reset();

			// Setup expectations again.
			mockSecurityOverrideProvider
				.Setup(m => m.ShouldPromptForGrantedConfirmation)
				.Returns(false);
			mockSecurityOverrideProvider
				.Setup(m => m.IsUserInitiatorAndNotAllowedToApprove)
				.Returns(false);

			//ReopenJob.IsAllowed = false, Should call PromptForTemporaryAccess again
			mockSecurityOverrideProvider
				.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()))
				.Returns(SecurityCertificate.Denied);
			AssertEquals(false, testHashtable[Env.Security.ReopenJob].IsAllowed);
			mockSecurityOverrideProvider.Verify();

			Env.Security.Commodity.IsAllowed = isAllowedCommodity;
			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullSecurityOverrideProvider()
		{
			new SecurityCertificateHashtable(null);
		}
	}
}
