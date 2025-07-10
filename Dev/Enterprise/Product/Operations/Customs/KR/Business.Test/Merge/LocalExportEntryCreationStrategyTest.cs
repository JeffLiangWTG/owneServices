using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void Test5DPAnd5DQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals("5DP EntryHeader", ElectronicDocumentTypeList.Codes._5DP, declaration.CustomsEntryHeaders[0].CH_MessageType);
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals("5DP EntryHeader", ElectronicDocumentTypeList.Codes._5DQ, declaration.CustomsEntryHeaders[0].CH_MessageType);
		}

		public void TestGetKeyForHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			var strategy = new LocalExportEntryCreationStrategy(declaration, ElectronicDocumentTypeList.Codes._5DP);
			var keys = strategy.GetKeyForHeader(invoiceLine).Keys;
			Assert("EffectiveValuationDate", !keys.Contains(invoice.EffectiveValuationDate));

			Assert("JZ_OH_Supplier", keys.Contains(declaration.JE_OH_Supplier));
			Assert("JZ_OH_Manufacturer", keys.Contains(invoice.JZ_OH_Manufacturer));
			Assert("JZ_OH_Buyer", keys.Contains(invoice.JZ_OH_Buyer));
			Assert("JZ_DRWApplicantType", keys.Contains(invoice.JZ_DRWApplicantType));
		}

		public void TestMergeKeyForLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var strategy = new LocalExportEntryCreationStrategy(declaration, ElectronicDocumentTypeList.Codes._5DP);
			var keys = strategy.GetKeyForLine(invoiceLine).Keys;
			Assert("PK", keys.Contains(invoiceLine.PK));
		}
	}
}
