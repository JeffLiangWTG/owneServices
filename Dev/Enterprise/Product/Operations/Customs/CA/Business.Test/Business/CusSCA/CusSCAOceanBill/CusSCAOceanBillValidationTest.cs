using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAOceanBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCusSCAOceanBill()
		{
			CusSCAOceanBill parent = Factory.New<CusSCAOceanBill>();
			AssertEquals(parent.Validation.OcealBill, parent);
		}

		public void TestOriginalCCNValidation()
		{
			oceanBill.OriginalCCN = ZString.Empty;
			oceanBill.Validation.ValidateOriginalCCN();
			AssertHasMessageErrorContaining(oceanBill.OriginalCCNInfo, "The CCN is defaulted from the consol");
			oceanBill.OriginalCCN = "081-";
			oceanBill.Validation.ValidateOriginalCCN();
			AssertHasMessageErrorContaining(oceanBill.OriginalCCNInfo, "The CCN is defaulted from the consol");
			oceanBill.OriginalCCN = "081-9990";
			oceanBill.Validation.ValidateOriginalCCN();
			AssertNoMessageErrors(oceanBill.OriginalCCNInfo);
		}

		public void TestCB_OceanBillValidation()
		{
			oceanBill.CB_OceanBill = ZString.Empty;
			oceanBill.Validation.ValidateCB_OceanBill();
			AssertHasMessageErrorContaining(oceanBill.CB_OceanBillInfo, MandatoryValidation.YouHaveNotEntered);
			oceanBill.CB_OceanBill = "XXX";
			AssertNoMessageErrors(oceanBill.CB_OceanBillInfo);

			oceanBill.CB_OceanBill = "1234567890123456789012345678901";
			AssertHasMessageErrorContaining(oceanBill.CB_OceanBillInfo, "The master bill number is too long for the message. It will result in a syntax error if you proceed. The maximum length allowed is 30.");

			oceanBill.CB_OceanBill = "123456789012345678901234567890";
			AssertNoMessageErrors(oceanBill.CB_OceanBillInfo);
		}

		CusSCAOceanBill oceanBill;
		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();
		}
	}
}
