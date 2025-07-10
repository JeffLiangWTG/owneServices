using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class VINDataAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXC_VIN()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { })
				.InvoiceLine.VINDataCollection.AddNew();
			var info = testItem.XC_VINInfo;
			testItem.AddInfoValidation.ValidateXC_VIN();
			AssertNoMessageErrors(info);
			testItem.XC_VIN = "123abc";
			AssertHasMessageErrorContaining(info, "VIN Number should be 17 alphanumeric.");
			testItem.XC_VIN = "123-4567890abcdef";
			AssertHasMessageErrorContaining(info, "VIN Number should be 17 alphanumeric.");
			testItem.XC_VIN = "01234567890abcdef";
			AssertNoMessageErrors(info);
		}

		public void TestCheckXC_ChassisNo()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { })
				.InvoiceLine.VINDataCollection.AddNew();
			var info = testItem.XC_ChassisNoInfo;
			testItem.AddInfoValidation.ValidateXC_ChassisNo();
			AssertNoMessageErrors(info);
			testItem.XC_ChassisNo = "123abc";
			AssertHasMessageErrorContaining(info, "Chassis Number should be 20 alphanumeric.");
			testItem.XC_ChassisNo = "1234567890-abcdefghi";
			AssertHasMessageErrorContaining(info, "Chassis Number should be 20 alphanumeric.");
			testItem.XC_ChassisNo = "1234567890abcdefghij";
			AssertNoMessageErrors(info);
		}

		public void TestValidationModeProvider()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { }).InvoiceLine.VINDataCollection.AddNew();
			ValidationExtensionsTest.AssertValidationModeProvider(testItem.Parent.Declaration, testItem.AddInfoValidation.ValidationModeProvider);

			testItem = Factory.New<VINData>();
			AssertNull(testItem.AddInfoValidation.ValidationModeProvider);
		}
	}
}
