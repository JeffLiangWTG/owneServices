using System;
using Enterprise.Customs.DE.Business.Declaration;
using Moq;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DutyDefermentApprovalProviderTest : Customs.Business.Testing.DataProviderTestCase<DutyDefermentApprovalProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new DutyDefermentApprovalProvider(null));
		}

		public void TestType()
		{
			mockDefermentAccount.SetupGet(p => p.Type).Returns("E");
			AssertEquals("E", Provider.Type);
		}

		public void TestApplicationType()
		{
			mockDefermentAccount.SetupGet(p => p.ApplicationType).Returns("10");
			AssertEquals("10", Provider.ApplicationType);
		}

		public void TestAccountPrefix()
		{
			mockDefermentAccount.SetupGet(p => p.AccountPrefix).Returns("F");
			AssertEquals("F", Provider.AccountPrefix);
		}

		public void TestAccountNumber()
		{
			mockDefermentAccount.SetupGet(p => p.AccountNumber).Returns("123456");
			AssertEquals("123456", Provider.AccountNumber);
		}

		public void TestAuthorisationNumber()
		{
			mockDefermentAccount.SetupGet(p => p.AuthorisationNumber).Returns("BINBINBIN");
			AssertEquals("BINBINBIN", Provider.AuthorisationNumber);
		}

		public void TestApplicant()
		{
			mockDefermentAccount.SetupGet(p => p.Applicant).Returns("GR1234567");
			AssertEquals("GR1234567", Provider.Applicant);
		}

		protected override void SetUp()
		{
			base.SetUp();

			mockDefermentAccount = new Mock<IDefermentAccount>();
		}

		Mock<IDefermentAccount> mockDefermentAccount;

		protected override DutyDefermentApprovalProvider GetProvider() => new DutyDefermentApprovalProvider(mockDefermentAccount.Object);
	}
}
