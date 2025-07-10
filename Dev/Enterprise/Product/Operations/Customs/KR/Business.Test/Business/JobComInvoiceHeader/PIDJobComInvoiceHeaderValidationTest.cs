using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PIDJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJZ_Incoterm()
		{
			invoice.JZ_IncoTerm = "XXX";
			AssertNoMessageErrors(invoice.JZ_IncoTermInfo);

			invoice.JZ_IncoTerm = ZString.Empty;
			AssertNoMessageErrors(invoice.JZ_IncoTermInfo);
		}

		public void TestTotalInvoiceLinesWeightInKG()
		{
			invoiceLine.JI_Weight = -1m;
			AssertEquals(-1m, invoice.TotalInvoiceLinesWeightInKG);
			invoice.Validation.ValidateAll();
			AssertNoMessageErrors(invoice.TotalInvoiceLinesWeightInKGInfo);

			invoiceLine.JI_Weight = 0m;
			AssertEquals(0m, invoice.TotalInvoiceLinesWeightInKG);
			invoice.Validation.ValidateAll();
			AssertNoMessageErrors(invoice.TotalInvoiceLinesWeightInKGInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "86420");

			Factory.Save();
		}
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
