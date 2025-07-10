using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ImmediateDeliveryValidationTest : CusCodeDataValidationTest
	{
		public void TestDuplicateImmediateDeliveryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var immediateDelivery = invoiceLine.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 1;
			immediateDelivery.CY_Data = "11111";

			immediateDelivery = invoiceLine.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 2;
			immediateDelivery.CY_Data = "11111";
			AssertNoMessageErrorContaining(immediateDelivery.CY_DataInfo, "You cannot enter an 'Immediate Delivery Number' that already exists");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			immediateDelivery.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(immediateDelivery.CY_DataInfo, "You cannot enter an 'Immediate Delivery Number' that already exists");

			immediateDelivery.CY_Data = "22222";
			AssertNoMessageErrorContaining(immediateDelivery.CY_DataInfo, "You cannot enter an 'Immediate Delivery Number' that already exists");
		}

		public void TestImmediateDeliveryNumberMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var immediateDelivery = invoiceLine.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 1;
			immediateDelivery.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(immediateDelivery.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			immediateDelivery.CY_Data = "22222";
			AssertNoMessageErrorContaining(immediateDelivery.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
