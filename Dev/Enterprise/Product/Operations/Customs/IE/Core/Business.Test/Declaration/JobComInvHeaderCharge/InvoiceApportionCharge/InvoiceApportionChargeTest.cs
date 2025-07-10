using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	sealed class InvoiceApportionChargeTest : EU.Business.Declaration.Testing.InvoiceApportionChargeTest
	{
		public void TestLookups()
		{
			AssertType<ImportInvoiceApportionChargeLookups>(invoiceApportionCharge.Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertType<EU.Business.Declaration.InvoiceApportionChargeLookups>("Lookups for EXP", declaration.Invoices.AddNew().GroupCharges.AddNew().Lookups);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			AssertType<EU.Business.Declaration.InvoiceApportionChargeLookups>("Lookups for non-IMP or EXP", declaration.Invoices.AddNew().GroupCharges.AddNew().Lookups);
		}

		public void TestGetNewValidation()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			AssertType<EU.Business.Declaration.InvoiceApportionChargeValidation>(invoiceApportionCharge.Validation);
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertType<ImportInvoiceApportionChargeValidation>(invoiceApportionCharge.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => declaration.Invoices.AddNew().GroupCharges.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceApportionCharge = declaration.Invoices.AddNew().GroupCharges.AddNew();
		}

		JobDeclaration declaration;
		InvoiceApportionCharge invoiceApportionCharge;
	}
}
