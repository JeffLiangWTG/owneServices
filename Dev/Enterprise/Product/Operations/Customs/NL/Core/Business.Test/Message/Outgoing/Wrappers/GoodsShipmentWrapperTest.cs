using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class GoodsShipmentWrapperTest : DataProviderTestCase<GoodsShipmentWrapper>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("entry header null", () => new GoodsShipmentWrapper(null, new JobDeclarationMessageSendingObject(entryHeader)));
		AssertExceptionThrown<ArgumentNullException>("message sending object null", () => new GoodsShipmentWrapper(entryHeader, null));
	});

	public void TestSequenceNumeric()
	{
		AssertEquals(1, wrapper.SequenceNumeric);
	}

	public void TestTransactionNatureCode_NoTP_Import()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DMSTransitionPeriod, Core.Constants.CountryCodes.Netherlands, ZDate.Today, false))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.H1;
			invoice.JZ_ValuationCode = "10";
			invoiceLine.ZG_TransNature = "11";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "12";
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.ZG_TransNature = "13";

			CombineAssertions(() =>
			{
				entryHeader.EntryInstruction.ZG_TransNature = "20";
				AssertEquals("Transaction Nature filled on EntryInstruction", "20", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, take from InvoiceHeader (multiple values on InvoiceHeaders), default from entryInstruction", "20", wrapper.TransactionNatureCode);

				invoiceLine3.ZG_TransNature = "";
				invoice2.JZ_ValuationCode = "10";
				AssertEquals("No TransactionNature on InvoiceLine, TransactionNature on InvoiceHeaders are the same", "10", wrapper.TransactionNatureCode);

				declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.H2;
				AssertNullOrEmpty("DeclarationType is not H1, H4, H5, H6 or I1", wrapper.TransactionNatureCode);
			});
		}
	}

	public void TestTransactionNatureCode_NoTP_Export()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Netherlands, ZDate.Today, false))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.B1;
			invoice.JZ_ValuationCode = "10";
			invoiceLine.ZG_TransNature = "11";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "12";
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.ZG_TransNature = "13";

			CombineAssertions(() =>
			{
				entryHeader.EntryInstruction.ZG_TransNature = "20";
				AssertEquals("Transaction Nature filled on EntryInstruction", "20", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, take from InvoiceHeader (multiple values on InvoiceHeaders), default from entryInstruction", "20", wrapper.TransactionNatureCode);

				invoiceLine3.ZG_TransNature = "";
				invoice2.JZ_ValuationCode = "10";
				AssertEquals("No TransactionNature on InvoiceLine, TransactionNature on InvoiceHeaders are the same", "10", wrapper.TransactionNatureCode);

				declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.B3;
				AssertNullOrEmpty("DeclarationType is not B1, B2, B4 or C1", wrapper.TransactionNatureCode);
			});
		}
	}

	public void TestTransactionNature_TP_Import()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DMSTransitionPeriod, Core.Constants.CountryCodes.Netherlands, ZDate.Today, true))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.H1;
			invoice.JZ_ValuationCode = "10";
			invoiceLine.ZG_TransNature = "11";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "12";

			CombineAssertions(() =>
			{
				entryHeader.EntryInstruction.ZG_TransNature = "20";
				AssertEquals("Transaction Nature filled on EntryInstruction", "20", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, multiple values on InvoiceHeaders", "20", wrapper.TransactionNatureCode);

				invoice2.JZ_ValuationCode = "10";
				AssertEquals("No TransactionNature on InvoiceLine, TransactionNature on InvoiceHeaders are the same", "20", wrapper.TransactionNatureCode);

				declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.H2;
				AssertNullOrEmpty("DeclarationType is not H1, H4, H5, H6 or I1", wrapper.TransactionNatureCode);
			});
		}
	}

	public void TestTransactionNature_TP_Export()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Netherlands, ZDate.Today, true))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.B1;
			invoice.JZ_ValuationCode = "10";
			invoiceLine.ZG_TransNature = "11";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "12";
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.ZG_TransNature = "13";

			CombineAssertions(() =>
			{
				entryHeader.EntryInstruction.ZG_TransNature = "20";
				AssertEquals("Transaction Nature filled on EntryInstruction", "20", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, multiple values on InvoiceHeaders", "20", wrapper.TransactionNatureCode);

				invoiceLine3.ZG_TransNature = "";
				invoice2.JZ_ValuationCode = "10";
				AssertEquals("No TransactionNature on InvoiceLine, TransactionNature on InvoiceHeaders are the same", "20", wrapper.TransactionNatureCode);

				declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.B3;
				AssertNullOrEmpty("DeclarationType is not B1, B2, B4 or C1", wrapper.TransactionNatureCode);
			});
		}
	}

	public void TestDispatchCountryCode()
	{
		var declaration = entryHeader.Declaration;
		declaration.JE_RL_NKOrigin = "USHOU";
		entryHeader.Declaration.CustomsEntryInstructions.FirstOrDefault().CEI_Style = "H1";
		AssertEquals("US", wrapper.DispatchCountryCode);
	}

	public void TestInvoiceAmount()
	{
		invoice.JZ_InvoiceAmount = 101.20;
		AssertEquals(Decimal.Parse("101.20"), wrapper.InvoiceAmount);
	}

	public void TestInvoiceCurrencyCode()
	{
		entryHeader.RandomHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		AssertEquals("EUR", wrapper.InvoiceCurrencyCode);
	}

	public void TestAcceptanceDateTime()
	{
		entryHeader.FallbackEntryNumberIssueDate = new ZDateTime(2025, 05, 24, 15, 21, 00);
		entryHeader.MovementReferenceNumberSetter("MRN123", new ZDateTime(2021, 09, 24, 15, 21, 00));
		entryHeader.DMSFallbackIsActive = false;
		AssertEquals("20210924", wrapper.AcceptanceDateTime);
		entryHeader.DMSFallbackIsActive = true;
		AssertEquals("20250524", wrapper.AcceptanceDateTime);
	}

	public void TestAEOMutualRecognitionParties()
	{
		CombineAssertions(() =>
		{
			var cusRef1 = entryHeader.EntryInstruction.CusSupplyChainActorReferences.AddNew();
			var cusRef2 = entryHeader.EntryInstruction.CusSupplyChainActorReferences.AddNew();
			AssertNotNull(wrapper.AEOMutualRecognitionParties.FirstOrDefault());
			AssertType<AEOMutualRecognitionPartyWrapper>(wrapper.AEOMutualRecognitionParties.FirstOrDefault());
			AssertNotNull(wrapper.AEOMutualRecognitionParties);
		});
	}

	public void TestBuyer()
	{
		CombineAssertions(() =>
		{
			var orgHeaderBuyer = WrapperTestHelper.CreateOrgHeader(Factory, "Buyer Full Name 22", "Buye", "651321");
			entryHeader.Declaration.ConsigneeAddressOrgPK = orgHeaderBuyer.PK;
			var invoiceLines = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>();
			invoiceLines.ForEach(x => x.BuyerDocAddress.OrganisationPK = orgHeaderBuyer.PK);

			entryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H1;
			AssertType<PartyWrapper>("Type", wrapper.Buyer);
			AssertNotNull(wrapper.Buyer);

			var invoiceLine = entryHeader.InvoiceLines.First() as JobComInvoiceLine;
			invoiceLine.BuyerDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertNull("Line buyers are different from declaration", wrapper.Buyer);

			invoiceLine.BuyerDocAddress.OrganisationPK = orgHeaderBuyer.PK;
			entryHeader.Declaration.ConsigneeAddressOrgPK = ZGuid.Empty;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertNull("declaration.ConsigneeOrgAddress is null", wrapper.Buyer);

			entryHeader.Declaration.ConsigneeAddressOrgPK = orgHeaderBuyer.PK;
			entryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.I1;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertType<PartyWrapper>("CEI_Style is I1", wrapper.Buyer);

			entryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H2;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertNull("CEI_Style isn't H1 or I1", wrapper.Buyer);
		});
	}

	public void TestAdditionalReferences()
	{
		CombineAssertions(() =>
		{
			var addInfo1 = entryHeader.EntryInstruction.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "REF";
			var addInfo2 = entryHeader.EntryInstruction.AdditionalInfos.AddNew();
			addInfo2.CSI_SubType = "REF";
			AssertNotNull(wrapper.AdditionalReferences.FirstOrDefault());
			AssertType<AdditionalReferenceWrapper>(wrapper.AdditionalReferences.FirstOrDefault());
		});
	}

	public void TestAdditionalInformations()
	{
		CombineAssertions(() =>
		{
			var addInfo1 = entryHeader.EntryInstruction.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "INF";
			var addInfo2 = entryHeader.EntryInstruction.AdditionalInfos.AddNew();
			addInfo2.CSI_SubType = "INF";
			AssertNotNull(wrapper.AdditionalInformations.FirstOrDefault());
			AssertType<AdditionalInformationWrapper>(wrapper.AdditionalInformations.FirstOrDefault());
		});
	}

	public void TestConsignment()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.Consignment);
			AssertType<ConsignmentWrapper>(wrapper.Consignment);
		});
	}

	public void TestDestination()
	{
		var declaration = entryHeader.Declaration;
		var instruction = entryHeader.EntryInstruction;
		declaration.JE_RL_NKFinalDestination = "NLDRE";
		instruction.CEI_Style = NLConstants.EntryStyles.ExportReExport;
		entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ZG_CountryOfDestination = "NL");

		CombineAssertions(() =>
		{
			var destination = wrapper.Destination;
			AssertNotNull(destination);
			AssertType<DestinationWrapper>(destination);

			entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault().ZG_CountryOfDestination = "SB";
			destination = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader)).Destination;
			AssertNull(destination);
		});
	}

	public void TestDomesticDutyTaxParties()
	{
		CombineAssertions(() =>
		{
			var fiscalRef1 = entryHeader.EntryInstruction.FiscalReferences.AddNew();
			fiscalRef1.CFR_Reference = "Fiscal Reference";
			var fiscalRef2 = entryHeader.EntryInstruction.FiscalReferences.AddNew();
			fiscalRef2.CFR_Reference = "Fiscal Reference 2";

			AssertNotNull(wrapper.DomesticDutyTaxParties.FirstOrDefault());
			AssertType<DomesticDutyTaxPartyWrapper>(wrapper.DomesticDutyTaxParties.FirstOrDefault());
			AssertEquals("Number of DomesticDutyTaxParties", 2, wrapper.DomesticDutyTaxParties.Count);
			AssertEquals("ID of 2nd DomesticDutyTaxParty", "Fiscal Reference 2", wrapper.DomesticDutyTaxParties.ElementAt(1).Id);
		});
	}

	public void TestExportCountry() => CombineAssertions(() =>
	{
		var invoiceLine2 = entryHeader.Declaration.InvoiceLines.AddNew();

		void TestCase(string shipmentExportCountry, string line1ExportCountry, string line2ExportCountry, string expectedExportCountry)
		{
			declaration.JE_RL_NKOrigin = shipmentExportCountry;
			invoiceLine.JI_RN_NKCountryOfExport = line1ExportCountry;
			invoiceLine2.JI_RN_NKCountryOfExport = line2ExportCountry;
			AssertEquals($"shipment: '{shipmentExportCountry}', line1: '{line1ExportCountry}', line2: '{line2ExportCountry}'", expectedExportCountry, wrapper.ExportCountry);
		}

		TestCase(string.Empty, "DE", "NL", string.Empty);
		TestCase(string.Empty, "DE", "DE", "DE");
		TestCase(string.Empty, "DE", string.Empty, "DE");
		TestCase("NLRTM", "BE", string.Empty, string.Empty);
		TestCase("BEBRU", "DE", "NL", string.Empty);
		TestCase("BEBRU", "DE", "DE", "DE");
		TestCase("BEBRU", "DE", string.Empty, string.Empty);
	});

	public void TestExporter()
	{
		CombineAssertions(() =>
		{
			entryHeader.Declaration.JE_OH_Exporter = Factory.New<OrgHeader>().PK;
			AssertNotNull(wrapper.Exporter);
			AssertType<PartyWrapper>(wrapper.Exporter);
		});
	}

	public void TestPreviousDocuments()
	{
		CombineAssertions(() =>
		{
			var prevDoc1 = entryHeader.EntryInstruction.PreviousDocuments.AddNew();
			var prevDoc2 = entryHeader.EntryInstruction.PreviousDocuments.AddNew();
			AssertNotNull(wrapper.PreviousDocuments.FirstOrDefault());
			AssertType<PreviousDocumentWrapper>(wrapper.PreviousDocuments.FirstOrDefault());
		});
	}

	public void TestSeller()
	{
		CombineAssertions(() =>
		{
			var orgHeaderSeller = WrapperTestHelper.CreateOrgHeader(Factory, "Seller Full Name 22", "SELLER", "651321");
			entryHeader.Declaration.SellerOrgPK = orgHeaderSeller.PK;
			var invoiceLines = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>();
			invoiceLines.ForEach(x => x.SellerDocAddress.OrganisationPK = orgHeaderSeller.PK);

			entryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H1;
			AssertType<PartyWrapper>("Type", wrapper.Seller);
			AssertNotNull(wrapper.Seller);

			var invoiceLine = entryHeader.InvoiceLines.First() as JobComInvoiceLine;
			invoiceLine.SellerDocAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertNull("Line sellers are different from declaration", wrapper.Seller);

			invoiceLine.SellerDocAddress.OrganisationPK = orgHeaderSeller.PK;
			entryHeader.Declaration.SellerOrgPK = ZGuid.Empty;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertNull("declaration.Seller is null", wrapper.Seller);

			entryHeader.Declaration.SellerOrgPK = orgHeaderSeller.PK;
			entryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.I1;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertType<PartyWrapper>("CEI_Style is I1", wrapper.Seller);

			entryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H2;
			wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertNull("CEI_Style isn't H1 or I1", wrapper.Seller);
		});
	}

	public void TestSupportingDocuments()
	{
		var supportingDoc = entryHeader.EntryInstruction.SupportingDocuments.AddNew();
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.SupportingDocuments);
			var wrapperSupportingDoc = wrapper.SupportingDocuments.First();
			AssertNotNull(wrapperSupportingDoc);
			AssertType<SupportingDocumentWrapper>(wrapperSupportingDoc);
		});
	}

	public void TestTradeTerms()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.TradeTerms);
			AssertType<TradeTermsWrapper>(wrapper.TradeTerms);
		});
	}

	public void TestGovernmentAgencyGoodsItems()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.GovernmentAgencyGoodsItems);
			AssertType<GoodsItemWrapper>(wrapper.GovernmentAgencyGoodsItems.FirstOrDefault());
		});
	}

	public void TestWarehouse()
	{
		CombineAssertions(() =>
		{
			var orgHeaderWarehouse = WrapperTestHelper.CreateOrgHeader(Factory, "Warehouse Full Name", "WAREHOUSE", "6247645");
			WrapperTestHelper.CreateAuthorizationHeader(orgHeaderWarehouse);
			entryHeader.Declaration.WarehouseDocAddress.OrganisationPK = orgHeaderWarehouse.PK;

			AssertType<WarehouseWrapper>("Has matched authorizationHeaders", wrapper.Warehouse);

			var warehouseHeader = entryHeader.Declaration.WarehouseAddress.Header;
			entryHeader.Declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("WarehouseDocAddress is empty", wrapper.Warehouse);

			entryHeader.Declaration.WarehouseDocAddress.E2_OA_Address = warehouseHeader.MainAddress.PK;
			var authorizationHeaders = CusAuthorisationHeader.Loader.GetAuthorisations(warehouseHeader.Factory, Core.Constants.CountryCodes.Netherlands, new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP }, ZDateTime.Today, warehouseHeader.PK);
			authorizationHeaders.ForEach(x => x.Delete());
			var wrapperWithoutauthorizations = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
			AssertNull("No authorizationHeaders", wrapperWithoutauthorizations.Warehouse);
		});
	}

	public void TestExitDateTime()
	{
		entryHeader.CH_ExitDate = new ZDateTime(2023, 08, 03);
		AssertEquals("ExitDateTime", new ZDateTime(2023, 08, 03).ToString("yyyyMMdd"), wrapper.ExitDateTime);
	}

	protected override GoodsShipmentWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = "NON";
		declaration.Declarant.Header.Contacts.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "H1";

		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_LineNo = 1;
		invoiceLine.JI_RN_NKCountryOfExport = string.Empty;
		invoiceLine.JI_CEI = entryInstruction.PK;
		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);
		entryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().FirstOrDefault();
		wrapper = new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
	}

	JobDeclaration declaration;
	Declaration.CusEntryHeader entryHeader;
	GoodsShipmentWrapper wrapper;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
}
