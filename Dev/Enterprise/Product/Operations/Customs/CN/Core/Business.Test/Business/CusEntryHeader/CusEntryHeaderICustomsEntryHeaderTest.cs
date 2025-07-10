using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryHeaderICustomsEntryHeaderTest : TestCaseWithFactory
	{
		[TestDate(2019, 2, 25)]
		public void TestDateOfValuation()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			var entryHeader = testItems.EntryHeader;
			testItems.EntryInstruction.CEI_DateForDuty = new ZDateTime(2019, 2, 1);
			AssertEquals("Should have falled from Declaration", new ZDateTime(2019, 2, 1), entryHeader.DateOfValuation);
			entryHeader.SetMovementReferenceNumber("MRN10000010", new ZDateTime(2019, 2, 26));
			AssertEquals("Should return MRN date", new ZDateTime(2019, 2, 26), entryHeader.DateOfValuation);
		}

		public void TestContractNo()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine11 = entry1.MergedLines.AddNew();
			var entryLine12 = entry1.MergedLines.AddNew();
			var invoice1 = testDec.Invoices.AddNew();
			var invoice2 = testDec.Invoices.AddNew();
			var invoiceLine11 = entryLine11.InvoiceLines.AddNew();
			var invoiceLine12 = entryLine12.InvoiceLines.AddNew();
			invoiceLine11.JI_JZ = invoice1.PK;
			invoiceLine12.JI_JZ = invoice2.PK;
			invoice1.ContractNumbers.AddNew().J2_ReferenceNumber = "0123";
			invoice1.ContractNumbers.AddNew().J2_ReferenceNumber = "4567";
			invoice1.ContractNumbers.AddNew().J2_ReferenceNumber = "8901";
			invoice1.ContractNumbers.AddNew().J2_ReferenceNumber = "45";
			invoice2.ContractNumbers.AddNew().J2_ReferenceNumber = "0123";
			invoice2.ContractNumbers.AddNew().J2_ReferenceNumber = "a567";
			invoice2.ContractNumbers.AddNew().J2_ReferenceNumber = "a901";
			invoice2.ContractNumbers.AddNew().J2_ReferenceNumber = "a345";
			invoice2.ContractNumbers.AddNew().J2_ReferenceNumber = "a777";
			var entry2 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine21 = entry2.MergedLines.AddNew();
			var invoice21 = testDec.Invoices.AddNew();
			var invoiceLine21 = entryLine21.InvoiceLines.AddNew();
			invoiceLine21.JI_JZ = invoice21.PK;
			invoice21.ContractNumbers.AddNew().J2_ReferenceNumber = "01234567890123456789012345678912";
			invoice21.ContractNumbers.AddNew().J2_ReferenceNumber = "4567";
			invoice21.ContractNumbers.AddNew().J2_ReferenceNumber = "8901";
			invoice21.ContractNumbers.AddNew().J2_ReferenceNumber = "2345";
			invoice21.ContractNumbers.AddNew().J2_ReferenceNumber = "6789";
			var entry3 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine3 = entry3.MergedLines.AddNew();
			var invoice32 = testDec.Invoices.AddNew();
			var invoiceLine32 = entryLine3.InvoiceLines.AddNew();
			invoiceLine32.JI_JZ = invoice32.PK;
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a123";
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a123";
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a901";
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a345";
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a789";
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a123";
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a5";
			invoice32.ContractNumbers.AddNew().J2_ReferenceNumber = "a78901";
			AssertEquals(2, entry1.InvoiceHeaders.Length);
			AssertEquals("0123,4567,8901,45,a567,a901等", entry1.ContractNo);
			AssertEquals(1, entry2.InvoiceHeaders.Length);
			AssertEquals("01234567890123456789012345678912", entry2.ContractNo);
			AssertEquals(1, entry3.InvoiceHeaders.Length);
			AssertEquals("a123,a901,a345,a789,a5,a78901", entry3.ContractNo);
		}

		public void TestEntryNumberAndPreEntryNumber()
		{
			var cusentryNumberMrnZa = Factory.New<CusEntryNumber>();
			cusentryNumberMrnZa.CE_EntryType = "MRN";
			cusentryNumberMrnZa.CE_RN_NKCountryCode = "ZA";
			cusentryNumberMrnZa.CE_ParentID = testCusEntryHeader.PK;
			cusentryNumberMrnZa.CE_ParentTable = testCusEntryHeader.TableName;
			cusentryNumberMrnZa.CE_EntryNum = "MRN001ZA";
			var cusentryNumberPreZa = Factory.New<CusEntryNumber>();
			cusentryNumberPreZa.CE_EntryType = "PRE";
			cusentryNumberPreZa.CE_RN_NKCountryCode = "ZA";
			cusentryNumberPreZa.CE_ParentID = testCusEntryHeader.PK;
			cusentryNumberPreZa.CE_ParentTable = testCusEntryHeader.TableName;
			cusentryNumberPreZa.CE_EntryNum = "MRN001ZA";
			var cusEntryNumberPre = Factory.New<CusEntryNumber>();
			cusEntryNumberPre.CE_EntryType = "PRE";
			cusEntryNumberPre.CE_RN_NKCountryCode = "CN";
			cusEntryNumberPre.CE_ParentID = testCusEntryHeader.PK;
			cusEntryNumberPre.CE_ParentTable = testCusEntryHeader.TableName;
			cusEntryNumberPre.CE_EntryNum = "PRE001CN";
			var cusEntryNumberMrn = Factory.New<CusEntryNumber>();
			cusEntryNumberMrn.CE_EntryType = "MRN";
			cusEntryNumberMrn.CE_RN_NKCountryCode = "CN";
			cusEntryNumberMrn.CE_ParentID = testCusEntryHeader.PK;
			cusEntryNumberPre.CE_ParentTable = testCusEntryHeader.TableName;
			cusEntryNumberMrn.CE_EntryNum = "MRN001CN";
			Factory.Save();
			AssertEquals("MRN001CN", testCusEntryHeader.EntryNumber);
			AssertEquals("PRE001CN", testCusEntryHeader.PreEntryNumber);
		}

		public void TestTradeParty()
		{
			var importer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Importer Company", "", "ImporterSocialCrd", "ImporterCIQCode").Header;
			var supplier = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company", "", "SupplierSocialCrd", "SupplierCIQCode").Header;
			testDeclaration.JE_OH_Importer = importer.PK;
			testDeclaration.JE_OH_Supplier = supplier.PK;
			var customsEntryHeader = testCusEntryHeader as ICustomsEntryHeader;
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("Importer Company", customsEntryHeader.TradePartyName);
			AssertNullOrEmpty(customsEntryHeader.TradePartyCCD);
			AssertEquals("ImporterSocialCrd", customsEntryHeader.TradePartyUSCI);
			AssertEquals("ImporterCIQCode", customsEntryHeader.TradePartyCIQ);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.RecordListing);
			AssertEquals("Importer Company", customsEntryHeader.TradePartyName);
			AssertNullOrEmpty(customsEntryHeader.TradePartyCCD);
			AssertEquals("ImporterSocialCrd", customsEntryHeader.TradePartyUSCI);
			AssertEquals("ImporterCIQCode", customsEntryHeader.TradePartyCIQ);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("Supplier Company", customsEntryHeader.TradePartyName);
			AssertNullOrEmpty(customsEntryHeader.TradePartyCCD);
			AssertEquals("SupplierSocialCrd", customsEntryHeader.TradePartyUSCI);
			AssertEquals("SupplierCIQCode", customsEntryHeader.TradePartyCIQ);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.RecordListing);
			AssertEquals("Supplier Company", customsEntryHeader.TradePartyName);
			AssertNullOrEmpty(customsEntryHeader.TradePartyCCD);
			AssertEquals("SupplierSocialCrd", customsEntryHeader.TradePartyUSCI);
			AssertEquals("SupplierCIQCode", customsEntryHeader.TradePartyCIQ);
			importer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Importer Company", "ImpCode", "ImporterSocialCrd", "").Header;
			supplier = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company", "SupCode", "SupplierSocialCrd", "").Header;
			testDeclaration.JE_OH_Importer = importer.PK;
			testDeclaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("SupCode", customsEntryHeader.TradePartyCCD);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("ImpCode", customsEntryHeader.TradePartyCCD);
		}

		public void TestOwnerAndOverseaValues()
		{
			void AssertOwnerAndOverseaValues(CusEntryHeader testItem, string ownerOrgName, string ownerOrgCCD, string ownerOrgUSCI, string ownerOrgCIQ, string overseasOrgName)
			{
				var customsEntryHeader = testItem as ICustomsEntryHeader;
				CombineAssertions(() =>
				{
					AssertEquals("CargoOwnerName", ownerOrgName, customsEntryHeader.CargoOwnerName);
					AssertEquals("CargoOwnerCCD", ownerOrgCCD, customsEntryHeader.CargoOwnerCCD);
					AssertEquals("CargoOwnerUSCI", ownerOrgUSCI, customsEntryHeader.CargoOwnerUSCI);
					AssertEquals("CargoOwnerCIQ", ownerOrgCIQ, customsEntryHeader.CargoOwnerCIQ);
					AssertEquals("OverseasPartyName", overseasOrgName, customsEntryHeader.OverseasPartyName);
				}
				);
			}

			var supplier = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company Full Name", "SupplierCus1", "SupplierSocial1", "SupplierCIQ1", "Supplier Company").Header;
			var supplierCN = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Supplier Company China Full Name", "SupplierCus2", "SupplierSocial2", "SupplierCIQ2", "Supplier Company China").Header;
			supplierCN.OH_RL_NKClosestPort = "CNTES";
			var importer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Importer Company Full Name", "ImporterCus1", "ImporterSocial1", "ImporterCIQ1", "Importer Company").Header;
			var importerCN = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Importer Company China Full Name", "ImporterCus2", "ImporterSocial2", "ImporterCIQ2", "Importer Company China").Header;
			importerCN.OH_RL_NKClosestPort = "CNTES";
			var buyer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Buyer Company Full Name", "BuyerCus1", "BuyerSocial1", "BuyerCIQ1", "Buyer Company").Header;
			var manufacturer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Manufacturer Company Full Name", "ManufCus1", "ManufSocial1", "ManufCIQ1", "Manufacturer Company").Header;
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			testDeclaration.ImporterDocumentaryAddress.OrganisationPK = importer.PK;
			AssertOwnerAndOverseaValues(testCusEntryHeader, ownerOrgName: "Importer Company", ownerOrgCCD: "ImporterCus1", ownerOrgUSCI: "ImporterSocial1", ownerOrgCIQ: "ImporterCIQ1", overseasOrgName: "NO");
			testDeclaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertOwnerAndOverseaValues(testCusEntryHeader, ownerOrgName: "Importer Company", ownerOrgCCD: "ImporterCus1", ownerOrgUSCI: "ImporterSocial1", ownerOrgCIQ: "ImporterCIQ1", overseasOrgName: "Supplier Company");
			testDeclaration.BuyerDocAddress.OrganisationPK = buyer.PK;
			AssertOwnerAndOverseaValues(testCusEntryHeader, ownerOrgName: "Buyer Company", ownerOrgCCD: "BuyerCus1", ownerOrgUSCI: "BuyerSocial1", ownerOrgCIQ: "BuyerCIQ1", overseasOrgName: "Supplier Company");
			testDeclaration.SupplierDocumentaryAddress.OrganisationPK = supplierCN.PK;
			AssertOwnerAndOverseaValues(testCusEntryHeader, ownerOrgName: "Buyer Company", ownerOrgCCD: "BuyerCus1", ownerOrgUSCI: "BuyerSocial1", ownerOrgCIQ: "BuyerCIQ1", overseasOrgName: "NO");
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			testDeclaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertOwnerAndOverseaValues(testCusEntryHeader, ownerOrgName: "Supplier Company", ownerOrgCCD: "SupplierCus1", ownerOrgUSCI: "SupplierSocial1", ownerOrgCIQ: "SupplierCIQ1", overseasOrgName: "Importer Company");
			testDeclaration.JE_OH_Manufacturer = manufacturer.PK;
			AssertOwnerAndOverseaValues(testCusEntryHeader, ownerOrgName: "Manufacturer Company", ownerOrgCCD: "ManufCus1", ownerOrgUSCI: "ManufSocial1", ownerOrgCIQ: "ManufCIQ1", overseasOrgName: "Importer Company");
			testDeclaration.ImporterDocumentaryAddress.OrganisationPK = importerCN.PK;
			AssertOwnerAndOverseaValues(testCusEntryHeader, ownerOrgName: "Manufacturer Company", ownerOrgCCD: "ManufCus1", ownerOrgUSCI: "ManufSocial1", ownerOrgCIQ: "ManufCIQ1", overseasOrgName: "NO");
		}

		public void TestDeclarant()
		{
			var proxy = Factory.New<OrgHeader>();
			proxy.OH_Code = "Agent";
			proxy.OH_FullName = "Agent Company";
			var ccdCode = proxy.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = "CCD";
			ccdCode.OK_CustomsRegNo = "AgentComCode";
			var cacCode = proxy.CustomsCodes.AddNew();
			cacCode.OK_CodeType = "USC";
			cacCode.OK_CustomsRegNo = "AgentUSCICode";
			var ciqCode = proxy.CustomsCodes.AddNew();
			ciqCode.OK_CodeType = "CIQ";
			ciqCode.OK_CustomsRegNo = "AgentCIQCode";
			Factory.Save();
			testDeclaration.Branch.GB_OH_OrgProxy = proxy.PK;
			AssertEquals("Declarant", proxy.PK, testCusEntryHeader.Declarant.PK);
			var customsEntryHeader = testCusEntryHeader as ICustomsEntryHeader;
			AssertEquals("Agent Company", customsEntryHeader.DeclarantName);
			AssertEquals("AgentComCode", customsEntryHeader.DeclarantCCD);
			AssertEquals("AgentUSCICode", customsEntryHeader.DeclarantUSCI);
			AssertEquals("AgentCIQCode", customsEntryHeader.DeclarantCIQ);
		}

		public void TestCustomsOffice()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(new BusinessObjectFactory(), "CUSOF", "OFC", "Test Customs Office");
			AssertNullOrEmpty(testCusEntryHeader.CustomsOfficeCode);
			testDeclaration.JE_CustomsOffice = "OFC";
			AssertEquals("OFC", testCusEntryHeader.CustomsOfficeCode);
		}

		public void TestImportOrExportDate()
		{
			testDeclaration.JE_DateOfArrival = new ZDateTime(2017, 11, 9);
			testDeclaration.JE_ExportDate = new ZDateTime(2017, 12, 10);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			AssertEquals(new ZDateTime(2017, 11, 9), testCusEntryHeader.ImportOrExportDate);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.RecordListing);
			AssertEquals(new ZDateTime(2017, 11, 9), testCusEntryHeader.ImportOrExportDate);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			AssertEquals(new ZDateTime(2017, 12, 10), testCusEntryHeader.ImportOrExportDate);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.RecordListing);
			AssertEquals(new ZDateTime(2017, 12, 10), testCusEntryHeader.ImportOrExportDate);
		}

		public void TestDeclarantDate()
		{
			AssertEquals(ZDateTime.Empty, testCusEntryHeader.DeclarantDate);
			testCusEntryHeader.CH_EntrySubmittedDate = new ZDateTime(2017, 11, 9);
			testCusEntryHeader.SetMovementReferenceNumber("MRN", new ZDateTime(2019, 2, 22));
			AssertEquals(new ZDateTime(2019, 2, 22), testCusEntryHeader.DeclarantDate);
		}

		public void TestTransportMode()
		{
			testDeclaration.JE_CNTransportMode = "X";
			testDeclaration.JE_RL_NKPortOfLoading = "A";
			testDeclaration.JE_RL_NKPortOfArrival = "A";
			testDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("5", testCusEntryHeader.TransportModeCode);
			testDeclaration.JE_RL_NKPortOfLoading = "CN";
			testDeclaration.JE_RL_NKPortOfArrival = "CN";
			testCusEntryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			AssertEquals("X", testCusEntryHeader.TransportModeCode);
			testCusEntryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
			AssertEquals("9", testCusEntryHeader.TransportModeCode);
		}

		public void TestVesselNameAndVoyage()
		{
			testDeclaration.JE_VesselName = "BUNGA DELIMA";
			testDeclaration.JE_VoyageFlightNo = "001Y";
			testDeclaration.JE_MasterBill = "MasterBill1";
			testDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			testDeclaration.JE_CNTransportMode = CNTransportModeList.Codes.Sea;
			Factory.Save();
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			testDeclaration.DoMerge(notifier);
			var entryHeader = testDeclaration.CustomsEntryHeaders.FirstOrDefault();
			AssertNotNull(entryHeader);
			AssertEquals(testDeclaration.JE_VesselName, entryHeader.VesselName);
			AssertEquals(testDeclaration.JE_VoyageFlightNo, entryHeader.Voyage);
			var document = new CusDataHeaderDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.VesselName, document.VesselName);
			AssertEquals(entryHeader.Voyage, document.Voyage);
			testDeclaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			testDeclaration.JE_CNTransportMode = CNTransportModeList.Codes.Mail;
			Factory.Save();
			testDeclaration.DoMerge(notifier);
			entryHeader = testDeclaration.CustomsEntryHeaders.FirstOrDefault();
			AssertNotNull(entryHeader);
			AssertEquals(testDeclaration.JE_MasterBill, entryHeader.VesselName);
			AssertEquals(entryHeader.ImportOrExportDate.ToString("yyyyMMdd"), entryHeader.Voyage);
			document = new CusDataHeaderDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.VesselName, document.VesselName);
			AssertEquals(entryHeader.Voyage, document.Voyage);
			testDeclaration.JE_TransportMode = TransportTypeList.Codes.Road;
			testDeclaration.JE_CNTransportMode = CNTransportModeList.Codes.Road;
			Factory.Save();
			testDeclaration.DoMerge(notifier);
			entryHeader = testDeclaration.CustomsEntryHeaders.FirstOrDefault();
			AssertNotNull(entryHeader);
			AssertEquals(ZString.Empty, entryHeader.VesselName);
			AssertEquals(testDeclaration.JE_MasterBill, entryHeader.Voyage);
			document = new CusDataHeaderDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.VesselName, document.VesselName);
			AssertEquals(entryHeader.Voyage, document.Voyage);
			testDeclaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			testDeclaration.JE_CNTransportMode = CNTransportModeList.Codes.Rail;
			Factory.Save();
			testDeclaration.DoMerge(notifier);
			entryHeader = testDeclaration.CustomsEntryHeaders.FirstOrDefault();
			AssertNotNull(entryHeader);
			AssertEquals(testDeclaration.JE_VesselName, entryHeader.VesselName);
			AssertEquals(entryHeader.ImportOrExportDate.ToString("yyyyMMdd"), entryHeader.Voyage);
			document = new CusDataHeaderDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.VesselName, document.VesselName);
			AssertEquals(entryHeader.Voyage, document.Voyage);
			testDeclaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			testDeclaration.JE_CNTransportMode = CNTransportModeList.Codes.Others;
			Factory.Save();
			testDeclaration.DoMerge(notifier);
			entryHeader = testDeclaration.CustomsEntryHeaders.FirstOrDefault();
			AssertNotNull(entryHeader);
			AssertEquals("管道", entryHeader.VesselName);
			AssertEquals(ZString.Empty, entryHeader.Voyage);
			document = new CusDataHeaderDocumentWrapper(entryHeader);
			AssertEquals(entryHeader.VesselName, document.VesselName);
			AssertEquals(entryHeader.Voyage, document.Voyage);
		}

		public void TestBillNumber()
		{
			testInstruction.BillOfLading = "BILL001";
			AssertEquals("BILL001", testCusEntryHeader.BillOfLading);
		}

		public void TestDepartureDate()
		{
			testDeclaration.JE_DateAtOrigin = new ZDateTime(2017, 11, 1);
			testDeclaration.JE_ExportDate = new ZDateTime(2017, 11, 8);
			testDeclaration.JE_DateOfArrival = new ZDateTime(2017, 11, 9);
			AssertEquals(new ZDateTime(2017, 11, 8), testCusEntryHeader.DepartureDate);
		}

		public void TestCustomsProcedure()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping("CN");
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ProcedureCode = "AB";
			procedure1.ZZ6_ZZZ_NKDataGrouping = "CN";
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_Description = "Procedure AB";
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "CD";
			procedure2.ZZ6_ZZZ_NKDataGrouping = "CN";
			procedure2.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure2.ZZ6_Description = "Procedure CD";
			testInstruction.CEI_Style = "AB";
			AssertEquals("AB", testCusEntryHeader.CustomsProcedureCode);
			AssertEquals("Procedure AB", testCusEntryHeader.CustomsProcedureDesc);
		}

		public void TestLevyType()
		{
			testInstruction.CEI_LevyType = "401";
			AssertEquals("401", testCusEntryHeader.LevyTypeCode);
		}

		public void TestManualNo()
		{
			testInstruction.CEI_ManualNo = "MN001";
			AssertEquals("MN001", testCusEntryHeader.ManualNo);
		}

		public void TestOfficeOfEntryOrExit()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(new BusinessObjectFactory(), "CUSOF", "OFC", "Test Customs Office");
			testDeclaration.JE_OfficeOfEntryExit = "OFC";
			AssertEquals("OFC", testCusEntryHeader.OfficeOfEntryOrExitCode);
		}

		public void TestBLNo()
		{
			testInstruction.BillOfLading = "B1000001";
			AssertEquals("B1000001", testCusEntryHeader.BillOfLading);
		}

		public void TestCusCodes()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseCPW = warehouse.CustomsCodes.AddNew();
			warehouseCPW.OK_RN_NKCodeCountry = "CN";
			warehouseCPW.OK_CodeType = "CPW";
			warehouseCPW.OK_CustomsRegNo = "CPW1000001";
			warehouseCPW.OK_OA_PremisesAddress = warehouse.Addresses.FirstOrDefault().PK;
			var warehouseCPD = warehouse.CustomsCodes.AddNew();
			warehouseCPD.OK_RN_NKCodeCountry = "CN";
			warehouseCPD.OK_CodeType = "CPD";
			warehouseCPD.OK_CustomsRegNo = "CPD1000001";
			warehouseCPD.OK_OA_PremisesAddress = warehouse.Addresses.FirstOrDefault().PK;
			var warehouseContact = Factory.NewWithValidTestData<OrgContact>();
			warehouseContact.OC_OH = warehouse.PK;
			testDeclaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			testDeclaration.WarehouseDocAddress.ContactPK = warehouseContact.PK;
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			var depotCPW = depot.CustomsCodes.AddNew();
			depotCPW.OK_RN_NKCodeCountry = "CN";
			depotCPW.OK_CodeType = "CPW";
			depotCPW.OK_CustomsRegNo = "CPW1000002";
			depotCPW.OK_OA_PremisesAddress = depot.Addresses.FirstOrDefault().PK;
			var depotCPD = depot.CustomsCodes.AddNew();
			depotCPD.OK_RN_NKCodeCountry = "CN";
			depotCPD.OK_CodeType = "CPD";
			depotCPD.OK_CustomsRegNo = "CPD1000002";
			depotCPD.OK_OA_PremisesAddress = depot.Addresses.FirstOrDefault().PK;
			var depotContact = Factory.NewWithValidTestData<OrgContact>();
			depotContact.OC_OH = depot.PK;
			testDeclaration.DepotDocAddress.OrganisationPK = depot.PK;
			testDeclaration.DepotDocAddress.ContactPK = depotContact.PK;
			AssertEquals("CPW1000001", testCusEntryHeader.BondedAreaCode);
			AssertEquals("CPD1000002", testCusEntryHeader.FreightYardCode);
		}

		public void TestCountryOfTrade()
		{
			var orgUS = Factory.New<OrgHeader>();
			orgUS.MainAddress.OA_RN_NKCountryCode = "US";
			var orgGB = Factory.New<OrgHeader>();
			orgGB.MainAddress.OA_RN_NKCountryCode = "GB";
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			testDeclaration.JE_OH_Supplier = orgUS.PK;
			testDeclaration.JE_OH_Importer = orgGB.PK;
			AssertEquals("USA", testCusEntryHeader.CountryOfTradeCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.RecordListing);
			AssertEquals("USA", testCusEntryHeader.CountryOfTradeCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("GBR", testCusEntryHeader.CountryOfTradeCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.RecordListing);
			AssertEquals("GBR", testCusEntryHeader.CountryOfTradeCode);
		}

		public void TestCountryOfOriginOrDest()
		{
			testDeclaration.JE_RL_NKOrigin = "US";
			testDeclaration.JE_RL_NKFinalDestination = "GB";
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("USA", testCusEntryHeader.CountryOfLoadOrDischargeCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.RecordListing);
			AssertEquals("USA", testCusEntryHeader.CountryOfLoadOrDischargeCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("GBR", testCusEntryHeader.CountryOfLoadOrDischargeCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.RecordListing);
			AssertEquals("GBR", testCusEntryHeader.CountryOfLoadOrDischargeCode);
		}

		public void TestPortOfLoadOrDisc()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "PORT", "SH", "上海");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "PORT", "LD", "伦敦");
			testDeclaration.JE_CNPortOfOrigin = "SH";
			testDeclaration.JE_CNPortOfDestination = "LD";
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("SH", testCusEntryHeader.PortOfOriginOrDestCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, true, EntryTypeList.Codes.RecordListing);
			AssertEquals("SH", testCusEntryHeader.PortOfOriginOrDestCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.CustomsEntry);
			AssertEquals("LD", testCusEntryHeader.PortOfOriginOrDestCode);
			CNCusEntryHeaderHelper.SetDeclarationAndEntry(testCusEntryHeader, false, EntryTypeList.Codes.RecordListing);
			AssertEquals("LD", testCusEntryHeader.PortOfOriginOrDestCode);
		}

		public void TestLicenseNo()
		{
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("IsLicense", ZString.Empty);
			Factory.Save();
			var supportingDoc1 = testInvoiceLine.CusSupportingDocuments.AddNew();
			supportingDoc1.CSI_Code = "CD1";
			supportingDoc1.CSI_ReferenceNumber = "NUM1";
			AssertEquals("NUM1", testCusEntryHeader.LicenseNo);
		}

		public void TestIncoTerm()
		{
			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals(ShipmentIncoTerm.Codes.CIF, testCusEntryHeader.IncoTermCode);
			AssertEquals("CIF", testCusEntryHeader.IncoTermDesc);
			testCusEntryHeader.CH_MessageType = EntryTypeList.Codes.RecordListing;
			testDeclaration.JE_RL_NKPortOfLoading = "CN123";
			testDeclaration.JE_RL_NKPortOfArrival = "CN456";
			testDeclaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ShipmentIncoTerm.Codes.FOB, testCusEntryHeader.IncoTermCode);
			AssertEquals("FOB", testCusEntryHeader.IncoTermDesc);
			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(ShipmentIncoTerm.Codes.CIF, testCusEntryHeader.IncoTermCode);
			AssertEquals("CIF", testCusEntryHeader.IncoTermDesc);
		}

		public void TestFreightFee()
		{
			var test = new CusEntryLineTest();
			var entryHeader = test.SetupCusEntryWithIMPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.FreightFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.CurrencyCode);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.MarkCode);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithIMPAndFOB();
			AssertNotNull(entryHeader);
			AssertEquals(new ZDecimal(4), entryHeader.FreightFee.Amount);
			AssertEquals("CNY", entryHeader.FreightFee.CurrencyCode);
			AssertEquals("3", entryHeader.FreightFee.MarkCode);
			AssertEquals("总价", entryHeader.FreightFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithIMPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.FreightFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.CurrencyCode);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.MarkCode);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(new ZDecimal(4), entryHeader.FreightFee.Amount);
			AssertEquals("CNY", entryHeader.FreightFee.CurrencyCode);
			AssertEquals("3", entryHeader.FreightFee.MarkCode);
			AssertEquals("总价", entryHeader.FreightFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndFOB();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.FreightFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.CurrencyCode);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.MarkCode);
			AssertEquals(ZString.Empty, entryHeader.FreightFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(new ZDecimal(4), entryHeader.FreightFee.Amount);
			AssertEquals("CNY", entryHeader.FreightFee.CurrencyCode);
			AssertEquals("3", entryHeader.FreightFee.MarkCode);
			AssertEquals("总价", entryHeader.FreightFee.MarkDesc);
		}

		public void TestInsuranceFee()
		{
			var test = new CusEntryLineTest();
			var entryHeader = test.SetupCusEntryWithIMPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.InsuranceFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.CurrencyCode);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.MarkCode);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithIMPAndFOB();
			AssertNotNull(entryHeader);
			AssertEquals(new ZDecimal(5), entryHeader.InsuranceFee.Amount);
			AssertEquals(entryHeader.InsuranceFee.CurrencyCode, "");
			AssertEquals("1", entryHeader.InsuranceFee.MarkCode);
			AssertEquals("费率", entryHeader.InsuranceFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithIMPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(new ZDecimal(5), entryHeader.InsuranceFee.Amount);
			AssertEquals(entryHeader.InsuranceFee.CurrencyCode, "");
			AssertEquals("1", entryHeader.InsuranceFee.MarkCode);
			AssertEquals("费率", entryHeader.InsuranceFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(new ZDecimal(5), entryHeader.InsuranceFee.Amount);
			AssertEquals(entryHeader.InsuranceFee.CurrencyCode, "");
			AssertEquals("1", entryHeader.InsuranceFee.MarkCode);
			AssertEquals("费率", entryHeader.InsuranceFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndFOB();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.InsuranceFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.CurrencyCode);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.MarkCode);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.InsuranceFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.CurrencyCode);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.MarkCode);
			AssertEquals(ZString.Empty, entryHeader.InsuranceFee.MarkDesc);
		}

		public void TestOtherFee()
		{
			var test = new CusEntryLineTest();
			var entryHeader = test.SetupCusEntryWithIMPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(1m, entryHeader.OtherFee.Amount);
			AssertEquals("CNY", entryHeader.OtherFee.CurrencyCode);
			AssertEquals("3", entryHeader.OtherFee.MarkCode);
			AssertEquals("总价", entryHeader.OtherFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithIMPAndFOB();
			AssertNotNull(entryHeader);
			AssertEquals(1m, entryHeader.OtherFee.Amount);
			AssertEquals("CNY", entryHeader.OtherFee.CurrencyCode);
			AssertEquals("3", entryHeader.OtherFee.MarkCode);
			AssertEquals("总价", entryHeader.OtherFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithIMPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(1m, entryHeader.OtherFee.Amount);
			AssertEquals("CNY", entryHeader.OtherFee.CurrencyCode);
			AssertEquals("3", entryHeader.OtherFee.MarkCode);
			AssertEquals("总价", entryHeader.OtherFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndCIF();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.OtherFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.OtherFee.CurrencyCode);
			AssertEquals("", entryHeader.OtherFee.MarkCode);
			AssertEquals("", entryHeader.OtherFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndFOB();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.OtherFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.OtherFee.CurrencyCode);
			AssertEquals("", entryHeader.OtherFee.MarkCode);
			AssertEquals("", entryHeader.OtherFee.MarkDesc);
			entryHeader = test.SetupCusEntryWithEXPAndCFR();
			AssertNotNull(entryHeader);
			AssertEquals(ZDecimal.Zero, entryHeader.OtherFee.Amount);
			AssertEquals(ZString.Empty, entryHeader.OtherFee.CurrencyCode);
			AssertEquals("", entryHeader.OtherFee.MarkCode);
			AssertEquals("", entryHeader.OtherFee.MarkDesc);
		}

		public void TestNoOfPacks()
		{
			testInstruction.CEI_Packages = 5;
			AssertEquals(5, testCusEntryHeader.NoOfPacks);
		}

		public void TestPackType()
		{
			testInstruction.CEI_PackageUQ = PackageType.Codes.Bag;
			AssertEquals(PackageType.Codes.Bag, testCusEntryHeader.PackTypeCode);
			AssertEquals(PackageType.Descriptions.Bag, testCusEntryHeader.PackTypeDesc);
		}

		public void TestGrossWeightInKG()
		{
			testInvoiceLine.JI_Weight = 1111.1M;
			testInvoiceLine.JI_WeightUQ = "G";
			var invoiceline2 = testEntryLine.InvoiceLines.AddNew();
			invoiceline2.JI_Weight = 2211.1M;
			invoiceline2.JI_WeightUQ = "G";
			AssertEquals(3.32M, testCusEntryHeader.GrossWeightInKG);

			testInvoiceLine.JI_Weight = 0.0005M;
			invoiceline2.JI_Weight = 0.0004M;
			AssertEquals(0.01M, testCusEntryHeader.GrossWeightInKG);
		}

		public void TestNetWeightInKG()
		{
			testInvoiceLine.JI_NetWeight = 3.3111M;
			testInvoiceLine.JI_NetWeightUQ = "KG";
			var invoiceline2 = testEntryLine.InvoiceLines.AddNew();
			invoiceline2.JI_NetWeight = 4.4111M;
			invoiceline2.JI_NetWeightUQ = "KG";
			AssertEquals(7.72M, testCusEntryHeader.NetWeightInKG);

			testInvoiceLine.JI_NetWeight = 0.0005M;
			invoiceline2.JI_NetWeight = 0.0004M;
			AssertEquals(0.01M, testCusEntryHeader.NetWeightInKG);
		}

		public void TestMarksAndNotes()
		{
			AssertNullOrEmpty(testCusEntryHeader.Remarks);
		}

		public void TestConfirms()
		{
			testInvoiceHeader.JZ_SpecialRelationshipConfirm = "0";
			testInvoiceHeader.JZ_PriceAffectConfirm = "1";
			testInvoiceHeader.JZ_PaymentOfRoyaltyConfirm = "9";
			AssertEquals("0", testCusEntryHeader.SpecialRelationshipConfirmCode);
			AssertEquals("1", testCusEntryHeader.PriceAffectConfirmCode);
			AssertEquals("9", testCusEntryHeader.PaymentOfRoyaltyConfirmCode);
		}

		public void TestCIQOfficeOfEntryOrExit()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CIQPO", "211907", "庄河港");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CIQPO", "211906", "旅顺新港");
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDeclaration.JE_CIQOfficeOfEntryExit = "211906";
			AssertEquals("211906", testCusEntryHeader.CIQOfficeOfEntryOrExitCode);
		}

		public void TestLocationOfGoods()
		{
			testDeclaration.JE_LocationOfGoods = "货物存放地点";
			AssertEquals("货物存放地点", testCusEntryHeader.LocationOfGoods);
		}

		public void TestPortOfStopover()
		{
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "PORT", "USA264", "洛杉矶（美国）");
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDeclaration.JE_CNLastPortBeforeEntry = "XXX";
			AssertEquals("XXX", testCusEntryHeader.PortOfStopoverCode);
			testDeclaration.JE_CNLastPortBeforeEntry = "USA264";
			AssertEquals("USA264", testCusEntryHeader.PortOfStopoverCode);
		}

		public void TestOtherPackageCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			Assert(!entryHeader.OtherPackageCodes.Any());
			instruction.OtherPackages.AddNew("00");
			AssertContainsExactElementsInAnyOrder(new[] { "00" }, entryHeader.OtherPackageCodes);
			instruction.OtherPackages.AddNew("01");
			AssertContainsExactElementsInAnyOrder(new[] { "00", "01" }, entryHeader.OtherPackageCodes);
		}

		public void TestRemarks()
		{
			var testDec = Factory.New<JobDeclaration>();
			var testInstruction = testDec.CustomsEntryInstructions.AddNew();

			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = testInstruction.PK;

			var entryLine11 = entry1.MergedLines.AddNew();
			var entryLine12 = entry1.MergedLines.AddNew();
			var entryLine13 = entry1.MergedLines.AddNew();

			entryLine11.CL_LineNumber = (ZShort)2;
			entryLine12.CL_LineNumber = (ZShort)1;
			entryLine13.CL_LineNumber = (ZShort)3;

			var invoice1 = testDec.Invoices.AddNew();
			var invoice2 = testDec.Invoices.AddNew();
			var invoiceLine11 = invoice1.InvoiceLines.AddNew();
			var invoiceLine12 = invoice2.InvoiceLines.AddNew();
			var invoiceLine13 = invoice2.InvoiceLines.AddNew();
			invoiceLine11.JI_CL = entryLine11.PK;
			invoiceLine12.JI_CL = entryLine12.PK;
			invoiceLine13.JI_CL = entryLine13.PK;
			invoiceLine11.JI_CEI = testInstruction.PK;
			invoiceLine12.JI_CEI = testInstruction.PK;
			invoiceLine13.JI_CEI = testInstruction.PK;

			AssertEquals(ZString.Empty, entry1.Remarks);

			testInstruction.CustomsMessageRemarks = "CustomsMessageRemarks";
			AssertEquals("CustomsMessageRemarks", entry1.Remarks);

			entryLine11.RandomLine.FormulaPricingRecordNumber = "1111111111111";
			AssertEquals($@"公式定价1111111111111@ CustomsMessageRemarks", entry1.Remarks);

			entryLine12.RandomLine.FormulaPricingRecordNumber = "2222222222222";

			AssertEquals($@"公式定价2222222222222#1@ 公式定价1111111111111#2@ CustomsMessageRemarks", entry1.Remarks);

			testInstruction.CustomsMessageRemarks = ZString.Empty;
			AssertEquals($@"公式定价2222222222222#1@ 公式定价1111111111111#2@", entry1.Remarks);

			entryLine11.RandomLine.FormulaPricingRecordNumber = "";
			entryLine13.RandomLine.FormulaPricingRecordNumber = "333333333333";
			AssertEquals($@"公式定价2222222222222#1@ 公式定价333333333333#3@", entry1.Remarks);
		}

		public void TestMarksAndNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction1.PK;
			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = (JobComInvoiceLine)invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction1.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entry = declaration.CustomsEntryHeaders[0];
			declaration.JE_MarksAndNumbers = "";
			AssertEquals(Core.Constants.ContainerMarking.NoMarks, entry.MarksAndNumbers);
			invoice1.JZ_MarksAndNumbers = Core.Constants.ContainerMarking.NoMarks;
			invoice3.JZ_MarksAndNumbers = Core.Constants.ContainerMarking.NoMarks;
			AssertEquals(Core.Constants.ContainerMarking.NoMarks, entry.MarksAndNumbers);
			declaration.JE_MarksAndNumbers = "JE_MarksAndNumbers";
			AssertEquals("JE_MarksAndNumbers", entry.MarksAndNumbers);
			invoice1.JZ_MarksAndNumbers = "JZ_MarksAndNumbers1";
			invoice3.JZ_MarksAndNumbers = "JZ_MarksAndNumbers3";
			AssertContains(";", entry.MarksAndNumbers);
			AssertContains("JZ_MarksAndNumbers1", entry.MarksAndNumbers);
			AssertContains("JZ_MarksAndNumbers3", entry.MarksAndNumbers);
			AssertContains("JE_MarksAndNumbers", entry.MarksAndNumbers);
		}

		public void TestDocumentSubmissionType()
		{
			testInstruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms;
			AssertEquals(EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms, testCusEntryHeader.DocumentSubmissionTypeCode);
		}

		public void TestOperationMatters()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			Assert("IsPaperlessTaxForm", !entryHeader.IsPaperlessTaxForm);
			Assert("IsAutonomousTaxFiling", !entryHeader.IsAutonomousTaxFiling);
			Assert("IsAssuredInspectClearance", !entryHeader.IsAssuredInspectClearance);
			instruction.OperationMatters.AddNew(OperationMatterList.Codes.PaperlessTaxForm);
			Assert("IsPaperlessTaxForm", entryHeader.IsPaperlessTaxForm);
			Assert("IsAutonomousTaxFiling", !entryHeader.IsAutonomousTaxFiling);
			Assert("IsAssuredInspectClearance", !entryHeader.IsAssuredInspectClearance);
			instruction.OperationMatters.AddNew(OperationMatterList.Codes.AutonomousTaxFiling);
			Assert("IsPaperlessTaxForm", entryHeader.IsPaperlessTaxForm);
			Assert("IsAutonomousTaxFiling", entryHeader.IsAutonomousTaxFiling);
			Assert("IsAssuredInspectClearance", !entryHeader.IsAssuredInspectClearance);
			instruction.OperationMatters.AddNew(OperationMatterList.Codes.AssuredInspectClearance);
			Assert("IsPaperlessTaxForm", entryHeader.IsPaperlessTaxForm);
			Assert("IsAutonomousTaxFiling", entryHeader.IsAutonomousTaxFiling);
			Assert("IsAssuredInspectClearance", entryHeader.IsAssuredInspectClearance);
		}

		public void TestCIQDetailsRelatedProperties()
		{
			var testBondage = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			var declaration = testBondage.JobDeclaration;
			var entryHeader = testBondage.EntryHeader;
			declaration.OfficeOfDestination = "2301";
			AssertEquals("2301", entryHeader.OfficeOfDestination);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "Ocean Bill 1";
			AssertEquals("Ocean Bill 1", entryHeader.BillNumber);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert(entryHeader.BillNumber.IsEmpty);
			AssertNoExceptionThrown(() => _ = entryHeader.IsOriginalContainerLoading);
			var invoiceLine = testBondage.InvoiceLine;
			AssertEquals(ZString.Empty, entryHeader.IsOriginalContainerLoading);
			invoiceLine.JI_OrigContainerFlag = ConfirmationTypeList.Codes.No;
			AssertEquals(ConfirmationTypeList.Codes.No, entryHeader.IsOriginalContainerLoading);
			invoiceLine.JI_OrigContainerFlag = ConfirmationTypeList.Codes.Yes;
			AssertEquals(ConfirmationTypeList.Codes.Yes, entryHeader.IsOriginalContainerLoading);
			var instruction = testBondage.EntryInstruction;
			instruction.CEI_CIQRelatedReason = CIQRelation.Codes._1;
			AssertEquals(CIQRelation.Codes._1, entryHeader.CIQRelatedReason);
			var importer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Importer Company China Full Name", "ImporterCus2", "ImporterSocial2", "ImporterCIQ2", "Importer Company China").Header;
			var impContact = importer.Contacts.AddNew();
			impContact.OC_ContactName = "Importer Contact";
			impContact.OC_Phone = "+86-156-0113-1981";
			var buyer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Buyer Company Full Name", "BuyerCus1", "BuyerSocial1", "BuyerCIQ1", "Buyer Company").Header;
			var byrContact = buyer.Contacts.AddNew();
			byrContact.OC_ContactName = "Buyer Contact";
			byrContact.OC_Phone = "222";
			declaration.JE_MessageType = "IMP";
			declaration.ImporterDocumentaryAddress.OrganisationPK = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_Contact = "Importer Contact";
			AssertEquals("Importer Contact", entryHeader.ConsumerContactName);
			AssertEquals("156 0113 1981", entryHeader.ConsumerContactPhone);
			declaration.BuyerDocAddress.OrganisationPK = buyer.PK;
			declaration.BuyerDocAddress.E2_Contact = "Buyer Contact";
			AssertEquals("Buyer Contact", entryHeader.ConsumerContactName);
			AssertEquals("", entryHeader.ConsumerContactPhone);
		}

		public void TestReadyForCompleteDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryHeader.CH_Status = JobMessageStatusList.Codes.ClearedPreliminaryDeclaration;
			Assert("ReadyForCompleteDeclaration should return true", entryHeader.ReadyForCompleteDeclaration);
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepManual;
			Assert("ReadyForCompleteDeclaration should return true", entryHeader.ReadyForCompleteDeclaration);
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepAuto;
			Assert("ReadyForCompleteDeclaration should return true", entryHeader.ReadyForCompleteDeclaration);
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;

			entryHeader.CH_Status = JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration;
			Assert("ReadyForCompleteDeclaration should return false", !entryHeader.ReadyForCompleteDeclaration);

			entryHeader.CH_Status = JobMessageStatusList.Codes.ClearedCompletedDeclaration;
			Assert("ReadyForCompleteDeclaration should return false", !entryHeader.ReadyForCompleteDeclaration);

			declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;

			entryHeader.CH_Status = JobMessageStatusList.Codes.ClearedPreliminaryDeclaration;
			Assert("ReadyForCompleteDeclaration should return false", !entryHeader.ReadyForCompleteDeclaration);
		}

		public void TestACDANumber()
		{
			var cusEntryHeader1 = Factory.NewWithValidTestData<CusEntryHeader>();
			var cusEntryInstruction1 = Factory.NewWithValidTestData<CusEntryInstruction>();
			var cusEntryHeader2 = Factory.NewWithValidTestData<CusEntryHeader>();
			var cusEntryInstruction2 = Factory.NewWithValidTestData<CusEntryInstruction>();
			var cusAttachment1 = cusEntryInstruction1.Attachments.AddNew();
			cusAttachment1.AttachmentType = CSDDocTypeList.Codes._10000001;
			cusAttachment1.AttachmentNumber = "12345678910111213";
			var cusAttachment2 = cusEntryInstruction2.Attachments.AddNew();
			cusAttachment2.AttachmentType = CSDDocTypeList.Codes._10000002;
			cusAttachment2.AttachmentNumber = "12345678910111213";
			cusEntryHeader1.CH_CEI_Instruction = cusEntryInstruction1.PK;
			cusEntryHeader2.CH_CEI_Instruction = cusEntryInstruction2.PK;
			Factory.Save();
			AssertEquals("cusEntryHeader1.ACDANumber should be 12345678910111213", "12345678910111213", cusEntryHeader1.ACDANumber);
			AssertEquals("cusEntryHeader2.ACDANumber should be empty", ZString.Empty, cusEntryHeader2.ACDANumber);
		}

		public void TestFormulaPricingConfirmCode()
		{
			testInvoiceHeader.JZ_Calc_FormulaPricingConfirm = null;
			AssertEquals(ZString.Empty, testCusEntryHeader.FormulaPricingConfirmCode);

			testInvoiceHeader.JZ_Calc_FormulaPricingConfirm = "1";
			AssertEquals("1", testCusEntryHeader.FormulaPricingConfirmCode);

			testInvoiceHeader.JZ_Calc_FormulaPricingConfirm = "2";
			AssertEquals("2", testCusEntryHeader.FormulaPricingConfirmCode);

			testInvoiceHeader.JZ_Calc_FormulaPricingConfirm = "9";
			AssertEquals("9", testCusEntryHeader.FormulaPricingConfirmCode);
		}

		public void TestTemporaryPricingConfirmCode()
		{
			testInvoiceHeader.JZ_Calc_TemporaryPricingConfirm = null;
			AssertEquals(ZString.Empty, testCusEntryHeader.TemporaryPricingConfirmCode);

			testInvoiceHeader.JZ_Calc_TemporaryPricingConfirm = "1";
			AssertEquals("1", testCusEntryHeader.TemporaryPricingConfirmCode);

			testInvoiceHeader.JZ_Calc_TemporaryPricingConfirm = "2";
			AssertEquals("2", testCusEntryHeader.TemporaryPricingConfirmCode);

			testInvoiceHeader.JZ_Calc_TemporaryPricingConfirm = "9";
			AssertEquals("9", testCusEntryHeader.TemporaryPricingConfirmCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var setup = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testCusEntryHeader = setup.EntryHeader;
			testDeclaration = setup.JobDeclaration;
			testEntryLine = setup.EntryLine;
			testInvoiceHeader = setup.InvoiceHeader;
			testInvoiceLine = setup.InvoiceLine;
			testInstruction = setup.EntryInstruction;
		}

		CusEntryHeader testCusEntryHeader;
		CusEntryLine testEntryLine;
		JobDeclaration testDeclaration;
		JobComInvoiceHeader testInvoiceHeader;
		JobComInvoiceLine testInvoiceLine;
		CusEntryInstruction testInstruction;
	}
}
