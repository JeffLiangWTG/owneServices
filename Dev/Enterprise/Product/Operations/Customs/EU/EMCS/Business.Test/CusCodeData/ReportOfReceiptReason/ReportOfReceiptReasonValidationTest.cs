using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class ReportOfReceiptReasonValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCY_Data_Optional()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(reason.CY_DataInfo);
		}

		public void TestCY_Data_Mandatory()
		{
			reason.CY_Code = "0";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(reason.CY_DataInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			reason = Factory.New<ReportOfReceiptReason>();
		}
		ReportOfReceiptReason reason;
	}
}
