using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCCI_GrossMass()
		{
			TestMassValidation(item.CCI_GrossMassInfo);
		}

		public void TestCheckCCI_NetMass()
		{
			TestMassValidation(item.CCI_NetMassInfo);
		}

		void TestMassValidation(ZPropertyInfo targetInfo)
		{
			CombineAssertions($"Validation on {targetInfo.Name}", () =>
			{
				targetInfo.Value = new ZDecimal(-1);
				AssertHasErrorContaining($"Negative check on {targetInfo.Name}", targetInfo, MandatoryValidation.ValueCannotBeNegative);
				targetInfo.Value = new ZDecimal(0);
				AssertHasMessageErrorContaining($"Negative check on {targetInfo.Name}", targetInfo, MandatoryValidation.ValueCannotBeZero);

				item.CCI_GrossMass = 100;
				item.CCI_NetMass = 101;
				item.Validation.ValidateAll();

				AssertHasMessageErrorContaining("Check that Gross mass should not be less than Net mass.", targetInfo, "should be greater than or at least equal to");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			(_, _, item) = CusExitConsignmentItemTest.GetNewBusinessObject(Factory);
		}
		CusExitConsignmentItem item;
	}
}
