using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Auto.Testing
{
	internal class AccStatementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAS_DebitCredit_List()
		{
			Statement testStatement = Factory.New<Statement>();

			testStatement.AS_DebitCredit = "DR";
			AssertEquals(0, testStatement.AS_DebitCreditInfo.GetErrors().Count());

			testStatement.AS_DebitCredit = "GR";
			AssertEquals(1, testStatement.AS_DebitCreditInfo.GetErrors().Count());

			testStatement.AS_DebitCredit = "DR";
			AssertEquals(0, testStatement.AS_DebitCreditInfo.GetErrors().Count());

			testStatement.AS_DebitCredit = "";
			AssertEquals(1, testStatement.AS_DebitCreditInfo.GetErrors().Count());
		}

		public void TestValidateAS_ChequeOrReference()
		{
			Statement testStatement = Factory.New<Statement>();

			testStatement.AS_DebitCredit = "DR";
			testStatement.AS_Type = ZArchitecture.Core.ReceiptTypes.Cheque;

			testStatement.AS_ChequeOrReference = "00001000";
			AssertNoErrors(testStatement.AS_ChequeOrReferenceInfo);

			testStatement.AS_ChequeOrReference = "";
			AssertHasErrors(testStatement.AS_ChequeOrReferenceInfo);

			testStatement.AS_Type = ReceiptTypes.DirectCredit;
			testStatement.Validation.ValidateAS_ChequeOrReference();
			AssertNoErrors(testStatement.AS_ChequeOrReferenceInfo);
			AssertHasWarnings(testStatement.AS_ChequeOrReferenceInfo);

			testStatement.AS_Type = ReceiptTypes.DirectDebit;
			testStatement.Validation.ValidateAS_ChequeOrReference();
			AssertNoErrors(testStatement.AS_ChequeOrReferenceInfo);
			AssertHasWarnings(testStatement.AS_ChequeOrReferenceInfo);
		}

		public void TestValidateAS_Amount()
		{
			Statement testStatement = Factory.New<Statement>();

			testStatement.AS_Amount = 100.0m;
			AssertEquals(0, testStatement.AS_AmountInfo.GetErrors().Count());

			testStatement.AS_Amount = -100.0m;
			AssertEquals(1, testStatement.AS_AmountInfo.GetErrors().Count());
		}

		public void TestValidateAS_Type()
		{
			Statement testStatement = Factory.New<Statement>();

			testStatement.AS_Type = "CHQ";
			AssertEquals(0, testStatement.AS_TypeInfo.GetErrors().Count());

			testStatement.AS_Type = "GsR";
			AssertEquals(1, testStatement.AS_TypeInfo.GetErrors().Count());

			testStatement.AS_Type = "";
			AssertEquals(1, testStatement.AS_TypeInfo.GetErrors().Count());
		}
	}
}
