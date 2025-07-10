using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD409;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(RD409Provider))]
	sealed class RD409ProviderTest : TestCase
	{
		public void TestApplicationReferenceId()
		{
			AssertEquals("ApplicationReferId", provider.ApplicationReferenceId);
		}

		public void TestDate()
		{
			AssertEquals("20230811", provider.Date);
		}

		public void TestApplicant()
		{
			AssertEquals("AN", provider.Applicant);
		}

		public void TestDepositRefundApplicationApproved()
		{
			AssertEquals(true, provider.DepositRefundApplicationApproved);
		}

		public void TestReasonNotApproved()
		{
			AssertEquals("Reason Not Approved", provider.ReasonNotApproved);
		}

		public void TestStatementOfTheDecisionTakingCustomsAuthority()
		{
			AssertEquals("Statement of the Decision Taking Customs Authority", provider.StatementOfTheDecisionTakingCustomsAuthority);
		}

		public void TestAmountOfDepositRefund()
		{
			AssertEquals(123.45m, provider.AmountOfDepositRefund);
		}

		public void TestPayerEORIForRefund()
		{
			AssertEquals("PR", provider.PayerEORIForRefund);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new RD409Provider(GenerateMessage());
		}

		RD409Provider provider;

		Rd409 GenerateMessage()
		{
			return new Rd409
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "ApplicationReferId",
					Date = "20230811",
					Applicant = "AN",
					DepositRefundApplicationApproved = true,
					ReasonNotApproved = "Reason Not Approved",
					StatementOfTheDecisionTakingCustomsAuthority = "Statement of the Decision Taking Customs Authority",
				},
				DepositRefundDetails = new DepositRefundDetailsType
				{
					AmountOfDepositRefund = 123.45m,
					PayerEoriForRefund = "PR"
				}
			};
		}
	}
}
