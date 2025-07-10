using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class MiscCusEntryHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFreightAndInsurance()
		{
			var invoice = declaration.Invoices[0];
			var entry = declaration.CustomsEntryHeaders[0];

			invoice.InvoiceLines[0].Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, -2m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice.InvoiceLines[0].Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, -2m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			AssertEquals(-2m, entry.Insurance);
			AssertEquals(-2m, entry.Freight);

			entry.Validation.ValidateAll();

			AssertNoMessageErrors(entry.FreightInfo);
			AssertNoMessageErrors(entry.InsuranceInfo);
		}

		public void TestTotalPackages()
		{
			var invoice = declaration.Invoices[0];
			var entry = declaration.CustomsEntryHeaders[0];
			invoice.JZ_NoOfPacks = 0m;

			entry.Validation.ValidateAll();
			AssertNoMessageErrors(entry.TotalPackagesInfo);
		}

		public void TestCustomsValue()
		{
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.AllEntryLines[0];
			entryLine.CL_CustomsValue = -1;
			entry.Validation.ValidateAll();
			AssertNoMessageErrors(entry.CustomsValueInfo);

			entryLine.CL_CustomsValue = 0;
			entry.ResetIsCustomsValueCalculated();
			entry.Validation.ValidateAll();
			AssertNoMessageErrors(entry.CustomsValueInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			var invoice = declaration.Invoices[0];
			invoice.JobComInvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;
	}
}
