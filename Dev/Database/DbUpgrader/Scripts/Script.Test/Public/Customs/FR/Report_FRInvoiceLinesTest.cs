using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.FR.Testing
{
	[TestedType(typeof(Report_FRInvoiceLines))]
	class Report_FRInvoiceLinesTest : CustomsReportDbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		#region Columns

		public void TestLinePK()
		{
			AssertFunctionReturnExpectedValue("LinePK", invoiceLinePK);
		}

		public void TestJobNumber()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "XXYYZZ", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("JobNumber", "XXYYZZ");
		}

		public void TestJobCreatedDate()
		{
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);
			AssertFunctionReturnExpectedValue("JobCreatedDate", new DateTime(2020, 01, 01));
		}

		public void TestBrokerDeclarant()
		{
			UpdateDeclarationColumn("JE_GS_NKCusAgent", "E", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("BrokerDeclarant", "CargoWise Support");
		}

		public void TestDeltaReference()
		{
			UpdateDeclarationColumn("JE_MessageType", "IMP", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR");
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			AssertFunctionReturnExpectedValue("DeltaReference", "1A");
		}

		public void TestJobRegisteredDate()
		{
			TestDataCreator.CreateStmALog("JobDeclaration", declarationPK, "", new DateTime(2020, 01, 01), "CLR");
			AssertFunctionReturnExpectedValue("JobRegisteredDate", new DateTime(2020, 01, 01));
		}

		public void TestBAEDateTime()
		{
			AssertFunctionReturnExpectedValue("BAEDateTime", DBNull.Value);

			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR", new DateTime(2020, 01, 01, 11, 15, 00));
			AssertFunctionReturnExpectedValue("BAEDateTime", new DateTime(2020, 01, 01, 11, 15, 00));
		}

		public void TestEntryStatus()
		{
			AssertFunctionReturnExpectedValue("EntryStatus", DBNull.Value);

			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR", new DateTime(2020, 01, 01));
			AssertFunctionReturnExpectedValue("EntryStatus", "BAE");
		}

		public void TestCustomsOfficeCAU()
		{
			TestDataCreator.CreateCusCodeData("EUO", "CAU", "FR123456", declarationPK, parentTableCode: "JE");
			AssertFunctionReturnExpectedValue("CustomsOfficeCAU", "FR123456");
		}

		public void TestCustomsOfficeENT()
		{
			TestDataCreator.CreateCusCodeData("EUO", "ENT", "FR123456", declarationPK, parentTableCode: "JE");
			AssertFunctionReturnExpectedValue("CustomsOfficeENT", "FR123456");
		}

		public void TestDeltaMode()
		{
			TestDataCreator.CreateGenAddOnColumn(declarationPK, "JE", "JE_DeltaMode", "STR", "G1");
			AssertFunctionReturnExpectedValue("DeltaMode", "G1");
		}

		public void TestCustomsProfile()
		{
			UpdateDeclarationColumn("JE_CustomsProfile", "AAA", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("CustomsProfile", "AAA");
		}

		public void TestTransportMode()
		{
			UpdateDeclarationColumn("JE_TransportMode", "ROA", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("TransportMode", "ROA");
		}

		public void TestJobType()
		{
			UpdateDeclarationColumn("JE_MessageType", "IMP", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("JobType", "IMP");
		}

		public void TestCpc()
		{
			UpdateInvoiceLineColumn("JI_Procedure", "4000", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("Cpc", "4000");
		}

		public void TestContainerMode()
		{
			UpdateDeclarationColumn("JE_ContainerMode", "FCL", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("ContainerMode", "FCL");
		}

		public void TestTransportModeInland()
		{
			UpdateDeclarationColumn("JE_TransportModeInland", "SEA", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("TransportModeInland", "SEA");
		}

		public void TestBranchCode()
		{
			AssertFunctionReturnExpectedValue("BranchCode", "TB1");
		}

		public void TestStandalone()
		{
			AssertFunctionReturnExpectedValue("Standalone", "Y");

			var shipmentPK = TestDataCreator.CreateShipment("123456");
			UpdateDeclarationColumn("JE_JS", shipmentPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("Standalone", "N");
		}

		public void TestInvoiceNo()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "123456", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceNo", "123456");
		}

		public void TestInvoiceLineAmount()
		{
			UpdateInvoiceLineColumn("JI_LinePrice", 100.1m, SqlDbType.Decimal, invoiceLinePK);
			AssertFunctionReturnExpectedValue("InvoiceLineAmount", 100.1m);
		}

		public void TestInvoiceLineCurrency()
		{
			UpdateInvoiceColumn("JZ_RX_NKInvoice_Currency", "EUR", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceLineCurrency", "EUR");
		}

		public void TestCurrencyRate()
		{
			UpdateInvoiceColumn("JZ_InvoiceCurrExRate", 0.99m, SqlDbType.Decimal, invoicePK);
			AssertFunctionReturnExpectedValue("CurrencyRate", 0.99m);
		}

		public void TestIncoTerm()
		{
			UpdateInvoiceColumn("JZ_IncoTerm", "FOB", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("IncoTerm", "FOB");
		}

		public void TestIncotermPlace()
		{
			UpdateInvoiceColumn("JZ_IncoTermPlace", "1", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("IncoTermPlace", "1");
		}

		public void TestSupplierPK()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);

			var decSupplierPK = TestDataCreator.CreateOrganisation("DECSUPCODE", "DECSUPNAME");
			UpdateDeclarationColumn("JE_OH_Supplier", decSupplierPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("SupplierPK", invSupplierPK);

			UpdateInvoiceColumn("JZ_OH_Supplier", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);
			AssertFunctionReturnExpectedValue("SupplierPK", decSupplierPK);
		}

		public void TestSupplierEoriFallbacksToForeignIfNecessary()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			var addressPk = TestDataCreator.CreateAddress(invSupplierPK, "ADD2", "There");

			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);
			UpdateInvoiceColumn("JZ_OA_SupplierAddress", addressPk, SqlDbType.UniqueIdentifier, invoicePK);

			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EOR", "GBEORI_CODE", "GB");
			AssertFunctionReturnExpectedValue("SupplierEori", "GBEORI_CODE");

			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EOR", "FREORI_CODE", "FR");
			AssertFunctionReturnExpectedValue("SupplierEori", "FREORI_CODE");
		}

		public void TestSupplierEoriSuffixFallbacksToEORIWithoutAddressIfNecessary()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			var addressPK1 = TestDataCreator.CreateAddress(invSupplierPK, "ADD1", "Here");
			var addressPK2 = TestDataCreator.CreateAddress(invSupplierPK, "ADD2", "There");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);
			UpdateInvoiceColumn("JZ_OA_SupplierAddress", addressPK2, SqlDbType.UniqueIdentifier, invoicePK);

			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EOR", "FREORI_CODE", "FR");
			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EBS", "00000", "FR");
			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EBS", "00001", "FR", addressPK1);

			AssertFunctionReturnExpectedValue("SupplierEori", "FREORI_CODE");
			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", "00000");
		}

		public void TestSupplierEoriSuffixFiltersOnCorrectAddressWhenAble()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			var addressPK1 = TestDataCreator.CreateAddress(invSupplierPK, "ADD1", "Here");
			var addressPK2 = TestDataCreator.CreateAddress(invSupplierPK, "ADD2", "There");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);
			UpdateInvoiceColumn("JZ_OA_SupplierAddress", addressPK1, SqlDbType.UniqueIdentifier, invoicePK);

			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EOR", "FREORI_CODE", "FR");
			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EBS", "00000", "FR");
			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EBS", "00001", "FR", addressPK1);
			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EBS", "00002", "FR", addressPK2);
			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", "00001");
		}

		public void TestSupplierEoriSuffixFallbacksToDeclarationSupplierEORISuffix()
		{
			UpdateInvoiceColumn("JZ_OH_Supplier", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);
			UpdateInvoiceColumn("JZ_OA_SupplierAddress", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);

			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			var addressPk = TestDataCreator.CreateAddress(supplierPk, "ADD1", "Here");
			TestDataCreator.CreateDocAddress(addressPk, "", declarationPK, "JE", "SUD");

			UpdateDeclarationColumn("JE_OH_Supplier", supplierPk, SqlDbType.UniqueIdentifier, declarationPK);
			TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "FREORI_CODE", "FR");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00001", "FR");
			AssertFunctionReturnExpectedValue("SupplierEori", "FREORI_CODE");

			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", "00001");
		}

		public void TestSupplierEoriFallbacksToDeclarationSupplierEORI()
		{
			UpdateInvoiceColumn("JZ_OH_Supplier", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);
			UpdateInvoiceColumn("JZ_OA_SupplierAddress", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);

			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			UpdateDeclarationColumn("JE_OH_Supplier", supplierPk, SqlDbType.UniqueIdentifier, declarationPK);
			TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "GBEORI_CODE", "GB");
			AssertFunctionReturnExpectedValue("SupplierEori", "GBEORI_CODE");
		}

		public void TestSupplierEoriSuffixEmptyWhenEORINotFrench()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			var addressPK1 = TestDataCreator.CreateAddress(invSupplierPK, "ADD1", "Here");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);
			UpdateInvoiceColumn("JZ_OA_SupplierAddress", addressPK1, SqlDbType.UniqueIdentifier, invoicePK);

			var eori = TestDataCreator.CreateOrgCusCode(invSupplierPK, "EORI", "12345", "GB");
			TestDataCreator.CreateOrgCusCode(invSupplierPK, "EBS", "00001", "FR", addressPK1);
			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", string.Empty);

			UpdateTableColumn("OrgCusCode", "OK_RN_NKCodeCountry", "FR", SqlDbType.VarChar, "OK_PK", eori);
			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", "00001");
		}

		public void TestSupplierCode()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);

			var decSupplierPK = TestDataCreator.CreateOrganisation("DECSUPCODE", "DECSUPNAME");
			UpdateDeclarationColumn("JE_OH_Supplier", decSupplierPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("SupplierCode", "INVSUPCODE");

			UpdateInvoiceColumn("JZ_OH_Supplier", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);
			AssertFunctionReturnExpectedValue("SupplierCode", "DECSUPCODE");
		}

		public void TestSupplierName()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);

			var decSupplierPK = TestDataCreator.CreateOrganisation("DECSUPCODE", "DECSUPNAME");
			UpdateDeclarationColumn("JE_OH_Supplier", decSupplierPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("SupplierName", "INVSUPNAME");

			UpdateInvoiceColumn("JZ_OH_Supplier", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);
			AssertFunctionReturnExpectedValue("SupplierName", "DECSUPNAME");
		}

		public void TestImporterPK()
		{
			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterPK", importerPK);
		}

		public void TestImporterCode()
		{
			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterCode", "IMPCODE");
		}

		public void TestImporterName()
		{
			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterName", "IMPNAME");
		}

		public void TestCountryOfSupply()
		{
			UpdateInvoiceLineColumn("JI_AddInfo", "CountryOfSupply=IT*", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("CountryOfSupply", "IT");
		}

		public void TestCountryOfDestination()
		{
			UpdateDeclarationColumn("JE_GoodsDestination", "IT", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("CountryOfDestination", "IT");
		}

		public void TestOriginCountry()
		{
			UpdateInvoiceLineColumn("JI_CountryOfOrigin", "IT", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("OriginCountry", "IT");
		}

		public void TestPreferenceCode()
		{
			UpdateInvoiceLineColumn("JI_PrimaryPreference", "100", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("PreferenceCode", "100");
		}

		public void TestTotalWeight()
		{
			UpdateInvoiceLineColumn("JI_Weight", 123m, SqlDbType.Decimal, invoiceLinePK);
			AssertFunctionReturnExpectedValue("TotalWeight", 123m);
		}

		public void TestTotalWeightUQ()
		{
			UpdateInvoiceLineColumn("JI_WeightUQ", "KG", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("TotalWeightUQ", "KG");
		}

		public void TestVolume()
		{
			UpdateInvoiceLineColumn("JI_Volume", 123m, SqlDbType.Decimal, invoiceLinePK);
			AssertFunctionReturnExpectedValue("Volume", 123m);
		}

		public void TestVolumeUQ()
		{
			UpdateInvoiceLineColumn("JI_VolumeUQ", "M3", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("VolumeUQ", "M3");
		}

		public void TestTariff()
		{
			UpdateInvoiceLineColumn("JI_Tariff", "123456", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("Tariff", "123456");
		}

		public void TestJobStatus()
		{
			var department1Pk = TestDataCreator.CreateDepartment("DEP");
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, department1Pk, "JE", "B00001", "B", "WRK");
			AssertFunctionReturnExpectedValue("JobStatus", "WRK");
		}

		public void TestSummaryDeclaration()
		{
			TestDataCreator.CreateCusSupportingInfo("PRE", "380", "", "", invoiceLinePK, "JI", "FR");
			AssertFunctionReturnExpectedValue("SummaryDeclaration", DBNull.Value);

			TestDataCreator.CreateCusSupportingInfo("PRE", "XXX", "YYY", "ZZZ", invoiceLinePK, "JI", "FR");
			AssertFunctionReturnExpectedValue("SummaryDeclaration", "XXX YYY ZZZ");

			TestDataCreator.CreateCusSupportingInfo("PRE", "AAA", "BBB", "CCC", invoiceLinePK, "JI", "FR");
			AssertFunctionReturnExpectedValue("SummaryDeclaration", "MLP");
		}

		public void TestContainerID()
		{
			var container1PK = TestDataCreator.CreateCusContainer("123", declarationPK, 1, "FR");
			var container2PK = TestDataCreator.CreateCusContainer("456", declarationPK, 1, "FR");
			TestDataCreator.CreateCusContainerInvoiceLinePivot(container1PK, invoiceLinePK, 1);
			TestDataCreator.CreateCusContainerInvoiceLinePivot(container2PK, invoiceLinePK, 1);
			AssertFunctionReturnExpectedValue("ContainerID", "123/456");
		}

		public void TestOrigin()
		{
			UpdateDeclarationColumn("JE_RL_NKOrigin", "AUSYD", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("Origin", "AUSYD");
		}

		public void TestOriginETD()
		{
			UpdateDeclarationColumn("JE_DateAtOrigin", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);
			AssertFunctionReturnExpectedValue("OriginETD", new DateTime(2020, 01, 01));
		}

		public void TestDestETA()
		{
			UpdateDeclarationColumn("JE_DateAtFinalDestination", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);
			AssertFunctionReturnExpectedValue("DestETA", new DateTime(2020, 01, 01));
		}

		public void TestLoadADT()
		{
			UpdateDeclarationColumn("JE_ExportDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);
			AssertFunctionReturnExpectedValue("LoadADT", new DateTime(2020, 01, 01));
		}

		public void TestArrivalATA()
		{
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);
			AssertFunctionReturnExpectedValue("ArrivalATA", new DateTime(2020, 01, 01));
		}

		public void TestFirstArrivalATA()
		{
			UpdateDeclarationColumn("JE_DateOfFirstArrival", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);
			AssertFunctionReturnExpectedValue("FirstArrivalATA", new DateTime(2020, 01, 01));
		}

		public void TestHouseBill()
		{
			UpdateDeclarationColumn("JE_HouseBill", "XXYYZZ", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("HouseBill", "XXYYZZ");
		}

		public void TestMasterBill()
		{
			UpdateDeclarationColumn("JE_MasterBill", "XXYYZZ", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("MasterBill", "XXYYZZ");
		}

		public void TestVessel()
		{
			UpdateDeclarationColumn("JE_VesselName", "XXYYZZ", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("Vessel", "XXYYZZ");
		}

		public void TestVoyageFlight()
		{
			UpdateDeclarationColumn("JE_VoyageFlightNo", "XXYYZZ", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("VoyageFlight", "XXYYZZ");
		}

		public void TestBranchPK()
		{
			AssertFunctionReturnExpectedValue("BranchPK", branchPK);
		}

		#endregion

		#region Filters

		public void TestFilterByCompanyPK()
		{
			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "FR", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "XXYYZZ");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "IMP", 2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK2, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "LinePK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.CompanyPK, companyPK));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.CompanyPK, companyPK2));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK2 }, filteredRows);
		}

		public void TestFilterByJobCreateTime()
		{
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 01, 10), SqlDbType.SmallDateTime, declarationPK2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK2, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "LinePK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.CreateDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.CreateDateFrom, new DateTime(2020, 01, 01)), (Report_FRInvoiceLinesParameter.CreateDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.CreateDateFrom, new DateTime(2020, 01, 05)), (Report_FRInvoiceLinesParameter.CreateDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<Guid>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.CreateDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK2 }, filteredRows);
		}

		public void TestFilterByProductCode()
		{
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 2);
			UpdateInvoiceLineColumn("JI_PartNo", "AAA", SqlDbType.VarChar, invoiceLinePK);
			UpdateInvoiceLineColumn("JI_PartNo", "BBB", SqlDbType.VarChar, invoiceLinePK2);

			var filteredRows = GetFilteredRows(selectedColumn: "LinePK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.ProductCode, "AAA"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.ProductCode, "BBB"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK2 }, filteredRows);
		}

		public void TestFilterByProductDescription()
		{
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 2);
			UpdateInvoiceLineColumn("JI_Description", "LIVE ANIMALS OTHER LIVE ANIMALS WHALES", SqlDbType.VarChar, invoiceLinePK);
			UpdateInvoiceLineColumn("JI_Description", "GLASS AND GLASSWARE GLASS FIBRES (INCLUDING GLASS WOOL)", SqlDbType.VarChar, invoiceLinePK2);

			var filteredRows = GetFilteredRows(selectedColumn: "LinePK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.ProductDescription, "OTHER"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.ProductDescription, "GLASS"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK2 }, filteredRows);
		}

		public void TestFilterByShipmentType()
		{
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK2, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "LinePK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.ShipmentType, "IMP"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.ShipmentType, "EXP"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK2 }, filteredRows);
		}

		public void TestFilterByInvoiceNumber()
		{
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 2);
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK2, 1);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows(selectedColumn: "LinePK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.InvoiceNumber, "111"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.InvoiceNumber, "222"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK2 }, filteredRows);
		}

		public void TestFilterByDeltaReference()
		{
			UpdateDeclarationColumn("JE_MessageType", "IMP", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR");
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);

			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), entryInstructionPK2, "", new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "2B", "IMP", "", "FR");
			var entryLinePK2 = TestDataCreator.CreateCusEntryLine(entryHeaderPK2, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);

			var filteredRows = GetFilteredRows(selectedColumn: "LinePK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK, invoiceLinePK2 }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.DeltaReference, "1A"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "LinePK", (Report_FRInvoiceLinesParameter.DeltaReference, "2B"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { invoiceLinePK2 }, filteredRows);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "FR", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "FRANCE");
			declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1);
			invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
		}
		Guid companyPK;
		Guid branchPK;
		Guid declarationPK;
		Guid invoicePK;
		Guid invoiceLinePK;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_FRInvoiceLinesParameter.CompanyPK);
			yield return (SqlDbType.SmallDateTime, Report_FRInvoiceLinesParameter.CreateDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_FRInvoiceLinesParameter.CreateDateTo);
			yield return (SqlDbType.VarChar, Report_FRInvoiceLinesParameter.ProductCode);
			yield return (SqlDbType.VarChar, Report_FRInvoiceLinesParameter.ProductDescription);
			yield return (SqlDbType.VarChar, Report_FRInvoiceLinesParameter.ShipmentType);
			yield return (SqlDbType.VarChar, Report_FRInvoiceLinesParameter.InvoiceNumber);
			yield return (SqlDbType.VarChar, Report_FRInvoiceLinesParameter.DeltaReference);
		}

		class Report_FRInvoiceLinesParameter
		{
			public const string CompanyPK = "@CompanyPK";
			public const string CreateDateFrom = "@CreateDateFrom";
			public const string CreateDateTo = "@CreateDateTo";
			public const string ProductCode = "@ProductCode";
			public const string ProductDescription = "@ProductDescription";
			public const string ShipmentType = "@ShipmentType";
			public const string InvoiceNumber = "@InvoiceNumber";
			public const string DeltaReference = "@DeltaReference";
		}
		#endregion
	}
}
