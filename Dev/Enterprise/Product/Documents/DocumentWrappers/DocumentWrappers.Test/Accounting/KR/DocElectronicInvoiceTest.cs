using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.DataTransfer.EInvoicing.KoreaSouth;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.DocumentWrappers.KR.Testing
{
	[TestedType(typeof(DocElectronicInvoice))]
	sealed class DocElectronicInvoiceTest : DocumentWrapperTestCase
	{
		public void TestSpecifiedMonetarySummationChargeTotalAmountDigits()
		{
			AssertSpecifiedMonetarySummationChargeTotalAmountDigits(new ZString[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " }, "");
			AssertSpecifiedMonetarySummationChargeTotalAmountDigits(new ZString[] { "1", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " }, "1");
			AssertSpecifiedMonetarySummationChargeTotalAmountDigits(new ZString[] { "0", "9", "8", "7", "6", "5", "4", "3", "2", "1", " ", " " }, "1234567890");
			AssertSpecifiedMonetarySummationChargeTotalAmountDigits(new ZString[] { "2", "1", "0", "9", "8", "7", "6", "5", "4", "3", "2", "1" }, "123456789012");
			AssertSpecifiedMonetarySummationChargeTotalAmountDigits(new ZString[] { "3", "2", "1", "0", "9", "8", "7", "6", "5", "4", "3", "2", "1" }, "1234567890123");

			void AssertSpecifiedMonetarySummationChargeTotalAmountDigits(ZString[] expectedAmountDigits, ZString amount)
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var additionalInfo = new AdditionalInfo()
				{
					SpecifiedMonetarySummationChargeTotalAmount = amount
				};
				var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

				AssertArrayEqualsByElements(expectedAmountDigits, wrapper.SpecifiedMonetarySummationChargeTotalAmountDigits);
			}
		}

		public void TestSpecifiedMonetarySummationTaxTotalAmountDigits()
		{
			AssertSpecifiedMonetarySummationTaxTotalAmount(new ZString[] { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " }, "");
			AssertSpecifiedMonetarySummationTaxTotalAmount(new ZString[] { "1", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " }, "1");
			AssertSpecifiedMonetarySummationTaxTotalAmount(new ZString[] { "9", "8", "7", "6", "5", "4", "3", "2", "1", " ", " " }, "123456789");
			AssertSpecifiedMonetarySummationTaxTotalAmount(new ZString[] { "1", "0", "9", "8", "7", "6", "5", "4", "3", "2", "1" }, "12345678901");
			AssertSpecifiedMonetarySummationTaxTotalAmount(new ZString[] { "2", "1", "0", "9", "8", "7", "6", "5", "4", "3", "2", "1" }, "123456789012");

			void AssertSpecifiedMonetarySummationTaxTotalAmount(ZString[] expectedAmountDigits, ZString amount)
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var additionalInfo = new AdditionalInfo()
				{
					SpecifiedMonetarySummationTaxTotalAmount = amount
				};
				var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

				AssertArrayEqualsByElements(expectedAmountDigits, wrapper.SpecifiedMonetarySummationTaxTotalAmountDigits);
			}
		}

		public void TestPropertiesWithAdditionalInfo()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new AdditionalInfo()
			{
				IssueID = "IssueID",
				IssueDateTime = "IssueDateTime",
				AmendStatusCode = "AmendmentStatusCode",

				DescriptionText = new List<ZString> { "DescriptionText 1", "DescriptionText 2", "DescriptionText 3" },

				SpecifiedPaymentMeansTypeCode = "TypeCode",
				SpecifiedPaymentMeansPaidAmount = "PaidAmount",

				SpecifiedMonetarySummationChargeTotalAmount = "ChargeTotalAmount",
				SpecifiedMonetarySummationTaxTotalAmount = "TaxTotalAmount",
				SpecifiedMonetarySummationGrandTotalAmount = "GrandTotalAmount",

				InvoiceeID = "InvoiceeID",
				InvoiceeTypeCode = "InvoiceeTypeCode",
				InvoiceeNameText = "InvoiceeNameText",
				InvoiceeClassificationCode = "InvoiceeClassificationCode",
				InvoiceeSpecifiedPersonNameText = "InvoiceeSpecifiedPersonNameText",
				InvoiceeSpecifiedAddressLineOneText = "InvoiceeSpecifiedAddressLineOneText",
				InvoiceePrimaryDefinedContactURICommunication = "InvoiceePrimaryDefinedContactURICommunication",
				InvoiceeSecondaryDefinedContactURICommunication = "InvoiceeSecondaryDefinedContactURICommunication",

				InvoicerID = "InvoicerID",
				InvoicerTypeCode = "InvoicerTypeCode",
				InvoicerNameText = "InvoicerNameText",
				InvoicerClassificationCode = "InvoicerClassificationCode",
				InvoicerSpecifiedPersonNameText = "InvoicerSpecifiedPersonNameText",
				InvoicerSpecifiedAddressLineOneText = "InvoicerSpecifiedAddressLineOneText",
				InvoicerDefinedContactURICommunication = "InvoicerDefinedContactURICommunication",
				FullTypeCode = "FullTypeCode",
			};

			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

			AssertEquals("IssueID", wrapper.IssueID);
			AssertEquals("IssueDateTime", wrapper.IssueDateTime);
			AssertEquals("AmendmentStatusCode", wrapper.AmendmentStatusCode);

			AssertEquals("DescriptionText 1", wrapper.DescriptionText1);
			AssertEquals("DescriptionText 2", wrapper.DescriptionText2);
			AssertEquals("DescriptionText 3", wrapper.DescriptionText3);

			AssertEquals("TypeCode", wrapper.SpecifiedPaymentMeansTypeCode);
			AssertEquals("PaidAmount", wrapper.SpecifiedPaymentMeansPaidAmount);

			AssertEquals("ChargeTotalAmount", wrapper.SpecifiedMonetarySummationChargeTotalAmount);
			AssertEquals("TaxTotalAmount", wrapper.SpecifiedMonetarySummationTaxTotalAmount);
			AssertEquals("GrandTotalAmount", wrapper.SpecifiedMonetarySummationGrandTotalAmount);

			AssertEquals("Inv-oi-ceeID", wrapper.InvoiceeID);
			AssertEquals("InvoiceeNameText", wrapper.InvoiceeNameText);
			AssertEquals("InvoiceeSpecifiedPersonNameText", wrapper.InvoiceeSpecifiedPersonNameText);
			AssertEquals("InvoiceeSpecifiedAddressLineOneText", wrapper.InvoiceeSpecifiedAddressLineOneText);
			AssertEquals("InvoiceeTypeCode", wrapper.InvoiceeTypeCode);
			AssertEquals("InvoiceeClassificationCode", wrapper.InvoiceeClassificationCode);
			AssertEquals("InvoiceePrimaryDefinedContactURICommunication", wrapper.InvoiceePrimaryDefinedContactURICommunication);
			AssertEquals("InvoiceeSecondaryDefinedContactURICommunication", wrapper.InvoiceeSecondaryDefinedContactURICommunication);

			AssertEquals("Inv-oi-cerID", wrapper.InvoicerID);
			AssertEquals("InvoicerNameText", wrapper.InvoicerNameText);
			AssertEquals("InvoicerSpecifiedPersonNameText", wrapper.InvoicerSpecifiedPersonNameText);
			AssertEquals("InvoicerSpecifiedAddressLineOneText", wrapper.InvoicerSpecifiedAddressLineOneText);
			AssertEquals("InvoicerTypeCode", wrapper.InvoicerTypeCode);
			AssertEquals("InvoicerClassificationCode", wrapper.InvoicerClassificationCode);
			AssertEquals("InvoicerDefinedContactURICommunication", wrapper.InvoicerDefinedContactURICommunication);
			AssertEquals("FullTypeCode", wrapper.TypeCode);

			AssertEquals(false, wrapper.InvoiceLines.Any());
		}

		public void TestPropertiesWithNullAdditionalInfo()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new XmlToAdditionalInfoConverter().Convert(null);
			AssertNull("Precondition", additionalInfo);

			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

			AssertNullOrEmpty(wrapper.IssueID);
			AssertNullOrEmpty(wrapper.IssueDateTime);
			AssertNullOrEmpty(wrapper.AmendmentStatusCode);
			AssertNullOrEmpty(wrapper.PaymentStatus);

			AssertNullOrEmpty(wrapper.DescriptionText1);
			AssertNullOrEmpty(wrapper.DescriptionText2);
			AssertNullOrEmpty(wrapper.DescriptionText3);

			AssertNullOrEmpty(wrapper.SpecifiedPaymentMeansTypeCode);
			AssertNullOrEmpty(wrapper.SpecifiedPaymentMeansPaidAmount);

			AssertNullOrEmpty(wrapper.SpecifiedMonetarySummationChargeTotalAmount);
			AssertNullOrEmpty(wrapper.SpecifiedMonetarySummationTaxTotalAmount);
			AssertNullOrEmpty(wrapper.SpecifiedMonetarySummationGrandTotalAmount);

			AssertNullOrEmpty(wrapper.InvoiceeID);
			AssertNullOrEmpty(wrapper.InvoiceeNameText);
			AssertNullOrEmpty(wrapper.InvoiceeSpecifiedPersonNameText);
			AssertNullOrEmpty(wrapper.InvoiceeSpecifiedAddressLineOneText);
			AssertNullOrEmpty(wrapper.InvoiceeTypeCode);
			AssertNullOrEmpty(wrapper.InvoiceeClassificationCode);
			AssertNullOrEmpty(wrapper.InvoiceePrimaryDefinedContactURICommunication);
			AssertNullOrEmpty(wrapper.InvoiceeSecondaryDefinedContactURICommunication);

			AssertNullOrEmpty(wrapper.InvoicerID);
			AssertNullOrEmpty(wrapper.InvoicerNameText);
			AssertNullOrEmpty(wrapper.InvoicerSpecifiedPersonNameText);
			AssertNullOrEmpty(wrapper.InvoicerSpecifiedAddressLineOneText);
			AssertNullOrEmpty(wrapper.InvoicerTypeCode);
			AssertNullOrEmpty(wrapper.InvoicerClassificationCode);
			AssertNullOrEmpty(wrapper.InvoicerDefinedContactURICommunication);
			AssertNullOrEmpty(wrapper.TypeCode);

			AssertEquals(false, wrapper.InvoiceLines.Any());
		}

		public void TestInvoiceLines()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new AdditionalInfo();
			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);
			AssertNull("Precondition", additionalInfo.Lines);
			AssertEquals(false, wrapper.InvoiceLines.Any());

			additionalInfo.Lines = new List<TaxInvoiceTradeLineItem>();
			wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);
			AssertEquals("Precondition", false, additionalInfo.Lines.Any());
			AssertEquals(false, wrapper.InvoiceLines.Any());

			additionalInfo.Lines.Add(new TaxInvoiceTradeLineItem());
			wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);
			AssertEquals(1, wrapper.InvoiceLines.Count);
			AssertNullOrEmpty(wrapper.InvoiceLines[0].DescriptionText);
			AssertNullOrEmpty(wrapper.InvoiceLines[0].InvoiceAmount);
			AssertNullOrEmpty(wrapper.InvoiceLines[0].CalculatedAmount);

			additionalInfo.Lines.Add(new TaxInvoiceTradeLineItem()
			{
				DescriptionText = "DescriptionText",
				InvoiceAmount = "InvoiceAmount",
				CalculatedAmount = "CalculatedAmount"
			});
			wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);
			AssertEquals(2, wrapper.InvoiceLines.Count);
			AssertEquals("DescriptionText", wrapper.InvoiceLines[1].DescriptionText);
			AssertEquals("InvoiceAmount", wrapper.InvoiceLines[1].InvoiceAmount);
			AssertEquals("CalculatedAmount", wrapper.InvoiceLines[1].CalculatedAmount);
		}

		public void TestNewUsingNullInvoice()
		{
			AssertNull(DocElectronicInvoice.New(null, Factory));
		}

		public void TestInvoiceTitle()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();

			var wrapper = new DocElectronicInvoice_TestOnly(invoice, new AdditionalInfo { OriginalIssueID = null }, Factory);
			AssertEquals("전자세금계산서", wrapper.InvoiceTitle);

			var wrapper_AmendmentInvoice = new DocElectronicInvoice_TestOnly(invoice, new AdditionalInfo { OriginalIssueID = "DummyOriginalIssueID" }, Factory);
			AssertEquals("수정전자세금계산서", wrapper_AmendmentInvoice.InvoiceTitle);

			var wrapper_AmendmentCreditNote = new DocElectronicInvoice_TestOnly(creditNote, new AdditionalInfo { OriginalIssueID = "DummyOriginalIssueID" }, Factory);
			AssertEquals("수정전자세금계산서", wrapper_AmendmentCreditNote.InvoiceTitle);

			var wrapper_InvoiceWithZeroTaxAmount = new DocElectronicInvoice_TestOnly(invoice, new AdditionalInfo { OriginalIssueID = null, FullTypeCode = "0102" }, Factory);
			AssertEquals("영세율전자세금계산서", wrapper_InvoiceWithZeroTaxAmount.InvoiceTitle);

			var wrapper_CreditNotesWithZeroTaxAmount = new DocElectronicInvoice_TestOnly(creditNote, new AdditionalInfo { OriginalIssueID = "DummyOriginalIssueID", FullTypeCode = "0202" }, Factory);
			AssertEquals("수정영세율전자세금계산서", wrapper_CreditNotesWithZeroTaxAmount.InvoiceTitle);

			var wrapper_ExemptInvoice = new DocElectronicInvoice_TestOnly(creditNote, new AdditionalInfo { OriginalIssueID = "DummyOriginalIssueID", FullTypeCode = "0301" }, Factory);
			AssertEquals("전자계산서", wrapper_ExemptInvoice.InvoiceTitle);

			var wrapper_ExemptCreditNotes = new DocElectronicInvoice_TestOnly(creditNote, new AdditionalInfo { OriginalIssueID = "DummyOriginalIssueID", FullTypeCode = "0401" }, Factory);
			AssertEquals("수정전자계산서", wrapper_ExemptCreditNotes.InvoiceTitle);
		}

		public void TestIssueID()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new AdditionalInfo();
			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

			AssertIssueID(ZString.Empty, ZString.Empty);
			AssertIssueID("1234", "1234");
			AssertIssueID("202207041234567800000001", "20220704-12345678-00000001");
			AssertIssueID("2022070412345678000000012", "2022070412345678000000012");

			void AssertIssueID(ZString issueID, ZString expectedIssueID)
			{
				additionalInfo.IssueID = issueID;
				AssertEquals(expectedIssueID, wrapper.IssueID);
			}
		}

		public void TestInvoiceeID()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new AdditionalInfo();
			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

			AssertInvoiceeID(ZString.Empty, ZString.Empty);
			AssertInvoiceeID("1234567", "1234567");
			AssertInvoiceeID("1234567890", "123-45-67890");
			AssertInvoiceeID("12345678901", "12345678901");
			AssertInvoiceeID("1234567890123", "123456-7890123");
			AssertInvoiceeID("12345678901234", "12345678901234");

			void AssertInvoiceeID(ZString invoiceeID, ZString expectedInvoiceeID)
			{
				additionalInfo.InvoiceeID = invoiceeID;
				AssertEquals(expectedInvoiceeID, wrapper.InvoiceeID);
			}
		}

		public void TestInvoicerID()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new AdditionalInfo();
			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

			AssertInvoicerID(ZString.Empty, ZString.Empty);
			AssertInvoicerID("1234567", "1234567");
			AssertInvoicerID("1234567890", "123-45-67890");
			AssertInvoicerID("12345678901", "12345678901");
			AssertInvoicerID("1234567890123", "1234567890123");
			AssertInvoicerID("12345678901234", "12345678901234");

			void AssertInvoicerID(ZString invoicerID, ZString expectedInvoicerID)
			{
				additionalInfo.InvoicerID = invoicerID;
				AssertEquals(expectedInvoicerID, wrapper.InvoicerID);
			}
		}

		public void TestAmendmentStatusDescription()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new AdditionalInfo();
			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

			AssertAmendmentStatusDescription(ZString.Empty, ZString.Empty);
			AssertAmendmentStatusDescription("01", "기재사항 착오.정정");
			AssertAmendmentStatusDescription("02", "공급금액 변동");
			AssertAmendmentStatusDescription("03", "제화의 환입");
			AssertAmendmentStatusDescription("04", "계약의 해제");
			AssertAmendmentStatusDescription("05", "내국신용장등 사후개설");
			AssertAmendmentStatusDescription("06", "착오에 의한 이중발급");

			void AssertAmendmentStatusDescription(ZString amendStatusCode, ZString expectedAmendmentStatusDescription)
			{
				additionalInfo.AmendStatusCode = amendStatusCode;
				AssertEquals(expectedAmendmentStatusDescription, wrapper.AmendmentStatusDescription);
			}
		}

		public void TestDescriptionText()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			var additionalInfo1 = new AdditionalInfo() { DescriptionText = null };
			var wrapper1 = new DocElectronicInvoice_TestOnly(invoice, additionalInfo1, Factory);
			AssertNullOrEmpty(wrapper1.DescriptionText1);
			AssertNullOrEmpty(wrapper1.DescriptionText2);
			AssertNullOrEmpty(wrapper1.DescriptionText3);

			var additionalInfo2 = new AdditionalInfo() { DescriptionText = new List<ZString> { "DescriptionText 1" } };
			var wrapper2 = new DocElectronicInvoice_TestOnly(invoice, additionalInfo2, Factory);
			AssertEquals("DescriptionText 1", wrapper2.DescriptionText1);
			AssertNullOrEmpty(wrapper2.DescriptionText2);
			AssertNullOrEmpty(wrapper2.DescriptionText3);

			var additionalInfo3 = new AdditionalInfo() { DescriptionText = new List<ZString> { "DescriptionText 1", "DescriptionText 2" } };
			var wrapper3 = new DocElectronicInvoice_TestOnly(invoice, additionalInfo3, Factory);
			AssertEquals("DescriptionText 1", wrapper3.DescriptionText1);
			AssertEquals("DescriptionText 2", wrapper3.DescriptionText2);
			AssertNullOrEmpty(wrapper3.DescriptionText3);

			var additionalInfo4 = new AdditionalInfo() { DescriptionText = new List<ZString> { "DescriptionText 1", "DescriptionText 2", "DescriptionText 3" } };
			var wrapper4 = new DocElectronicInvoice_TestOnly(invoice, additionalInfo4, Factory);
			AssertEquals("DescriptionText 1", wrapper4.DescriptionText1);
			AssertEquals("DescriptionText 2", wrapper4.DescriptionText2);
			AssertEquals("DescriptionText 3", wrapper4.DescriptionText3);
		}

		public void TestPaymentStatus()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var additionalInfo = new AdditionalInfo();
			var wrapper = new DocElectronicInvoice_TestOnly(invoice, additionalInfo, Factory);

			AssertPaymentStatus(ZString.Empty, ZString.Empty);
			AssertPaymentStatus("01", "이 금액을 영수 함");
			AssertPaymentStatus("02", "이 금액을 청구 함");

			void AssertPaymentStatus(ZString paymentStatus, ZString expectedPaymentStatus)
			{
				additionalInfo.PaymentStatus = paymentStatus;
				AssertEquals(expectedPaymentStatus, wrapper.PaymentStatus);
			}
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			Invoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.KRW, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = testObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(batch, Invoice, Core.Constants.EInvoicingPivotState.Sent);

			var referenceKRI = Factory.New<AccTransactionHeaderReference>();
			referenceKRI.AH1_AH = Invoice.PK;
			referenceKRI.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
			referenceKRI.AH1_Reference = "TEST001";

			var authRecord = Factory.New<KoreaSouthAccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = Invoice.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authRecord.AHF_AuthorisationData = ZBlob.FromUTF8("<TaxInvoice><TaxInvoiceDocument><IssueID>TEST001</IssueID></TaxInvoiceDocument></TaxInvoice>");

			return new DocumentWrapper[] { DocElectronicInvoice.New(Invoice, Factory) };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocElectronicInvoice.New(Invoice, Factory);
		}

		public class DocElectronicInvoice_TestOnly : DocElectronicInvoice
		{
			public DocElectronicInvoice_TestOnly(InvoicingBase invoice, AdditionalInfo additionalInfo, BusinessObjectFactory factoryToWrap)
				: base(invoice, additionalInfo, factoryToWrap)
			{
			}
		}

		InvoicingBase Invoice;
	}
}
