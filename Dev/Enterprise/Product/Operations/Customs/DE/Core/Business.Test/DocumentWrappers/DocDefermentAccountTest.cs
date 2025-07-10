using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Moq;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	sealed class DocDefermentAccountTest : DocBaseWrapperTest
	{
		public void TestNew()
		{
			AssertNull(DocDefermentAccount.New(null, Factory));
		}

		public void TestAccountHolder()
		{
			SetupMock();
			AssertEquals("Account Holder", Wrapper.AccountHolder);
		}

		public void TestAccountNumber()
		{
			SetupMock();
			AssertEquals("999888", Wrapper.AccountNumber);
		}

		public void TestEori()
		{
			SetupMock();
			AssertEquals("DE123567890", Wrapper.Eori);
		}

		public void TestType()
		{
			SetupMock();
			AssertEquals("15", Wrapper.Type);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			SetupMock();
			return DocDefermentAccount.New(defermentAccount, Factory);
		}

		new DocDefermentAccount Wrapper => (DocDefermentAccount)base.Wrapper;

		void SetupMock()
		{
			var mock = new Mock<IDefermentAccount>();
			mock.SetupGet(x => x.Applicant).Returns("DE123567890");
			mock.SetupGet(x => x.AccountHolder).Returns("Account Holder");
			mock.SetupGet(x => x.AccountNumber).Returns("999888");
			mock.SetupGet(x => x.Type).Returns("15");

			defermentAccount = mock.Object;
		}

		IDefermentAccount defermentAccount;
	}
}
