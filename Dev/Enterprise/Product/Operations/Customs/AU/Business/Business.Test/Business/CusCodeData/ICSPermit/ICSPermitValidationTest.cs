using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ICSPermitValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Code()
		{
			var permit = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ICSPermits.AddNew();
			permit.CY_Code = ZString.Empty;
			AssertNoNotifications(permit.CY_CodeInfo);
			permit.CY_Code = "@#$";
			AssertNoNotifications(permit.CY_CodeInfo);
		}

		public void TestCheckCY_Type()
		{
			var permit = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ICSPermits.AddNew();
			permit.CY_Type = ZString.Empty;
			AssertNoNotifications(permit.CY_TypeInfo);
			permit.CY_Type = "@#$";
			AssertNoNotifications(permit.CY_TypeInfo);
		}

		public void TestCheckCY_Data()
		{
			var invoiceLine1 = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var invoiceLine2 = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var invoiceLine2Permit1 = invoiceLine2.ICSPermits.AddNew();
			var invoiceLine1Permit1 = invoiceLine1.ICSPermits.AddNew();
			var invoiceLine1Permit2 = invoiceLine1.ICSPermits.AddNew();
			invoiceLine2Permit1.CY_Data = "PERT1";
			invoiceLine1Permit1.CY_Data = "PERT1";
			AssertNoNotifications(invoiceLine1Permit1.CY_DataInfo);
			invoiceLine1Permit2.CY_Data = "PERT1";
			AssertHasMessageError(invoiceLine1Permit2.CY_DataInfo, "'PERT1' has been entered multiple times.");
			invoiceLine1Permit2.CY_Data = "";
			AssertHasMessageErrorContaining(invoiceLine1Permit2.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine1Permit2.CY_Data = "PERT2";
			AssertNoNotifications(invoiceLine1Permit2.CY_DataInfo);

			invoiceLine2Permit1.CY_Data = "PERT1";
			invoiceLine1Permit1.CY_Data = "PERT1";
			invoiceLine1Permit2.CY_Data = "PERT1";
			AssertNoNotifications(invoiceLine1Permit1.CY_DataInfo);
			invoiceLine1Permit2.CY_Data = "";
			AssertNoNotifications(invoiceLine1Permit1.CY_DataInfo);
		}
	}
}
