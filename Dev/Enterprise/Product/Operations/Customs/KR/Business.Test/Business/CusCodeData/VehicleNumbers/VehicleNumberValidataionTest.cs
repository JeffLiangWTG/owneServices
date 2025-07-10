using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class VehicleNumberValidataionTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.InvoiceLines.AddNew();
			var vehicleNo = invoiceline.VehicleNumbers.AddNew();
			vehicleNo.CY_Data = "";
			AssertNoMessageErrors(vehicleNo.CY_DataInfo);

			declaration.JE_MessageType = "EXP";
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			vehicleNo.CY_Data = "";
			AssertHasMessageErrorContaining(vehicleNo.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			vehicleNo.CY_Data = "HSO1225874551";
			AssertNoMessageErrors(vehicleNo.CY_DataInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			vehicleNo.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(vehicleNo.CY_DataInfo, "If 'Second Hand Vehicles' is inserted, 'Declaration Type' must be 'B'.");
		}
	}
}
