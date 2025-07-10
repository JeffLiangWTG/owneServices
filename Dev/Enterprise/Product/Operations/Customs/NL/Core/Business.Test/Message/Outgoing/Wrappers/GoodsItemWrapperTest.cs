using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class GoodsItemWrapperTest : DataProviderTestCase<GoodsItemWrapper>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("entry header null", () => new GoodsItemWrapper(null, new JobDeclarationMessageSendingObject(cusEntryHeader)));
		AssertExceptionThrown<ArgumentNullException>("message sending object null", () => new GoodsItemWrapper(Factory.New<Declaration.CusEntryLine>(), null));
	});

	public void TestSequenceNumeric()
	{
		AssertEquals(1, wrapper.SequenceNumeric);
	}

	public void TestCustomsValueAmount()
	{
		cusEntryLine.CL_CustomsValue = 20.3;
		AssertEquals(20.3m, wrapper.CustomsValueAmount);
	}

	public void TestCurrencyId()
	{
		cusEntryHeader.RandomHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		AssertEquals("EUR", wrapper.CurrencyId);
	}

	public void TestCurrencyID_InvoiceCurrencyIsNull()
	{
		cusEntryLine.RandomLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
		AssertNull(wrapper.CurrencyId);
	}

	public void TestStatisticalValueAmount()
	{
		cusEntryLine.CL_StatisticalValue = 10.2;
		AssertEquals(10.2m, wrapper.StatisticalValueAmount);
	}

	public void TestTransactionNatureCode_NoTP_Import()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.DMSTransitionPeriod, Core.Constants.CountryCodes.Netherlands, ZDate.Today, false))
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.H1;
			invoice.JZ_ValuationCode = "10";
			invoiceLine.ZG_TransNature = "11";
			invoiceLine2.ZG_TransNature = "12";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "13";
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.ZG_TransNature = "14";

			CombineAssertions(() =>
			{
				declaration.CustomsEntryInstructions.First().ZG_TransNature = "10";
				AssertEquals("Transaction Nature filled on EntryInstruction", string.Empty, wrapper.TransactionNatureCode);
				declaration.CustomsEntryInstructions.First().ZG_TransNature = string.Empty;
				AssertEquals("Transaction Nature filled on InvoiceLine", "11", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, take from InvoiceHeader", "10", wrapper.TransactionNatureCode);

				invoiceLine2.ZG_TransNature = "";
				invoiceLine3.ZG_TransNature = "";
				AssertEquals("All TransactionNature-fields are empty on InvoiceLine, take from InvoiceHeader", "10", wrapper.TransactionNatureCode);

				invoice2.JZ_ValuationCode = "10";
				AssertNullOrEmpty("No TransactionNature on InvoiceLine, TransactionNature on InvoiceHeaders are the same, leave blank", wrapper.TransactionNatureCode);

				declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.H2;
				invoice2.JZ_ValuationCode = "13";
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
			invoiceLine2.ZG_TransNature = "12";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "13";
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.ZG_TransNature = "14";

			CombineAssertions(() =>
			{
				declaration.CustomsEntryInstructions.First().ZG_TransNature = "10";
				AssertEquals("Transaction Nature filled on EntryInstruction", string.Empty, wrapper.TransactionNatureCode);
				declaration.CustomsEntryInstructions.First().ZG_TransNature = string.Empty;
				AssertEquals("Transaction Nature filled on InvoiceLine", "11", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, take from InvoiceHeader", "10", wrapper.TransactionNatureCode);

				invoiceLine2.ZG_TransNature = "";
				invoiceLine3.ZG_TransNature = "";
				AssertEquals("All TransactionNature-fields are empty on InvoiceLine, take from InvoiceHeader", "10", wrapper.TransactionNatureCode);

				invoice2.JZ_ValuationCode = "10";
				AssertNullOrEmpty("No TransactionNature on InvoiceLine, TransactionNature on InvoiceHeaders are the same, leave blank", wrapper.TransactionNatureCode);

				declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.B3;
				invoice2.JZ_ValuationCode = "13";
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
			invoiceLine2.ZG_TransNature = "12";

			CombineAssertions(() =>
			{
				declaration.CustomsEntryInstructions.First().ZG_TransNature = "10";
				AssertEquals("Transaction Nature filled on EntryInstruction", string.Empty, wrapper.TransactionNatureCode);
				declaration.CustomsEntryInstructions.First().ZG_TransNature = string.Empty;
				AssertEquals("Transaction Nature filled on InvoiceLine", "11", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, take from InvoiceHeader", "10", wrapper.TransactionNatureCode);

				invoiceLine2.ZG_TransNature = "";
				AssertEquals("All TransactionNature-fields are empty on InvoiceLine, take from InvoiceHeader", "10", wrapper.TransactionNatureCode);

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
			invoiceLine2.ZG_TransNature = "12";

			CombineAssertions(() =>
			{
				declaration.CustomsEntryInstructions.First().ZG_TransNature = "10";
				AssertEquals("Transaction Nature filled on EntryInstruction", string.Empty, wrapper.TransactionNatureCode);
				declaration.CustomsEntryInstructions.First().ZG_TransNature = string.Empty;
				AssertEquals("Transaction Nature filled on InvoiceLine", "11", wrapper.TransactionNatureCode);

				invoiceLine.ZG_TransNature = "";
				AssertEquals("Transaction Nature not filled on InvoiceLine, take from InvoiceHeader", "10", wrapper.TransactionNatureCode);

				declaration.CustomsEntryInstructions.First().CEI_Style = DeclarationTypeList.Codes.B3;
				AssertNullOrEmpty("DeclarationType is not B1, B2, B4, or C1", wrapper.TransactionNatureCode);
			});
		}
	}

	public void TestDispatchCountryCode()
	{
		cusEntryHeader.Declaration.JE_RL_NKOrigin = "USHOU";
		cusEntryHeader.Declaration.CustomsEntryInstructions.FirstOrDefault().CEI_Style = "H1";
		cusEntryLine.RandomLine.ZG_CountryOfDispatch = "NL";
		AssertEquals("NL", wrapper.DispatchCountryCode);
	}

	public void TestAcceptanceDateTime()
	{
		AssertNull(wrapper.AcceptanceDateTime);
	}

	public void TestAdditionalReferences()
	{
		var addRef1 = invoiceLine.AdditionalInfos.AddNew();
		addRef1.CSI_SubType = "REF";
		addRef1.CSI_ReferenceNumber = "REFREF1";
		addRef1.CSI_Code = "REFCOD1";
		var addRef2 = invoiceLine.AdditionalInfos.AddNew();
		addRef2.CSI_SubType = "REF";
		addRef2.CSI_ReferenceNumber = "REFREF2";
		addRef2.CSI_Code = "REFCOD2";

		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.AdditionalReferences.FirstOrDefault());
			AssertType<AdditionalReferenceWrapper>(wrapper.AdditionalReferences.FirstOrDefault());
			AssertEquals("Number of AdditionalReferences", 2, wrapper.AdditionalReferences.Count);
			AssertEquals("ID of 2nd AdditionalReference", "REFREF2", wrapper.AdditionalReferences.ElementAt(1).Id);
		});
	}

	public void TestAdditionalInformations()
	{
		CombineAssertions(() =>
		{
			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_SubType = "INF";
			var addInfo2 = invoiceLine.AdditionalInfos.AddNew();
			addInfo2.CSI_Description = "INFDES2";
			addInfo2.CSI_SubType = "INF";
			AssertNotNull(wrapper.AdditionalInformations.FirstOrDefault());
			AssertType<AdditionalInformationWrapper>(wrapper.AdditionalInformations.FirstOrDefault());
			AssertEquals("Number of AdditionalInformations", 2, wrapper.AdditionalInformations.Count);
			AssertEquals("StatementDescription of 2nd AdditionalInformation", "INFDES2", wrapper.AdditionalInformations.ElementAt(1).StatementDescription);
		});
	}

	public void TestAEOMutualRecognitionParties()
	{
		CombineAssertions(() =>
		{
			var cusRef1 = invoiceLine.CusSupplyChainActorReferences.AddNew();
			var cusRef2 = invoiceLine.CusSupplyChainActorReferences.AddNew();
			cusRef2.CFR_Reference = "REFJI2";
			AssertNotNull(wrapper.AEOMutualRecognitionParties.FirstOrDefault());
			AssertType<AEOMutualRecognitionPartyWrapper>(wrapper.AEOMutualRecognitionParties.FirstOrDefault());
			AssertEquals("Number of AEOMutualRecognitionParties", 2, wrapper.AEOMutualRecognitionParties.Count);
			AssertEquals("ID of 2nd AEOMutualRecognitionParty", "REFJI2", wrapper.AEOMutualRecognitionParties.ElementAt(1).Id);
		});
	}

	public void TestAuthorisations()
	{
		CombineAssertions(() =>
		{
			var cusAuthorizationUsage1 = invoiceLine.CusAuthorizationUsages.AddNew();
			var cusAuthorizationUsage2 = invoiceLine.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage2.AGC_Number = "9876543";

			AssertNotNull(wrapper.Authorisations.FirstOrDefault());
			AssertType<AuthorisationWrapper>(wrapper.Authorisations.FirstOrDefault());
			AssertEquals("No Authorisations", 2, wrapper.Authorisations.Count);
			AssertEquals("ID of 2nd Authorisations", "9876543", wrapper.Authorisations.ElementAt(1).Id);
		});
	}

	public void TestBuyer()
	{
		CombineAssertions(() =>
		{
			var orgHeaderBuyer = WrapperTestHelper.CreateOrgHeader(Factory, "Buyer Full Name 22", "Buye", "651321");
			var invoiceLines = cusEntryHeader.InvoiceLines.Cast<JobComInvoiceLine>();
			invoiceLines.ForEach(x => x.BuyerDocAddress.OrganisationPK = orgHeaderBuyer.PK);

			cusEntryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H1;
			var wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertType<PartyWrapper>("Type", wrapper.Buyer);
			AssertEquals("Buyer Eori", "NL651321", wrapper.Buyer.Id);

			cusEntryHeader.Declaration.ConsigneeAddressOrgPK = orgHeaderBuyer.PK;
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertNull("Line buyers are equal to declaration", wrapper.Buyer);

			cusEntryHeader.Declaration.ConsigneeAddressOrgPK = ZGuid.Empty;
			invoiceLines.ForEach(x => x.BuyerDocAddress.OrganisationPK = ZGuid.Empty);
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertNull("line.BuyerDocAddress.Organisation is null", wrapper.Buyer);

			invoiceLines.ForEach(x => x.BuyerDocAddress.OrganisationPK = orgHeaderBuyer.PK);
			cusEntryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.I1;
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertType<PartyWrapper>("CEI_Style is I1", wrapper.Buyer);

			cusEntryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H2;
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertNull("CEI_Style isn't H1 or I1", wrapper.Buyer);
		});
	}

	public void TestCommodity()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.Commodity);
			AssertType<CommodityWrapper>(wrapper.Commodity);
		});
	}

	public void TestCustomsValuation()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.CustomsValuation);
			AssertType<CustomsValuationWrapper>(wrapper.CustomsValuation);
		});
	}

	public void TestDestination()
	{
		var declaration = cusEntryLine.Declaration;
		declaration.JE_RL_NKFinalDestination = "NLDRE";
		var invoiceLine = cusEntryLine.RandomLine;
		var instruction = invoiceLine.EntryInstruction;
		instruction.CEI_Style = NLConstants.EntryStyles.ExportReExport;
		invoiceLine.ZG_CountryOfDestination = "NB";

		CombineAssertions(() =>
		{
			var destination = wrapper.Destination;
			AssertNotNull(destination);
			AssertType<DestinationWrapper>("Type", destination);
			AssertEquals("ZG_CountryOfDestination isn't empty or same as JE_RL_NKFinalDestination", "NB", destination.CountryCode);

			invoiceLine.ZG_CountryOfDestination = ZString.Empty;
			AssertEquals("ZG_CountryOfDestination is empty", "NL", new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader)).Destination.CountryCode);

			cusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ZG_CountryOfDestination = "NL");
			AssertNull("All invoice lines ZG_CountryOfDestination are the same as JE_RL_NKFinalDestination", new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader)).Destination);
		});
	}

	public void TestExporter()
	{
		CombineAssertions(() =>
		{
			cusEntryHeader.Declaration.JE_OH_Exporter = Factory.New<OrgHeader>().PK;

			AssertNotNull(wrapper.Exporter);
			AssertType<PartyWrapper>(wrapper.Exporter);
		});
	}

	public void TestGovernmentProcedure()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.GovernmentProcedure);
			AssertType<GovernmentProcedureWrapper>(wrapper.GovernmentProcedure);
		});
	}

	public void TestOrigins()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CountryOfOrigin = "AB";
			invoiceLine.ZG_CountryOfSupply = "S1";
			invoiceLine.JI_PrimaryPreference = "2PR";
			cusEntryHeader.Declaration.JE_MessageType = MessageTypeList.Codes.Import;

			AssertNotNull(wrapper.Origins.FirstOrDefault());
			AssertType<OriginWrapper>(wrapper.Origins.FirstOrDefault());
			AssertEquals("No Origins", 2, wrapper.Origins.Count);
			AssertEquals("Country of 2nd Origins", "AB", wrapper.Origins.ElementAt(1).CountryCode);
		});
	}

	public void TestOriginsWhenNoOrigin()
	{
		foreach (var entryLine in cusEntryHeader.AllEntryLines)
		{
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
			}
		}
		AssertEquals("No origins filled in", 0, wrapper.Origins.Count);
	}

	public void TestOriginsForExport()
	{
		invoiceLine.JI_CountryOfOrigin = "AB";

		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.Origins.FirstOrDefault());
			AssertType<OriginWrapper>(wrapper.Origins.FirstOrDefault());
			AssertEquals("Count Origins", 1, wrapper.Origins.Count);
			AssertEquals("Country of Origins", "AB", wrapper.Origins.ElementAt(0).CountryCode);
		});
	}

	public void TestPackagings()
	{
		CombineAssertions(() =>
		{
			var cw1 = cusEntryHeader.Declaration.Packages.AddNew();
			var cw2 = cusEntryHeader.Declaration.Packages.AddNew();
			cw2.CW_PackType = "PK";

			AssertNotNull(wrapper.Packagings.FirstOrDefault());
			AssertType<PackagingWrapper>(wrapper.Packagings.FirstOrDefault());
			AssertEquals("No Packagings", 2, wrapper.Packagings.Count);
			AssertEquals("TypeCode of 2nd Packagings", "PK", wrapper.Packagings.ElementAt(1).TypeCode);
		});
	}

	public void TestPreviousDocuments()
	{
		CombineAssertions(() =>
		{
			var prevDoc1 = invoiceLine.PreviousDocuments.AddNew();
			var prevDoc2 = invoiceLine.PreviousDocuments.AddNew();
			prevDoc2.CSI_ReferenceNumber = "PRV-987";
			AssertNotNull(wrapper.PreviousDocuments.FirstOrDefault());
			AssertType<PreviousDocumentWrapper>(wrapper.PreviousDocuments.FirstOrDefault());
			AssertEquals("No PreviousDocuments", 2, wrapper.PreviousDocuments.Count);
			AssertEquals("ID of 2nd PreviousDocuments", "PRV-987", wrapper.PreviousDocuments.ElementAt(1).Id);
		});
	}

	public void TestSeller()
	{
		CombineAssertions(() =>
		{
			var orgHeaderSeller = WrapperTestHelper.CreateOrgHeader(Factory, "Seller Full Name 22", "SELLER", "651321");
			var invoiceLines = cusEntryHeader.InvoiceLines.Cast<JobComInvoiceLine>();
			invoiceLines.ForEach(x => x.SellerDocAddress.OrganisationPK = orgHeaderSeller.PK);

			cusEntryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H1;
			var wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertType<PartyWrapper>("Type", wrapper.Seller);
			AssertEquals("Seller Eori", "NL651321", wrapper.Seller.Id);

			cusEntryHeader.Declaration.SellerOrgPK = orgHeaderSeller.PK;
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertNull("Line sellers are equal to declaration", wrapper.Seller);

			cusEntryHeader.Declaration.SellerOrgPK = ZGuid.Empty;
			invoiceLines.ForEach(x => x.SellerDocAddress.OrganisationPK = ZGuid.Empty);
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertNull("line.SellerDocAddress.Organisation is null", wrapper.Seller);

			invoiceLines.ForEach(x => x.SellerDocAddress.OrganisationPK = orgHeaderSeller.PK);
			cusEntryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.I1;
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertType<PartyWrapper>("CEI_Style is I1", wrapper.Seller);

			cusEntryHeader.EntryInstruction.CEI_Style = DeclarationTypeList.Codes.H2;
			wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
			AssertNull("CEI_Style isn't H1 or I1", wrapper.Seller);
		});
	}

	public void TestSupportingDocuments()
	{
		CombineAssertions(() =>
		{
			WrapperTestHelper.CreateCusCodeLists(Factory);
			var supDoc1 = invoiceLine.SupportingDocuments.AddNew();
			var supDoc2 = invoiceLine.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "SPCD1";
			supDoc1.CSI_ReferenceNumber = "SUPREF11";
			supDoc2.CSI_Code = "SPCD2";
			supDoc2.CSI_ReferenceNumber = "SUPREF21";
			var supDoc3 = invoiceLine2.SupportingDocuments.AddNew();
			var supDoc4 = invoiceLine2.SupportingDocuments.AddNew();
			supDoc3.CSI_Code = "SPCD1";
			supDoc3.CSI_ReferenceNumber = "SUPREF11";
			supDoc4.CSI_Code = "SPCD2";
			supDoc4.CSI_ReferenceNumber = "SUPREF21";

			AssertNotNull(wrapper.SupportingDocuments.FirstOrDefault());
			AssertType<SupportingDocumentWrapper>(wrapper.SupportingDocuments.FirstOrDefault());
			AssertEquals("No SupportingDocuments", 2, wrapper.SupportingDocuments.Count);
			AssertContainsExactElementsInAnyOrder("ID's SupportingDocuments", new ZString[] { "SUPREF11", "SUPREF21" }, wrapper.SupportingDocuments.Select(x => x.Id).ToArray());
		});
	}

	public void TestUCR()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.UCR);
			AssertType<UCRWrapper>(wrapper.UCR);
		});
	}

	public void TestValuationAdjustment()
	{
		AssertNull(wrapper.ValuationAdjustment);
	}

	public void TestTransportContractDocuments()
	{
		var addRef1 = invoiceLine.AdditionalInfos.AddNew();
		addRef1.CSI_SubType = "TRA";
		addRef1.CSI_ReferenceNumber = "TRAREF1";
		addRef1.CSI_Code = "TRACOD1";
		var addRef2 = invoiceLine.AdditionalInfos.AddNew();
		addRef2.CSI_SubType = "TRA";
		addRef2.CSI_ReferenceNumber = "TRAREF2";
		addRef2.CSI_Code = "TRACOD2";

		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.TransportContractDocuments.FirstOrDefault());
			AssertType<TransportContractDocumentWrapper>(wrapper.TransportContractDocuments.FirstOrDefault());
			AssertEquals("No TransportContractDocuments", 2, wrapper.TransportContractDocuments.Count);
			AssertEquals("ID of 2nd TransportContractDocuments", "TRAREF2", wrapper.TransportContractDocuments.ElementAt(1).Id);
		});
	}

	public void TestExportCountry() => CombineAssertions(() =>
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = "NON";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);
		cusEntryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().FirstOrDefault();

		var line1Wrapper = new GoodsItemWrapper(cusEntryHeader.MergedLines.First(), new JobDeclarationMessageSendingObject(cusEntryHeader));
		var line2Wrapper = new GoodsItemWrapper(cusEntryHeader.MergedLines.Last(), new JobDeclarationMessageSendingObject(cusEntryHeader));

		void TestCase(string shipmentExportCountry, string line1ExportCountry, string line2ExportCountry, string expectedLine1, string expectedLine2)
		{
			declaration.JE_RL_NKOrigin = shipmentExportCountry;
			invoiceLine.JI_RN_NKCountryOfExport = line1ExportCountry;
			invoiceLine2.JI_RN_NKCountryOfExport = line2ExportCountry;
			AssertEquals($"shipment: '{shipmentExportCountry}', line1: '{line1ExportCountry}', line2: '{line2ExportCountry}' | line 1 output", expectedLine1, line1Wrapper.ExportCountry);
			AssertEquals($"shipment: '{shipmentExportCountry}', line1: '{line1ExportCountry}', line2: '{line2ExportCountry}' | line 2 output", expectedLine2, line2Wrapper.ExportCountry);
		}

		TestCase(string.Empty, "BE", "NL", "BE", "NL");
		TestCase(string.Empty, "BE", "BE", string.Empty, string.Empty);
		TestCase(string.Empty, "BE", string.Empty, string.Empty, string.Empty);
		TestCase("NLRTM", "BE", string.Empty, "BE", "NL");
		TestCase("DERTM", "BE", "NL", "BE", "NL");
		TestCase("DERTM", "BE", "BE", string.Empty, string.Empty);
		TestCase("DERTM", "BE", string.Empty, "BE", "DE");
	});

	public void TestConsignee()
	{
		var orgHeaderSeller = WrapperTestHelper.CreateOrgHeader(invoiceLine.Factory, "Seller Full Name", "SELLER", "123456");
		var addressSeller = WrapperTestHelper.CreateAddress(orgHeaderSeller, "SEL", "Rijksweg 102", "Deventer", "7201MG");

		var orgHeaderSellerForDeclaration = WrapperTestHelper.CreateOrgHeader(invoiceLine.Factory, "diff", "SELLER2", "654321");
		var addressSellerForDeclaration = WrapperTestHelper.CreateAddress(orgHeaderSellerForDeclaration, "SEL", "diff", "Deventer", "1000MG");

		declaration.JE_MessageType = "EXP";
		var messageSendingObject = new JobDeclarationMessageSendingObject(cusEntryHeader);
		messageSendingObject.MessageType = "DEC";
		invoiceLine.JI_OA_ConsigneeAddress = addressSeller.PK;
		var consignee = wrapper.Consignee;
		var consigneeAddress = consignee.Address;
		CombineAssertions(() =>
		{
			AssertNotNull(consignee);
			AssertType<PartyWrapper>(consignee);
			AssertNull("Consignee Name", consignee.Name);
			AssertEquals("Consignee Eori", "NL123456", consignee.Id);
			AssertNull("Consignee Address", consigneeAddress);

			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.CSI_Code = EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600;
			wrapper = new GoodsItemWrapper(cusEntryLine, messageSendingObject);
			AssertNull("30600 addInfo present on InvoiceLine", wrapper.Consignee);

			invoiceLine.AdditionalInfos.RemoveAndDeleteAll();
			addInfo = entryInstruction.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.CSI_Code = EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600;
			wrapper = new GoodsItemWrapper(cusEntryLine, messageSendingObject);
			AssertNull("30600 addInfo present on entryInstruction", wrapper.Consignee);

			entryInstruction.AdditionalInfos.RemoveAndDeleteAll();
			cusEntryHeader.Declaration.ImporterDocumentaryAddress.E2_OA_Address = addressSeller.PK;
			wrapper = new GoodsItemWrapper(cusEntryLine, messageSendingObject);
			AssertNotNull("consignee at item level will not be empty", wrapper.Consignee);
		});
	}

	public void TestConsignor()
	{
		var orgHeaderSeller = WrapperTestHelper.CreateOrgHeader(invoiceLine.Factory, "Seller Full Name", "SELLER", "123456");
		var addressSeller = WrapperTestHelper.CreateAddress(orgHeaderSeller, "SEL", "Rijksweg 102", "Deventer", "7201MG");

		var orgHeaderSellerForDeclaration = WrapperTestHelper.CreateOrgHeader(invoiceLine.Factory, "diff", "SELLER2", "654321");
		var addressSellerForDeclaration = WrapperTestHelper.CreateAddress(orgHeaderSellerForDeclaration, "SEL", "diff", "Deventer", "1000MG");

		declaration.JE_MessageType = "EXP";
		var messageSendingObject = new JobDeclarationMessageSendingObject(cusEntryHeader);
		messageSendingObject.MessageType = "DEC";
		invoiceLine.JI_OA_ExporterAddress = addressSeller.PK;
		var consignor = wrapper.Consignor;
		var consignorAddress = consignor.Address;
		CombineAssertions(() =>
		{
			AssertNotNull(consignor);
			AssertType<PartyWrapper>(consignor);
			AssertNull("Consignor Name", consignor.Name);
			AssertEquals("Consignor Eori", "NL123456", consignor.Id);
			AssertNull("Consignor Address Type", consignorAddress);

			cusEntryHeader.Declaration.SupplierDocumentaryAddress.E2_OA_Address = addressSeller.PK;
			wrapper = new GoodsItemWrapper(cusEntryLine, messageSendingObject);
			AssertNotNull("consignor at item level will not be empty", wrapper.Consignor);
		});
	}

	public void TestFreight()
	{
		invoice.ZG_TransportChargesMethodOfPayment = "X";
		AssertEquals("Freight", "X", wrapper.Freight.PaymentMethodCode);
	}

	protected override GoodsItemWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);

		cusEntryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().FirstOrDefault();
		cusEntryLine = cusEntryHeader.MergedLines.First();
		wrapper = new GoodsItemWrapper(cusEntryLine, new JobDeclarationMessageSendingObject(cusEntryHeader));
	}
	Declaration.CusEntryHeader cusEntryHeader;
	Declaration.CusEntryLine cusEntryLine;
	Declaration.CusEntryInstruction entryInstruction;
	GoodsItemWrapper wrapper;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceLine invoiceLine2;
	JobComInvoiceHeader invoice;
	JobDeclaration declaration;
}
