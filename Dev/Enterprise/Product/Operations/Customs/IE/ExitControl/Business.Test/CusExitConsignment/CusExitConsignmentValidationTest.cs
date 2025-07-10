using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXC_MovementReference()
		{
			var mRNFormatErrorText = "MRN is not in valid format";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusExitConsignment.CXC_MovementReferenceInfo);
			cusExitConsignment.CXC_MovementReference = "54321";
			AssertHasMessageError("Movement reference invalid pattern", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
			cusExitConsignment.CXC_MovementReference = "160000";
			AssertHasMessageError("Movement reference invalid pattern", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
			cusExitConsignment.CXC_MovementReference = " 1";
			AssertHasMessageError("Movement reference invalid pattern", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
			cusExitConsignment.CXC_MovementReference = "1 1";
			AssertHasMessageError("Movement reference invalid pattern", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
			cusExitConsignment.CXC_MovementReference = "24ABCD1234WXYZ01";
			AssertHasMessageError("Movement reference invalid pattern", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
			cusExitConsignment.CXC_MovementReference = "24-IEABCD1234WXYZ01";
			AssertHasMessageError("Movement reference invalid pattern", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
			cusExitConsignment.CXC_MovementReference = "45IEABCD1234WXYZ01";
			AssertHasMessageError("Movement reference invalid pattern", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
			cusExitConsignment.CXC_MovementReference = "24IEABCD1234WXYZ01";
			AssertNoMessageError("Movement reference pattern is valid", cusExitConsignment.CXC_MovementReferenceInfo, mRNFormatErrorText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			(_, cusExitConsignment) = CreateData();
		}

		(CusExitHeader exitHeader, CusExitConsignment exitConsignment) CreateData()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			return (exitHeader, exitHeader.CusExitConsignments.AddNew());
		}

		CusExitConsignment cusExitConsignment;
	}
}
