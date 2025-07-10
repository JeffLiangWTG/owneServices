using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	internal sealed class NveCusCodeDataValidationTest : CusCodeDataValidationTest
	{
		public void TestParent()
		{
			var parent = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().NVECusCodeDataCollection.AddNew();
			AssertEquals("Parent", parent, parent.Validation.Parent);
		}

		public new void TestCheckCY_Code()
		{
			var nve = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().NVECusCodeDataCollection.AddNew();
			nve.CY_Code = ZString.Empty;
			AssertNoMessageErrors("No message error on CY_Code", nve.CY_CodeInfo);
			nve.CY_Code = "XXXX";
			AssertNoMessageErrors("No message error on CY_Code", nve.CY_CodeInfo);
		}

		public void TestSpecificationNotification()
		{
			ReferenceTestDataHelper.CreateNCMTETariffBRCharacteristic(Factory);
			ReferenceTestDataHelper.CreateNVETariffBRCharacteristic(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11111111";

			var nveDeclaration = invoiceLine.NVECusCodeDataCollection[0];
			nveDeclaration.Specification = "Outros";
			AssertNoMessageError("Must NOT contain message error for Specification when ISW and Specification NOT Empty", nveDeclaration.SpecificationInfo, "You have not entered a NVE Attributes Specification.");
			nveDeclaration.Specification = ZString.Empty;
			AssertHasMessageError("Must contain message error for Specification when ISW and Specification is Empty", nveDeclaration.SpecificationInfo, "You have not entered a NVE Attributes Specification.");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			nveDeclaration.Specification = "Outros";
			AssertNoMessageError("Must NOT contain message error for Specification when LIC and Specification NOT Empty", nveDeclaration.SpecificationInfo, "You have not entered a NVE Attributes Specification.");
			nveDeclaration.Specification = ZString.Empty;
			AssertHasMessageError("Must contain message error for Specification when LIC and Specification is Empty", nveDeclaration.SpecificationInfo, "You have not entered a NVE Attributes Specification.");

			var nveOrg = Factory.New<CusClassPartPivot>().NveCusCodeDataCollection.AddNew();
			nveOrg.Specification = ZString.Empty;
			AssertNoMessageError("Must NOT contain message error for Specification when Product", nveOrg.SpecificationInfo, "You have not entered a NVE Attributes Specification.");
		}
	}
}
