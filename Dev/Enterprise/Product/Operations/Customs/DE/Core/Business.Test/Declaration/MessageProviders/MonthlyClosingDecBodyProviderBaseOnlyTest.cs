using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MonthlyClosingDecBodyProvider))]
	class MonthlyClosingDecBodyProviderBaseOnlyTest : MonthlyClosingDecBodyProviderAbstractTest<MonthlyClosingDecBodyProvider>
	{
		public void TestReferenceNumber()
		{
			entry.CRE_OriginalEntryNumber = "EntryNumber";
			AssertEquals("EntryNumber", Provider.ReferenceNumber);
		}

		public void TestConsignee()
		{
			entry.CRE_OA_ImporterAddress = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
			declaration.CRD_OA_ImporterAddress = ZGuid.Empty;
			AssertNull(Provider.Consignee);
		}

		public void TestConsignee_DifferentImporterAndDeclarant()
		{
			entry.CRE_EntryType = "TYP";
			entry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			TestHelper.CreateCL010CoutryList(Factory);
			var consigeeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			entry.CRE_OA_ImporterAddress = consigeeAddress.PK;
			declaration.CRD_OA_ImporterAddress = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.CRD_OA_DeclarantAddress = consigeeAddress.PK;
			AssertEquals("Consignee's EORI", "GREOR1", Provider.Consignee.Identification.EoriNumber);
		}

		public void TestConsignee_SameImporterAndDeclarant()
		{
			var orgAddress = Factory.New<OrgHeader>().MainAddress;
			entry.CRE_OA_ImporterAddress = orgAddress.PK;
			declaration.CRD_OA_ImporterAddress = orgAddress.PK;
			declaration.CRD_OA_DeclarantAddress = orgAddress.PK;
			AssertNull(Provider.Consignee);
		}

		public void TestConsigneePK()
		{
			entry.CRE_OA_ImporterAddress = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.CRD_OA_DeclarantAddress = ZGuid.Empty;
			declaration.CRD_OA_ImporterAddress = ZGuid.Empty;
			AssertEquals(Guid.Empty, Provider.ConsigneePK);
		}

		public void TestConsigneePK_DifferentImporterAndDeclarant()
		{
			var consigeeAddress = Factory.New<OrgHeader>().MainAddress;
			entry.CRE_OA_ImporterAddress = consigeeAddress.PK;
			declaration.CRD_OA_ImporterAddress = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.CRD_OA_DeclarantAddress = consigeeAddress.PK;
			AssertEquals(consigeeAddress.PK, Provider.ConsigneePK);
		}

		public void TestConsigneePK_SameImporterAndDeclarant()
		{
			var orgAddress = Factory.New<OrgHeader>().MainAddress;
			entry.CRE_OA_ImporterAddress = orgAddress.PK;
			declaration.CRD_OA_ImporterAddress = orgAddress.PK;
			declaration.CRD_OA_DeclarantAddress = orgAddress.PK;
			AssertEquals(Guid.Empty, Provider.ConsigneePK);
		}

		public void TestDeliveryTermsCode()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			AssertEquals("FOB", Provider.DeliveryTermsCode);
		}

		public void TestDeliveryTermsDescription()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			AssertEquals(string.Empty, Provider.DeliveryTermsDescription);
		}

		public void TestDeliveryTermsDescription_XXX()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTerm = IncoTerms.Other;
			invoice.JZ_IncoTermDescription = "OtherDesc1";
			AssertEquals("OtherDesc1", Provider.DeliveryTermsDescription);
		}

		public void TestDeliveryTermsPlace()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTermPlace = "INCOPLACE";

			AssertEquals("INCOPLACE", Provider.DeliveryTermsPlace);
		}

		public void TestDeliveryTermsKey()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.ZG_AgreedPlaceCode = "C";

			AssertEquals("C", Provider.DeliveryTermsKey);
		}

		public void TestPaymentTransaction()
		{
			var invoice1 = AddInvoiceWithInvoiceLine();
			invoice1.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoice1.InvoiceLines.Cast<JobComInvoiceLine>().First().JI_LinePrice = 12.12m;
			var invoice2 = AddInvoiceWithInvoiceLine();
			invoice2.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			invoice2.InvoiceLines.Cast<JobComInvoiceLine>().First().JI_LinePrice = 13.13m;
			CombineAssertions(() =>
			{
				AssertEquals("USD", Provider.PaymentTransaction.CurrencyCode);
				AssertEquals(25.25m, Provider.PaymentTransaction.Value);
			});
		}

		public void TestForeignTradeStatisticsEntryCustomsOffice()
		{
			var office = jobDeclaration.CustomsOffices.AddNew();
			office.CY_Data = "DE009988";
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
			AssertEquals("DE009988", Provider.ForeignTradeStatisticsEntryCustomsOffice);
		}

		public void TestCustomsValue_True()
		{
			jobDeclaration.ZG_IsHighValueOvrd = true;
			AssertNotNull(Provider.CustomsValue);
		}

		public void TestCustomsValue_False()
		{
			jobDeclaration.ZG_IsHighValueOvrd = false;
			AssertNull(Provider.CustomsValue);
		}

		public void TestDocuments_Modification36()
		{
			CreateCurrentSnapshot();

			var invoice = AddInvoiceWithInvoiceLine();
			var doc1 = invoice.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N380";
			doc1.CSI_ReferenceNumber = "REF1";
			doc1.CSI_DateOfIssue = ZDate.Today;
			var doc2 = invoice.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N999";
			doc2.CSI_ReferenceNumber = "REF2";
			doc2.CSI_DateOfIssue = ZDate.Today;
			var doc3 = invoice.SupportingDocuments.AddNew();
			doc3.CSI_Code = ZString.Empty;
			doc3.CSI_ReferenceNumber = "should not show";
			doc3.CSI_DateOfIssue = ZDate.Today;
			AssertContainsExactElementsInAnyOrder(new[] { "REF1", "REF2" }, Provider.Documents.Select(x => x.ReferenceNumber));
		}

		public void TestDocuments_Snapshot()
		{
			CreateCurrentSnapshot();

			var provider = new MonthlyClosingDecBodyProviderForTest(entry, isModificationMessage: false);
			AssertEquals(1, provider.Documents.Count);
		}

		public void TestDocuments_Snapshot_EmptyDocuments()
		{
			var snapshot = entry.CusReconSnapshots.AddNew();
			snapshot.CRS_Type = CusReconConstants.Current;
			snapshot.CRS_SnapshotXml = @"<DEMonthlyClosingEntrySnapshot xmlns=""http://www.cargowise.com/Schemas/DEMonthlyClosing""></DEMonthlyClosingEntrySnapshot>";

			var provider = new MonthlyClosingDecBodyProviderForTest(entry, isModificationMessage: false);
			AssertEquals(0, provider.Documents.Count);
		}

		public void TestDocuments_NoExistingSnapshot()
		{
			var provider = new MonthlyClosingDecBodyProviderForTest(entry, isModificationMessage: false);
			AssertEquals(0, provider.Documents.Count);
		}

		public void TestDocuments_MultipleInvoices()
		{
			var invoiceHeader1 = AddInvoiceWithInvoiceLine();
			var doc1 = invoiceHeader1.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N300";
			doc1.CSI_ReferenceNumber = "REF1";
			doc1.CSI_DateOfIssue = ZDate.Today;

			var doc2 = invoiceHeader1.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N301";
			doc2.CSI_ReferenceNumber = "REF2";
			doc2.CSI_DateOfIssue = ZDate.Today;

			var invoiceHeader2 = AddInvoiceWithInvoiceLine();

			var doc3 = invoiceHeader2.SupportingDocuments.AddNew();
			doc3.CSI_Code = "N302";
			doc3.CSI_ReferenceNumber = "REF3";
			doc3.CSI_DateOfIssue = ZDate.Today;

			var doc4 = invoiceHeader2.SupportingDocuments.AddNew();
			doc4.CSI_Code = "N303";
			doc4.CSI_ReferenceNumber = "REF4";
			doc4.CSI_DateOfIssue = ZDate.Today;

			AssertEquals(4, Provider.Documents.Count);
		}

		public void TestDocuments_MultipleNonUniqueInvoices()
		{
			var invoiceHeader1 = AddInvoiceWithInvoiceLine();
			var doc1 = invoiceHeader1.SupportingDocuments.AddNew();
			doc1.CSI_Code = "N300";
			doc1.CSI_ReferenceNumber = "REF1";
			doc1.CSI_DateOfIssue = ZDate.Today;

			var doc2 = invoiceHeader1.SupportingDocuments.AddNew();
			doc2.CSI_Code = "N300";
			doc2.CSI_ReferenceNumber = "REF1";
			doc2.CSI_DateOfIssue = ZDate.Today;

			var invoiceHeader2 = AddInvoiceWithInvoiceLine();

			var doc3 = invoiceHeader2.SupportingDocuments.AddNew();
			doc3.CSI_Code = "N301";
			doc3.CSI_ReferenceNumber = "REF2";
			doc3.CSI_DateOfIssue = ZDate.Today;

			var doc4 = invoiceHeader2.SupportingDocuments.AddNew();
			doc4.CSI_Code = "N301";
			doc4.CSI_ReferenceNumber = "REF2";
			doc4.CSI_DateOfIssue = ZDate.Today;

			AssertEquals(2, Provider.Documents.Count);
		}

		void CreateCurrentSnapshot()
		{
			var snapshot = entry.CusReconSnapshots.AddNew();
			snapshot.CRS_Type = CusReconConstants.Current;
			snapshot.CRS_SnapshotXml = @"<DEMonthlyClosingEntrySnapshot xmlns=""http://www.cargowise.com/Schemas/DEMonthlyClosing""><Document><Division>4</Division><Type>A123</Type><ReferenceNumber>Refnr</ReferenceNumber><IssuingDate>2021-10-22</IssuingDate></Document></DEMonthlyClosingEntrySnapshot>";
		}

		protected override MonthlyClosingDecBodyProvider GetProvider() => new MonthlyClosingDecBodyProviderForTest(entry, isModificationMessage: true);

		class MonthlyClosingDecBodyProviderForTest : MonthlyClosingDecBodyProvider
		{
			public MonthlyClosingDecBodyProviderForTest(CusReconEntry entry, bool isModificationMessage) : base(entry, isModificationMessage)
			{
			}
		}
	}
}
