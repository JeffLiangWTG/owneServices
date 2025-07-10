using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.TW
{
	[TestedType(typeof(DocumentWrappers.TW.DocARComplianceDocument))]
	sealed class DocARComplianceDocumentHeaderTest : Accounting.DocARComplianceDocumentHeaderTest
	{
		public override void TestTaxInvoiceDate()
		{
			AssertEquals("107年07月08日", Wrapper.InvoiceDateString);
		}

		public void TestUniversalInvoiceDateString()
		{
			AssertEquals("2018-07-08", Wrapper.InvoiceDateAsUniversalString);
		}

		public void TestComplianceBookValidityPeriodString()
		{
			Header.ADH_DocumentDate = new ZDateTime(2019, 1, 2);
			AssertEquals("108年 01-02月", Wrapper.ComplianceBookValidityPeriodString);

			Header.ADH_DocumentDate = new ZDateTime(2019, 2, 1);
			AssertEquals("108年 01-02月", Wrapper.ComplianceBookValidityPeriodString);

			Header.ADH_DocumentDate = new ZDateTime(2019, 7, 10);
			AssertEquals("108年 07-08月", Wrapper.ComplianceBookValidityPeriodString);

			Header.ADH_DocumentDate = new ZDateTime(2019, 8, 1);
			AssertEquals("108年 07-08月", Wrapper.ComplianceBookValidityPeriodString);

			Header.ADH_DocumentDate = new ZDateTime(2019, 10, 2);
			AssertEquals("108年 09-10月", Wrapper.ComplianceBookValidityPeriodString);

			Header.ADH_DocumentDate = new ZDateTime(2019, 11, 1);
			AssertEquals("108年 11-12月", Wrapper.ComplianceBookValidityPeriodString);
		}

		public void TestTaxCategory()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "TX1";
				line.AL_AT = taxRate.PK;
				line.AL_GSTVAT = 0m;

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				var query = new ZQuery();
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.ABIGAS.PK);
				var documentHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);

				var wrapper = DocumentWrappers.TW.DocARComplianceDocument.New(documentHeader, Factory);

				AssertComplianceWithDifferentTaxCode();

				line.AL_GSTVAT = 5m;
				documentHeader.Delete();
				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				documentHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);

				wrapper = DocumentWrappers.TW.DocARComplianceDocument.New(documentHeader, Factory);

				AssertComplianceWithDifferentTaxCode();

				void AssertComplianceWithDifferentTaxCode()
				{
					taxRate.AT_Code = "VAT";
					AssertEquals(true, wrapper.Taxable);
					AssertEquals(false, wrapper.ZeroRated);
					AssertEquals(false, wrapper.TaxExempt);

					taxRate.AT_Code = "CAPVAT";
					AssertEquals(true, wrapper.Taxable);
					AssertEquals(false, wrapper.ZeroRated);
					AssertEquals(false, wrapper.TaxExempt);

					taxRate.AT_Code = "FREEVAT";
					AssertEquals(false, wrapper.Taxable);
					AssertEquals(true, wrapper.ZeroRated);
					AssertEquals(false, wrapper.TaxExempt);

					taxRate.AT_Code = "EXEMPT";
					AssertEquals(false, wrapper.Taxable);
					AssertEquals(false, wrapper.ZeroRated);
					AssertEquals(true, wrapper.TaxExempt);
				}
			}
		}

		public void TestElectronicGUIChargeLines()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);

				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				var query = new ZQuery();
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, TestObjectCreator.ABIGAS.PK);
				var documentHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);
				documentHeader.ComplianceDocumentLines[0].ADL_Description = "ABCDEFGHIJKLMNOP";
				AssertEquals(1, documentHeader.ComplianceDocumentLines.Count);

				var wrapper = DocumentWrappers.TW.DocARComplianceDocument.New(documentHeader, Factory);
				AssertEquals(1, wrapper.ElectronicGUIChargeLinesFirstPart.Count);
				AssertEquals(0, wrapper.ElectronicGUIChargeLinesSecondPart.Count);

				documentHeader.Delete();

				for (int i = 0; i < 10; i++)
				{
					line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
					line.AL_AT = TestObjectCreator.GST1.PK;
					line.AL_LocalExTaxAmount = 5000m;
				}
				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
				documentHeader = Factory.LoadTop1<ARComplianceDocumentHeader>(query);
				AssertEquals(11, documentHeader.ComplianceDocumentLines.Count);

				wrapper = DocumentWrappers.TW.DocARComplianceDocument.New(documentHeader, Factory);
				AssertEquals(10, wrapper.ElectronicGUIChargeLinesFirstPart.Count);
				AssertEquals(1, wrapper.ElectronicGUIChargeLinesSecondPart.Count);
			}
		}

		public void TestElectronicGUIChargeLinesForPaper()
		{
			AssertEquals(9, Wrapper.ElectronicGUIChargeLinesForPaper.Count);
			AssertEquals(100m, Wrapper.ElectronicGUIChargeLinesForPaper[0].LocalAmount);
			AssertEquals("Test Line1", Wrapper.ElectronicGUIChargeLinesForPaper[0].Description);
			AssertEquals(200m, Wrapper.ElectronicGUIChargeLinesForPaper[1].LocalAmount);
			AssertEquals("Test Line2", Wrapper.ElectronicGUIChargeLinesForPaper[1].Description);
			AssertEquals(300m, Wrapper.ElectronicGUIChargeLinesForPaper[2].LocalAmount);
			AssertEquals("Test Line3", Wrapper.ElectronicGUIChargeLinesForPaper[2].Description);
			AssertEquals(400m, Wrapper.ElectronicGUIChargeLinesForPaper[3].LocalAmount);
			AssertEquals("Test Line4", Wrapper.ElectronicGUIChargeLinesForPaper[3].Description);
			AssertEquals(500m, Wrapper.ElectronicGUIChargeLinesForPaper[4].LocalAmount);
			AssertEquals("Test Line5", Wrapper.ElectronicGUIChargeLinesForPaper[4].Description);
			AssertEquals(600m, Wrapper.ElectronicGUIChargeLinesForPaper[5].LocalAmount);
			AssertEquals("Test Line6", Wrapper.ElectronicGUIChargeLinesForPaper[5].Description);
			AssertEquals(700m, Wrapper.ElectronicGUIChargeLinesForPaper[6].LocalAmount);
			AssertEquals("Test Line7", Wrapper.ElectronicGUIChargeLinesForPaper[6].Description);
			AssertEquals(800m, Wrapper.ElectronicGUIChargeLinesForPaper[7].LocalAmount);
			AssertEquals("Test Line8", Wrapper.ElectronicGUIChargeLinesForPaper[7].Description);
			AssertEquals(1000m, Wrapper.ElectronicGUIChargeLinesForPaper[8].LocalAmount);
			AssertEquals("Test Line9", Wrapper.ElectronicGUIChargeLinesForPaper[8].Description);
		}

		public override void TestChargeLines()
		{
			AssertEquals(7, Wrapper.ChargeLines.Count);
			AssertEquals(100m, Wrapper.ChargeLines[0].LocalAmount);
			AssertEquals("Test Line1", Wrapper.ChargeLines[0].Description);
			AssertEquals(200m, Wrapper.ChargeLines[1].LocalAmount);
			AssertEquals("Test Line2", Wrapper.ChargeLines[1].Description);
			AssertEquals(300m, Wrapper.ChargeLines[2].LocalAmount);
			AssertEquals("Test Line3", Wrapper.ChargeLines[2].Description);
			AssertEquals(400m, Wrapper.ChargeLines[3].LocalAmount);
			AssertEquals("Test Line4", Wrapper.ChargeLines[3].Description);
			AssertEquals(500m, Wrapper.ChargeLines[4].LocalAmount);
			AssertEquals("Test Line5", Wrapper.ChargeLines[4].Description);
			AssertEquals(600m, Wrapper.ChargeLines[5].LocalAmount);
			AssertEquals("Test Line6", Wrapper.ChargeLines[5].Description);
			AssertEquals(2500m, Wrapper.ChargeLines[6].LocalAmount);
			AssertEquals("其他費用", Wrapper.ChargeLines[6].Description);
		}

		public void TestCompanyPhoneAndFaxNumStr()
		{
			var address = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().First(x => x.OK_CustomsRegNo == "55667788").PremisesAddress;
			address.OA_Phone = "123456789";
			address.OA_Fax = "987654321";
			AssertEquals("TEL:123456789/FAX:+886 987 654 321", Wrapper.CompanyPhoneAndFaxNumStr);

			address.OA_Phone = "123456789";
			address.OA_Fax = ZString.Empty;
			AssertEquals("TEL:123456789", Wrapper.CompanyPhoneAndFaxNumStr);

			address.OA_Phone = ZString.Empty;
			address.OA_Fax = "987654321";
			AssertEquals("FAX:+886 987 654 321", Wrapper.CompanyPhoneAndFaxNumStr);
		}

		public void TestChargeLinesDescription()
		{
			AssertEquals("Test Line1\r\nTest Line2\r\nTest Line3\r\nTest Line4\r\nTest Line5\r\nTest Line6\r\n其他費用\r\n", Wrapper.ChargeLinesDescription);
		}

		public void TestChargeLinesAmount()
		{
			AssertEquals("100\r\n200\r\n300\r\n400\r\n500\r\n600\r\n2,500\r\n", Wrapper.ChargeLinesAmount);
		}

		public void TestSumAmountStr()
		{
			AssertEquals("4600", Wrapper.SumAmountStr); // Should exclude tax
		}

		[TestDate(2019, 2, 28)]
		public void TestBarCode()
		{
			var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			var book = Factory.NewWithValidTestData<AccComplianceSequence>();
			header.ADH_XD_ComplianceBook = book.PK;
			header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			header.ADH_DocumentNumber = "TX01234567";
			header.ADH_BarCode = "1234";
			book.XD_ExpiryDate = ZDateTime.Now;

			var wrapper = DocumentWrappers.TW.DocARComplianceDocument.New(header, Factory);
			var expect = "*10802TX01234567*";
			AssertEquals(expect, wrapper.BarCode);

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Category = "NAT";
			header.ADH_OH_Organisation = debtor.PK;

			wrapper = DocumentWrappers.TW.DocARComplianceDocument.New(header, Factory);
			expect = "*10802TX012345671234*";
			AssertEquals(expect, wrapper.BarCode);
		}

		public void TestQRCode()
		{
			AccountingMasterFilesRegistry.Instance.AESEncryptionKey.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "78D92C1FA999954120227B664B29FF93");

			var invoice = Header.TransactionHeaders[0];

			var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.TWD, 1m, invoice.Header, DocumentDate);
			arInvoice.AH_JH = invoice.AH_JH;
			arInvoice.AH_TransactionNum = "00000001";
			arInvoice.AH_TransactionReference = "10000001";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = "NAT";
			arInvoice.AH_OH = orgHeader.PK;

			var line1 = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "雞腿", 100m);
			line1.AL_AT = TestObjectCreator.GST1.PK;

			var expectQRCodeLeft = "TX0000000110707081234000000640000006E0000000055667788WX/WLpaaLgUdYp5MI+iNNg==:**********:1:1:1:雞腿:1:100:";
			var expectQRCodeRight = "**";
			AssertQRCode(arInvoice, expectQRCodeLeft, expectQRCodeRight);

			var line2 = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.CC2, TestObjectCreator.TWD, 1m, "可樂", 50m);
			line2.AL_AT = TestObjectCreator.GST1.PK;

			expectQRCodeLeft = "TX000000011070708123400000096000000A50000000055667788WX/WLpaaLgUdYp5MI+iNNg==:**********:2:2:1:雞腿:1:100:";
			expectQRCodeRight = "**可樂:1:50";
			AssertQRCode(arInvoice, expectQRCodeLeft, expectQRCodeRight);

			orgHeader.OH_Category = "BUS";
			expectQRCodeLeft = "TX000000011070708    00000096000000A50000000055667788lPCLB0s17Mc9ve/n6DSYBQ==:**********:2:2:1:雞腿:1:100:";
			AssertQRCode(arInvoice, expectQRCodeLeft, expectQRCodeRight);
		}

		void AssertQRCode(InvoicingBase invoice, string expectQRCodeLeft, string expectQRCodeRight)
		{
			Header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			Header.Delete();

			new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
			Header = Factory.LoadTop1<ARComplianceDocumentHeader>(new ZQuery());
			Header.ADH_DocumentDate = DocumentDate;
			Header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			Header.ADH_DocumentNumber = "TX00000001";
			Header.ADH_BarCode = "1234";

			var wrapper = GetDocumentWrappers()[0] as DocumentWrappers.TW.DocARComplianceDocument;
			AssertEquals(expectQRCodeLeft, wrapper.QRCodeLeft);
			AssertEquals(expectQRCodeRight, wrapper.QRCodeRight);
		}

		public void TestDebtorVATRegNumForCRD()
		{
			AssertEquals("11223344", Wrapper.DebtorVATRegNumForCRD);

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Category = "NAT";
			debtor.CustomsCodes.AddNew("VAT", "123123", "TW");
			var address = debtor.Addresses.AddNew();
			address.OA_CompanyNameOverride = "Test Debtor 2";
			address.OA_Address1 = "Test Address2";
			address.OA_Address2 = "Addtional Address2";
			address.OA_Code = "Test Debtor Address Code2";
			address.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			address.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);

			Header.ADH_OH_Organisation = debtor.PK;
			Header.ADH_OA_AddressOverride = address.PK;
			Header.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
			Factory.Save();

			AssertEquals(ZString.Empty, Wrapper.DebtorVATRegNumForCRD);

			debtor.OH_Category = "BUS";
			Factory.Save();

			AssertEquals("123123", Wrapper.DebtorVATRegNumForCRD);
		}

		[TestDate(2019, 1, 17)]
		public void TestOriginalGUIDocumentDate()
		{
			Assert(Wrapper.OriginalGUIDocumentDate.IsEmpty);

			var creditNoteDocumentHeader = (ARComplianceDocumentHeader)TestObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Des", "DEF");
			creditNoteDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			creditNoteDocumentHeader.ADH_DocumentNumber = "D00001";
			creditNoteDocumentHeader.ADH_ReportingPeriod = 201901;
			Factory.Save();

			var newWrapper = DocumentWrappers.TW.DocARComplianceDocument.New(creditNoteDocumentHeader, Factory);
			AssertNull(creditNoteDocumentHeader.INVComplianceDocumentHeaderForCRD);
			AssertEquals(ZDateTime.Empty, newWrapper.OriginalGUIDocumentDate);

			var invoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Des", "ABC");
			invoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_DocumentNumber = "D00001";
			invoiceDocumentHeader.ADH_ReportingPeriod = 201901;
			Factory.Save();

			newWrapper = DocumentWrappers.TW.DocARComplianceDocument.New(creditNoteDocumentHeader, Factory);
			AssertNotNull(creditNoteDocumentHeader.INVComplianceDocumentHeaderForCRD);
			AssertEquals(new ZDateTime(2019, 1, 17), newWrapper.OriginalGUIDocumentDate);
		}

		public override void TestInternalReference()
		{
			AssertEquals("2018070800001000", Wrapper.InternalReference);
		}

		DocumentWrappers.TW.DocARComplianceDocument Wrapper;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocumentWrappers.TW.DocARComplianceDocument.New(Header, Factory) };
		}

		protected override void SetUp()
		{
			base.SetUp();
			Wrapper = (DocumentWrappers.TW.DocARComplianceDocument)GetDocumentWrappers()[0];
		}
	}
}
