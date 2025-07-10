using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee;
using EUUniversalReferenceConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		public void TestNew()
		{
			AssertNull(DocCusEntryHeader.New(null, Factory));
		}

		public void TestMergedLines()
		{
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();
			AssertEquals(2, Wrapper.MergedLines.Count);
		}

		public void TestPreviousDocuments()
		{
			var pd1 = entryInstruction.PreviousDocuments.AddNew();
			pd1.CSI_Code = "ABC";
			entryInstruction.PreviousDocuments.AddNew();
			AssertEquals("empty documents should not show", 1, Wrapper.PreviousDocuments.Count);
		}

		public void TestSupportingDocuments()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var doc1 = invoice1.SupportingDocuments.AddNew();
			doc1.CSI_Code = "1";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var doc2 = invoice2.SupportingDocuments.AddNew();
			doc2.CSI_Code = "2";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			var doc3 = invoice3.SupportingDocuments.AddNew();
			doc3.CSI_Code = "3";
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction2.PK;

			AssertContainsExactElementsInAnyOrder("LineNumbers", new[] { "1", "2" }, Wrapper.SupportingDocuments.Cast<DocSupportingDocument>().Select(x => x.Code));
		}

		public void TestFees()
		{
			CreateFees();
			declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			var wrappedFees = Wrapper.Fees.Cast<DocEntryHeaderFee>();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 3, wrappedFees.Count());
				var a00 = wrappedFees.SingleOrDefault(x => x.ChargeType == "A00");
				AssertEquals("A00: Amount", "3,79", a00.ChargeAmountAsString);
				var b00 = wrappedFees.SingleOrDefault(x => x.ChargeType == "B00");
				AssertEquals("B00: Amount", "1,26", b00.ChargeAmountAsString);
				var c00 = wrappedFees.SingleOrDefault(x => x.ChargeType == "C00");
				AssertEquals("C00: Amount", "1,23", c00.ChargeAmountAsString);
			});
		}

		public void TestFees_Description()
		{
			const string ExpectedB00GermanDescription = "Einfuhrumsatzsteuer (EUSt)";

			CreateFees();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("DE", "German");
			var rateCodeLanguage1 = helper.LoadOrCreateNewCusRateCodeLanguage(Factory, "A00", "DE");
			rateCodeLanguage1.ZXC_Description = "DE Description1";
			var rateCodeLanguage3 = helper.LoadOrCreateNewCusRateCodeLanguage(Factory, "C00", "DE");
			rateCodeLanguage3.ZXC_Description = "DE Description3";
			Factory.Save();

			declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			var wrappedFees = Wrapper.Fees.Cast<DocEntryHeaderFee>().ToArray();
			CombineAssertions("Description must be always in german", () =>
			{
				AssertEquals("Precondition", 3, wrappedFees.Length);
				var a00 = wrappedFees.Single(x => x.ChargeType == "A00");
				AssertEquals("A00", "DE Description1", a00.Description);
				var b00 = wrappedFees.Single(x => x.ChargeType == "B00");
				AssertEquals("B00", ExpectedB00GermanDescription, b00.Description);
				var c00 = wrappedFees.Single(x => x.ChargeType == "C00");
				AssertEquals("C00", "DE Description3", c00.Description);
			});
		}

		public void TestLocalReferenceNumber()
		{
			entryHeader.LocalReferenceNumber = "LOCALREFERENCENUMBER";
			AssertEquals("LOCALREFERENCENUMBER", Wrapper.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			entryHeader.MovementReferenceNumberSetter("MRN123");
			AssertEquals("MRN123", Wrapper.MovementReferenceNumber);
		}

		public void TestResponsiblePerson()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_SystemLastEditUser = "KCH";
				AssertEquals("No GlbStaff", "n/a", Wrapper.ResponsiblePerson);

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "KCH";
				staff.GS_FullName = "Karim Chelmouni";

				AssertEquals("GlbStaff exists", "Karim Chelmouni", Wrapper.ResponsiblePerson);
			});
		}

		public void TestEntryStyle()
		{
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			AssertEquals("IM", Wrapper.EntryStyle);
		}

		public void TestSender()
		{
			declaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
			AssertNotNull(Wrapper.Sender);
		}

		public void TestRecipient()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
				AssertNotNull(Wrapper.Recipient);
			}
		}

		public void TestRecipientEORIDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				declaration.JE_OH_Importer = testOrgAddress.Header.PK;
				AssertEquals("(GREOR1 EBS1)", Wrapper.RecipientEORIDetails);
			}
		}

		public void TestDeclarant()
		{
			declaration.JE_OA_DeclarantAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertNotNull(Wrapper.Declarant);
		}

		public void TestDeclarantEORIDetails()
		{
			declaration.JE_OA_DeclarantAddress = testOrgAddress.PK;
			AssertEquals("(GREOR1 EBS1)", Wrapper.DeclarantEORIDetails);
		}

		public void TestRepresentative()
		{
			var representativeAddress = Factory.New<OrgHeader>().MainAddress;
			representativeAddress.Address1 = "RepresentativeStreet";
			var buyingAgentAddress = Factory.New<OrgHeader>().MainAddress;
			buyingAgentAddress.Address1 = "BuyingAgentStreet";
			declaration.JE_OA_Representative = representativeAddress.PK;
			declaration.JE_OA_BuyingAgentAddress = buyingAgentAddress.PK;
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = ZString.Empty;
				AssertNull("No DeclarantType", Wrapper.Representative);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("DeclarantType 'DIR'", "RepresentativeStreet", Wrapper.Representative.Address1);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("DeclarantType 'IND'", "BuyingAgentStreet", Wrapper.Representative.Address1);
			});
		}

		public void TestRepresentativeEORIDetails_DIR()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("JE_OA_Representative empty", Wrapper.RepresentativeEORIDetails);

				declaration.JE_OA_Representative = testOrgAddress.PK;
				AssertEquals("(GREOR1 EBS1)", Wrapper.RepresentativeEORIDetails);
				AssertEquals("N", Wrapper.IsRepresented);
			});
		}

		public void TestRepresentativeEORIDetails_IND()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("JE_OA_BuyingAgentAddress empty", Wrapper.RepresentativeEORIDetails);

				declaration.JE_OA_BuyingAgentAddress = testOrgAddress.PK;
				AssertEquals("(GREOR1 EBS1)", Wrapper.RepresentativeEORIDetails);
				AssertEquals("Y", Wrapper.IsRepresented);
			});
		}

		public void TestDepartureCountry()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("GoodsOrigin empty", Wrapper.DepartureCountry);

				var helper = new UniversalReferenceTestDataHelper(Factory);
				var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

				helper.CreateNewOrGetExistingCusCodeType(EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "Origin country/territory for entry style IM");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "DE", "Test DE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

				helper.CreateNewOrGetExistingCusCodeType(EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "Origin country/territory for entry style EX");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "FR", "Test FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
				Factory.Save();

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;

				declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Germany;
				AssertEquals("GoodsOrigin 'DE'", "DE Germany", Wrapper.DepartureCountry);
			});
		}

		public void TestBorderMOT()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.ZG_BorderTransportMeans = ImportBorderTransportMeansList.Codes.Vessel;
			AssertEquals("02 Vessel", Wrapper.BorderMOT);
		}

		public void TestBorderTransportMeansNationality()
		{
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Germany;
			AssertEquals("DE", Wrapper.BorderTransportMeansNationality);
		}

		public void TestTransportIDInland()
		{
			declaration.ZG_Box18TransportID = "WI ZG 1234";
			AssertEquals("WI ZG 1234", Wrapper.TransportIDInland);
		}

		public void TestIncoterm()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("No InvoiceHeader", Wrapper.Incoterm);

				var invoiceHeader = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader.JZ_IncoTerm = IncotermA1840CodeList.Codes.EXW;
				invoiceHeader.JZ_IncoTermPlace = "Wiesbaden";
				AssertEquals("EXW Ex Works - Wiesbaden", Wrapper.Incoterm);
			});
		}

		public void TestTransactionNature()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TranNature");

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"01", "DESC1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"02", "DESC2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.C0091, RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			var invoiceHeader = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
			invoiceHeader.JZ_ValuationCode = "01";
			AssertEquals("01 DESC1", Wrapper.TransactionNature);
		}

		public void TestTotalInvoiceAmount()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No invoices", "0,00", Wrapper.TotalInvoiceAmount);

				var invoiceHeader1 = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader1.JZ_InvoiceAmount = 11m;
				var invoiceHeader2 = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader2.JZ_InvoiceAmount = 22m;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader3 = declaration.Invoices.AddNew();
				var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
				invoiceLine3.JI_CEI = entryInstruction2.PK;
				invoiceHeader3.JZ_InvoiceAmount = 44m;
				AssertEquals("With invoices", "33,00", Wrapper.TotalInvoiceAmount);
			});
		}

		public void TestTotalInvoiceAmountCurrency()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No invoices", ZString.Empty, Wrapper.TotalInvoiceAmountCurrency);

				var invoiceHeader = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("With invoices", "EUR", Wrapper.TotalInvoiceAmountCurrency);
			});
		}

		public void TestTotalInvoiceAmountEURValue()
		{
			CombineAssertions(() =>
			{
				var invoiceHeader = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader.JZ_InvoiceAmount = 1.234m;
				AssertEquals("ExchangeRate == 0", "0,00", Wrapper.TotalInvoiceAmountEURValue);

				invoiceHeader.JZ_InvoiceCurrExRate = 1.2345678m;
				AssertEquals("Rate > 0", "1,00", Wrapper.TotalInvoiceAmountEURValue);
			});
		}

		public void TestExchangeRate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No invoices", "0,000000", Wrapper.ExchangeRate);

				var invoiceHeader = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader.JZ_InvoiceCurrExRate = 1.2345678m;
				AssertEquals("With invoice", "1,234568", Wrapper.ExchangeRate);
			});
		}

		public void TestPaymentNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No invoices", ZString.Empty, Wrapper.PaymentNumber);

				var invoiceHeader = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader.JZ_PaymentNo = "12345";
				AssertEquals("With invoice", "12345", Wrapper.PaymentNumber);
			});
		}

		public void TestValuationDateOverride()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No invoices", ZDateTime.Empty, Wrapper.ValuationDateOverride);

				var invoiceHeader = CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
				invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2022, 2, 15);
				AssertEquals("With invoice", new ZDateTime(2022, 2, 15), Wrapper.ValuationDateOverride);
			});
		}

		public void TestTotalChargesAmount()
		{
			CreateFees();
			AssertEquals("6,28", Wrapper.TotalChargesAmount);
		}

		public void TestTotalChargesAmountFinal()
		{
			CreateFees();
			CreateConfirmedFees();
			AssertEquals("11,28", Wrapper.TotalChargesAmount);
		}

		public void TestIsFinalTaxReport_Yes()
		{
			CreateFees();
			CreateConfirmedFees();
			AssertEquals("final", YesNoList.Codes.Yes, Wrapper.IsFinalTaxReport);
		}

		public void TestIsFinalTaxReport_No()
		{
			CreateFees();
			AssertEquals("not final", YesNoList.Codes.No, Wrapper.IsFinalTaxReport);
		}

		public void TestCustomsOffice123()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeDE000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, DocCusEntryHeader.CustomsOfficeAttributes.PostCode, "12345");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, DocCusEntryHeader.CustomsOfficeAttributes.City, "Berlin");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeDE000001.PK, DocCusEntryHeader.CustomsOfficeAttributes.Street, "Hintergasse 13");
			Factory.Save();

			declaration.JE_CustomsOffice = "DE000001";

			CombineAssertions(() =>
			{
				AssertEquals("DE000001 - Central Community Transit Office", Wrapper.CustomsOfficeIdentification);
				AssertEquals("Hintergasse 13", Wrapper.CustomsOfficeStreet);
				AssertEquals("12345 Berlin", Wrapper.CustomsOfficePostCodeAndCity);
			});
		}

		public void TestCustomsOffice123_notSet()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, Wrapper.CustomsOfficeIdentification);
				AssertEquals(ZString.Empty, Wrapper.CustomsOfficeStreet);
				AssertEquals(ZString.Empty, Wrapper.CustomsOfficePostCodeAndCity);
			});
		}

		public void TestContainerNumbers()
		{
			SetupContainersForContainerIdentificationNumbers();
			CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();

			AssertEquals("CON1; CON2", Wrapper.ContainerNumbers);
		}

		public void TestContainerNumbers_NonContainerized()
		{
			SetupContainersForContainerIdentificationNumbers();
			CreateInvoiceHeaderWithLineAndLinkToEntryInstruction();
			declaration.JE_ContainerMode = "NCT";

			AssertEquals(string.Empty, Wrapper.ContainerNumbers);
		}

		public void TestDefermentAccounts()
		{
			SetupDefermentAccount();
			AssertEquals(1, Wrapper.DefermentAccounts.Count);
		}

		void SetupDefermentAccount()
		{
			declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			declaration.ZG_MethodOfPayment = MethodOfPaymentTypes.E;
			var orgCusCode = declaration.Declarant.Header.DefermentAccountNumberCollection.AddNew();
			orgCusCode.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			orgCusCode.CZ_Account = "123456";
			declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;
			declaration.JE_DefermentAccountNumber = "123456";
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.Germany;

		protected override CusEntryHeader GetNewEntryHeader()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_CustomsOffice = "DE00001";
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			}
			return entryHeader;
		}

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			return DocCusEntryHeader.New(entryHeader, Factory);
		}

		DocCusEntryHeader Wrapper => EntryHeaderWrapperInternal;

		protected override void SetUp()
		{
			base.SetUp();
			CreateCL010CoutryList();
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryInstruction entryInstruction;
		CusEntryLine entryLine1;
		CusEntryLine entryLine2;
		CusEntryLine entryLine3;

		void CreateFees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunId);
			helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var dut = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var @int = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Interest, "Interest");
			var msc = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Miscellaneous, "Miscellaneous");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "C00", msc.PK);
			Factory.Save();

			entryLine1 = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var fee1_1 = entryLine1.Fees.AddNew();
			fee1_1.CF_ChargeType = "A00";
			fee1_1.CF_ChargeAmount = 1.234567m;
			var fee1_2 = entryLine1.Fees.AddNew();
			fee1_2.CF_ChargeType = "A00";
			fee1_2.CF_ChargeAmount = 1.29876m;
			var fee1_3 = entryLine1.Fees.AddNew();
			fee1_3.CF_ChargeType = "B00";
			fee1_3.CF_ChargeAmount = 1.2589745m;

			entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var fee2_1 = entryLine2.Fees.AddNew();
			fee2_1.CF_ChargeType = "A00";
			fee2_1.CF_ChargeAmount = 1.2555555m;

			entryLine3 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			var fee3_1 = entryLine3.Fees.AddNew();
			fee3_1.CF_ChargeType = "C00";
			fee3_1.CF_ChargeAmount = 1.2333333m;
		}

		void CreateConfirmedFees()
		{
			var fee1_1 = Factory.New<CusEntryLineFee>();
			fee1_1.CF_ChargeType = "A00";
			fee1_1.CF_ChargeAmount = 2.234567m;
			entryLine1.ConfirmedFees.Add(fee1_1);
			var fee1_2 = Factory.New<CusEntryLineFee>();
			fee1_2.CF_ChargeType = "A00";
			fee1_2.CF_ChargeAmount = 2.29876m;
			entryLine1.ConfirmedFees.Add(fee1_2);
			var fee1_3 = Factory.New<CusEntryLineFee>();
			fee1_3.CF_ChargeType = "B00";
			fee1_3.CF_ChargeAmount = 2.2589745m;
			entryLine1.ConfirmedFees.Add(fee1_3);

			var fee2_1 = Factory.New<CusEntryLineFee>();
			fee2_1.CF_ChargeType = "A00";
			fee2_1.CF_ChargeAmount = 2.2555555m;
			entryLine2.ConfirmedFees.Add(fee2_1);

			var fee3_1 = Factory.New<CusEntryLineFee>();
			fee3_1.CF_ChargeType = "C00";
			fee3_1.CF_ChargeAmount = 2.2333333m;
			entryLine2.ConfirmedFees.Add(fee3_1);
		}

		OrgAddress testOrgAddress
		{
			get
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Greece);
				var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "EBS1", Core.Constants.CountryCodes.Germany);
				ebsCode.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
				return orgHeader.MainAddress;
			}
		}

		JobComInvoiceHeader CreateInvoiceHeaderWithLineAndLinkToEntryInstruction()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			return invoiceHeader;
		}

		void SetupContainersForContainerIdentificationNumbers()
		{
			var cusContainer1 = declaration.CusContainers.AddNew();
			cusContainer1.CO_ContainerNumber = "CON1";
			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "CON2";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CON3";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.ContainersPivot.AddPivotFor(cusContainer1);
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddPivotFor(cusContainer2);
		}
		void CreateCL010CoutryList()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN;
			helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingCusCodeType(EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "European Countries Of Destination");
			helper.CreateCusCodeList(eunCode, EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Greece, Core.Constants.CountryCodes.Greece, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Italy, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.Netherlands, Core.Constants.CountryCodes.Netherlands, startDate, endDate);
			helper.CreateCusCodeList(eunCode, EUUniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France, startDate, endDate);
			Factory.Save();
		}
	}
}
