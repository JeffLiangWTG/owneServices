using System.Linq;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class CADMessageWrapperTest : TestCaseWithFactory
	{
		public void TestCADDeclarationAmendment_GenerateAmendmentsBySendingActions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var actionWrapper = new CADCorrectionMessageSendingActionWrapper(entry);
			var actions = new CADCorrectionMessageSendingActionCollection(actionWrapper);
			var amendments = CADDeclarationAmendment.GenerateAmendmentsBySendingActions(actions);
			AssertEquals(0, amendments.Count());

			var action1 = actions.AddNew();
			action1.InvoiceLineSequence = 1;
			action1.InvoiceSequence = 1;
			action1.CSI_Code = "001";
			action1.CSI_SubType = "1";
			action1.CSI_Description = "Description1";
			var action2 = actions.AddNew();
			action2.InvoiceLineSequence = 2;
			action2.InvoiceSequence = 1;
			action2.CSI_Code = "001";
			action2.CSI_SubType = "1";
			var action3 = actions.AddNew();
			action3.InvoiceLineSequence = 1;
			action3.InvoiceSequence = 2;
			action3.CSI_Code = "001";
			action3.CSI_SubType = "1";
			action3.CSI_Description = "Description3";
			var action4 = actions.AddNew();
			action4.InvoiceLineSequence = 1;
			action4.InvoiceSequence = 1;
			action4.CSI_Code = "002";
			action4.CSI_SubType = "1";
			var action5 = actions.AddNew();
			action5.InvoiceLineSequence = 1;
			action5.InvoiceSequence = 1;
			action5.CSI_Code = "001";
			action5.CSI_SubType = "2";
			var action6 = actions.AddNew();
			action6.InvoiceLineSequence = 1;
			action6.InvoiceSequence = 1;
			action6.CSI_Code = "001";
			action6.CSI_SubType = "1";
			action6.CSI_Description = "Description6";
			amendments = CADDeclarationAmendment.GenerateAmendmentsBySendingActions(actions);
			AssertEquals(3, amendments.Count());

			var decAmendent1 = (ICADMessageDeclarationAmendment)amendments.FirstOrDefault();
			AssertEquals("ChangeReasonCode", "001", decAmendent1.ChangeReasonCode);

			AssertEquals(2, decAmendent1.AdditionalInformation.Count());
			var subTypeActual = decAmendent1.AdditionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.APC).StatementCode;
			AssertEquals("APC StatementCode", "1", subTypeActual);
			var descriptionActual = decAmendent1.AdditionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.CHG).StatementDescription;
			AssertEquals("Description", "Description1", descriptionActual);

			var location = decAmendent1.Pointer;
			AssertEquals(3, location.Count());
			AssertArrayEqualsByElements(new[] {
				"DocumentMetaData/Declaration/GoodsShipment[1]/GovernmentAgencyGoodsItem/Commodity[1]",
				"DocumentMetaData/Declaration/GoodsShipment[1]/GovernmentAgencyGoodsItem/Commodity[2]",
				"DocumentMetaData/Declaration/GoodsShipment[2]/GovernmentAgencyGoodsItem/Commodity[1]"
			}, location.ToArray());

			var amendment2 = amendments.OfType<ICADMessageDeclarationAmendment>().First(x => x.ChangeReasonCode == "002" && x.AdditionalInformation.Any(y => y.StatementTypeCode == "APC" && y.StatementCode == "1"));
			AssertEquals("002 - 1", 1, amendment2.Pointer.Count());

			var amendment3 = amendments.OfType<ICADMessageDeclarationAmendment>().First(x => x.ChangeReasonCode == "001" && x.AdditionalInformation.Any(y => y.StatementTypeCode == "APC" && y.StatementCode == "2"));
			AssertEquals("001 - 2", 1, amendment3.Pointer.Count());
		}

		public void TestCADDeclarationAmendment_DeserializePointer()
		{
			AssertEquals((new ZString("1"), new ZString("2")), CADDeclarationAmendment.DeserializePointer("DocumentMetaData/Declaration/GoodsShipment[1]/GovernmentAgencyGoodsItem/Commodity[2]"));
			AssertEquals((ZString.Empty, ZString.Empty), CADDeclarationAmendment.DeserializePointer("DocumentMetaData/Declaration/GoodsShipment[]/GovernmentAgencyGoodsItem/Commodity[A]"));
		}

		public void TestClearSendingActions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			var sendingWrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			var actions = new CADCorrectionMessageSendingActionCollection(sendingWrapper);
			actions.AddNew();

			AssertEquals(1, actions.Count);
			var wrapper = new CADMessageWrapper(cadEntry, actions);
			wrapper.ClearSendingActions();
			AssertEquals(0, actions.Count);
		}

		public void TestStripOutInvalidCharacters()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV001 !÷ü😊測試";
			var buyerDocAddress = invoice.BuyerDocumentaryAddress;
			buyerDocAddress.E2_AddressOverride = true;
			buyerDocAddress.E2_CompanyName = "COMPNAYNAME FOR TEST !÷ü😊測試";
			buyerDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			buyerDocAddress.E2_City = "CITY FOR TEST !÷ü😊測試";
			buyerDocAddress.E2_Address1 = "ADDRESS1 FOR TEST !÷ü😊測試";
			buyerDocAddress.E2_Address2 = "ADDRESS2 FOR TEST !÷ü😊測試";

			var sellerDocAddress = invoice.SupplierDocumentaryAddress;
			sellerDocAddress.E2_AddressOverride = true;
			sellerDocAddress.E2_CompanyName = "COMPNAYNAME FOR TEST !÷ü😊測試";
			sellerDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			sellerDocAddress.E2_City = "CITY FOR TEST !÷ü😊測試";
			sellerDocAddress.E2_Address1 = "ADDRESS1 FOR TEST !÷ü😊測試";
			sellerDocAddress.E2_Address2 = "ADDRESS2 FOR TEST !÷ü😊測試";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			var goodsShipment = idata.Declaration.GoodsShipment.FirstOrDefault();
			var buyer = goodsShipment.Buyer;
			AssertEquals("Name", "COMPNAYNAME FOR TEST", buyer.Name);
			AssertEquals("CityName", "CITY FOR TEST", buyer.BuyerAddress.CityName);
			AssertEquals("LineText", "ADDRESS1 FOR TEST ADDRESS2 FOR TEST", buyer.BuyerAddress.LineText);

			var seller = goodsShipment.Seller;
			AssertEquals("Name", "COMPNAYNAME FOR TEST", seller.Name);
			AssertEquals("CityName", "CITY FOR TEST", seller.SellerAddress.CityName);
			AssertEquals("LineText", "ADDRESS1 FOR TEST ADDRESS2 FOR TEST", seller.SellerAddress.LineText);
		}

		public void TestCADDeclarationGoodsShipmentBuyer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var invoice = declaration.Invoices.AddNew();
			var buyerDocAddress = invoice.BuyerDocumentaryAddress;
			buyerDocAddress.E2_AddressOverride = true;
			buyerDocAddress.E2_CompanyName = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			buyerDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			buyerDocAddress.E2_City = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			buyerDocAddress.E2_Address1 = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			buyerDocAddress.E2_Address2 = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			buyerDocAddress.E2_State = "abcdefghij";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			var buyer = idata.Declaration.GoodsShipment.FirstOrDefault().Buyer;
			AssertEquals("Name", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ", buyer.Name);
			AssertEquals("CityName", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ", buyer.BuyerAddress.CityName);
			AssertEquals("LineText", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ ABCDEFGHI", buyer.BuyerAddress.LineText);
			AssertEquals("CountrySubDivisionCode", "AB", buyer.BuyerAddress.CountrySubDivisionCode);
		}

		public void TestCADDeclarationGoodsShipmentSeller()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var sellerDocAddress = invoice.SupplierDocumentaryAddress;
			sellerDocAddress.E2_AddressOverride = true;
			sellerDocAddress.E2_CompanyName = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			sellerDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			sellerDocAddress.E2_City = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			sellerDocAddress.E2_Address1 = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			sellerDocAddress.E2_Address2 = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
			sellerDocAddress.E2_State = "abcdefghij";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			var seller = idata.Declaration.GoodsShipment.FirstOrDefault().Seller;
			AssertEquals("Name", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ", seller.Name);
			AssertEquals("CityName", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ", seller.SellerAddress.CityName);
			AssertEquals("LineText", "ABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJABCDEFGHIJ ABCDEFGHI", seller.SellerAddress.LineText);
			AssertEquals("CountrySubDivisionCode", "AB", seller.SellerAddress.CountrySubDivisionCode);
		}

		public void TestLanguageCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			var wrapper = new CADMessageWrapper(entry);
			AssertEquals("EN", wrapper.CADDocumentMetaData.Declaration.LanguageCode);
		}

		public void TestWarehouseEntryType()
		{
			var wareHouse = Factory.New<OrgHeader>();
			wareHouse.OH_Code = "WAREHOUSE";
			var cpw = wareHouse.CustomsCodes.AddNew(MasterFiles.Business.OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW0001", Core.Constants.CountryCodes.Canada);
			var wareHouseAddress = wareHouse.MainAddress;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			declaration.GrossWeight = new ZArchitecture.ZWeight(10, "KG");
			declaration.JE_CarrierCode = "CARR";
			declaration.JE_CustomsOffice = "0497";
			declaration.WarehouseDocAddress.E2_OA_Address = wareHouseAddress.PK;
			var ccn = declaration.CargoControlNumbers.AddNew("CCN0001");
			AssertEquals(1, declaration.CargoControlNumbers.Count);

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			var oft1 = invoice1.Charges.AddNew();
			oft1.J7_ChargeType = Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight;
			oft1.J7_Amount = 24.12m;
			oft1.J7_RX_NKCurrency = "CAD";
			AssertEquals(24.12m, invoice1.OverseasFreight.Amount);

			var preWarehouse = Factory.New<OrgHeader>();
			preWarehouse.OH_Code = "PREWHS";
			var preCPW = preWarehouse.CustomsCodes.AddNew(MasterFiles.Business.OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW0002", Core.Constants.CountryCodes.Canada);
			var preWHSAddress = preWarehouse.MainAddress;
			var preDeclaration = Factory.New<JobDeclaration>();
			var entryNum = CusEntryNumber.LoadOrCreate(preDeclaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
			entryNum.CE_EntryNum = "11334422";
			preDeclaration.WarehouseDocAddress.E2_OA_Address = preWHSAddress.PK;
			preDeclaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_CustomsValue = 3300m;
			var dutyTax11 = invoiceLine1.DutiesAndTaxes.AddNew();
			dutyTax11.C1_PreviousTranNumber = "11334422";
			var dutyTax12 = invoiceLine1.DutiesAndTaxes.AddNew();
			dutyTax12.C1_PreviousTranNumber = "11334422";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			var wrapper = new CADMessageWrapper(entry);
			var documentMetaData = wrapper.CADDocumentMetaData;
			var declarationData = documentMetaData.Declaration;
			AssertEquals("Declaration - TypeCode", "30", declarationData.TypeCode);
			AssertEquals("Declaration - TotalGrossMassMeasure", 0m, declarationData.TotalGrossMassMeasure);
			AssertEquals("Declaration - AdditionalDocument", 0, declarationData.AdditionalDocument.Count());

			var additionalInformation = declarationData.AdditionalInformation.First();
			AssertEquals("Declaration - AdditionalInformation - StatementCode", "30-1", additionalInformation.StatementCode);
			AssertEquals("Declaration - AdditionalInformation - StatementTypeCode", "STC", additionalInformation.StatementTypeCode);

			AssertEquals("Declaration - CarrierID", "", declarationData.CarrierID);
			AssertEquals("Declaration - ConsignmentFreightRate", 1m, declarationData.ConsignmentFreightRate);

			var previousDocument = declarationData.PreviousDocument;
			AssertEquals("Declaration - PreviousDocument - ID", "11334422", previousDocument.ID);
			AssertEquals("Declaration - PreviousDocument - TypeCode", "632", previousDocument.TypeCode);

			var releaseLocation = declarationData.ReleaseLocation;
			AssertEquals("Declaration - ReleaseLocation - ID", "0497", releaseLocation.ID);
			var wareHouses = releaseLocation.Warehouse.ToArray();
			AssertEquals(2, wareHouses.Length);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - ID", "CPW0001", wareHouses[0].ID);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - TypeCode", "18", wareHouses[0].TypeCode);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - RoleCode", "ST", wareHouses[0].RoleCode);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - ID", "CPW0002", wareHouses[1].ID);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - TypeCode", "18", wareHouses[1].TypeCode);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - RoleCode", "SF", wareHouses[1].RoleCode);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Confirming;
			AssertEquals("Declaration - CarrierID", "CARR", declarationData.CarrierID);
			AssertEquals("Declaration - ConsignmentFreightRate", 24m, declarationData.ConsignmentFreightRate);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			preDeclaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			wareHouses = declarationData.ReleaseLocation.Warehouse.ToArray();
			AssertEquals(1, wareHouses.Length);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - ID", "CPW0001", wareHouses[0].ID);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - TypeCode", "18", wareHouses[0].TypeCode);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - RoleCode", "ST", wareHouses[0].RoleCode);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			preDeclaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			wareHouses = declarationData.ReleaseLocation.Warehouse.ToArray();
			AssertEquals(1, wareHouses.Length);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - ID", "CPW0002", wareHouses[0].ID);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - TypeCode", "18", wareHouses[0].TypeCode);
			AssertEquals("Declaration - ReleaseLocation - Warehouse - RoleCode", "SF", wareHouses[0].RoleCode);
		}

		public void TestStatementCodeOfAdditionalInformationOfDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var newDeclaration = declaration.GetNewCopyToPRECARMAdjustmentDeclaration();
			Factory.Save();
			ICADMessageDeclaration cadDeclaration1 = new CADDeclaration(newDeclaration, MessageSubTypes.Create, ZString.Empty, null);
			AssertEquals(StatementCodes.Codes.AsDeclared, cadDeclaration1.AdditionalInformation.First().StatementCode);
			ICADMessageDeclaration cadDeclaration2 = new CADDeclaration(newDeclaration, MessageSubTypes.Amend, ZString.Empty, null);
			AssertEquals(StatementCodes.Codes.AsAdjusted, cadDeclaration2.AdditionalInformation.First().StatementCode);
		}

		public void TestCarrierID_UnloadingLocationID_ExitDateTime_ExitOfficeID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			declaration.JE_CarrierCode = "AB";
			declaration.CA_UnladingOffice = "ABCD";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2022, 07, 08);
			invoice.CA_USPortOfExit = "EF";
			var invoiceLine = invoice.InvoiceLines.AddNew();

			ICADMessageDeclaration cadDeclaration1 = new CADDeclaration(declaration, MessageSubTypes.Create, ZString.Empty, null);
			AssertEquals("AB", cadDeclaration1.CarrierID);
			AssertEquals("ABCD", cadDeclaration1.UnloadingLocationID);
			var goodsShipment1 = cadDeclaration1.GoodsShipment.First();
			AssertEquals("20220708", goodsShipment1.ExitDateTime);
			AssertEquals("EF", goodsShipment1.ExitOfficeID);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			ICADMessageDeclaration cadDeclaration2 = new CADDeclaration(declaration, MessageSubTypes.Create, ZString.Empty, null);
			AssertEquals(ZString.Empty, cadDeclaration2.CarrierID);
			AssertEquals(ZString.Empty, cadDeclaration2.UnloadingLocationID);
			var goodsShipment2 = cadDeclaration2.GoodsShipment.First();
			AssertEquals(ZString.Empty, goodsShipment2.ExitDateTime);
			AssertEquals(ZString.Empty, goodsShipment2.ExitOfficeID);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			ICADMessageDeclaration cadDeclaration3 = new CADDeclaration(declaration, MessageSubTypes.Create, ZString.Empty, null);
			AssertEquals(ZString.Empty, cadDeclaration3.CarrierID);
			AssertEquals(ZString.Empty, cadDeclaration3.UnloadingLocationID);
			var goodsShipment3 = cadDeclaration3.GoodsShipment.First();
			AssertEquals(ZString.Empty, goodsShipment3.ExitDateTime);
			AssertEquals(ZString.Empty, goodsShipment3.ExitOfficeID);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			ICADMessageDeclaration cadDeclaration4 = new CADDeclaration(declaration, MessageSubTypes.Create, ZString.Empty, null);
			AssertEquals(ZString.Empty, cadDeclaration4.CarrierID);
			AssertEquals(ZString.Empty, cadDeclaration4.UnloadingLocationID);
			var goodsShipment4 = cadDeclaration4.GoodsShipment.First();
			AssertEquals(ZString.Empty, goodsShipment4.ExitDateTime);
			AssertEquals(ZString.Empty, goodsShipment4.ExitOfficeID);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.LowValueShipments;
			ICADMessageDeclaration cadDeclaration5 = new CADDeclaration(declaration, MessageSubTypes.Create, ZString.Empty, null);
			AssertEquals(ZString.Empty, cadDeclaration5.CarrierID);
			AssertEquals(ZString.Empty, cadDeclaration5.UnloadingLocationID);
			var goodsShipment5 = cadDeclaration5.GoodsShipment.First();
			AssertEquals(ZString.Empty, goodsShipment5.ExitDateTime);
			AssertEquals("1001", goodsShipment5.ExitOfficeID);
		}

		public void TestCADGoodsShipmentCommodityWrapper()
		{
			#region Create Test Data
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RMASDF");
			CreateCusCodeListWithAttribute("Alcohol", CasualImportConstants.CasualImpCommodityType.Alcohol);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2022, 07, 08);
			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter.PK;
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			invoice.JZ_IncoTerm = "FOB";
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine.CL_CommoditySequence = 2;
			entryLine.CL_AdValoremTariff = "1234567890";
			entryLine.CL_CustomsValue = 13456m;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			line1.JI_Description = "ABC";
			line1.JI_InvoiceQuantity = 132.0004;
			line1.JI_InvoiceUQ = "KG";
			line1.CA_AuthorityNumber = "DEF";
			line1.CA_RemissionType = "GHI";
			line1.CA_ValueForDutyCode = "JK";
			line1.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			line1.JI_CustomsSecondQuantity = 45;
			line1.CA_99TariffCode = "9903";
			line1.CA_IsCasualImport = true;
			line1.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			line1.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			line1.JI_LinePrice = 2000m;
			line1.JI_Model = "XXX";
			line1.CA_CasualImportCommodity = "Alcohol";
			line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var cpt1 = line1.DutiesAndTaxes.AddNew();
			cpt1.C1_Override = true;
			cpt1.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt1.C1_Amount = 33m;
			var cta1 = line1.DutiesAndTaxes.AddNew();
			cta1.C1_Override = true;
			cta1.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta1.C1_Amount = 34m;
			var dty1 = line1.DutiesAndTaxes.AddNew();
			dty1.C1_Override = true;
			dty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty1.C1_Amount = 35m;
			dty1.C1_Code = "AB";
			var exs1 = line1.DutiesAndTaxes.AddNew();
			exs1.C1_Override = true;
			exs1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs1.C1_Amount = 36m;
			exs1.C1_Code = "BC";
			var sur1 = line1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "CD";
			var add1 = line1.DutiesAndTaxes.AddNew();
			add1.C1_Override = true;
			add1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add1.C1_Amount = 39m;
			add1.Quantity = 40m;
			add1.C1_UnitOfMeasure = "M4";
			add1.C1_Code = "DE";
			var cvd1 = line1.DutiesAndTaxes.AddNew();
			cvd1.C1_Override = true;
			cvd1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd1.C1_Amount = 43;
			cvd1.Quantity = 44;
			cvd1.C1_UnitOfMeasure = "M5";
			cvd1.C1_Code = "EF";
			var saf1 = line1.DutiesAndTaxes.AddNew();
			saf1.C1_Override = true;
			saf1.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf1.C1_Amount = 45m;
			saf1.C1_Code = "FG";
			var gst1 = line1.DutiesAndTaxes.AddNew();
			gst1.C1_Override = true;
			gst1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst1.C1_Amount = 46m;
			gst1.C1_ExemptCode = "GH";
			var ded1 = line1.Charges.AddNew();
			ded1.J7_ChargeType = Common.CustomsChargeTypeList.Codes.DeductionCharge;
			ded1.J7_Amount = 46m;
			ded1.J7_RX_NKCurrency = "USD";
			#endregion

			CombineAssertions(() =>
			{
				var cadMessageWrapper = new CADMessageWrapper(entryHeader, MessageSubTypes.Change);
				var commodity = cadMessageWrapper.CADDocumentMetaData.Declaration.GoodsShipment.First().GovernmentAgencyGoodsItem.First();
				AssertEquals("ExitDateTime", "20220708", commodity.ExitDateTime);
				AssertEquals("SequenceNumeric", 2, commodity.SequenceNumeric);
				AssertEquals("Description", "ABC", commodity.Description);
				AssertEquals("CountQuantity", 132.000m, commodity.CountQuantity);
				AssertEquals("CountQtyUnit", "", commodity.CountQtyUnit);
				AssertEquals("AcquisitionDateTime", ZString.Empty, commodity.AcquisitionDateTime);
				AssertEquals("AdditionalDocument.ID", "DEF", commodity.AdditionalDocument.First().ID);
				AssertEquals("AdditionalDocument.TypeCode", "GHI", commodity.AdditionalDocument.First().TypeCode);
				AssertEquals("AdditionalDocumentSpecified", false, commodity.AdditionalDocumentSpecified);
				var additionalInformation = commodity.AdditionalInformation;
				AssertEquals("AdditionalInformation Count", 5, additionalInformation.Count());
				var asj = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.ASJ);
				AssertEquals("ASJ AdditionalInformation.StatementCode", ZString.Empty, asj.StatementCode);
				var mif = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.MIF);
				AssertEquals("MIF AdditionalInformation.StatementCode", "DE", mif.StatementCode);
				var bsj = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.BSJ);
				AssertEquals("BSJ AdditionalInformation.StatementCode", ZString.Empty, bsj.StatementCode);
				var vdc = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.VDC);
				AssertEquals("VDC AdditionalInformation.StatementCode", "0JK", vdc.StatementCode);
				var csj = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.CSJ);
				AssertEquals("CSJ AdditionalInformation.StatementCode", ZString.Empty, csj.StatementCode);
				AssertEquals("ConstituentElementPercentNumeric", 45m, commodity.ConstituentElementPercentNumeric);
				var classifications = commodity.Classification;
				AssertEquals("Classification Count", 2, classifications.Count());
				AssertEquals("Classification 1", true, classifications.Any(x => x.ID == "1234567890"));
				AssertEquals("Classification 2", true, classifications.Any(x => x.ID == "9903"));
				AssertEquals("DestinationRegionID", CanadianProvinceList.Codes.Alberta, commodity.DestinationRegionID);
				AssertEquals("ExportCountry.CountryCode", Core.Constants.CountryCodes.China, commodity.ExportCountry.CountryCode);
				AssertEquals("ExportCountry.RegionID", ZString.Empty, commodity.ExportCountry.RegionID);
				AssertEquals("ExporterID", "123456789RMASDF", commodity.ExporterID);
				AssertEquals("InvoiceLineCharge.Amount", 1936.06m, commodity.InvoiceLineCharge.Amount);
				AssertEquals("InvoiceLineCharge.CurrencyCode", "CAD", commodity.InvoiceLineCharge.CurrencyCode);
				AssertEquals("Origin.CountryCode", Core.Constants.CountryCodes.Australia, commodity.Origin.CountryCode);
				AssertEquals("Origin.RegionID", ZString.Empty, commodity.Origin.RegionID);
				AssertEquals("PreviousDocument", null, commodity.PreviousDocument);
				AssertEquals("ProductID", "XXX", commodity.ProductID);
				AssertEquals("TradeTermsConditionCode", "FOB", commodity.TradeTermsConditionCode);
				var dutyTaxFees = commodity.DutyTaxFee;
				AssertEquals("DutyTaxFee Count", 12, dutyTaxFees.Count());
				var aai = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.AAI);
				AssertEquals("AAI Payment.PaymentAmount.Amount", 33m, aai.Payment.PaymentAmount.Amount);
				AssertEquals("AAI Payment.PaymentAmount.CurrencyCode", "CAD", aai.Payment.PaymentAmount.CurrencyCode);
				var tac = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.TAC);
				AssertEquals("TAC Payment.PaymentAmount.Amount", 34m, tac.Payment.PaymentAmount.Amount);
				AssertEquals("TAC Payment.PaymentAmount.CurrencyCode", "CAD", tac.Payment.PaymentAmount.CurrencyCode);
				var exd = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.EXD);
				AssertEquals("EXD DeductAmount.Amount", 46m, exd.DeductAmount.Amount);
				AssertEquals("EXD DeductAmount.CurrencyCode", "USD", exd.DeductAmount.CurrencyCode);
				var fetDTY = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.FET && x.DutyRegimeCode == "AB");
				AssertNotNull("FET DTY", fetDTY);
				var fetEXS = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.FET && x.DutyRegimeCode == "BC");
				AssertNotNull("FET EXS", fetEXS);
				var gst = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.GST);
				AssertEquals("GST DutyRegimeCode", "GH", gst.DutyRegimeCode);
				var sur = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.SUR);
				AssertEquals("SUR SpecificTaxBaseQuantity", 0m, sur.SpecificTaxBaseQuantity);
				AssertEquals("SUR SpecificTaxBaseQtyUnit", "", sur.SpecificTaxBaseQtyUnit);
				AssertEquals("SUR SpecificTaxBaseQtyUnit", "CD", sur.DutyRegimeCode);
				var add = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.ADD);
				AssertEquals("ADD Payment.PaymentAmount.Amount", 39m, add.Payment.PaymentAmount.Amount);
				AssertEquals("ADD Payment.PaymentAmount.CurrencyCode", "CAD", add.Payment.PaymentAmount.CurrencyCode);
				AssertEquals("ADD SpecificTaxBaseQuantity", 40m, add.SpecificTaxBaseQuantity);
				AssertEquals("ADD SpecificTaxBaseQtyUnit", "M4", add.SpecificTaxBaseQtyUnit);
				AssertEquals("ADD SpecificTaxBaseQtyUnit", "DE", add.DutyRegimeCode);
				AssertEquals("ADD RequestOverrideCode", "X", add.RequestOverrideCode);
				var cvd = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.CVD);
				AssertEquals("CVD Payment.PaymentAmount.Amount", 43m, cvd.Payment.PaymentAmount.Amount);
				AssertEquals("CVD Payment.PaymentAmount.CurrencyCode", "CAD", cvd.Payment.PaymentAmount.CurrencyCode);
				AssertEquals("CVD SpecificTaxBaseQuantity", 44m, cvd.SpecificTaxBaseQuantity);
				AssertEquals("CVD SpecificTaxBaseQtyUnit", "M5", cvd.SpecificTaxBaseQtyUnit);
				AssertEquals("CVD SpecificTaxBaseQtyUnit", "EF", cvd.DutyRegimeCode);
				AssertEquals("CVD RequestOverrideCode", "X", cvd.RequestOverrideCode);
				var oth = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.OTH);
				AssertEquals("OTH DutyRegimeCode", "FG", oth.DutyRegimeCode);
				var cud = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.CUD);
				AssertEquals("CUD DutyRegimeCode", TariffTreatmentCodes.Codes.MostFavouredNation, cud.DutyRegimeCode);
				var tot = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.TOT);
				AssertEquals("TOT DutyTaxFeeAssessmentBasis.Amount", 13456m, tot.DutyTaxFeeAssessmentBasis.First().Amount);
				AssertEquals("TOT DutyTaxFeeAssessmentBasis.CurrencyCode", "CAD", tot.DutyTaxFeeAssessmentBasis.First().CurrencyCode);
			});
		}

		public void TestCADGoodsShipmentCommodityWrapper_LVS()
		{
			#region Create Test Data
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = exporter.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RMASDF");
			CreateCusCodeListWithAttribute("Alcohol", CasualImportConstants.CasualImpCommodityType.Alcohol);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.LowValueShipments;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2022, 07, 08);
			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter.PK;
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			invoice.JZ_IncoTerm = "FOB";
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine.CL_CommoditySequence = 2;
			entryLine.CL_AdValoremTariff = "1234567890";
			entryLine.CL_CustomsValue = 13456m;

			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			line1.JI_Description = "ABC";
			line1.JI_CustomsQuantity = 131.0005;
			line1.JI_CustomsUnitQty = "ML";
			line1.JI_InvoiceQuantity = 132.0005;
			line1.JI_InvoiceUQ = "KG";
			line1.CA_AuthorityNumber = "DEF";
			line1.CA_RemissionType = "GHI";
			line1.CA_ValueForDutyCode = "JK";
			line1.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			line1.JI_CustomsSecondQuantity = 45;
			line1.CA_99TariffCode = "9903";
			line1.CA_IsCasualImport = true;
			line1.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Alberta;
			line1.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			line1.JI_LinePrice = 2000m;
			line1.JI_Model = "XXX";
			line1.CA_CasualImportCommodity = "Alcohol";
			line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var cpt1 = line1.DutiesAndTaxes.AddNew();
			cpt1.C1_Override = true;
			cpt1.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt1.C1_Amount = 33m;
			var cta1 = line1.DutiesAndTaxes.AddNew();
			cta1.C1_Override = true;
			cta1.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta1.C1_Amount = 34m;
			var dty1 = line1.DutiesAndTaxes.AddNew();
			dty1.C1_Override = true;
			dty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty1.C1_Amount = 35m;
			dty1.C1_Code = "AB";
			var exs1 = line1.DutiesAndTaxes.AddNew();
			exs1.C1_Override = true;
			exs1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs1.C1_Amount = 36m;
			exs1.C1_Code = "BC";
			var sur1 = line1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "CD";
			var add1 = line1.DutiesAndTaxes.AddNew();
			add1.C1_Override = true;
			add1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add1.C1_Amount = 39m;
			add1.Quantity = 40m;
			add1.C1_UnitOfMeasure = "M4";
			add1.C1_Code = "DE";
			var cvd1 = line1.DutiesAndTaxes.AddNew();
			cvd1.C1_Override = true;
			cvd1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd1.C1_Amount = 43;
			cvd1.Quantity = 44;
			cvd1.C1_UnitOfMeasure = "M5";
			cvd1.C1_Code = "EF";
			var saf1 = line1.DutiesAndTaxes.AddNew();
			saf1.C1_Override = true;
			saf1.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf1.C1_Amount = 45m;
			saf1.C1_Code = "FG";
			var gst1 = line1.DutiesAndTaxes.AddNew();
			gst1.C1_Override = true;
			gst1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst1.C1_Amount = 46m;
			gst1.C1_ExemptCode = "GH";
			var ded1 = line1.Charges.AddNew();
			ded1.J7_ChargeType = Common.CustomsChargeTypeList.Codes.DeductionCharge;
			ded1.J7_Amount = 46m;
			ded1.J7_RX_NKCurrency = "USD";
			#endregion

			CombineAssertions(() =>
			{
				var cadMessageWrapper = new CADMessageWrapper(entryHeader, MessageSubTypes.Change);
				var commodity = cadMessageWrapper.CADDocumentMetaData.Declaration.GoodsShipment.First().GovernmentAgencyGoodsItem.First();
				AssertEquals("ExitDateTime", ZString.Empty, commodity.ExitDateTime);
				AssertEquals("SequenceNumeric", 2, commodity.SequenceNumeric);
				AssertEquals("Description", "ABC", commodity.Description);
				AssertEquals("CountQuantity", 131.001m, commodity.CountQuantity);
				AssertEquals("CountQtyUnit", "ML", commodity.CountQtyUnit);
				AssertEquals("AcquisitionDateTime", ZString.Empty, commodity.AcquisitionDateTime);
				AssertEquals("AdditionalDocument.ID", "DEF", commodity.AdditionalDocument.First().ID);
				AssertEquals("AdditionalDocument.TypeCode", "GHI", commodity.AdditionalDocument.First().TypeCode);
				AssertEquals("AdditionalDocumentSpecified", false, commodity.AdditionalDocumentSpecified);
				var additionalInformation = commodity.AdditionalInformation;
				AssertEquals("AdditionalInformation Count", 5, additionalInformation.Count());
				var vdc = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.VDC);
				AssertEquals("VDC AdditionalInformation.StatementCode", "013", vdc.StatementCode);
				AssertEquals("ConstituentElementPercentNumeric", 45m, commodity.ConstituentElementPercentNumeric);
				var asj = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.ASJ);
				AssertEquals("ASJ AdditionalInformation.StatementCode", ZString.Empty, asj.StatementCode);
				var mif = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.MIF);
				AssertEquals("MIF AdditionalInformation.StatementCode", "DE", mif.StatementCode);
				var bsj = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.BSJ);
				AssertEquals("BSJ AdditionalInformation.StatementCode", ZString.Empty, bsj.StatementCode);
				var csj = additionalInformation.First(x => x.StatementTypeCode == AdditionalInformationTypeCodes.Codes.CSJ);
				AssertEquals("CSJ AdditionalInformation.StatementCode", ZString.Empty, csj.StatementCode);
				var classifications = commodity.Classification;
				AssertEquals("Classification Count", 2, classifications.Count());
				AssertEquals("Classification 1", true, classifications.Any(x => x.ID == "1234567890"));
				AssertEquals("Classification 2", true, classifications.Any(x => x.ID == "9903"));
				AssertEquals("DestinationRegionID", CanadianProvinceList.Codes.Alberta, commodity.DestinationRegionID);
				AssertEquals("ExporterID", "123456789RMASDF", commodity.ExporterID);
				AssertNull("InvoiceLineCharge", commodity.InvoiceLineCharge);
				AssertEquals("PreviousDocument", null, commodity.PreviousDocument);
				AssertEquals("ProductID", ZString.Empty, commodity.ProductID);
				AssertEquals("TradeTermsConditionCode", ZString.Empty, commodity.TradeTermsConditionCode);
				var dutyTaxFees = commodity.DutyTaxFee;
				AssertEquals("DutyTaxFee Count", 12, dutyTaxFees.Count());
				var aai = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.AAI);
				AssertEquals("AAI Payment.PaymentAmount.Amount", 33m, aai.Payment.PaymentAmount.Amount);
				AssertEquals("AAI Payment.PaymentAmount.CurrencyCode", "CAD", aai.Payment.PaymentAmount.CurrencyCode);
				var tac = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.TAC);
				AssertEquals("TAC Payment.PaymentAmount.Amount", 34m, tac.Payment.PaymentAmount.Amount);
				AssertEquals("TAC Payment.PaymentAmount.CurrencyCode", "CAD", tac.Payment.PaymentAmount.CurrencyCode);
				var exd = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.EXD);
				AssertEquals("EXD DeductAmount.Amount", 46m, exd.DeductAmount.Amount);
				AssertEquals("EXD DeductAmount.CurrencyCode", "USD", exd.DeductAmount.CurrencyCode);
				var fetDTY = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.FET && x.DutyRegimeCode == "AB");
				AssertNotNull("FET DTY", fetDTY);
				var fetEXS = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.FET && x.DutyRegimeCode == "BC");
				AssertNotNull("FET EXS", fetEXS);
				var gst = dutyTaxFees.FirstOrDefault(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.GST);
				AssertEquals("GST DutyRegimeCode", "GH", gst.DutyRegimeCode);
				AssertNull("GST Payment.PaymentAmount", gst.Payment.PaymentAmount);
				var sur = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.SUR);
				AssertEquals("SUR SpecificTaxBaseQuantity", 0m, sur.SpecificTaxBaseQuantity);
				AssertEquals("SUR SpecificTaxBaseQtyUnit", "", sur.SpecificTaxBaseQtyUnit);
				AssertEquals("SUR SpecificTaxBaseQtyUnit", "CD", sur.DutyRegimeCode);
				var add = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.ADD);
				AssertEquals("ADD Payment.PaymentAmount.Amount", 39m, add.Payment.PaymentAmount.Amount);
				AssertEquals("ADD Payment.PaymentAmount.CurrencyCode", "CAD", add.Payment.PaymentAmount.CurrencyCode);
				AssertEquals("ADD SpecificTaxBaseQuantity", 40m, add.SpecificTaxBaseQuantity);
				AssertEquals("ADD SpecificTaxBaseQtyUnit", "M4", add.SpecificTaxBaseQtyUnit);
				AssertEquals("ADD SpecificTaxBaseQtyUnit", "DE", add.DutyRegimeCode);
				AssertEquals("ADD RequestOverrideCode", "X", add.RequestOverrideCode);
				var cvd = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.CVD);
				AssertEquals("CVD Payment.PaymentAmount.Amount", 43m, cvd.Payment.PaymentAmount.Amount);
				AssertEquals("CVD Payment.PaymentAmount.CurrencyCode", "CAD", cvd.Payment.PaymentAmount.CurrencyCode);
				AssertEquals("CVD SpecificTaxBaseQuantity", 44m, cvd.SpecificTaxBaseQuantity);
				AssertEquals("CVD SpecificTaxBaseQtyUnit", "M5", cvd.SpecificTaxBaseQtyUnit);
				AssertEquals("CVD SpecificTaxBaseQtyUnit", "EF", cvd.DutyRegimeCode);
				AssertEquals("CVD RequestOverrideCode", "X", cvd.RequestOverrideCode);
				var oth = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.OTH);
				AssertEquals("OTH DutyRegimeCode", "FG", oth.DutyRegimeCode);
				var cud = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.CUD);
				AssertEquals("CUD DutyRegimeCode", TariffTreatmentCodes.Codes.MostFavouredNation, cud.DutyRegimeCode);
				var tot = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.TOT);
				AssertNotNull(tot.DutyTaxFeeAssessmentBasis.FirstOrDefault(x => x.Amount == 13456m && x.CurrencyCode == Core.Constants.CurrencyCodes.Canada));
				AssertNotNull(tot.DutyTaxFeeAssessmentBasis.FirstOrDefault(x => x.Amount == 0m && x.CurrencyCode == Core.Constants.CurrencyCodes.Canada));
			});
		}

		public void TestNoDeclarantWhenDeclarantIDEqualsImporterID()
		{
			var declaration = Factory.New<JobDeclaration>();

			var broker = GlbBranch.GetCurrentBranch(Factory).OrgProxy;
			broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0001");
			declaration.JE_OH_ExternalBroker = broker.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0002");
			declaration.JE_OH_Importer = importer.PK;

			Factory.Save();

			var cadDeclaration1 = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("842957342RM0001", ((ICADMessageDeclaration)cadDeclaration1).DeclarantID);
			AssertEquals("842957342RM0002", ((ICADMessageDeclaration)cadDeclaration1).ImporterID);

			importer.CustomsCodes.RemoveAndDeleteAll();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0001");
			declaration.JE_OH_Importer = importer.PK;

			Factory.Save();

			var cadDeclaration2 = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("Declarant should not be passed in the CAD message when DeclarantID equals to ImporterID", "", ((ICADMessageDeclaration)cadDeclaration2).DeclarantID);
			AssertEquals("842957342RM0001", ((ICADMessageDeclaration)cadDeclaration2).ImporterID);
		}

		public void TestImporterID()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "000000000RM0006");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.CA_IsCasualImport = true;
			declaration.ImporterOfRecordAddress.OrganisationPK = orgHeader1.PK;
			declaration.JE_OH_Importer = orgHeader2.PK;

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.ImporterOfRecordAddress.OrganisationPK = orgHeader1.PK;
			declaration1.JE_OH_Importer = orgHeader2.PK;

			var cadDeclaration1 = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			var cadDeclaration2 = new CADDeclaration(declaration1, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("000000000RM0006", ((ICADMessageDeclaration)cadDeclaration1).ImporterID);
			AssertEquals("000000000RM0006", ((ICADMessageDeclaration)cadDeclaration2).ImporterID);

			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, "000000000RM0004");
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "000000000RM0005");
			cadDeclaration1 = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			cadDeclaration2 = new CADDeclaration(declaration1, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("000000000RM0004", ((ICADMessageDeclaration)cadDeclaration1).ImporterID);
			AssertEquals("000000000RM0005", ((ICADMessageDeclaration)cadDeclaration2).ImporterID);

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "000000000RM0003");
			cadDeclaration1 = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			cadDeclaration2 = new CADDeclaration(declaration1, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("000000000RM0003", ((ICADMessageDeclaration)cadDeclaration1).ImporterID);
			AssertEquals("000000000RM0003", ((ICADMessageDeclaration)cadDeclaration2).ImporterID);

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, "000000000RM0001");
			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "000000000RM0002");
			cadDeclaration1 = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			cadDeclaration2 = new CADDeclaration(declaration1, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("000000000RM0001", ((ICADMessageDeclaration)cadDeclaration1).ImporterID);
			AssertEquals("000000000RM0002", ((ICADMessageDeclaration)cadDeclaration2).ImporterID);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration3.JE_OH_Importer = orgHeader1.PK;
			var cadDeclaration3 = new CADDeclaration(declaration3, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("000000000RM0003", ((ICADMessageDeclaration)cadDeclaration3).ImporterID);

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "000000000RM0001");
			cadDeclaration3 = new CADDeclaration(declaration3, MessageSubTypes.Undefined, ZString.Empty, null);
			AssertEquals("000000000RM0001", ((ICADMessageDeclaration)cadDeclaration3).ImporterID);
		}

		public void TestDeclarantIDAndAKSenders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Branch.Company.OrgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "123BRM456");
			var cadDeclaration = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			var documentMetaData = new CADDocumentMetaDataCommunicationMetaData(declaration, "00001");
			AssertEquals("123BRM456", ((ICADMessageDeclaration)cadDeclaration).DeclarantID);
			AssertEquals("123BRM456", ((ICADMessageCommunicationMetaData)documentMetaData).Senders.FirstOrDefault(x => x.RoleCode == "AK").ID);

			declaration.Branch.OrgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "456BRM456");
			cadDeclaration = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			documentMetaData = new CADDocumentMetaDataCommunicationMetaData(declaration, "00001");
			AssertEquals("456BRM456", ((ICADMessageDeclaration)cadDeclaration).DeclarantID);
			AssertEquals("456BRM456", ((ICADMessageCommunicationMetaData)documentMetaData).Senders.FirstOrDefault(x => x.RoleCode == "AK").ID);

			declaration.Branch.OrgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "789BRM789");
			cadDeclaration = new CADDeclaration(declaration, MessageSubTypes.Undefined, ZString.Empty, null);
			documentMetaData = new CADDocumentMetaDataCommunicationMetaData(declaration, "00001");
			AssertEquals("789BRM789", ((ICADMessageDeclaration)cadDeclaration).DeclarantID);
			AssertEquals("789BRM789", ((ICADMessageCommunicationMetaData)documentMetaData).Senders.FirstOrDefault(x => x.RoleCode == "AK").ID);
		}

		public void TestTotalGrossMassMeasure()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.GrossWeight = new ZArchitecture.ZWeight(10, "KG");

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.GrossWeight = new ZArchitecture.ZWeight(10.4, "KG");

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.GrossWeight = new ZArchitecture.ZWeight(10.5, "KG");

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.GrossWeight = new ZArchitecture.ZWeight(0.4, "KG");

			var declaration5 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration5.GrossWeight = new ZArchitecture.ZWeight(0.5, "KG");

			Factory.Save();

			var cadDeclaration1 = new CADDeclaration(declaration1, MessageSubTypes.Undefined, ZString.Empty, null);
			var cadDeclaration2 = new CADDeclaration(declaration2, MessageSubTypes.Undefined, ZString.Empty, null);
			var cadDeclaration3 = new CADDeclaration(declaration3, MessageSubTypes.Undefined, ZString.Empty, null);
			var cadDeclaration4 = new CADDeclaration(declaration4, MessageSubTypes.Undefined, ZString.Empty, null);
			var cadDeclaration5 = new CADDeclaration(declaration5, MessageSubTypes.Undefined, ZString.Empty, null);

			AssertEquals(10m, ((ICADMessageDeclaration)cadDeclaration1).TotalGrossMassMeasure);
			AssertEquals(10m, ((ICADMessageDeclaration)cadDeclaration2).TotalGrossMassMeasure);
			AssertEquals(11m, ((ICADMessageDeclaration)cadDeclaration3).TotalGrossMassMeasure);
			AssertEquals("Round it to 1 if the value is less than 0.5", 1m, ((ICADMessageDeclaration)cadDeclaration4).TotalGrossMassMeasure);
			AssertEquals(1m, ((ICADMessageDeclaration)cadDeclaration5).TotalGrossMassMeasure);
		}

		public void TestCADDocumentMetaData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var metaData = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals("CBSA", metaData.ResponsibleAgencyName);
			AssertEquals("CAD", metaData.AgencyAssignedCustomizationCode);
			AssertEquals("001", metaData.AgencyAssignedCustomizationVersionCode);
			AssertEquals("CAD-IN", metaData.FunctionalDefinition);
		}

		public void TestMessageSubType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			var wrapper = new CADMessageWrapper(entry);
			AssertEquals(MessageSubTypes.Create, wrapper.messageSubType);

			entry.CH_Status = MessageStatusList.Codes.ClearOriginal;
			wrapper = new CADMessageWrapper(entry);
			AssertEquals(MessageSubTypes.Change, wrapper.messageSubType);

			entry.CH_Status = MessageStatusList.Codes.ErrorOriginal;
			wrapper = new CADMessageWrapper(entry);
			AssertEquals(MessageSubTypes.Create, wrapper.messageSubType);

			entry.CH_Status = MessageStatusList.Codes.AcknowledgedChange;
			wrapper = new CADMessageWrapper(entry);
			AssertEquals(MessageSubTypes.Change, wrapper.messageSubType);

			entry.CH_Status = MessageStatusList.Codes.NotSent;
			wrapper = new CADMessageWrapper(entry);
			AssertEquals(MessageSubTypes.Create, wrapper.messageSubType);

			entry.CH_Status = MessageStatusList.Codes.AcknowledgedOriginal;
			wrapper = new CADMessageWrapper(entry);
			AssertEquals(MessageSubTypes.Create, wrapper.messageSubType);
		}

		public void TestFunctionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals(CADMessageFunctionCodes.Codes.Original, idata.Declaration.FunctionCode);

			entry.CH_Status = MessageStatusList.Codes.ClearOriginal;
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals(CADMessageFunctionCodes.Codes.Change, idata.Declaration.FunctionCode);

			entry.CH_Status = MessageStatusList.Codes.ErrorOriginal;
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals(CADMessageFunctionCodes.Codes.Original, idata.Declaration.FunctionCode);

			entry.CH_Status = MessageStatusList.Codes.AcknowledgedChange;
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals(CADMessageFunctionCodes.Codes.Change, idata.Declaration.FunctionCode);

			entry.CH_Status = MessageStatusList.Codes.NotSent;
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals(CADMessageFunctionCodes.Codes.Original, idata.Declaration.FunctionCode);

			entry.CH_Status = MessageStatusList.Codes.AcknowledgedOriginal;
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals(CADMessageFunctionCodes.Codes.Original, idata.Declaration.FunctionCode);
		}

		public void TestVersionID()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_VersionID = 1;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			entry.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			var idata2 = new CADMessageWrapper(entry, null).CADDocumentMetaData;

			AssertEquals("Do not populate version id for create messages", ZString.Empty, idata.Declaration.VersionID);
			AssertEquals("Populate version id for create messages", "00002", idata2.Declaration.VersionID);
		}

		public void TestCADGoodsShipmentExitDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2022, 07, 29);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals("CADGoodsShipment ExitDateTime", "20220729", idata.Declaration.GoodsShipment.First().ExitDateTime);
		}

		public void TestDutyTaxFeeRegimeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var duty1 = invoiceLine.DutiesAndTaxes.AddNew();
			var duty2 = invoiceLine.DutiesAndTaxes.AddNew();
			duty1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			duty2.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals("Duty Tax Fee count", 2, idata.Declaration.GoodsShipment.First().GovernmentAgencyGoodsItem.First().DutyTaxFee.Count());

			duty1.C1_Code = "XXX";
			duty2.C1_ExemptCode = "ZZZ";
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals("Duty Tax Fee count", 4, idata.Declaration.GoodsShipment.First().GovernmentAgencyGoodsItem.First().DutyTaxFee.Count());
		}

		public void TestAddressLineText()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			var address = org.MainAddress;
			address.Address1 = "Address 1";
			address.City = "Nanjing";
			address.State = "JS";
			address.Postcode = "1234";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = org.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals("ADDRESS 1", idata.Declaration.GoodsShipment.FirstOrDefault().Seller.SellerAddress.LineText);

			address.Address2 = "Address 2";
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals("ADDRESS 1 ADDRESS 2", idata.Declaration.GoodsShipment.FirstOrDefault().Seller.SellerAddress.LineText);

			address.Address1 = ZString.Empty;
			idata = new CADMessageWrapper(entry).CADDocumentMetaData;
			AssertEquals("ADDRESS 2", idata.Declaration.GoodsShipment.FirstOrDefault().Seller.SellerAddress.LineText);
		}

		[TestDate(2022, 09, 05)]
		public void TestExitDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2022, 09, 04);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault();
			AssertEquals("20220904", idata.ExitDateTime);
			AssertEquals(ZString.Empty, idata.GovernmentAgencyGoodsItem.FirstOrDefault().ExitDateTime);
		}

		public void TestCPT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_IsCasualImport = true;
			var duty = invoiceLine.DutiesAndTaxes.AddNew();
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			duty.C1_Amount = 12m;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "AAI");
			AssertNotNull("AAI", idata);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Canada, idata.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("Amount", 12m, idata.Payment.PaymentAmount.Amount);
		}

		public void TestCTA()
		{
			CreateCusCodeListWithAttribute("Alcohol", CasualImportConstants.CasualImpCommodityType.Alcohol);
			CreateCusCodeListWithAttribute("Cannabis", CasualImportConstants.CasualImpCommodityType.Tobacco);
			CreateCusCodeListWithAttribute("Tobacco", CasualImportConstants.CasualImpCommodityType.Tobacco);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_IsCasualImport = true;
			invoiceLine.CA_CasualImportCommodity = "Alcohol";
			var duty = invoiceLine.DutiesAndTaxes.AddNew();
			duty.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			duty.C1_Amount = 12m;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "TAC");
			AssertNotNull("TAC", idata);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Canada, idata.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("Amount", 12m, idata.Payment.PaymentAmount.Amount);

			invoiceLine.CA_CasualImportCommodity = "Cannabis";
			idata = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "PAT");
			AssertNotNull("PAT", idata);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Canada, idata.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("Amount", 12m, idata.Payment.PaymentAmount.Amount);

			invoiceLine.CA_CasualImportCommodity = "Tobacco";
			idata = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "AAD");
			AssertNotNull("AAD", idata);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Canada, idata.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("Amount", 12m, idata.Payment.PaymentAmount.Amount);
		}

		public void TestTOT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_IsCasualImport = true;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 10m;
			entryLine.InvoiceLines.Add(invoiceLine);
			var link = invoiceLine.AdditionalEntryLineLinks.AddNew();
			link.BU_CL = entryLine.PK;
			var idata = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "TOT");
			AssertNotNull("TOT", idata);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Canada, idata.DutyTaxFeeAssessmentBasis.FirstOrDefault().CurrencyCode);
			AssertEquals("Amount", 10m, idata.DutyTaxFeeAssessmentBasis.FirstOrDefault().Amount);
		}

		public void TestAmendmentActions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var actionWrapper = new CADCorrectionMessageSendingActionWrapper(entry);
			var actions = new CADCorrectionMessageSendingActionCollection(actionWrapper);
			var action1 = actions.AddNew();
			action1.InvoiceLineSequence = 1;
			action1.InvoiceSequence = 1;
			action1.CSI_Code = "001";
			action1.CSI_SubType = "1";
			var wrapper = new CADMessageWrapper(entry, actions);
			var amendments = wrapper.CADDocumentMetaData.Declaration.Amendment;
			AssertEquals(1, amendments.Count());

			var amendment1 = amendments.First();
			AssertEquals("001", amendment1.ChangeReasonCode);
		}

		public void TestConsignmentFreightRate()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TEST";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.CSAImporter;
			declaration.GrossWeight = new ZArchitecture.ZWeight(10, "KG");
			declaration.JE_OH_Importer = orgHeader.PK;
			var orgImpAddInfo = OrgImpAddInfo.Get(orgHeader);
			var freightPercentage = orgImpAddInfo.FreightPercentages.AddNew();
			freightPercentage.CY_Code = "ROA";
			freightPercentage.DefaultFreightPercentage = 0.2m;

			var invoice = declaration.Invoices.AddNew();
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.PuertoRico;
			var oft = invoice.Charges.AddNew();
			oft.J7_ChargeType = Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight;
			oft.J7_Amount = 369.85m;
			oft.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(369.85m, invoice.OverseasFreight.Amount);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CustomsValue = 3300m;

			var uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			AssertEquals(1.39m, invoice.JZ_InvoiceCurrExRate);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 5000m;

			var wrapper = new CADMessageWrapper(entry);
			var documentMetaData = wrapper.CADDocumentMetaData;
			var declarationData = documentMetaData.Declaration;
			AssertEquals("ConsignmentFreightRate should from invoice header.", 514m, declarationData.ConsignmentFreightRate);

			oft.J7_Amount = 0m;
			AssertEquals(0m, invoice.OverseasFreight.Amount);

			wrapper = new CADMessageWrapper(entry);
			documentMetaData = wrapper.CADDocumentMetaData;
			declarationData = documentMetaData.Declaration;
			AssertEquals("ConsignmentFreightRate should from default freight.", 10m, declarationData.ConsignmentFreightRate);

			invoiceLine.CA_CustomsValue = 300m;

			wrapper = new CADMessageWrapper(entry);
			documentMetaData = wrapper.CADDocumentMetaData;
			declarationData = documentMetaData.Declaration;
			AssertEquals("The value should never be 0.", 1m, declarationData.ConsignmentFreightRate);
		}

		void CreateCusCodeListWithAttribute(ZString code, ZString attribute)
		{
			new UniversalReferenceTestDataHelper(Factory).CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				code, code, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, attribute);
		}
	}
}
