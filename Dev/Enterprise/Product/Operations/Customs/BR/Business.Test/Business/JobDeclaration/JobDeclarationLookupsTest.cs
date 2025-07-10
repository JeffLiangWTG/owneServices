using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContainsExactElementsInAnyOrder(new string[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake, TransportTypeList.Codes.Air, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road, TransportTypeList.Codes.Fixed, TransportTypeList.Codes.Own, TransportTypeList.Codes.Other }, declaration.Lookups.TransportTypeList.GetAllCodes());
		}

		public void TestCustomsTransportModeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder(new string[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake, TransportTypeList.Codes.Air, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road, TransportTypeList.Codes.Fixed, TransportTypeList.Codes.Own, }, declaration.Lookups.CustomsTransportModeList.GetAllCodes());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertContainsExactElementsInAnyOrder(new string[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake, TransportTypeList.Codes.Air, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road, TransportTypeList.Codes.Fixed, TransportTypeList.Codes.Own, BRTransportModeList.Codes.FIC, BRTransportModeList.Codes.OTH, }, declaration.Lookups.CustomsTransportModeList.GetAllCodes());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder(new string[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake, TransportTypeList.Codes.Air, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road, TransportTypeList.Codes.Fixed, TransportTypeList.Codes.Own, BRTransportModeList.Codes.FIC, BRTransportModeList.Codes.OTH, }, declaration.Lookups.CustomsTransportModeList.GetAllCodes());
		}

		public void TestMessageSubTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContains(Enterprise.Customs.Business.TransportTypeList.Codes.Road, declaration.Lookups.TransportTypeList.CodesAsString);
			declaration.JE_MessageType = "ISW";
			AssertContainsExactElementsInAnyOrder(new string[] { MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05, MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10, MessageSubTypeList.Codes._11, MessageSubTypeList.Codes._12, MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15, MessageSubTypeList.Codes._16, MessageSubTypeList.Codes._17, MessageSubTypeList.Codes._18, MessageSubTypeList.Codes._19, MessageSubTypeList.Codes._20, MessageSubTypeList.Codes._21, MessageSubTypeList.Codes._22, MessageSubTypeList.Codes._23, MessageSubTypeList.Codes._24, MessageSubTypeList.Codes._25, MessageSubTypeList.Codes._26, MessageSubTypeList.Codes._27, MessageSubTypeList.Codes._28, }, declaration.Lookups.MessageSubTypeList.GetAllCodes());
			declaration.JE_MessageType = "EXP";
			AssertEquals("No codes", 0, declaration.Lookups.MessageSubTypeList.GetAllCodes().Length);
			declaration.JE_MessageType = "IMP";
			AssertEquals("No codes", 0, declaration.Lookups.MessageSubTypeList.GetAllCodes().Length);
			declaration.JE_MessageType = "LIC";
			AssertEquals("No codes", 0, declaration.Lookups.MessageSubTypeList.GetAllCodes().Length);
		}

		public void TestDeclarantTypeList_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var lookups = new JobDeclarationLookups(declaration);
			var list = lookups.DeclarantTypeList;
			AssertType<TypeOfOperationExportList>(list);
			AssertSame(list, lookups.DeclarantTypeList);
			var listExport = (CodeDescriptionPairList)lookups.DeclarantTypeList;
			AssertContainsExactElementsInAnyOrder(new string[] { TypeOfOperationExportList.Codes._1001, TypeOfOperationExportList.Codes._1002, TypeOfOperationExportList.Codes._1003, }, listExport.GetAllCodes());
		}

		public void TestDeclarantTypeList_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var lookups = new JobDeclarationLookups(declaration);
			var list = lookups.DeclarantTypeList;
			AssertType<CodeDescriptionPairList>(list);
			AssertEquals(0, list.Count);
		}

		public void TestDeclarantTypeList_ImportSiscomex()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var lookups = new JobDeclarationLookups(declaration);
			var list = lookups.BRDeclarantTypeList;
			AssertType<DeclarantTypeList>(list);
			AssertSame(list, lookups.BRDeclarantTypeList);
			AssertContainsExactElementsInExactOrder(new string[] { "1", "2", "4", "6" }, list.GetAllCodes());
		}

		public void TestDeclarantOrganisations()
		{
			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			AssertType<OrganisationsFindBoxCollection>(lookups.DeclarantOrganisations);
		}

		public void TestMessageStatusList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<BRMessageStatusList>(declaration.Lookups.MessageStatusList);
		}

		public void TestMergeByList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var list = declaration.Lookups.MergeByList;
			AssertEquals(8, list.Count);
			Assert("Contains TRF", list.ContainsCode(OrgConstants.MergeInvoiceLines.Tariff));

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			list = declaration.Lookups.MergeByList;
			AssertEquals(8, list.Count);
			Assert("Contains TRF", list.ContainsCode(OrgConstants.MergeInvoiceLines.Tariff));
		}

		public void TestCustomsOfficeList()
		{
			ReferenceTestDataHelper.CreateCustomsOfficeCodes(Factory);

			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			var list = lookups.CustomsOfficeList as CodeDescriptionPairList;
			AssertContainsExactElementsInAnyOrder(new[] { "CO00001", "CO00002" }, list.GetAllCodes());
			AssertSame(list, lookups.CustomsOfficeList);
		}

		public void TestCustomsEnclosureList()
		{
			ReferenceTestDataHelper.CreateCustomsEnclosureCodes(Factory);

			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			var list = lookups.CustomsEnclosureList as CodeDescriptionPairList;
			AssertContainsExactElementsInAnyOrder(new[] { "CE00001", "CE00002" }, list.GetAllCodes());
			AssertSame(list, lookups.CustomsEnclosureList);
		}

		public void TestSubLocationOfGoodsList()
		{
			ReferenceTestDataHelper.CreateWarehousingSectorsCodes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			declaration.JE_CustomsOffice = ZString.Empty;
			declaration.JE_LocationOfGoods = ZString.Empty;
			var lookups = new JobDeclarationLookups(declaration);
			var list = lookups.SubLocationOfGoodsList as CodeDescriptionPairList;
			AssertEquals("Must be empty", 0, list.Count);

			declaration.JE_LocationOfGoods = "CE00001";
			lookups = new JobDeclarationLookups(declaration);
			list = lookups.SubLocationOfGoodsList as CodeDescriptionPairList;
			AssertEquals("Must be empty", 0, list.Count);

			declaration.JE_CustomsOffice = "CO00001";
			declaration.JE_LocationOfGoods = ZString.Empty;
			lookups = new JobDeclarationLookups(declaration);
			list = lookups.SubLocationOfGoodsList as CodeDescriptionPairList;
			AssertEquals("Must be empty", 0, list.Count);

			declaration.JE_CustomsOffice = "CO00001";
			declaration.JE_LocationOfGoods = "CE00001";
			lookups = new JobDeclarationLookups(declaration);
			list = lookups.SubLocationOfGoodsList as CodeDescriptionPairList;
			AssertContainsExactElementsInExactOrder(new[] { "001", "005" }, list.GetAllCodes());

			declaration.JE_CustomsOffice = "CO00002";
			declaration.JE_LocationOfGoods = "CE00001";
			lookups = new JobDeclarationLookups(declaration);
			list = lookups.SubLocationOfGoodsList as CodeDescriptionPairList;
			AssertContainsExactElementsInExactOrder(new[] { "004" }, list.GetAllCodes());
		}

		public void TestInvolvedPartyAddressOrganisations()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<OrgHeaderCollection>(declaration.Lookups.InvolvedPartyAddressOrganisations);
		}

		public void TestBoardingLocalAddressOrganisations()
		{
			var oDeclaration = Factory.New<JobDeclaration>();
			AssertNotNull(oDeclaration.Lookups.BoardingLocalAddressOrganisations);
			AssertType<OrgHeaderCollection>(oDeclaration.Lookups.BoardingLocalAddressOrganisations);
		}

		public void TestMessageTypeList()
		{
			using (BRCustomsDataRegistry.Instance.EnableLPCO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableImportLicense.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var list = declaration.Lookups.MessageTypeList;

				AssertEquals("MessageTypeList should be", 7, list.Count);
				Assert("MessageTypeList not has LPC code", !list.ContainsCode(BRJobMessageTypeList.Codes.LPCO));
				Assert("MessageTypeList not has LIC code", !list.ContainsCode(BRJobMessageTypeList.Codes.ImportLicense));
			}
		}

		public void TestMessageTypeListLICAndLPC()
		{
			var declaration = new BusinessObjectFactory().New<JobDeclaration>();
			var list = declaration.Lookups.MessageTypeList;
			AssertEquals("MessageTypeList should be", 7, list.Count);

			using (BRCustomsDataRegistry.Instance.EnableLPCO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableImportLicense.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration = new BusinessObjectFactory().New<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
				list = declaration.Lookups.MessageTypeList;
				AssertContainsExactElementsInExactOrder("MessageTypeList has LPC code", new[] { BRJobMessageTypeList.Codes.LPCO }, list.GetAllCodes());

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				list = declaration.Lookups.MessageTypeList;
				AssertContainsExactElementsInExactOrder("MessageTypeList has LIC code", new[] { BRJobMessageTypeList.Codes.ImportLicense }, list.GetAllCodes());

				declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.LPCO;
				list = declaration.Lookups.MessageTypeList;
				AssertContainsExactElementsInExactOrder("MessageTypeList has LPC code", new[] { BRJobMessageTypeList.Codes.LPCO }, list.GetAllCodes());

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
				list = declaration.Lookups.MessageTypeList;
				AssertContainsExactElementsInExactOrder("MessageTypeList has LIC code", new[] { BRJobMessageTypeList.Codes.ImportLicense }, list.GetAllCodes());
			}
		}

		public void TestMessageTypeListISW()
		{
			var declaration = new BusinessObjectFactory().New<JobDeclaration>();
			var list = declaration.Lookups.MessageTypeList;
			Assert("MessageTypeList has not ISW code", !list.ContainsCode(BRJobMessageTypeList.Codes.ImportSiscomex));

			using (BRCustomsDataRegistry.Instance.EnableImportSiscomex.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration = Factory.New<JobDeclaration>();
				Assert("MessageTypeList has ISW code", declaration.Lookups.MessageTypeList.ContainsCode(BRJobMessageTypeList.Codes.ImportSiscomex));
			}
		}

		public void TestPackingUnitTypesListCore()
		{
			var helperCustomsOffice = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helperCustomsOffice.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Units");
			helperCustomsOffice.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "01", "Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helperCustomsOffice.CreateCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "02", "Test2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var lookups = new JobDeclarationLookups(declaration);
			var list = lookups.PackingUnitTypesList;
			AssertContainsExactElementsInAnyOrder(new[] { "01", "02" }, list.GetAllCodes());
			AssertSame(list, lookups.PackingUnitTypesList);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("No codes", 0, lookups.PackingUnitTypesList.GetAllCodes().Length);
		}

		public void TestBankAccounts()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<AccBankAccountCollection>(declaration.Lookups.BankAccounts);
		}

		public void TestMessageTypeListNotContainsEXX()
		{
			using (BRCustomsDataRegistry.Instance.EnableImportLicense.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var list = declaration.Lookups.MessageTypeList;

				Assert("MessageTypeList has NOT EXX code", !list.ContainsCode(JobMessageTypeList.Codes.ExportDeclarationByExternalBroker));
			}
		}

		public void TestMessageTypeListNotContainsIMX()
		{
			using (BRCustomsDataRegistry.Instance.EnableImportLicense.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var list = declaration.Lookups.MessageTypeList;

				Assert("MessageTypeList has NOT IMX code", !list.ContainsCode(JobMessageTypeList.Codes.ImportDeclarationByExternalBroker));
			}
		}

		public void TestConsignees()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertType<ConsigneeCollection>(declaration.Lookups.Consignees);
		}

		public void TestBillTypeList()
		{
			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			var list = lookups.BillTypeList;
			AssertType<BillTypeList>(list);
			AssertSame(list, lookups.BillTypeList);
			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					BillTypeList.Codes.AWB,
					BillTypeList.Codes.Barcode,
					BillTypeList.Codes.CRT,
					BillTypeList.Codes.DSIC,
					BillTypeList.Codes.HAWB,
					BillTypeList.Codes.HBL,
					BillTypeList.Codes.HRWB,
					BillTypeList.Codes.RWB,
					BillTypeList.Codes.TIFDTA,
					BillTypeList.Codes.UCR
				}, lookups.BillTypeList.GetAllCodes());
		}

		public void TestEntryStatusList()
		{
			ReferenceTestDataHelper.CreateEntryStatusForISWList(Factory);
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryStatusList1 = declaration.Lookups.EntryStatusList;
			AssertEquals("S01, S02, S03, S04, S05, S06", entryStatusList1.CodesAsString);
			var entryStatusList2 = declaration.Lookups.EntryStatusList;
			AssertSame("List should be cached", entryStatusList1, entryStatusList2);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			entryStatusList1 = declaration.Lookups.EntryStatusList;
			AssertEquals("L01, L02, L03, L04, L05, L06, L07, L08, L09, L10, L15, L16, L17, L18", entryStatusList1.CodesAsString);
			entryStatusList2 = declaration.Lookups.EntryStatusList;
			AssertSame("List should be cached", entryStatusList1, entryStatusList2);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			entryStatusList1 = declaration.Lookups.EntryStatusList;
			AssertEquals("E10, E11", entryStatusList1.CodesAsString);
			entryStatusList2 = declaration.Lookups.EntryStatusList;
			AssertSame("List should be cached", entryStatusList1, entryStatusList2);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;

			entryStatusList1 = declaration.Lookups.EntryStatusList;
			AssertEquals("E10, E11, I31, L01, L02, L03, L04, L05, L06, L07, L08, L09, L10, L15, L16, L17, L18, S01, S02, S03, S04, S05, S06", entryStatusList1.CodesAsString);
			entryStatusList2 = declaration.Lookups.EntryStatusList;
			AssertSame("List should be cached", entryStatusList1, entryStatusList2);
		}

		public void TestSpecialTransportModesList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var list = declaration.Lookups.SpecialTransportModesList;
			AssertType<SpecialTransportModesList>(list);
			AssertSame("List should be cached", list, declaration.Lookups.SpecialTransportModesList);
			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					SpecialTransportModesList.Codes._4001,
					SpecialTransportModesList.Codes._4002,
					SpecialTransportModesList.Codes._4003,
					SpecialTransportModesList.Codes._4004,
					SpecialTransportModesList.Codes._4005,
					SpecialTransportModesList.Codes._4006
				}, declaration.Lookups.SpecialTransportModesList.GetAllCodes());
		}

		public void TestDispatchModalityList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var list = declaration.Lookups.DispatchModalityList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new string[] { "1", "2", "5", "6", "7" }, list.GetAllCodes());
				AssertEquals("Normal", list.GetDescriptionFromCode("1"));
				AssertEquals("Anticipated", list.GetDescriptionFromCode("2"));
				AssertEquals("Fractional Delivery", list.GetDescriptionFromCode("5"));
				AssertEquals("Anticipated with Fractional Delivery", list.GetDescriptionFromCode("6"));
				AssertEquals("Over Sea OEA", list.GetDescriptionFromCode("7"));
				AssertSame("List should be cached", list, declaration.Lookups.DispatchModalityList);
			});

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			list = declaration.Lookups.DispatchModalityList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new string[] { "1", "2", "3", "4", "5", "6", "7" }, list.GetAllCodes());
				AssertEquals("Dispatch for consumption of goods admitted under special regime, except ATUE, Temporary REPETRO and Temporary LNG", list.GetDescriptionFromCode("1"));
				AssertEquals("Transfer to another special regime/replacement of beneficiary", list.GetDescriptionFromCode("2"));
				AssertEquals("Nationalization of goods admitted under DE/DAF after the end of the validity period", list.GetDescriptionFromCode("3"));
				AssertEquals("Import of goods subject to export without leaving the national territory", list.GetDescriptionFromCode("4"));
				AssertEquals("Dispatch for consumption of waste arising from the destruction of goods admitted under a special customs regime", list.GetDescriptionFromCode("5"));
				AssertEquals("Dispatch for consumption of goods temporarily admitted into the country for economic use, including REPETRO and LNG", list.GetDescriptionFromCode("6"));
				AssertEquals("New Temporary Admission for Economic Use (art. 75 of IN/RFB 1600/2015)", list.GetDescriptionFromCode("7"));
				AssertSame("List should be cached", list, declaration.Lookups.DispatchModalityList);
			});
		}

		public void TestCargoArrivalDocumentList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals(4, declaration.Lookups.CargoArrivalDocumentList.Count);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertContainsExactElementsInAnyOrder(new[] { "2", "3" }, declaration.Lookups.CargoArrivalDocumentList.GetAllCodes());

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertContainsExactElementsInAnyOrder(new[] { "1", "3" }, declaration.Lookups.CargoArrivalDocumentList.GetAllCodes());

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertContainsExactElementsInAnyOrder(new[] { "1", "3", "4" }, declaration.Lookups.CargoArrivalDocumentList.GetAllCodes());

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			AssertContainsExactElementsInAnyOrder(new[] { "1", "3" }, declaration.Lookups.CargoArrivalDocumentList.GetAllCodes());

			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			AssertContainsExactElementsInAnyOrder(new[] { "1", "3" }, declaration.Lookups.CargoArrivalDocumentList.GetAllCodes());

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertContainsExactElementsInAnyOrder(new[] { "1", "3" }, declaration.Lookups.CargoArrivalDocumentList.GetAllCodes());
		}

		public void TestUtilizationList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertSame(Factory.GetCachedValue<BRUtilizationList>(), declaration.Lookups.CargoArrivalDocumentUtilizationList);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "2", "3" }, declaration.Lookups.CargoArrivalDocumentUtilizationList.GetAllCodes());
		}
	}
}
