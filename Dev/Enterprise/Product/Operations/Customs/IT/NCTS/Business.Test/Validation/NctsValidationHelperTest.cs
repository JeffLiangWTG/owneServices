using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsValidationHelperTest : TestCaseWithDummy
{
	public void TestGuardClause()
	{
		var validation = new ZValidationForTest(Dummy);
		AssertExceptionThrown<ArgumentNullException>(
			"Action cannot be null",
			() => validation.RunActionIfValidationOfType<NctsHeaderValidation>(action: null));
	}

	public void TestRunActionIfValidationOfType_WhenTypeMatches()
	{
		var i = 0;
		var validation = new ZValidationForTest(Dummy);
		validation.RunActionIfValidationOfType<ZValidationForTest>((_) => i++);
		AssertEquals("Action has been called", i, 1);
	}

	public void TestRunActionIfValidationOfType_WhenTypeDoesNotMatch()
	{
		var i = 0;
		var validation = new ZValidationForTest(Dummy);
		validation.RunActionIfValidationOfType<NctsHeaderValidation>((_) => i++);
		AssertEquals("Action has not been called", i, 0);
	}

	sealed class ZValidationForTest : ZValidation
	{
		public ZValidationForTest(BusinessObject parent) : base(parent)
		{
		}

		public override Type AutoValidationType => typeof(ZValidationForTest);

		public override void ValidateAll()
		{
		}
	}
}
