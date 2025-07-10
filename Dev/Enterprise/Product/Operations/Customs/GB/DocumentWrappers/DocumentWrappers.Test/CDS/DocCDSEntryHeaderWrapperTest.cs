using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.GB.DocumentWrappers.CDS.Testing
{
	public class DocCDSEntryHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestDocCDSEntryHeaderWrapperProperties()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "EXPer";
			exporter.MainAddress.Address1 = "Exporter Street and Number";
			exporter.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EXPREG001", Core.Constants.CountryCodes.UnitedKingdom);

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPer";
			importer.MainAddress.Address1 = "Importer Street and Number";
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IMPREG002", Core.Constants.CountryCodes.UnitedKingdom);

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_FullName = "DCLer";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DCLREG003", Core.Constants.CountryCodes.UnitedKingdom);

			var declarantAddr = declarant.Addresses.AddNew();
			declarantAddr.Address1 = "Declarant Street and Number";

			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Seller";
			seller.MainAddress.Address1 = "Seller Street and Number";
			seller.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "SELLER004", Core.Constants.CountryCodes.UnitedKingdom);

			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			buyer.MainAddress.Address1 = "Buyer Street and Number";
			buyer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BUYER005", Core.Constants.CountryCodes.UnitedKingdom);

			var representative = Factory.New<OrgHeader>();
			representative.OH_FullName = "Representative";
			representative.MainAddress.Address1 = "Representative Street and Number";
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REPRESENTATIVE006", Core.Constants.CountryCodes.UnitedKingdom);

			var supervising = Factory.New<OrgHeader>();
			supervising.OH_FullName = "Supervising";
			supervising.MainAddress.Address1 = "Representative Street and Number";
			supervising.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "SUPERVISING007", Core.Constants.CountryCodes.UnitedKingdom);

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			consignor.MainAddress.Address1 = "Consignor Street and Number";
			consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "CONSIGNOR008", Core.Constants.CountryCodes.UnitedKingdom);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "EDF";
			cei.CEI_SubStyle = EntrySubStyleListImport.Codes.C21GoodsArrived;

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;
			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);

			entryHeader.EntryNumber = "ENT123456";
			dec.JE_EntryStyle = "02";
			dec.SupplierDocumentaryAddress.E2_OA_Address = exporter.MainAddress.PK;
			dec.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			dec.JE_OA_DeclarantAddress = declarantAddr.PK;
			dec.JE_OA_SellerAddress = seller.MainAddress.PK;
			dec.BuyerDocAddress.OrganisationPK = buyer.PK;
			dec.JE_OA_Representative = representative.MainAddress.PK;
			dec.SupervisingOfficeDocAddress.OrganisationPK = supervising.PK;
			dec.CusEntryInstruction.CEI_Style = "EDF";
			dec.JE_CustomsOffice = "GB012345";
			dec.JE_TotalWeight = 55.55m;

			Assert(ReferenceEquals(entryHeader, wrapper.EntryHeader));
			AssertEquals("ENT123456", wrapper.FormattedEntryNumber);
			AssertEquals("EDF", wrapper.EntryStyle);

			AssertEquals("EXPer", wrapper.ExporterFullName);
			AssertEquals("GBEXPREG001", wrapper.ExporterEORI);
			AssertEquals("Exporter Street and Number", wrapper.ExporterStreetAndNumber);

			AssertEquals("[8] Importer [3/15]", wrapper.ImporterCaption);
			AssertEquals("IMPer", wrapper.ImporterFullName);
			AssertEquals("No [3/16]", wrapper.ImporterEORICaption);
			AssertEquals("GBIMPREG002", wrapper.ImporterEORI);
			AssertEquals("Importer Street and Number", wrapper.ImporterStreetAndNumber);

			AssertEquals("DCLer", wrapper.DeclarantFullName);
			AssertEquals("GBDCLREG003", wrapper.DeclarantEORI);
			AssertEquals("Declarant Street and Number", wrapper.DeclarantStreetAndNumber);

			AssertEquals("[2] Seller [3/24]", wrapper.SellerConsignorCaption);
			AssertEquals("Seller", wrapper.SellerConsignorFullName);
			AssertEquals("No [3/25]", wrapper.SellerConsignorEORICaption);
			AssertEquals("GBSELLER004", wrapper.SellerConsignorEORI);
			AssertEquals("Seller Street and Number", wrapper.SellerConsignorStreetAndNumber);

			AssertEquals("Buyer", wrapper.BuyerFullName);
			AssertEquals("GBBUYER005", wrapper.BuyerEORI);
			AssertEquals("Buyer Street and Number", wrapper.BuyerStreetAndNumber);

			AssertEquals("Representative", wrapper.RepresentativeFullName);
			AssertEquals("GBREPRESENTATIVE006", wrapper.RepresentativeEORI);
			AssertEquals("Representative Street and Number", wrapper.RepresentativeStreetAndNumber);

			AssertEquals("GB012345", wrapper.Box44OfficeOfPresentation);
			AssertEquals("55.55", wrapper.Box35GrossMass);
			AssertEquals("SUPERVISING007", wrapper.Box44SupervisingOffice);

			AssertNotNull(wrapper.Items);
			AssertType<DocCDSEntryLineWrapperCollection>(wrapper.Items);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			dec.JE_OA_ShipperAddress = consignor.MainAddress.PK;

			AssertEquals("[8] Importer [3/9]", wrapper.ImporterCaption);
			AssertEquals("No [3/10]", wrapper.ImporterEORICaption);

			AssertEquals("[2] Consignor [3/7]", wrapper.SellerConsignorCaption);
			AssertEquals("Consignor", wrapper.SellerConsignorFullName);
			AssertEquals("No [3/8]", wrapper.SellerConsignorEORICaption);
			AssertEquals("GBCONSIGNOR008", wrapper.SellerConsignorEORI);
			AssertEquals("Consignor Street and Number", wrapper.SellerConsignorStreetAndNumber);
		}

		public void TestBuyerFromShipment()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer";
			orgHeader.OH_RL_NKClosestPort = "GBLHR";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI123", Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.MainAddress.CompanyName = "Buyer Company";
			orgHeader.MainAddress.Address1 = "Buyer Address1";

			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_JS = shipment.PK;
			shipment.BuyerDocAddress.OrganisationPK = orgHeader.PK;

			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);

			CombineAssertions(() =>
			{
				AssertEquals("BuyerFullName", "Buyer Company", wrapper.BuyerFullName);
				AssertEquals("BuyerStreetAndNumber", "Buyer Address1", wrapper.BuyerStreetAndNumber);
				AssertEquals("BuyerEORI", "GBEORI123", wrapper.BuyerEORI);
			});
		}

		public void TestDeliveryTerms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertWrapperDeliveryTerms(entryHeader, "", "", "", "", "");
			AssertWrapperDeliveryTerms(entryHeader, "A", "", "", "", "A");
			AssertWrapperDeliveryTerms(entryHeader, "", "B", "", "", "B");
			AssertWrapperDeliveryTerms(entryHeader, "", "", "C", "", "C");
			AssertWrapperDeliveryTerms(entryHeader, "", "", "", "D", "D");
			AssertWrapperDeliveryTerms(entryHeader, "A", "B", "", "", "A");
			AssertWrapperDeliveryTerms(entryHeader, "", "B", "C", "", "B | C");
			AssertWrapperDeliveryTerms(entryHeader, "", "", "C", "D", "C");
			AssertWrapperDeliveryTerms(entryHeader, "A", "", "C", "", "A | C");
			AssertWrapperDeliveryTerms(entryHeader, "A", "", "", "D", "A | D");
			AssertWrapperDeliveryTerms(entryHeader, "", "B", "", "D", "B | D");
			AssertWrapperDeliveryTerms(entryHeader, "A", "B", "C", "", "A | C");
			AssertWrapperDeliveryTerms(entryHeader, "A", "B", "", "D", "A | D");
			AssertWrapperDeliveryTerms(entryHeader, "A", "", "C", "D", "A | C");
			AssertWrapperDeliveryTerms(entryHeader, "", "B", "C", "D", "B | C");
			AssertWrapperDeliveryTerms(entryHeader, "A", "B", "C", "", "A | C");
			AssertWrapperDeliveryTerms(entryHeader, "A", "B", "C", "", "A | C");
			AssertWrapperDeliveryTerms(entryHeader, "A", "B", "C", "D", "A | C");
		}

		void AssertWrapperDeliveryTerms(CusEntryHeader entryHeader, ZString incoTerm, ZString shipmentIncoTerm, ZString incoTermPlace, ZString shipmentIncoTermPlace, ZString expectedDeliveryTerm)
		{
			var declaration = entryHeader.Declaration;
			var invoiceHeader = declaration.Invoices[0];

			invoiceHeader.JZ_IncoTerm = incoTerm;
			declaration.JE_ShipmentIncoTerm = shipmentIncoTerm;
			invoiceHeader.JZ_IncoTermPlace = incoTermPlace;
			declaration.JE_ShipmentIncoTermPlace = shipmentIncoTermPlace;

			var wrapper = DocCDSEntryHeaderWrapper.New(entryHeader, Factory);
			AssertEquals("Expected wrapper.Box20DeliveryTerms value", expectedDeliveryTerm, wrapper.Box20DeliveryTerms);
		}

		public void TestInvoiceTotal()
		{
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "EDF";
			cei.CEI_SubStyle = Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.C21GoodsArrived;

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;
			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);

			var invoiceHeader1 = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceHeader2 = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceHeader1.JZ_InvoiceAmount = 100.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine1.JI_LinePrice = 100.00m;

			AssertEquals(100.00m, wrapper.Box22InvoiceTotal);
			AssertEquals("USD", wrapper.Box22InvoiceTotalCurrency);
			invoiceHeader2.JZ_InvoiceAmount = 200.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoiceLine2.JI_LinePrice = 200.00m;
			AssertEquals(300.00m, wrapper.Box22InvoiceTotal);
		}

		public void TestContainers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			declaration.CusContainers.AddNew();
			declaration.CusContainers[0].CO_ContainerNumber = "CONT1";
			declaration.CusContainers[0].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.CusContainers.AddNew();
			declaration.CusContainers[1].CO_ContainerNumber = "CONT2";
			declaration.CusContainers[1].CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);
			AssertEquals("CONT1\r\nCONT2", wrapper.Box31Containers);
		}

		public void TestPreviousDocuments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.PreviousDocuments.AddNew();
			declaration.PreviousDocuments[0].CSI_SubType = "Z";
			declaration.PreviousDocuments[0].CSI_Code = "MCR";
			declaration.PreviousDocuments[0].CSI_ReferenceNumber = "HBAC12578834930";

			var previousDocument2 = declaration.PreviousDocuments.AddNew();
			previousDocument2.CSI_SubType = "Z";
			previousDocument2.CSI_Code = "DCR";
			previousDocument2.CSI_ReferenceNumber = "HBAC12578834930-1234";

			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);
			AssertEquals("Z | MCR | HBAC12578834930\r\nZ | DCR | HBAC12578834930-1234", wrapper.Box40SummaryDeclarationAndPreviousDocsCombined);
		}

		public void TestAuthorisationHolders()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure4071 = helper.CreateRefCusProcedure("CDS", "A", "40", "71", "000", "whatever", "IMP", group: "C21", outOfWarehouse: true);
			var procedure7100 = helper.CreateRefCusProcedure("CDS", "A", "71", "00", "000", "whatever", "IMP", group: "C21", intoWarehouse: true);
			Factory.Save();

			var warehouseINTO = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseOUTOF = Factory.NewWithValidTestData<OrgHeader>();
			warehouseINTO.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseOUTOF = Factory.NewWithValidTestData<OrgHeader>();
			warehouseOUTOF.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseINTO.MainAddress.OA_RN_NKCountryCode = "GB";
			warehouseOUTOF.MainAddress.OA_RN_NKCountryCode = "GB";
			warehouseINTO.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U1234567INN", "GB");
			warehouseOUTOF.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U7654321OUT", "GB");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_ApplicationCode = "CDS";
			dec.JE_Calc_LocationOtherInformationCountry = "AA";
			dec.JE_Calc_LocationOtherInformationType = "BB";
			dec.JE_LocationQualifier = "CC";
			dec.JE_GoodsLocation = "DDDDDDDDD";

			var cei = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "EDF";
			cei.CEI_SubStyle = Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.C21GoodsArrived;
			cei.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			cei.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;

			var invoice = dec.Invoices.AddNew();
			var inLine4071 = invoice.InvoiceLines.AddNew();
			var inLine7100 = invoice.InvoiceLines.AddNew();
			inLine4071.JI_CEI = cei.PK;
			inLine7100.JI_CEI = cei.PK;
			inLine4071.JI_Procedure = procedure4071.FullCodeCurrentPlusPreviousPlusConcession;
			inLine7100.JI_Procedure = procedure7100.FullCodeCurrentPlusPreviousPlusConcession;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_MasterUCR = "MUCR";
			entry.CH_CEI_Instruction = cei.PK;

			AssertEquals("pre-req IsOutOfWarehouseWarehousing", true, cei.HasOutOfWarehouseProcedure);
			AssertEquals("pre-req IsIntoWarehouseWarehousing", true, cei.HasIntoWarehouseProcedure);
			AssertEquals("pre-req HasAnyChangeOfOwnershipProcedure", true, cei.HasAnyChangeOfOwnershipProcedure);

			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			var owner3 = Factory.NewWithValidTestData<OrgHeader>();
			owner3.CustomsCodes.AddNew("EOR", "EOR1234567890");
			owner3.OH_Code = "123";

			var auth1 = entry.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = "CGU";
			auth1.AGC_Number = "GB945390992000";
			auth1.AGC_OH_Owner = owner1.PK;

			var auth2 = entry.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = "OLD";
			auth2.AGC_Number = "OLDOWNER";
			auth2.AGC_OH_Owner = owner2.PK;

			var auth3 = entry.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth3.AGC_Code = "ZZZ";
			auth3.AGC_Number = "";
			auth3.AGC_OH_Owner = owner3.PK;

			entry.Declaration.JE_OH_Importer = owner2.PK;
			cei.CEI_OH_Owner = owner2.PK;
			AssertNotEquals("pre-req old owner exists", null, cei.OldOwner);
			AssertNotEquals("pre-req owner exists", ZGuid.Empty, cei.CEI_OH_Owner);

			var wrapper = new DocCDSEntryHeaderWrapper(entry, Factory);
			AssertEquals("CGU | GB945390992000\r\nZZZ | GBEOR1234567890", wrapper.Box44AuthorisationHolders);
		}

		public void TestFiscalReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;

			var fiscalReference1 = declaration.CusEntryInstruction.FiscalReferences.AddNew();
			fiscalReference1.CFR_Code = "G";
			fiscalReference1.CFR_Reference = "ABC12345";

			var fiscalReference2 = declaration.CusEntryInstruction.FiscalReferences.AddNew();
			fiscalReference2.CFR_Code = "G";
			fiscalReference2.CFR_Reference = "DEF12345";

			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);
			AssertEquals("G | ABC12345\r\nG | DEF12345", wrapper.Box44AdditionalFiscalReferences);
		}

		public void TestBox18TransportReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);

			// AIR
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "F-1234";
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertEquals("F-1234", wrapper.Box18TransportReference);

			declaration.JE_ExportDate = new ZDateTime(2022, 1, 1);
			AssertEquals("F-1234 / 01-Jan-22", wrapper.Box18TransportReference);

			// SEA
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "V-1234";
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertEquals("V-1234 / ", wrapper.Box18TransportReference);

			declaration.JE_VoyageFlightNo = "F-1234";
			AssertEquals("V-1234 / F-1234", wrapper.Box18TransportReference);

			// ROA/ROR/IWT/OWN/RAI
			var transportModes = new[]
			{
				Customs.Business.TransportTypeList.Codes.Road,
				Business.CodeDescriptionPairLists.GBTransportTypeList.Codes.ROR,
				Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport,
				Customs.Business.TransportTypeList.Codes.OwnPropulsion,
				Customs.Business.TransportTypeList.Codes.Rail
			};
			foreach (var mode in transportModes)
			{
				declaration.JE_TransportMode = mode;
				declaration.ZG_Box18TransportID = "T-1234";
				AssertEquals("T-1234", wrapper.Box18TransportReference);
			}

			// Others
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_VesselName = "V-5678";
			AssertEquals("V-5678", wrapper.Box18TransportReference);
		}

		public void TestBox49WarehouseID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "A", "71", "00", "000", "", MessageTypeList.Codes.Import,
				group: ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing, intoWarehouse: true);
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U123456A", Core.Constants.CountryCodes.UnitedKingdom);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForCustomsWarehousing;
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "7100000";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);
			AssertEquals("U123456A", wrapper.Box49WarehouseID);
		}

		public void TestExchangeRateElement415SadBox23()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var wrapper = DocCDSEntryHeaderWrapper.New(entryHeader, Factory);
			AssertEquals(0.0m, wrapper.ExchangeRateElement415SadBox23);
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceHeader.JZ_InvoiceCurrExRate = 1.3333;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceHeader2.JZ_InvoiceCurrExRate = 1.3333;
			AssertEquals(1.3333m, wrapper.ExchangeRateElement415SadBox23);
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader2.JZ_InvoiceCurrExRate = 1.35;
			AssertEquals("Exchange rate is 1 for multi currency", 1.0m, wrapper.ExchangeRateElement415SadBox23);
		}

		public void TestExitedStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);

			foreach (ICodeDescription codepair in new ExportExitStatus())
			{
				entryHeader.CH_ExitedStatus = codepair.Code;
				AssertEquals(codepair.Description, wrapper.ExitedStatusDescription);
			}
		}

		public void TestBox52Guarantee()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var wrapper = new DocCDSEntryHeaderWrapper(entryHeader, Factory);
			AssertEquals(ZString.Empty, wrapper.Box52Guarantee1);
			AssertEquals(ZString.Empty, wrapper.Box52Guarantee2);
			AssertEquals(ZString.Empty, wrapper.Box52GuaranteesRemaining);

			AddGuarantee(declaration);
			AssertEquals("1 | GB1001", wrapper.Box52Guarantee1);
			AssertEquals(ZString.Empty, wrapper.Box52Guarantee2);
			AssertEquals(ZString.Empty, wrapper.Box52GuaranteesRemaining);

			AddGuarantee(declaration);
			AssertEquals("1 | GB1001", wrapper.Box52Guarantee1);
			AssertEquals("2 | GB1002", wrapper.Box52Guarantee2);
			AssertEquals(ZString.Empty, wrapper.Box52GuaranteesRemaining);

			AddGuarantee(declaration);
			AssertEquals("1 | GB1001", wrapper.Box52Guarantee1);
			AssertEquals("2 | GB1002", wrapper.Box52Guarantee2);
			AssertEquals("3 | GB1003", wrapper.Box52GuaranteesRemaining);

			AddGuarantee(declaration, "Y");
			AssertEquals("1 | GB1001", wrapper.Box52Guarantee1);
			AssertEquals("2 | GB1002", wrapper.Box52Guarantee2);
			AssertEquals("3 | GB1003\r\nY | GB1004", wrapper.Box52GuaranteesRemaining);
		}

		void AddGuarantee(JobDeclaration declaration, string code = null)
		{
			var count = declaration.Guarantees.Count + 1;
			var guarantee = declaration.Guarantees.AddNew();
			guarantee.PW_Password = code ?? $"{count}";
			guarantee.PW_HolderIdentification = $"GB1{count:D3}";
		}
	}
}
