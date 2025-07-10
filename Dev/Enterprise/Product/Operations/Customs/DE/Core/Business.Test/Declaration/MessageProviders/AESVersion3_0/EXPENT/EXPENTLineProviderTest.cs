using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPENTLineProvider))]
	class EXPENTLineProviderTest : AESLineProviderAbstractTest<EXPENTLineProvider>
	{
		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine.PK;
				invoiceLine1.JI_OA_ConsigneeAddress = declaration.ImporterDocumentaryAddress.E2_OA_Address;
				var invoiceLine2 = invoice1.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine.PK;

				var importerAddress = GetOrgWithEORNumberAndEORIBranch("", "EBS1");
				importerAddress.Header.OH_FullName = "Importer1";
				importerAddress.Address1 = "Address1";
				importerAddress.City = "City";
				importerAddress.Postcode = "2730018";
				importerAddress.OA_RN_NKCountryCode = "US";
				invoiceLine2.JI_OA_ConsigneeAddress = importerAddress.PK;

				entryLine.Header.ResetInvoiceHeadersAndLines();
				entryLine.InvoiceLines.ReloadFromLocalCache();

				AssertEquals("EoriNumber", "GREOR1", Provider.Consignee.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS1", Provider.Consignee.EoriBranchSuffix);
				AssertEquals("Name", "Importer", Provider.Consignee.Name);
				AssertEquals("Line", "Address1", Provider.Consignee.Address);
				AssertEquals("City", "City", Provider.Consignee.City);
				AssertEquals("Postcode", "2730018", Provider.Consignee.Postcode);
				AssertEquals("Country", "US", Provider.Consignee.Country);
			});
		}

		public void TestConsignee_Empty()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_OA_ConsigneeAddress = declaration.ImporterDocumentaryAddress.E2_OA_Address;

				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine.PK;
				invoiceLine2.JI_OA_ConsigneeAddress = declaration.ImporterDocumentaryAddress.E2_OA_Address;

				entryLine.Header.ResetInvoiceHeadersAndLines();
				entryLine.InvoiceLines.ReloadFromLocalCache();

				AssertEquals(null, Provider.Consignee);
			});
		}

		public void TestPreviousDocuments()
		{
			AssertEquals(1, Provider.PreviousDocuments.Count);
		}

		public void TestPreviousDocuments_HasExportDataMessage()
		{
			var message = Factory.New<AesEDIMessage>();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2021, 11, 04);
			message.EM_ApplicationReference = "DEXPDF";
			message.EM_LinkedObject = entryLine.Header;
			entryLine.Header.Messages.Add(message);
			Factory.Save();

			invoiceLine.PreviousDocuments.RemoveAndDeleteAll();
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_SystemCreateTimeUtc = new ZDateTime(2021, 11, 03);
			previousDocument.CSI_Code = "1234";
			var previousDocument2 = invoiceLine.PreviousDocuments.AddNew();
			previousDocument2.CSI_SystemCreateTimeUtc = new ZDateTime(2021, 11, 06);
			previousDocument2.CSI_Code = "ABCD";
			var previousDocumentInHeader = invoice.PreviousDocuments.AddNew();
			previousDocumentInHeader.CSI_SystemCreateTimeUtc = new ZDateTime(2021, 11, 05);
			previousDocumentInHeader.CSI_Code = "5678";
			AssertEquals("ABCD", Provider.PreviousDocuments.Single().FullType);
		}

		public void TestDeliveryTerms() => AssertNull(Provider.DeliveryTerms);

		[ExpectNoExceptions]
		public void TestTransactionType_Same()
		{
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "3";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = invoiceLine.CusEntryLine.PK;
			invoiceLine.CusEntryLine.Header.ResetInvoiceHeadersAndLines();
			invoiceLine.CusEntryLine.InvoiceLines.ReloadFromLocalCache();
			NUnit.Framework.Assert.That(Provider.TransactionType, Is.Null);
		}

		[ExpectNoExceptions]
		public void TestTransactionType_Different()
		{
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "4";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = invoiceLine.CusEntryLine.PK;
			invoiceLine.CusEntryLine.Header.ResetInvoiceHeadersAndLines();
			invoiceLine.CusEntryLine.InvoiceLines.ReloadFromLocalCache();
			NUnit.Framework.Assert.That(Provider.TransactionType, Is.EqualTo("3"));
		}

		public void TestCountryOfOrigin()
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Kyrgyzstan;
			AssertEquals(Core.Constants.CountryCodes.Kyrgyzstan, Provider.CountryOfOrigin);
		}

		public void TestRegionOfDispatch()
		{
			invoiceLine.JI_StateOrRegionOfOrigin = Core.Constants.CountryCodes.Russia;
			AssertEquals(Core.Constants.CountryCodes.Russia, Provider.RegionOfDispatch);
		}

		protected override IEnumerable<Expression<Func<EXPENTLineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Consignee;
			yield return x => x.DeliveryTerms;
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLine.CL_StatisticalValue = 1111m;
			invoice.JZ_ValuationCode = "3";
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_IncoTermPlace = "PLACE";
			invoice.JZ_InvoiceAmount = 1000.75m;
			invoiceLine.JI_LinePrice = 1000.75m;
			invoiceLine.JI_StateOrRegionOfOrigin = OriginFederalStateList.Codes.Hamburg;
			var importerAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			importerAddress.Header.OH_FullName = "Importer";
			importerAddress.Address1 = "Address1";
			importerAddress.City = "City";
			importerAddress.Postcode = "2730018";
			importerAddress.OA_RN_NKCountryCode = "US";
			invoiceLine.JI_OA_ConsigneeAddress = importerAddress.PK;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "235";
			previousDocument.CSI_ReferenceNumber = "REF1";
			previousDocument.CSI_Description = "Complement1";

			action = new ExportEntryMessageSendingAction(entryLine.Header);
			headerProvider = new EXPENTHeaderProvider(action);
		}
		ExportEntryMessageSendingAction action;
		EXPENTHeaderProvider headerProvider;

		protected override EXPENTLineProvider GetProvider() => new EXPENTLineProvider(entryLine, action, headerProvider);
	}
}
