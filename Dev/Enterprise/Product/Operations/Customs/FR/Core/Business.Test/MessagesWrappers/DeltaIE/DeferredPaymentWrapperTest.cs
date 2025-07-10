using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class DeferredPaymentWrapperTest : Customs.Business.Testing.DataProviderTestCase<DeferredPaymentWrapper>
	{
		const string DefermentAccountNumber = "Account1";

		JobDeclaration jobDeclaration;

		JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DefermentAccountNumber = DefermentAccountNumber;
			return declaration;
		}

		protected override DeferredPaymentWrapper GetProvider()
		{
			return DeferredPaymentWrapper.New(jobDeclaration);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be empty.", ZString.Empty, Provider.CcQualifier);
		}

		public void TestDeferredPayment()
		{
			jobDeclaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			AssertEquals("When JE_PaymentMethod is 'R' DeferredPayment should be equal to JE_DefermentAccountNumber", DefermentAccountNumber, Provider.DeferredPayment);

			jobDeclaration.JE_PaymentMethod = MethodOfPaymentList.Codes.M;
			AssertEquals("When JE_PaymentMethod is 'M' DeferredPayment should be equal to JE_DefermentAccountNumber", DefermentAccountNumber, Provider.DeferredPayment);

			jobDeclaration.JE_PaymentMethod = MethodOfPaymentList.Codes.A;
			AssertEquals("DeferredPayment should be empty.", ZString.Empty, Provider.DeferredPayment);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = GetDeclaration();
		}
	}
}
