using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class JobComInvoiceLineTaxValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckMethodOfCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew().Data;
			taxLine.JLT_MethodOfCalculation = "";
			AssertHasMessageErrorContaining(taxLine.JLT_MethodOfCalculationInfo, MandatoryValidation.YouHaveNotEntered);
			taxLine.JLT_MethodOfCalculation = "XXX";
			AssertNoMessageErrorContaining(taxLine.JLT_MethodOfCalculationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(taxLine.JLT_MethodOfCalculationInfo, ListValidation.InvalidCodeMessageError);
			taxLine.JLT_MethodOfCalculation = MethodOfCalculationList.Codes.MoC0;
			AssertNoMessageError(taxLine.JLT_MethodOfCalculationInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
