using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class DocSADH_DoNotInheritTest : DocumentWrappers.Testing.DocBaseWrapperTest
	{
		public void TestBox23ExchangeRate_MultiCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "GBP";
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "AUD";
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			CombineAssertions("Exchange rate when Entry Header is multi-currency", () =>
			{
				var wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("When no exchange rate has been provided, 1.000000 is expected", "1.000000", wrapper.Box23ExchangeRate);

				invoiceHeader2.JZ_InvoiceCurrExRate = 0.989888m;
				AssertEquals("Even when exchange rate has been provided, 1.000000 must be shown in the wrapper", "1.000000", wrapper.Box23ExchangeRate);
			});
		}

		public void TestBox23ExchangeRate_SingleCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			CombineAssertions("Exchange rate when Entry Header is single-currency", () =>
			{
				var wrapper = DocSADH.New(entryHeader, Factory);

				AssertEquals("When no currency has been provided, empty string is expected", string.Empty, wrapper.Box23ExchangeRate);

				invoiceHeader1.JZ_RX_NKInvoice_Currency = "GBP";
				invoiceHeader1.JZ_InvoiceCurrExRate = 0.989888m;
				invoiceHeader2.JZ_RX_NKInvoice_Currency = "GBP";
				invoiceHeader2.JZ_InvoiceCurrExRate = 0.989888m;
				AssertEquals("When the same exchange rate and currency have been provided to all invoice headers, the former must be shown in the wrapper", "0.989888", wrapper.Box23ExchangeRate);
			});
		}

		[TestDate(2008, 11, 11)]
		public void TestBox54Details_Box54Date()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("entryHeader.Box54Date", "11-Nov-08", wrapper.Box54Date);
		}

		public void TestShowEpuEnoDoeLabelsText()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Print EPU, ENO, DOE Labels", true, wrapper.ShowEpuEnoDoeLabelsText);
		}

		public void TestShowEpuEnoDoeLabelsTextOnBIS()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("On BIS pages, EPU, ENO, DOE row always visible in EU", true, wrapper.ShowEpuEnoDoeLabelsTextOnBIS);
		}

		public void TestBox18IdentityOfTransportAtDeparture()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			declaration.ZG_Box18TransportID = "RAIL 1234";
			AssertEquals("declaration.ZG_Box18TransportID", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);
		}

		public void TestBoxS29TransportChargesMoP()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "A";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var wrapper = DocSADH.New(entryHeader, Factory);

			CombineAssertions("Whatever the declaration type, Method of payment must be EUC_TransportChargesMethodOfPayment", () =>
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("A", wrapper.BoxS29TransportChargesMoP);

				declaration.JE_MessageType = "IMP";
				AssertEquals("A", wrapper.BoxS29TransportChargesMoP);
			});
		}

		public void TestBox7ReferenceNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OwnerRef = "WHATEVER";
			declaration.JE_DeclarationReference = "FOREVER";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			CombineAssertions("Box7ReferenceNumber depending on JE_OwnerRef and JE_DeclarationReference", () =>
			{
				AssertEquals("When available, JE_OwnerRef is used as Box7ReferenceNumber", "WHATEVER", wrapper.Box7ReferenceNumber);

				declaration.JE_OwnerRef = ZString.Empty;
				AssertEquals("When JE_OwnerRef is not available, JE_DeclarationReference is used as Box7ReferenceNumber", "FOREVER", wrapper.Box7ReferenceNumber);
			});
		}

		public void TestBox29ExitOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT018100", "Bari", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "IT017000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IT018100");

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Box29ExitOffice Export case", "IT018100 Bari", wrapper.Box29ExitOffice);
		}

		public void TestSheetName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("When no mrn and no CH_BGMReference", ZString.Empty, wrapper.SheetName);

				entryHeader.CH_BGMReference = "Reference";
				wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("When no mrn and CH_BGMReference is not empty", "Reference", wrapper.SheetName);

				entryHeader.MovementReferenceNumberSetter("MRNCode");
				wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("When mrn is not empty", "MRNCode", wrapper.SheetName);
			});
		}

		public void TestAgentsReference()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();
			AssertEquals(ZString.Empty, documentWrapper.AgentsReference);
		}

		public void TestBox16CountryOfOrigin()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Empty value for Country Of Origin", ZString.Empty, wrapper.Box16CountryOfOrigin);
		}

		public void TestBox17bImporterState()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("Province of Destination", ZString.Empty, wrapper.Box17ImporterState);
		}

		public void TestBox20ShipmentIncoTermAndAgreedPlaceAndCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions("All empty", () =>
			{
				var wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("Box20ShipmentIncoTerm", "", wrapper.Box20ShipmentIncoTerm);
				AssertEquals("Box20AgreedPlace", "", wrapper.Box20AgreedPlace);
				AssertEquals("Box20AgreedPlaceCode", "", wrapper.Box20AgreedPlaceCode);
				AssertEquals("Box20AgreedPlaceCode2 last box", ZString.Empty, wrapper.Box20AgreedPlaceCode2);
			});

			declaration.JE_ShipmentIncoTerm = "DAP";
			declaration.JE_ShipmentIncoTermPlace = "SYDNEY";
			declaration.ZG_AgreedPlaceCode = "3";

			CombineAssertions("Declaration set up", () =>
			{
				var wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("Box20ShipmentIncoTerm", "DAP", wrapper.Box20ShipmentIncoTerm);
				AssertEquals("Box20AgreedPlace", "SYDNEY", wrapper.Box20AgreedPlace);
				AssertEquals("Box20AgreedPlaceCode", "", wrapper.Box20AgreedPlaceCode);
				AssertEquals("Box20AgreedPlaceCode2 last box", "3", wrapper.Box20AgreedPlaceCode2);
			});

			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_IncoTermPlace = "TARANTO";
			invoiceHeader.ZG_AgreedPlaceCode = "1";

			CombineAssertions("Invoice Header set up", () =>
			{
				var wrapper = DocSADH.New(entryHeader, Factory);
				AssertEquals("Box20ShipmentIncoTerm", "FOB", wrapper.Box20ShipmentIncoTerm);
				AssertEquals("Box20AgreedPlace", "TARANTO", wrapper.Box20AgreedPlace);
				AssertEquals("Box20AgreedPlaceCode", ZString.Empty, wrapper.Box20AgreedPlaceCode);
				AssertEquals("Box20AgreedPlaceCode2 last box", "1", wrapper.Box20AgreedPlaceCode2);
			});
		}

		public void TestBox30LocationOfGoods()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryNumber number = Factory.New<CusEntryNumber>();
			number.CE_IssueDate = new ZDateTime(1987, 12, 11);
			number.CE_ParentID = entryHeader.PK;
			number.CE_ParentTable = CusEntryHeader.Schema.TableName;
			number.CE_EntryType = "EXP";

			AssertBox30LocationOfGoods(declaration, entryHeader, "", "", "");
			AssertBox30LocationOfGoods(declaration, entryHeader, "LHR", "", "LVLHRLHR");
			AssertBox30LocationOfGoods(declaration, entryHeader, "LHR", "BAC", "LVLHRLHRBAC");
		}

		void AssertBox30LocationOfGoods(JobDeclaration declaration, CusEntryHeader entryHeader, ZString locationOfGoods, ZString shedCode, ZString expectedAssertion)
		{
			declaration.JE_LocationOfGoods = locationOfGoods;
			declaration.SubLocation = shedCode;
			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals(expectedAssertion, wrapper.Box30LocationOfGoods);
		}

		public void TestBoxDControlResult()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();
			AssertEquals(ZString.Empty, documentWrapper.BoxDControlResult);
		}

		public void TestBoxDSignature()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();
			AssertEquals(ZString.Empty, documentWrapper.BoxDSignature);
		}

		public void TestLayoutStyle()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();

			CombineAssertions("Test all LayoutStyle properties for EU", () =>
			{
				AssertEquals("LayoutStyle2", "6", documentWrapper.LayoutStyle2);
				AssertEquals("LayoutStyle2Description", "Copy for the country of destination", documentWrapper.LayoutStyle2Description);
			});
		}

		public void TestBoxABarcode()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();
			AssertEquals($"{nameof(DocSADH.BoxABarcode)} must be empty for EU", "", documentWrapper.BoxABarcode);
		}

		public void TestBoxAType()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();
			AssertEquals($"{nameof(DocSADH.BoxAType)} must be empty for EU", "", documentWrapper.BoxAType);
		}

		public void TestBoxBAccountingDetails()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();
			AssertEquals($"{nameof(DocSADH.BoxBAccountingDetails)} must be empty for EU", "", documentWrapper.BoxBAccountingDetails);
		}

		public void TestBoxCOfficeOfDeparture()
		{
			var documentWrapper = (DocSADH)GetNewDocumentWrapper();
			AssertEquals($"{nameof(DocSADH.BoxCOfficeOfDeparture)} must be empty for EU", "", documentWrapper.BoxCOfficeOfDeparture);
		}

		public void TestIssuingDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var number = Factory.New<CusEntryNumber>();
			number.CE_IssueDate = new ZDateTime(1987, 12, 11);
			number.CE_ParentID = entryHeader.PK;
			number.CE_ParentTable = CusEntryHeader.Schema.TableName;
			number.CE_EntryType = "EXP";

			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertEquals("19871211", wrapper.IssuingDate);
		}

		public void TestShouldShowNotInEcsCaptionIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals(nameof(DocSADH.ShouldShowNotInEcsCaptionIfNeeded), ZBool.True, wrapper.ShouldShowNotInEcsCaptionIfNeeded);
		}

		public void TestBox29ExitOfficeLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals(nameof(DocSADH.Box29ExitOfficeLabel), "29 Office of exit/entry", wrapper.Box29ExitOfficeLabel);
		}

		public void TestBoxS32SpecificCircumstanceIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals(nameof(DocSADH.BoxS32SpecificCircumstanceIndicator), "", wrapper.BoxS32SpecificCircumstanceIndicator);
		}

		public void TestBoxS00SafetyAndSecurity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);

			AssertEquals(nameof(DocSADH.BoxS00SafeAndSecurity), ZBool.False, wrapper.BoxS00SafeAndSecurity);
		}

		public void TestBisPageTraderBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = DocSADH.New(entryHeader, Factory);
			AssertType<ImporterBisPageTraderBox>("BisPageTraderBox Type", wrapper.BisPageTraderBox);
		}

		public void TestBoxC()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var docSadH = DocSADH.New(entryHeader, Factory);
			AssertEquals(nameof(docSadH.BoxC), "", docSadH.BoxC);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return DocSADH.New(entryHeader, Factory);
		}
	}
}
