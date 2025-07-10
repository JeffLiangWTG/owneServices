using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(SimplifiedLVS))]
	sealed class SimplifiedLVSTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "CA Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "US Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "1111", "CA Customs Port Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "CA Customs Office Code (Expired)", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var simLVS = new SimplifiedLVS(Factory);
			var list = simLVS.Offices;
			list.Load();

			AssertEquals(1, list.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1111"));
			AssertEquals("CA Customs Office Code", list.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == "1111").ZZD_Description);
			Assert(!list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1234"));
		}

		public void TestPortOfClearance()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "CA Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "US Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "1111", "CA Customs Port Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "CA Customs Office Code (Expired)", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var simLVS = new SimplifiedLVS(Factory);
			simLVS.CA_PortOfClearance = "1111";
			var portOfClearance = simLVS.PortOfClearance;
			AssertNotNull(portOfClearance);
			AssertEquals("CA Customs Office Code", portOfClearance.ZZD_Description);

			simLVS.CA_PortOfClearance = "1234";
			portOfClearance = simLVS.PortOfClearance;
			AssertNull(portOfClearance);
		}

		[TestDate(2015, 12, 9)]
		public void TestSetDefaultValues()
		{
			var simLVS = new SimplifiedLVS(Factory);
			AssertEquals("JE_GB", simLVS.JE_GB, GlbBranch.CurrentBranch.PK);
			AssertEquals("JE_GS_NKCusAgent", simLVS.JE_GS_NKCusAgent, GlbStaff.CurrentUser.GS_Code);
			AssertEquals("JZ_ValuationDateOverride", new ZDateTime(2015, 12, 9), simLVS.JZ_ValuationDateOverride);
			AssertEquals("JZ_InvoiceDate", new ZDateTime(2015, 12, 8), simLVS.JZ_InvoiceDate);
			AssertEquals("JE_EntryAuthorisationDate", new ZDateTime(2015, 12, 1), simLVS.JE_EntryAuthorisationDate);
			AssertEquals("JE_PeriodYear", 2015, simLVS.JE_PeriodYear);
			AssertEquals("JE_PeriodMonth", 12, simLVS.JE_PeriodMonth);
			AssertEquals("JZ_WeightUQ", "LB", simLVS.JZ_WeightUQ);
		}

		public void TestJE_MessageSubType()
		{
			var importer = Factory.New<OrgHeader>();
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = importer.PK;
			AssertEquals(LowValueShipmentsTypes.Codes.ConsolidationByImporter, simLVS.JE_MessageSubType);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			simLVS.JE_OH_Importer = importer.PK;
			AssertEquals(LowValueShipmentsTypes.Codes.TotalConsolidation, simLVS.JE_MessageSubType);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, true);
			simLVS.JE_GB = branch.PK;
			AssertEquals(LowValueShipmentsTypes.Codes.ConsolidationByImporter, simLVS.JE_MessageSubType);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, false);
			simLVS.JE_GB = branch.PK;
			AssertEquals(LowValueShipmentsTypes.Codes.TotalConsolidation, simLVS.JE_MessageSubType);

			var impAddInfo = OrgImpAddInfo.Get(importer);
			impAddInfo.ZO_IsLVSConsolidated = true;
			simLVS.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(LowValueShipmentsTypes.Codes.TotalConsolidation, simLVS.JE_MessageSubType);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, true);
			simLVS.JE_OH_Importer = importer.PK;
			AssertEquals(LowValueShipmentsTypes.Codes.ConsolidationByImporter, simLVS.JE_MessageSubType);
			CACustomsDataRegistry.Instance.ConsolidateByImporter.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, false);
			simLVS.JE_OH_Importer = importer.PK;
			AssertEquals(LowValueShipmentsTypes.Codes.ConsolidationByImporter, simLVS.JE_MessageSubType);
		}

		public void TestLoadInvoiceHeader_Consolidate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PA");

			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2222", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PB");
			Factory.Save();

			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "Importer2";
			importer2.OH_FullName = "Importer2";
			var vendor = Factory.New<OrgHeader>();
			vendor.OH_Code = "Vendor";
			vendor.OH_FullName = "Vendor";
			vendor.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			vendor.MainAddress.OA_RN_NKCountryCode = "US";
			vendor.MainAddress.OA_State = "IL";
			var link = importer2.SupplierLinks.AddNew(vendor);
			link.OL_RN_NKImporterCountry = "CA";
			link.OL_RX_NKDefaultCurrency = "CNY";
			var linkMode = link.OrgSupBuyLinkTrnModes.AddNew();
			linkMode.PF_TransportMode = "ALL";
			linkMode.PF_IncoTerm = "CIF";
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			var branch2 = company1.Branches.AddNew();
			branch2.GB_Code = "GBB";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "GC2";
			var branch3 = company2.Branches.AddNew();
			branch3.GB_Code = "GBC";
			var lvs1 = CreateLVS("B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			var lvsInvoice1 = lvs1.Invoices.AddNew();
			lvsInvoice1.JZ_RN_NKDefaultOrigin = "US";
			lvsInvoice1.JZ_RW_NKOriginState = "AL";
			lvsInvoice1.CA_RN_NKExport = "US";
			lvsInvoice1.CA_USStateOfExport = "AL";
			lvsInvoice1.CA_TreatmentCode = "02";
			lvsInvoice1.CA_OtherReference = "XXX";
			lvsInvoice1.CA_TimeLimit = 1;
			lvsInvoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			var lvs2 = CreateLVS("B00008002", "VAR", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			var lvs3 = CreateLVS("B00008003", "MSI", 2015, 4, importer1.PK, branch1.PK, "PA", "A");
			lvs3.CA_LVSCloseDate = ZDateTime.Now;
			var lvs4 = CreateLVS("B00008004", "MSI", 2015, 4, importer1.PK, branch1.PK, "PA", "A");
			var entryHeader = lvs4.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			var lvs5 = CreateLVS("B00008005", "MSI", 2015, 4, importer1.PK, branch1.PK, "PA", "A");
			var lvs6 = CreateLVS("B00008006", "MSI", 2015, 3, importer1.PK, branch3.PK, "PA", "A");
			entryHeader = lvs5.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateByBroker.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateByBranch.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateByBroker.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateByImporter.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateByProvinceOfClearance.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, false);
				CACustomsDataRegistry.Instance.ConsolidateToOneFTypePerCLVSEntry.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, false);

				SetConsolidationStrategy(importer1, true, true, true, true, true);
				var simLVS01 = CreateSimplifiedLVS(2015, 3, importer1.PK, branch1.PK, "1111", "A", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0101", new DateTime(2015, 12, 1), "", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day);
				simLVS01.LoadInvoiceHeader(() => { return true; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS01.Declaration.MessageInitiator);
				});
				var invoiceHeader = simLVS01.Invoices[0];
				AssertNotNull(invoiceHeader);
				AssertEquals("Should create a new declaration", true, lvs1.PK != invoiceHeader.JobDeclaration.PK);
				AssertEquals("Should create a new InvoiceHeader", "INV0101", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer1.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "1111", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "FOB", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CAD", invoiceHeader.JZ_RX_NKInvoice_Currency);
				AssertEquals("JZ_RN_NKDefaultOrigin should be defaulted from vendor", "US", invoiceHeader.JZ_RN_NKDefaultOrigin);
				AssertEquals("JZ_RW_NKOriginState should be defaulted from vendor", "IL", invoiceHeader.JZ_RW_NKOriginState);
				AssertEquals("CA_RN_NKExport should be defaulted from vendor", "US", invoiceHeader.CA_RN_NKExport);
				AssertEquals("CA_USStateOfExport should be defaulted from vendor", "IL", invoiceHeader.CA_USStateOfExport);
				AssertEquals("CA_TreatmentCode should be defaulted from registry", "02", invoiceHeader.CA_TreatmentCode);
				AssertEquals("CA_OtherReference should not be defaulted from the last line", "", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should not be defaulted from the last line", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should not be defaulted from the last line", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);

				SetConsolidationStrategy(importer2, false, false, false, false, true);
				var simLVS02 = CreateSimplifiedLVS(2015, 3, importer2.PK, branch2.PK, "2222", "B", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0102", new DateTime(2015, 12, 1), "OtherReference", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day);
				simLVS02.LoadInvoiceHeader(() => { return true; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS02.Declaration.MessageInitiator);
				});
				invoiceHeader = simLVS02.Invoices[0];
				AssertNotNull(invoiceHeader);
				AssertEquals("Should create a new declaration", true, lvs2.PK != invoiceHeader.JobDeclaration.PK);
				AssertEquals("Should create a new InvoiceHeader", "INV0102", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer2.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "2222", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("CA_OtherReference should be set", "OtherReference", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should be set", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should be set", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "CIF", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CNY", invoiceHeader.JZ_RX_NKInvoice_Currency);

				SetConsolidationStrategy(importer1, true, true, true, true, false);
				var simLVS1 = CreateSimplifiedLVS(2015, 3, importer1.PK, branch1.PK, "1111", "A", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0001", new DateTime(2015, 12, 1), "", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day);
				simLVS1.LoadInvoiceHeader(() => { return false; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS1.Declaration.MessageInitiator);
				});
				invoiceHeader = simLVS1.Invoices[0];
				AssertNotNull(invoiceHeader);
				AssertEquals("Should find the match declaration", lvs1.PK, invoiceHeader.JobDeclaration.PK);
				AssertEquals("Should create a new InvoiceHeader", "INV0001", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer1.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "1111", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "FOB", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CAD", invoiceHeader.JZ_RX_NKInvoice_Currency);
				AssertEquals("JZ_RN_NKDefaultOrigin should be defaulted from vendor", "US", invoiceHeader.JZ_RN_NKDefaultOrigin);
				AssertEquals("JZ_RW_NKOriginState should be defaulted from vendor", "IL", invoiceHeader.JZ_RW_NKOriginState);
				AssertEquals("CA_RN_NKExport should be defaulted from vendor", "US", invoiceHeader.CA_RN_NKExport);
				AssertEquals("CA_USStateOfExport should be defaulted from vendor", "IL", invoiceHeader.CA_USStateOfExport);
				AssertEquals("CA_TreatmentCode should be defaulted from registry", "02", invoiceHeader.CA_TreatmentCode);
				AssertEquals("CA_OtherReference should not be defaulted from the last line", "", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should not be defaulted from the last line", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should not be defaulted from the last line", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);

				SetConsolidationStrategy(importer2, false, false, false, false, false);
				var simLVS2 = CreateSimplifiedLVS(2015, 3, importer2.PK, branch2.PK, "2222", "B", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0002", new DateTime(2015, 12, 1), "OtherReference", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day);
				simLVS2.LoadInvoiceHeader(() => { return false; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS2.Declaration.MessageInitiator);
				});
				invoiceHeader = simLVS2.Invoices[0];
				AssertNotNull(invoiceHeader);
				AssertEquals("Should find the match declaration", lvs2.PK, invoiceHeader.JobDeclaration.PK);
				AssertEquals("Should create a new InvoiceHeader", "INV0002", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer2.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "2222", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("CA_OtherReference should be set", "OtherReference", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should be set", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should be set", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "CIF", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CNY", invoiceHeader.JZ_RX_NKInvoice_Currency);

				SetConsolidationStrategy(importer2, false, true, false, false, false);
				var simLVS5 = CreateSimplifiedLVS(2015, 3, importer2.PK, branch1.PK, "2222", "B", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0003", new DateTime(2015, 12, 1), "OtherReference", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day);
				simLVS5.LoadInvoiceHeader(() => { return false; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS5.Declaration.MessageInitiator);
				});
				invoiceHeader = simLVS5.Invoices[0];
				AssertNotNull(invoiceHeader);
				AssertEquals("Should find the match declaration", lvs2.PK, invoiceHeader.JobDeclaration.PK);
				AssertEquals("Should create a new InvoiceHeader", "INV0003", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer2.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "2222", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("CA_OtherReference should be set", "OtherReference", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should be set", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should be set", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "CIF", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CNY", invoiceHeader.JZ_RX_NKInvoice_Currency);

				SetConsolidationStrategy(importer2, false, false, true, false, false);
				var simLVS6 = CreateSimplifiedLVS(2015, 3, importer2.PK, branch2.PK, "1111", "B", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0004", new DateTime(2015, 12, 1), "OtherReference", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day);
				simLVS6.LoadInvoiceHeader(() => { return false; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS6.Declaration.MessageInitiator);
				});
				invoiceHeader = simLVS6.Invoices[0];
				AssertNotNull(invoiceHeader);
				AssertEquals("Should find the match declaration", lvs2.PK, invoiceHeader.JobDeclaration.PK);
				AssertEquals("Should create a new InvoiceHeader", "INV0004", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer2.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "1111", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("CA_OtherReference should be set", "OtherReference", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should be set", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should be set", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "CIF", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CNY", invoiceHeader.JZ_RX_NKInvoice_Currency);

				SetConsolidationStrategy(importer2, false, false, false, true, false);
				var simLVS7 = CreateSimplifiedLVS(2015, 3, importer2.PK, branch2.PK, "2222", "A", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0005", new DateTime(2015, 12, 1), "OtherReference", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day);
				simLVS7.LoadInvoiceHeader(() => { return false; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS7.Declaration.MessageInitiator);
				});
				invoiceHeader = simLVS7.Invoices[0];
				AssertNotNull(invoiceHeader);
				AssertEquals("Should find the match declaration", lvs2.PK, invoiceHeader.JobDeclaration.PK);
				AssertEquals("Should create a new InvoiceHeader", "INV0005", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer2.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "2222", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("CA_OtherReference should be set", "OtherReference", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should be set", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should be set", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "CIF", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CNY", invoiceHeader.JZ_RX_NKInvoice_Currency);

				SetConsolidationStrategy(importer1, true, true, true, true, false);
				var simLVS8 = CreateSimplifiedLVS(2015, 4, importer1.PK, branch1.PK, "1111", "A", vendor.PK, vendor.MainAddress.PK, "XXXX", "INV0006", new DateTime(2015, 12, 1), "OtherReference", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day, true);
				simLVS8.LoadInvoiceHeader(() => { return true; });
				AssertNoExceptionThrown(() =>
				{
					AssertNotNull(simLVS8.Declaration.MessageInitiator);
				});
				invoiceHeader = simLVS8.Invoices[0];
				AssertNotNull(invoiceHeader);
				Assert("Should create a new declaration", !invoiceHeader.JobDeclaration.IsInDatabase);
				AssertEquals(importer1.PK, invoiceHeader.JobDeclaration.JE_OH_Importer);
				AssertEquals("MSI", invoiceHeader.JobDeclaration.JE_MessageSubType);
				AssertEquals(branch1.PK, invoiceHeader.JobDeclaration.JE_GB);
				AssertEquals("A", invoiceHeader.JobDeclaration.JE_GS_NKCusAgent);
				AssertEquals("1111", invoiceHeader.JobDeclaration.JE_CustomsOffice);
				AssertEquals("PA", invoiceHeader.JobDeclaration.CA_ProvinceOfClearance);
				AssertEquals(4, invoiceHeader.JobDeclaration.JE_PeriodMonth);
				AssertEquals(2015, invoiceHeader.JobDeclaration.JE_PeriodYear);
				Assert(invoiceHeader.JobDeclaration.CA_AllowOIC);
				AssertEquals("Should create a new InvoiceHeader", "INV0006", invoiceHeader.JZ_InvoiceNumber);
				AssertEquals("JZ_OH_Buyer should be set", importer1.PK, invoiceHeader.JZ_OH_Buyer);
				AssertEquals("JZ_OH_Supplier should be set", vendor.PK, invoiceHeader.JZ_OH_Supplier);
				AssertEquals("SupplierDocumentaryAddress.E2_OA_Address should be set", vendor.MainAddress.PK, invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("CA_LVSCarrier should be set", "XXXX", invoiceHeader.CA_LVSCarrier);
				AssertEquals("CA_PortOfClearance should be set", "1111", invoiceHeader.CA_PortOfClearance);
				AssertEquals("JZ_ValuationDateOverride should be set", ZDateTime.Today, invoiceHeader.JZ_ValuationDateOverride);
				AssertEquals("JZ_InvoiceDate should be set", new DateTime(2015, 12, 1), invoiceHeader.JZ_InvoiceDate);
				AssertEquals("CA_OtherReference should be set", "OtherReference", invoiceHeader.CA_OtherReference);
				AssertEquals("CA_TimeLimit should be set", 1, invoiceHeader.CA_TimeLimit);
				AssertEquals("CA_TimeLimitCode should be set", TimeLimitUnitCodes.Codes.Day, invoiceHeader.CA_TimeLimitCode);
				AssertEquals("JZ_Weight should be set", 10m, invoiceHeader.JZ_Weight);
				AssertEquals("JZ_WeightUQ should be set", "KG", invoiceHeader.JZ_WeightUQ);
				AssertEquals("JZ_IncoTerm should be set", "FOB", invoiceHeader.JZ_IncoTerm);
				AssertEquals("JZ_RX_NKInvoice_Currency should be set", "CAD", invoiceHeader.JZ_RX_NKInvoice_Currency);
			}
		}

		JobDeclaration CreateLVS(string reference, string type, int periodYear, int perodMonth, ZGuid importer, ZGuid branch, ZString provinceOfClearance, ZString broker, bool isAllowOIC = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = reference;
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = type;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(periodYear, perodMonth, 1);
			declaration.JE_OH_Importer = importer;
			declaration.JE_GB = branch;
			declaration.CA_ProvinceOfClearance = provinceOfClearance;
			declaration.JE_GS_NKCusAgent = broker;
			declaration.CA_AllowOIC = isAllowOIC;

			return declaration;
		}

		SimplifiedLVS CreateSimplifiedLVS(int periodYear, int perodMonth, ZGuid importer, ZGuid branch, ZString portOfClearance, ZString broker,
			ZGuid vendor, ZGuid supplierDocAddress, ZString carrierCode, ZString invoiceNumber, ZDateTime invoiceDate, ZString otherReference, ZDecimal weight, ZString weightUQ,
			ZInt timeLimit, ZString timeLimitCode, bool isAllowOIC = false)
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_EntryAuthorisationDate = new ZDateTime(periodYear, perodMonth, 1);
			simLVS.JE_OH_Importer = importer;
			simLVS.JE_GB = branch;
			simLVS.JE_GS_NKCusAgent = broker;
			simLVS.CA_PortOfClearance = portOfClearance;
			simLVS.SupplierDocumentaryAddress.OrganisationPK = vendor;
			simLVS.SupplierDocumentaryAddress.E2_OA_Address = supplierDocAddress;
			simLVS.CA_LVSCarrier = carrierCode;
			simLVS.JZ_InvoiceNumber = invoiceNumber;
			simLVS.JZ_InvoiceDate = invoiceDate;
			simLVS.CA_OtherReference = otherReference;
			simLVS.CA_TimeLimit = timeLimit;
			simLVS.CA_TimeLimitCode = timeLimitCode;
			simLVS.JZ_Weight = weight;
			simLVS.JZ_WeightUQ = weightUQ;
			simLVS.CA_AllowOIC = isAllowOIC;

			return simLVS;
		}

		public static void SetConsolidationStrategy(OrgHeader importer, bool consolidateByImporter, bool consolidateByBranch, bool consolidateByProvinceOfClearance, bool consolidateByBroker, bool consolidateToOneFTypePerCLVSEntry)
		{
			var impAddInfo = OrgImpAddInfo.Get(importer);
			impAddInfo.ZO_IsCreateIndividualLVS = false;
			impAddInfo.ZO_IsLVSConsolidated = consolidateByImporter;
			impAddInfo.ZO_IsConsolidateByBranch = consolidateByBranch;
			impAddInfo.ZO_IsConsolidateByBroker = consolidateByBroker;
			impAddInfo.ZO_IsConsolidateByProvinceofClearance = consolidateByProvinceOfClearance;
			impAddInfo.ZO_IsConsolidateToOneFTypePerCLVSEntry = consolidateToOneFTypePerCLVSEntry;
		}

		public void TestLoadInvoiceHeader_CreateIndividualLVSShipments()
		{
			var today = ZDateTime.Today;

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = importer.PK;
			simLVS.CA_PortOfClearance = "T";
			simLVS.JE_PeriodMonth = today.AddMonths(-1).Month;
			simLVS.JE_PeriodYear = today.AddYears(-1).Year;

			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			orgImpAddInfo.ZO_IsCreateIndividualLVS = true;
			AssertCreateLVXJob(simLVS);

			orgImpAddInfo.ZO_IsCreateIndividualLVS = false;
			using (CACustomsDataRegistry.Instance.CreateIndividualLVSShipments.SetTemporaryValue(Guid.Empty, simLVS.JE_GB.ToGuid(), Guid.Empty, true))
			{
				AssertCreateLVXJob(simLVS);
			}
		}

		void AssertCreateLVXJob(SimplifiedLVS simLVS)
		{
			simLVS.LoadInvoiceHeader(() => { return true; });
			AssertNoExceptionThrown(() =>
			{
				AssertNotNull(simLVS.Declaration.MessageInitiator);
			});
			var invoiceHeader = simLVS.Invoices[0];
			AssertNotNull(invoiceHeader);
			Assert("Should create a new InvoiceHeader", !invoiceHeader.IsInDatabase);
			var declaration = invoiceHeader.JobDeclaration;
			Assert("Should create a new JobDeclaration", !declaration.IsInDatabase);
			AssertEquals("JE_OH_Importer", simLVS.JE_OH_Importer, declaration.JE_OH_Importer);
			AssertEquals("JE_MessageType", JobMessageTypeList.Codes.LVSForConsolidation, declaration.JE_MessageType);
			AssertEquals("JE_MessageSubType", B3EntryTypeList.Codes.NoB3, declaration.JE_MessageSubType);
			AssertEquals("JE_GB", GlbBranch.CurrentBranch.PK, declaration.JE_GB);
			AssertEquals("JE_GS_NKCusAgent", GlbStaff.CurrentUser.GS_Code, declaration.JE_GS_NKCusAgent);
			AssertEquals("JE_CustomsOffice", "000T", declaration.JE_CustomsOffice);
			AssertEquals("JE_PeriodMonth", simLVS.JE_PeriodMonth, declaration.JE_PeriodMonth);
			AssertEquals("JE_PeriodYear", simLVS.JE_PeriodYear, declaration.JE_PeriodYear);
			AssertEquals("CA_AllowOIC", simLVS.CA_AllowOIC, declaration.CA_AllowOIC);
		}

		[TestDate(2015, 12, 1)]
		public void TestCreateNewSimplifiedLVS()
		{
			var simLVS = CreateSimplifiedLVS(2015, 3, ZGuid.NewZGuid(), ZGuid.NewZGuid(), "1111", "B", ZGuid.NewZGuid(), ZGuid.NewZGuid(), "XXXX", "INV0001", new DateTime(2015, 12, 1), "OtherReference", 10m, "KG", 1, TimeLimitUnitCodes.Codes.Day, true);
			var newSimLVS = simLVS.CreateNewSimplifiedLVS(Factory);

			AssertEquals("JE_OH_Importer", simLVS.JE_OH_Importer, newSimLVS.JE_OH_Importer);
			AssertEquals("JE_GB", simLVS.JE_GB, newSimLVS.JE_GB);
			AssertEquals("CA_PortOfClearance", simLVS.CA_PortOfClearance, newSimLVS.CA_PortOfClearance);
			AssertEquals("JE_EntryAuthorisationDate", simLVS.JE_EntryAuthorisationDate, newSimLVS.JE_EntryAuthorisationDate);
			AssertEquals("JE_PeriodYear", simLVS.JE_PeriodYear, newSimLVS.JE_PeriodYear);
			AssertEquals("JE_PeriodMonth", simLVS.JE_PeriodMonth, newSimLVS.JE_PeriodMonth);
			AssertEquals("JE_GS_NKCusAgent", simLVS.JE_GS_NKCusAgent, newSimLVS.JE_GS_NKCusAgent);
			AssertEquals("SupplierDocumentaryAddress.OrganisationPK", ZGuid.Empty, newSimLVS.SupplierDocumentaryAddress.OrganisationPK);
			AssertEquals("SupplierDocumentaryAddress.E2_OA_Address", ZGuid.Empty, newSimLVS.SupplierDocumentaryAddress.E2_OA_Address);
			AssertEquals("CA_LVSCarrier", simLVS.CA_LVSCarrier, newSimLVS.CA_LVSCarrier);
			AssertEquals("JZ_ValuationDateOverride", simLVS.JZ_ValuationDateOverride, newSimLVS.JZ_ValuationDateOverride);
			AssertEquals("JZ_InvoiceNumber", ZString.Empty, newSimLVS.JZ_InvoiceNumber);
			AssertEquals("JZ_InvoiceDate", new ZDateTime(2015, 11, 30), newSimLVS.JZ_InvoiceDate);
			AssertEquals("CA_OtherReference", ZString.Empty, newSimLVS.CA_OtherReference);
			AssertEquals("CA_TimeLimit", ZInt.Zero, newSimLVS.CA_TimeLimit);
			AssertEquals("CA_TimeLimitCode", ZString.Empty, newSimLVS.CA_TimeLimitCode);
			AssertEquals("JZ_Weight", 0m, newSimLVS.JZ_Weight);
			AssertEquals("JZ_WeightUQ", "LB", newSimLVS.JZ_WeightUQ);
			Assert("CA_AllowOIC", newSimLVS.CA_AllowOIC);
		}

		public void TestJE_OH_BillTo()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "AAA";
			importer.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);

			var simLVS = new SimplifiedLVS(Factory);
			AssertEquals("JE_OH_BillTo", ZGuid.Empty, simLVS.JE_OH_BillTo);
			simLVS.JE_OH_Importer = importer.PK;
			AssertEquals("JE_OH_BillTo", relatedParty.PK, simLVS.JE_OH_BillTo);
		}

		public void TestSetTotalAmountWhenSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 0m;
			invoice.InvoiceLines.AddNew().JI_LinePrice = 100m;
			invoice.InvoiceLines.AddNew().JI_LinePrice = 200m;

			var simLVS = new SimplifiedLVS(Factory);
			simLVS.Invoices.Add(invoice);
			Factory.Save();
			AssertEquals("Total amount should be set to the total amount of all lines", 300m, invoice.JZ_InvoiceAmount);

			invoice.InvoiceLines.AddNew().JI_LinePrice = 200m;
			AssertEquals("Total amount should not be set when it is not zero", 300m, invoice.JZ_InvoiceAmount);
		}

		public void TestIsSimplifiedLVSMode()
		{
			var today = ZDateTime.Today;

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = importer.PK;
			simLVS.CA_PortOfClearance = "T";
			simLVS.JE_PeriodMonth = today.AddMonths(-1).Month;
			simLVS.JE_PeriodYear = today.AddYears(-1).Year;

			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			orgImpAddInfo.ZO_IsCreateIndividualLVS = true;

			simLVS.LoadInvoiceHeader(() => { return true; });
			AssertNoExceptionThrown(() =>
			{
				AssertNotNull(simLVS.Declaration.MessageInitiator);
			});
			var invoice = simLVS.Invoices[0];

			Assert("IsSimplifiedLVSMode", invoice.IsSimplifiedLVSMode);
			simLVS.Invoices.RemoveAll();
			Assert("IsSimplifiedLVSMode", !invoice.IsSimplifiedLVSMode);
		}

		public void TestDefaultingPortOfClearance()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "CC1";
			carrier1.ZZ4_Description = "CC1 Desc";
			carrier1.ZZ4_IsSea = true;
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier1.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "CC2";
			carrier2.ZZ4_Description = "CC2 Desc";
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier2.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			carrier2.Attributes.AddNew(TransportTypeList.Codes.Air, TransportTypeList.Codes.Air);
			Factory.Save();

			var locoMap11 = Factory.New<RefLocoMap>();
			locoMap11.RY_LocalPortCode = "P11!";
			locoMap11.RY_RL_NKLocoPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			locoMap11.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap11.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;

			var unlocoX = Factory.New<RefUNLOCO>();
			unlocoX.RL_Code = "!XX";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CXX";
			company.GC_RN_NKCountryCode = "CA";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BXX";
			branch.GB_RL_NKHomePort = "!XX";
			var locoMap21 = Factory.New<RefLocoMap>();
			locoMap21.RY_LocalPortCode = "P21!";
			locoMap21.RY_RL_NKLocoPort = "!XX";
			locoMap21.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap21.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			var locoMap22 = Factory.New<RefLocoMap>();
			locoMap22.RY_LocalPortCode = "P22!";
			locoMap22.RY_RL_NKLocoPort = "!XX";
			locoMap22.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap22.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;

			try
			{
				var simLVS = new SimplifiedLVS(Factory);
				AssertEquals("JE_GB", GlbBranch.CurrentBranch.PK, simLVS.JE_GB);
				AssertEquals("CA_PortOfClearance", "P11!", simLVS.CA_PortOfClearance);

				simLVS.CA_LVSCarrier = carrier1.ZZ4_Code;
				simLVS.JE_GB = branch.PK;
				AssertEquals("CA_PortOfClearance", "P21!", simLVS.CA_PortOfClearance);

				simLVS.CA_LVSCarrier = carrier2.ZZ4_Code;
				AssertEquals("CA_PortOfClearance", "P22!", simLVS.CA_PortOfClearance);
			}
			finally
			{
				locoMap11.Delete();
			}
		}

		#region Validation

		public void TestValidateJE_GB()
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_GB = ZGuid.Empty;
			AssertHasErrorContaining(simLVS.JE_GBInfo, MandatoryValidation.MustBeEntered);
			simLVS.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertNoErrorContaining(simLVS.JE_GBInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateCA_PortOfClearance()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0ZZZ", "0ZZZ", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var simLVS = new SimplifiedLVS(Factory);
			simLVS.CA_PortOfClearance = "0XXX";
			simLVS.CA_PortOfClearance = ZString.Empty;
			AssertHasErrorContaining(simLVS.CA_PortOfClearanceInfo, MandatoryValidation.MustBeEntered);
			simLVS.CA_PortOfClearance = "0XXX";
			AssertHasErrorContaining(simLVS.CA_PortOfClearanceInfo, ListValidation.InvalidCodeError);

			simLVS.CA_PortOfClearance = "0ZZZ";
			AssertNoErrorContaining(simLVS.CA_PortOfClearanceInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(simLVS.CA_PortOfClearanceInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateJE_OH_Importer()
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = ZGuid.NewZGuid();
			simLVS.JE_OH_Importer = ZGuid.Empty;
			AssertHasErrorContaining(simLVS.JE_OH_ImporterInfo, MandatoryValidation.MustBeEntered);
			var importer = Factory.New<OrgHeader>();
			simLVS.JE_OH_Importer = importer.PK;
			AssertNoErrorContaining(simLVS.JE_OH_ImporterInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateJE_GS_NKCusAgent()
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_GS_NKCusAgent = ZString.Empty;
			AssertHasErrorContaining(simLVS.JE_GS_NKCusAgentInfo, MandatoryValidation.MustBeEntered);
			simLVS.JE_GS_NKCusAgent = "@@@";
			AssertNoErrorContaining(simLVS.JE_GS_NKCusAgentInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeError);
			simLVS.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrorContaining(simLVS.JE_GS_NKCusAgentInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(simLVS.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateCA_LVSCarrier()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "CC1";
			carrier1.ZZ4_Description = "CC1 Desc";
			carrier1.ZZ4_IsSea = true;
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier1.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			var simLVS = new SimplifiedLVS(Factory);
			simLVS.CA_LVSCarrier = "@@@";
			AssertHasErrorContaining(simLVS.CA_LVSCarrierInfo, ListValidation.InvalidCodeError);
			simLVS.CA_LVSCarrier = "CC1";
			AssertNoErrorContaining(simLVS.CA_LVSCarrierInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateCA_LVSCarrier_CarrierAttribute()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "CC1";
			carrier1.ZZ4_Description = "CC1 Desc";
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier1.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			carrier1.Attributes.AddNew(TransportTypeList.Codes.Sea, TransportTypeList.Codes.Sea);
			Factory.Save();

			var simLVS = new SimplifiedLVS(Factory);
			simLVS.CA_LVSCarrier = "@@@";
			AssertHasErrorContaining(simLVS.CA_LVSCarrierInfo, ListValidation.InvalidCodeError);
			simLVS.CA_LVSCarrier = "CC1";
			AssertNoErrorContaining(simLVS.CA_LVSCarrierInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateJZ_WeightUQ()
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JZ_WeightUQ = "@@";
			AssertHasErrorContaining(simLVS.JZ_WeightUQInfo, ListValidation.InvalidCodeError);
			simLVS.JZ_WeightUQ = simLVS.WeightUQList[0].Code;
			AssertNoErrorContaining(simLVS.JZ_WeightUQInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateAll()
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_GB = ZGuid.Empty;
			simLVS.JE_GS_NKCusAgent = ZString.Empty;
			simLVS.CA_LVSCarrier = "XXXX";
			simLVS.JZ_WeightUQ = "@@";

			simLVS.OnlyValidateCargoListHeaderProperty = false;
			simLVS.RunPreSaveValidation();
			AssertHasErrorContaining(simLVS.JE_GBInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.JE_GS_NKCusAgentInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.CA_PortOfClearanceInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.JE_OH_ImporterInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.CA_LVSCarrierInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(simLVS.JZ_WeightUQInfo, ListValidation.InvalidCodeError);

			simLVS.OnlyValidateCargoListHeaderProperty = true;
			simLVS.RunPreSaveValidation();
			AssertHasErrorContaining(simLVS.JE_GBInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.JE_GS_NKCusAgentInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.CA_PortOfClearanceInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(simLVS.CA_LVSCarrierInfo, ListValidation.InvalidCodeError);
			AssertNoErrorContaining(simLVS.JE_OH_ImporterInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(simLVS.JZ_WeightUQInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCA_TimeLimit()
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			simLVS.CA_TimeLimit = 0;
			AssertHasMessageErrorContaining(simLVS.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
			simLVS.CA_TimeLimit = 1;
			AssertNoMessageErrorContaining(simLVS.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
		}

		public void TestCheckCA_TimeLimitCode()
		{
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.CA_TimeLimit = 0;
			simLVS.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			AssertHasMessageErrorContaining(simLVS.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
			simLVS.CA_TimeLimitCode = ZString.Empty;
			AssertNoMessageErrorContaining(simLVS.CA_TimeLimitInfo, "You must enter a time limit when you have entered a time limit code");
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SimplifiedLVS(Factory);
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case SimplifiedLVS.Schema.JE_PeriodMonth:
					((SimplifiedLVS)info.BizObj).JE_PeriodMonth = 1;
					break;
				default:
					base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
					break;
			}
		}

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}
	}
}
