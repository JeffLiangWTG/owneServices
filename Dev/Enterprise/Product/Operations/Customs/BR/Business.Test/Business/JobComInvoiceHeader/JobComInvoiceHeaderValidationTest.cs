using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest() => typeof(ExportJobComInvoiceHeaderValidation);

		protected new JobComInvoiceHeader invoiceHeader => base.invoiceHeader as JobComInvoiceHeader;

		public void TestCheckJZ_InvoiceAmount()
		{
			invoiceHeader.JZ_InvoiceAmount = 0m;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_InvoiceAmount = 343m;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckNetWeightInKG()
		{
			invoiceHeader.JZ_NetWeight = 45m;

			AssertHasMessageErrorContaining(invoiceHeader.JZ_NetWeightInfo, "The total Net Weight from the Invoice Lines (0 KG) differs from the Net Weight entered against this Invoice Header (45 KG).");

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 20m;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_NetWeight = 20m;

			invoiceHeader.JZ_NetWeight = 40m;

			AssertNoMessageErrors(invoiceHeader.JZ_NetWeightInfo);
		}

		public void TestValidateExchangeHedge()
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoiceHeader.ExchangeHedgeType = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader.ExchangeHedgeTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoiceHeader.ExchangeHedgeType = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.ExchangeHedgeTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceHeader.ExchangeHedgeType = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.ExchangeHedgeTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceHeader.ExchangeHedgeType = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.ExchangeHedgeTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestHouseBillEnteredValidation()
		{
			declaration.ActiveEntryHeaders.AddNew();
			declaration.ActiveEntryHeaders.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceHeader.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_CU_RelatedHouseBillInfo, InvoiceHeaderValidation.MultipleEntriesHouseBillMessage);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoiceHeader.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_CU_RelatedHouseBillInfo, InvoiceHeaderValidation.MultipleEntriesHouseBillMessage);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			invoiceHeader.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_CU_RelatedHouseBillInfo, InvoiceHeaderValidation.MultipleEntriesHouseBillMessage);
		}
	}
}
