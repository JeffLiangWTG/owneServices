using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	sealed class InvoiceLineApportionChargeTest : EU.Business.Declaration.Testing.InvoiceLineApportionChargeTest
	{
		public void TestLookups()
		{
			AssertType<ImportInvoiceLineApportionChargeLookups>(invoiceLineApportionCharge.Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertType<EU.Business.Declaration.InvoiceLineApportionChargeLookups>("Lookups for EXP", declaration.Invoices.AddNew().InvoiceLines.AddNew().ApportionedCharges.AddNew().Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			AssertType<EU.Business.Declaration.InvoiceLineApportionChargeLookups>("Lookups for non-IMP or EXP", declaration.Invoices.AddNew().InvoiceLines.AddNew().ApportionedCharges.AddNew().Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => invoiceLineApportionCharge;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLineApportionCharge = declaration.Invoices.AddNew().InvoiceLines.AddNew().ApportionedCharges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceLineApportionCharge invoiceLineApportionCharge;
	}
}
