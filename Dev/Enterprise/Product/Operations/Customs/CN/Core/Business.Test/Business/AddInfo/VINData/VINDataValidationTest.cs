using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class VINDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB7_AddInfoDataIsWesternEuropean()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var vinData = testItems.InvoiceLine.VINDataCollection.AddNew();
			vinData.XC_QGP = "不低于责任规定";
			vinData.Validation.ValidateAll();
			AssertNoErrors(vinData.B7_AddInfoDataInfo);
		}

		public void TestCheckParentSuppingVIN()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var vinData = testItems.InvoiceLine.VINDataCollection.AddNew();
			vinData.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(vinData, "No VIN data is required if there is no CIQ Product Qualification with Type 408/409/603.");
			var pq = testItems.InvoiceLine.CIQProductQualifications.AddNew();
			pq.CSI_Code = "408";
			vinData.Validation.ValidateAll();
			AssertNoRowMessageErrors(vinData);
			pq.CSI_Code = "101";
			vinData.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(vinData, "No VIN data is required if there is no CIQ Product Qualification with Type 408/409/603.");
		}

		public void TestVINDataValidation()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var vinData = testItems.InvoiceLine.VINDataCollection.AddNew();
			var validation = vinData.Validation as VINDataValidation;
			ValidationExtensionsTest.AssertValidationModeProvider(testItems.JobDeclaration, validation.ValidationModeProvider);

			vinData = Factory.New<VINData>();
			validation = new VINDataValidation(vinData);
			AssertNull(validation.ValidationModeProvider);
		}
	}
}
