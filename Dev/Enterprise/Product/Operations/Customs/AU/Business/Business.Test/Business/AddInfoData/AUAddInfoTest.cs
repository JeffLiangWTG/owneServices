using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUAddInfo))]
	sealed class AUAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetKeysIsCached()
		{
			Customs.Business.IAddInfo bO1AddInfo = header.JobComInvoiceLines.AddNew().AddInfo;
			Customs.Business.IAddInfo bO2AddInfo = header.JobComInvoiceLines.AddNew().AddInfo;
			Customs.Business.IAddInfo bO3InDiffFactoryAddInfo = new BusinessObjectFactory().New<JobComInvoiceLine>().AddInfo;
			var keys = bO1AddInfo.GetKeys();
			AssertEquals("Same type", true, Object.ReferenceEquals(keys, bO2AddInfo.GetKeys()));
			AssertEquals("Different Factory", false, Object.ReferenceEquals(keys, bO3InDiffFactoryAddInfo.GetKeys()));
		}

		public void TestZA_SettlementPeriodType_Hidden()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(AUAddInfo), AUAddInfo.Schema.ZA_SettlementPeriodType_Hidden, true, attrib => attrib.ListDataSourceMember == "Lookups.SettlementPeriodTypeList");
		}

		public void TestZA_SettlementPeriodType_Hidden_UpdatesZA_SettlementPeriodEndDate_Hidden()
		{
			CombineAssertions(() =>
			{
				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.WeeklyLegacy;
				AssertEquals("PeriodStartDate empty, PeriodEndDate not set", ZDateTime.Empty, addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 6, 15);
				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.Weekly;
				AssertEquals("Weekly", new ZDateTime(2023, 6, 21), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.Monthly;
				AssertEquals("Monthly", new ZDateTime(2023, 6, 30), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 12, 18);
				AssertEquals("Monthly period starting in December ends on the last day of December (31/12/2023)", new ZDateTime(2023, 12, 31), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.Quarterly;
				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 6, 15);
				AssertEquals("Quarterly", new ZDateTime(2023, 8, 31), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 11, 12);
				AssertEquals("Quarterly period starting in November ends on the last day of January next year (31/01/2024)", new ZDateTime(2024, 01, 31), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.WeeklyLegacy;
				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 6, 15);
				AssertEquals("WeeklyLegacy", new ZDateTime(2023, 6, 21), addInfo.ZA_SettlementPeriodEndDate_Hidden);
			});
		}

		public void TestZA_SettlementPeriodStartDate_Hidden_UpdatesZA_SettlementPeriodEndDate_Hidden()
		{
			CombineAssertions(() =>
			{
				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.Weekly;
				addInfo.ZA_SettlementPeriodStartDate_Hidden = ZDateTime.Empty;
				AssertEquals("When PeriodStartDate is empty, PeriodEndDate should remain unset",
					ZDateTime.Empty, addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 6, 15);
				AssertEquals("For Weekly type, PeriodStartDate = 15/06/2023 should result in PeriodEndDate = 21/06/2023",
					new ZDateTime(2023, 6, 21), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.Monthly;
				AssertEquals("For Monthly type, PeriodStartDate = 15/06/2023 should result in PeriodEndDate = 30/06/2023",
					new ZDateTime(2023, 6, 30), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 12, 18);
				AssertEquals("For Monthly type, PeriodStartDate = 18/12/2023 should result in PeriodEndDate = 31/12/2023",
					new ZDateTime(2023, 12, 31), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodType_Hidden = SettlementPeriodTypeList.Codes.Quarterly;
				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 8, 15);
				AssertEquals("For Quarterly type, PeriodStartDate = 15/08/2023 should result in PeriodEndDate = 31/10/2023",
					new ZDateTime(2023, 10, 31), addInfo.ZA_SettlementPeriodEndDate_Hidden);

				addInfo.ZA_SettlementPeriodStartDate_Hidden = new ZDateTime(2023, 11, 12);
				AssertEquals("For Quarterly type, PeriodStartDate = 12/11/2023 should result in PeriodEndDate = 31/01/2024",
					new ZDateTime(2024, 1, 31), addInfo.ZA_SettlementPeriodEndDate_Hidden);
			});
		}

		public void TestZA_SettlementPeriodEndDate_Hidden_ReadOnly() => CombineAssertions(() =>
		{
			AssertEquals("ZA_SettlementPeriodType_Hidden not specified, ZA_SettlementPeriodEndDate_Hidden editable", false, addInfo.ZA_SettlementPeriodEndDate_HiddenInfo.ReadOnly);
			addInfo.ZA_SettlementPeriodType_Hidden = "SM";
			AssertEquals("ZA_SettlementPeriodType_Hidden specified, ZA_SettlementPeriodEndDate_Hidden readonly", true, addInfo.ZA_SettlementPeriodEndDate_HiddenInfo.ReadOnly);
		});

		public void TestZA_UPEIndicator_Hidden()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_UPEIndicator_Hidden = false;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var question1 = Factory.New<CMRCusEntryCPDec>();
			question1.ON_CH = entryHeader.PK;
			question1.ON_CPDecNum = 1;

			var question2 = Factory.New<CMRCusEntryCPDec>();
			question2.ON_CH = entryHeader.PK;
			question2.ON_CPDecNum = 3;

			var question3 = Factory.New<CMRCusEntryCPDec>();
			question3.ON_CH = entryHeader.PK;
			question3.ON_CPDecNum = 375;

			var collection = entryHeader.Questions;
			collection.Load();

			var expectedPks = new[] { question1.PK, question2.PK, question3.PK };
			AssertContainsExactElementsInAnyOrder("Should load all related questions as default.", expectedPks, collection.GetPKs());

			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;

			AssertEquals(1, collection.Count);
			AssertCollectionContains("Should contains the question1 as the CP Dec Num is not 3 or 375.", question1, collection);
			AssertCollectionNotContains("Should remove the question2 as the UPE is ticked and the CP Dec Num is 3.", question2, collection);
			AssertCollectionNotContains("Should remove the question3 as the UPE is ticked and the CP Dec Num is 375.", question3, collection);
		}

		public void TestZA_DrawbackID_ReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Assert(declaration.AddInfo.ZA_DrawbackID_ReadOnly);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			Assert(!invoice.AddInfo.ZA_DrawbackID_ReadOnly);

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Assert(invoiceLine.AddInfo.ZA_DrawbackID_ReadOnly);
		}

		public void TestAddInfoClone()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			((ILightValidationInternals)invoiceLine.AddInfo).IsValid = true;
			AssertNoExceptionThrown(() => invoiceLine.AddInfo.Clone());
		}

		public void TestClearADJThroughAddInfoLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			AssertEquals("Apportionment dirty", false, declaration.ApportionmentDirty);

			invoiceLine.AddInfo.ZA_ADJ = "100AUD";
			AssertEquals("Apportionment dirty", true, declaration.ApportionmentDirty);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			var initialised = decLoaded.InvoiceLines[0].AddInfo;
			AssertEquals("Initialising AddInfo should not cause Apportionment to be dirty", false, decLoaded.ApportionmentDirty);

			decLoaded.InvoiceLines[0].AddInfo.AddInfoLine = "";
			AssertEquals("Should cause apportionment to be dirty now", true, decLoaded.ApportionmentDirty);
		}

		public void TestConsigneeNameReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.ZA_ConsigneeNameHiddenInfo.ReadOnly);
			declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertEquals(false, declaration.ZA_ConsigneeNameHiddenInfo.ReadOnly);
		}

		public void TestConsigneeCityReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.ZA_ConsigneeCityHiddenInfo.ReadOnly);
			declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertEquals(false, declaration.ZA_ConsigneeCityHiddenInfo.ReadOnly);
		}

		public void TestGoodsPartyReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.ZA_GoodsOwnerPartyIDHiddenInfo.ReadOnly);
			declaration.JE_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertEquals(false, declaration.ZA_GoodsOwnerPartyIDHiddenInfo.ReadOnly);
		}

		public void TestAddInfoMergeString()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			CMRTariffRatePeriodSnapshot ratePeriod = Factory.New<CMRTariffRatePeriodSnapshot>();
			ratePeriod.TT_TariffClassificationNumber = "11111111";
			ratePeriod.TT_RateNumber = "001";
			ratePeriod.TT_PreferenceSchemeType = "GEN";
			ratePeriod.TT_StartDate = ZDateTime.Today;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_PST = "GEN";
			invoice.JZ_RN_NKDefaultOrigin = "JP";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1111111111";
			invoiceLine1.AddInfo.ZA_RNO = "";//effective value should be 001

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1111111111";
			invoiceLine2.AddInfo.ZA_PST = "GEN";
			invoiceLine2.AddInfo.ZA_RNO = "001";

			AssertEquals("AddInfoMergeString should contain GEN", true, invoiceLine1.AddInfo.MergeAddInfoString.Contains("GEN"));
			AssertEquals("AddInfoMergeString should contain GEN", true, invoiceLine2.AddInfo.MergeAddInfoString.Contains("GEN"));
			AssertEquals("AddInfoMergeString should contain 001", true, invoiceLine1.AddInfo.MergeAddInfoString.Contains("001"));
			AssertEquals("AddInfoMergeString should contain 001", true, invoiceLine2.AddInfo.MergeAddInfoString.Contains("001"));

			declaration.DoMerge();

			AssertEquals("Invoice line 1 and 2 should be merged together", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
		}

		public void TestTILVMoney()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_TILV = "123";
			AssertEquals("TILVMoney.Amount", 123m, invoiceLine.AddInfo.TILVMoney.Amount);
			AssertEquals("TILVMoney.Currency", "AUD", invoiceLine.AddInfo.TILVMoney.Currency.Code);

			invoiceLine.AddInfo.ZA_TILV = "123USD";
			AssertEquals("TILVMoney.Amount", 123m, invoiceLine.AddInfo.TILVMoney.Amount);
			AssertEquals("TILVMoney.Currency", "USD", invoiceLine.AddInfo.TILVMoney.Currency.Code);

			invoiceLine.AddInfo.ZA_TILV = "";
			AssertEquals("TILVMoney.Amount", 0m, invoiceLine.AddInfo.TILVMoney.Amount);
			AssertEquals("TILVMoney.Currency", "AUD", invoiceLine.AddInfo.TILVMoney.Currency.Code);
		}

		public void TestInvoiceHeader()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			AssertNull("Invoice Header", declaration.AddInfo.InvoiceHeader);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			AssertNotNull("Invoice Header", invoiceHeader.AddInfo.InvoiceHeader);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertNull("Invoice Header", invoiceLine.AddInfo.InvoiceHeader);
		}

		public void TestWarehouseOrgFK()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			AssertEquals(ZGuid.Empty, addInfo.WarehouseOrgFK);
			AssertEquals(ZGuid.Empty, addInfo.ZA_OA_WarehouseAddress_Hidden);

			OrgHeader org1 = OrgHeader.New(Factory);
			OrgAddress address1_1 = org1.MainAddress;
			OrgAddress address1_2 = org1.Addresses.AddNew();

			OrgHeader org2 = OrgHeader.New(Factory);
			OrgAddress address2_1 = org2.MainAddress;

			OrgHeader org3 = OrgHeader.New(Factory);

			addInfo.WarehouseOrgFK = org1.PK;
			AssertEquals(org1.PK, addInfo.WarehouseOrgFK);
			AssertEquals(address1_1.PK, addInfo.ZA_OA_WarehouseAddress_Hidden);

			addInfo.ZA_OA_WarehouseAddress_Hidden = address1_2.PK;
			AssertEquals(org1.PK, addInfo.WarehouseOrgFK);
			AssertEquals(address1_2.PK, addInfo.ZA_OA_WarehouseAddress_Hidden);

			addInfo.WarehouseOrgFK = org2.PK;
			AssertEquals(org2.PK, addInfo.WarehouseOrgFK);
			AssertEquals(address2_1.PK, addInfo.ZA_OA_WarehouseAddress_Hidden);

			addInfo.WarehouseOrgFK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, addInfo.WarehouseOrgFK);
			AssertEquals(ZGuid.Empty, addInfo.ZA_OA_WarehouseAddress_Hidden);
		}

		public void TestWarehouseAddress()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			OrgAddress address = OrgHeader.New(Factory).MainAddress;
			addInfo.ZA_OA_WarehouseAddress_Hidden = address.PK;
			AssertEquals(address, addInfo.WarehouseAddress);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			AssertNull(addInfo.WarehouseAddress);
			AssertEquals(address.PK, addInfo.ZA_OA_WarehouseAddress_Hidden);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(address, addInfo.WarehouseAddress);
			AssertEquals(address.PK, addInfo.ZA_OA_WarehouseAddress_Hidden);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull(addInfo.WarehouseAddress);
			AssertEquals(address.PK, addInfo.ZA_OA_WarehouseAddress_Hidden);

			addInfo.ZA_OA_WarehouseAddress_Hidden = address.PK;
			AssertNull(addInfo.WarehouseAddress);
			AssertEquals(ZGuid.Empty, addInfo.ZA_OA_WarehouseAddress_Hidden);
		}

		public void TestUseBondedWarehouseAutomationGetsSetsAddInfoField()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.AddInfo.UseBondedWarehouseAutomation);

			invoiceLine.AddInfo.ZA_UseBondedWarehouseAutomation_Hidden = "Y";
			AssertEquals(true, invoiceLine.AddInfo.UseBondedWarehouseAutomation);

			invoiceLine.AddInfo.ZA_UseBondedWarehouseAutomation_Hidden = "";
			AssertEquals(false, invoiceLine.AddInfo.UseBondedWarehouseAutomation);

			invoiceLine.AddInfo.UseBondedWarehouseAutomation = true;
			AssertEquals("Y", invoiceLine.AddInfo.ZA_UseBondedWarehouseAutomation_Hidden);

			invoiceLine.AddInfo.UseBondedWarehouseAutomation = false;
			AssertEquals("", invoiceLine.AddInfo.ZA_UseBondedWarehouseAutomation_Hidden);
		}

		public void TestUseBondedWarehouseIsPersisted()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine1.UseBondedWarehouseAutomation);

			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			declaration.Importer.FillWithValidTestData();
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(true, invoiceLine2.UseBondedWarehouseAutomation);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decReloaded = factory2.Load<JobDeclaration>(declaration.PK);
			JobComInvoiceLine invoiceLine1Reloaded = (JobComInvoiceLine)decReloaded.FilteredInvoiceLines.FindByPK(invoiceLine1.PK);
			JobComInvoiceLine invoiceLine2Reloaded = (JobComInvoiceLine)decReloaded.FilteredInvoiceLines.FindByPK(invoiceLine2.PK);
			AssertEquals(invoiceLine1.PK, invoiceLine1Reloaded.PK);
			AssertEquals(invoiceLine2.PK, invoiceLine2Reloaded.PK);
			Assert(invoiceLine2Reloaded != invoiceLine1Reloaded);
			AssertEquals(2, decReloaded.FilteredInvoiceLines.Count);
			AssertEquals(2, decReloaded.InvoiceLines.Count);
			AssertEquals(false, invoiceLine1Reloaded.UseBondedWarehouseAutomation);
			AssertEquals(true, invoiceLine2Reloaded.UseBondedWarehouseAutomation);
		}

		public void TestSetInstrumentType()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsImport CMR", true, testDec.IsImportCMR);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_InstrumentType_Hidden = CustomsInstrumentTypeList.Codes.TariffConcession;
			AssertEquals("TC1 is mapped to TC for CMR", CMRInstrumentTypeList.Codes.TariffConcessionOrder, invoiceLine.AddInfo.ZA_InstrumentType_Hidden);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("IsImport CMR", false, testDec.IsImportCMR);

			invoiceLine.AddInfo.ZA_InstrumentType_Hidden = CMRInstrumentTypeList.Codes.TariffConcessionOrder;
			AssertEquals("TC is mapped to TC1 for Edifice", CustomsInstrumentTypeList.Codes.TariffConcession, invoiceLine.AddInfo.ZA_InstrumentType_Hidden);
		}

		public void TestSetInstrumentTypeForClassification()
		{
			var classification = Factory.New<Classification>();
			classification.AddInfo.ZA_InstrumentType_Hidden = CustomsInstrumentTypeList.Codes.TariffConcession;
			AssertEquals("Instrument type should stay so", CustomsInstrumentTypeList.Codes.TariffConcession, classification.AddInfo.ZA_InstrumentType_Hidden);
		}

		public void TestSettingWRQUpdatesWUVTakingCurrencyIntoAccount()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2000, 1, 1), new ZDateTime(2050, 1, 1), 0.5m, helper.USDCurrency);

			header.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			line.JI_IsPackToBondForLine = true;
			line.JI_LinePrice = 1000m;
			line.AddInfo.ZA_WRQ = 5m;
			AssertEquals("WUV AddInfo", 400m, line.WUV);
		}

		public void TestAddInfoHasChanges()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "NZ";
			AssertEquals("Has Changes == true. If you override HasChanges and do not set, then CachedProperties will break", true, invoiceHeader.AddInfo.HasChanges);
		}

		[ExpectNoExceptions]
		public void TestAddInfoHasChanges_WhenParentRowDetached()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)((IBindingList)declaration.Invoices).AddNew();
			var addInfo = invoiceHeader.AddInfo;
			((ICancelAddNew)declaration.Invoices).CancelNew(declaration.Invoices.Count - 1);
			addInfo.ZA_POC = "NZ";
		}

		public void TestAggregatedPOC_PRT_PST()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "NZ";
			invoiceHeader.AddInfo.ZA_PST = "NZ";
			invoiceHeader.AddInfo.ZA_PRT = "P50";

			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated POC", "NZ", line.AggregatedZA_POC);
			AssertEquals("Aggregated PST", "NZ", line.AggregatedZA_PST);
			AssertEquals("Aggregated PRT", "P50", line.AggregatedZA_PRT);

			line.AddInfo.ZA_POC = "ZA";
			AssertEquals("Aggregated POC as all preference info is not entered", "ZA", line.AggregatedZA_POC);
			AssertEquals("Aggregated PST as all preference info is not entered", "", line.AggregatedZA_PST);
			AssertEquals("Aggregated PRT all preference info is not entered", "", line.AggregatedZA_PRT);

			line.AddInfo.ZA_PST = "DC";
			AssertEquals("Aggregated POC as all preference info is not entered", "ZA", line.AggregatedZA_POC);
			AssertEquals("Aggregated PST as all preference info is not entered", "DC", line.AggregatedZA_PST);
			AssertEquals("Aggregated PRT all preference info is not entered", "", line.AggregatedZA_PRT);

			line.AddInfo.ZA_PRT = "WO";
			AssertEquals("Aggregated POC", "ZA", line.AggregatedZA_POC);
			AssertEquals("Aggregated PST", "DC", line.AggregatedZA_PST);
			AssertEquals("Aggregated PRT", "WO", line.AggregatedZA_PRT);
		}

		public void TestAggregatedValueForPreferenceWithNoPreferentialRate()
		{
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 03, 03);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "NZ";
			invoiceHeader.AddInfo.ZA_PST = "NZ";
			invoiceHeader.AddInfo.ZA_PRT = "P50";

			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated POC", "NZ", line.AggregatedValue(AUAddInfoSchema.ZA_POC.Name));
			AssertEquals("Aggregated PST", "NZ", line.AggregatedValue(AUAddInfoSchema.ZA_PST.Name));
			AssertEquals("Aggregated PRT", "P50", line.AggregatedValue(AUAddInfoSchema.ZA_PRT.Name));

			CMRTariffRatePeriodSnapshot gENRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			gENRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TT_RateNumber = "001";
			gENRateNumber.TT_PreferenceSchemeType = "GEN";
			gENRateNumber.TT_CalculationType = "FREE";
			gENRateNumber.TT_TariffClassificationNumber = "00000000";

			line.JI_Tariff = "00000000 00";
			AssertEquals("Is general rate", true, line.IsGeneralRate);
			AssertEquals("Aggregated POC", "", line.AggregatedValue(AUAddInfoSchema.ZA_POC.Name));
			AssertEquals("Aggregated PST", "GEN", line.AggregatedValue(AUAddInfoSchema.ZA_PST.Name));
			AssertEquals("Aggregated PRT", "", line.AggregatedValue(AUAddInfoSchema.ZA_PRT.Name));
		}

		public void TestSerialisationOfAddInfoWithSendZeroAmountForOverridenDuty()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_DTY = 0m;
			line.AddInfo.ZA_SendZeroDutyOverride_Hidden = true;
			ZString addInfoLine = line.AddInfo.AddInfoLine;
			AssertEquals("AddInfoLine should have DTY=0", true, addInfoLine.Contains("DTY=0"));

			ZString toString = line.AddInfo.ToString();
			AssertEquals("AddInfoLine should have DTY=0", true, addInfoLine.Contains("DTY=0"));

			line.AddInfo.ZA_SendZeroDutyOverride_Hidden = false;
			addInfoLine = line.AddInfo.AddInfoLine;
			AssertEquals("AddInfoLine should have DTY=0", false, addInfoLine.Contains("DTY=0"));

			toString = line.AddInfo.ToString();
			AssertEquals("AddInfoLine should have DTY=0", false, addInfoLine.Contains("DTY=0"));
		}

		public void TestDefaultingOfRNOOnPSTChanges()
		{
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);
			CMRTariffRatePeriodSnapshot gENRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			gENRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TT_RateNumber = "001";
			gENRateNumber.TT_PreferenceSchemeType = "GEN";
			gENRateNumber.TT_CalculationType = "FREE";
			gENRateNumber.TT_TariffClassificationNumber = "49011000";

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.AddInfo.ZA_PST = "GEN";
			AssertEquals("Rate number defaulted", "001", line.AddInfo.ZA_RNO);
		}

		public void TestDefaultingRNODoesNotHappenWithMoreThanOneInTheList()
		{
			CMRTariffRatePeriodSnapshot gENRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			gENRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TT_RateNumber = "001";
			gENRateNumber.TT_PreferenceSchemeType = "GEN";
			gENRateNumber.TT_CalculationType = "FREE";
			gENRateNumber.TT_TariffClassificationNumber = "49011000";

			CMRTariffRatePeriodSnapshot rateNumber44 = CMRTariffRatePeriodSnapshot.New(Factory);
			rateNumber44.TT_StartDate = new ZDateTime(2005, 03, 03);
			rateNumber44.TT_RateNumber = "044";
			rateNumber44.TT_PreferenceSchemeType = "GEN";
			rateNumber44.TT_CalculationType = "FREE";
			rateNumber44.TT_TariffClassificationNumber = "49011000";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.AddInfo.ZA_PST = "GEN";
			AssertEquals("Rate number not defaulted", ZString.Empty, line.AddInfo.ZA_RNO);
		}

		public void TestAggregatedPST()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_PST = "GEN";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated PST", "GEN", invoiceLine.AddInfo.AggregatedPST);

			invoiceLine.AddInfo.ZA_PST = "CA";
			AssertEquals("Aggregated PST", "CA", invoiceLine.AddInfo.AggregatedPST);
		}

		public void TestAggregatedPOC()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_POC = "US";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Aggregated POC", "US", invoiceLine.AddInfo.AggregatedZA_POC);

			invoiceLine.AddInfo.ZA_POC = "NZ";
			AssertEquals("Aggregated POC", "NZ", invoiceLine.AddInfo.AggregatedZA_POC);
		}

		public void TestAggregatedPOCFallingBackToORG()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = "US";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("AggregatedPOCFallBackToORG", "US", invoiceLine.AddInfo.AggregatedPOCFallBackToORG);

			invoiceLine.AddInfo.ZA_ORG = "CA";
			AssertEquals("AggregatedPOCFallBackToORG", "CA", invoiceLine.AddInfo.AggregatedPOCFallBackToORG);

			invoiceLine.AddInfo.ZA_POC = "NZ";
			AssertEquals("AggregatedPOCFallBackToORG", "NZ", invoiceLine.AddInfo.AggregatedPOCFallBackToORG);
		}

		public void TestPOCAndPRTWhenCurrentLevelIsGeneral()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("ReadOnly", false, invoice.AddInfo.ZA_PRTInfo.ReadOnly);
			AssertEquals("ReadOnly", false, invoice.AddInfo.ZA_POCInfo.ReadOnly);

			invoice.AddInfo.ZA_POC = "NZ";
			invoice.AddInfo.ZA_PRT = "P50";
			invoice.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;

			AssertEquals("ZA_POC", "", invoice.AddInfo.ZA_POC);
			AssertEquals("ZA_PRT", "", invoice.AddInfo.ZA_PRT);
			AssertEquals("ReadOnly", true, invoice.AddInfo.ZA_PRTInfo.ReadOnly);
			AssertEquals("ReadOnly", true, invoice.AddInfo.ZA_POCInfo.ReadOnly);
		}

		public void TestIsGeneralRate()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Is general rate", true, invoiceLine.AddInfo.IsGeneralRate);

			invoiceLine.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			AssertEquals("Is general rate", true, invoiceLine.AddInfo.IsGeneralRate);

			invoiceLine.AddInfo.ZA_PST = "DC";
			AssertEquals("Is not general rate", false, invoiceLine.AddInfo.IsGeneralRate);
		}

		public void TestReadOnly()
		{
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			AssertEquals(false, declaration.AddInfo.ReadOnly);
			AssertEquals(false, header.AddInfo.ReadOnly);

			header.ReadOnly = true;
			AssertEquals(false, declaration.AddInfo.ReadOnly);
			AssertEquals(true, header.AddInfo.ReadOnly);
		}

		public void TestTILVWithoutCurrencyPopulateLocalCurrency()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_TILV = "150";
			AssertEquals("TILV has a local currency now", "150.00AUD", invoiceLine.AddInfo.ZA_TILV);
		}

		public void TestMergeAddInfoLine()
		{
			addInfo.ZA_ELA = "11111111";
			addInfo.ZA_TCI = "AAA:22222222";
			addInfo.ZA_CL2 = "3333.33.33";
			addInfo.ZA_TI2 = "BBB:44444444";
			addInfo.ZA_PRI = "CCC:55555555";
			addInfo.ZA_VID = "66666666";
			AssertEquals("Merge AddInfo String contains ELAC number", true, addInfo.MergeAddInfoString.Contains(addInfo.ZA_ELA));
			AssertEquals("Merge AddInfo String contains TCI number", true, addInfo.MergeAddInfoString.Contains(addInfo.ZA_TCI));
			AssertEquals("Merge AddInfo String contains CL2 number", true, addInfo.MergeAddInfoString.Contains(addInfo.ZA_CL2));
			AssertEquals("Merge AddInfo String contains TI2 number", true, addInfo.MergeAddInfoString.Contains(addInfo.ZA_TI2));
			AssertEquals("Merge AddInfo String contains PRI number", true, addInfo.MergeAddInfoString.Contains(addInfo.ZA_PRI));
			AssertEquals("Merge AddInfo String does not contain VID number", false, addInfo.MergeAddInfoString.Contains(addInfo.ZA_VID));
		}

		public void TestAddInfoLineWithPermitAndEncryptionNumbers()
		{
			string permitNumbers = "743214|87403";
			string testAddInfoLine = "ISS=48.3*SCN=3344225";
			addInfo.ZA_PermitNumbers_Hidden = permitNumbers;

			addInfo.AddInfoLine = testAddInfoLine;
			AssertEquals("Hidden Fields should not have been overwritten from the AddInfoLine", permitNumbers, addInfo.ZA_PermitNumbers_Hidden);

			addInfo.ZA_PrinterNumber_Hidden = "4";
			AssertEquals("Setting a hidden field should not clear any existing add info", "3344225", addInfo.ZA_SCN);
			AssertEquals("Hidden fields should not appear in the Add Info Line", false, addInfo.AddInfoLine.Contains(permitNumbers));
			AssertEquals("Add Info Line should have ISS", true, addInfo.AddInfoLine.Contains("ISS=48.3"));
			AssertEquals("Add Info Line should have SCN", true, addInfo.AddInfoLine.Contains("SCN=3344225"));

			addInfo.ZA_UQ2 = "KG";
			AssertEquals("UQ2 should have 'KG'", true, addInfo.AddInfoLine.Contains("UQ2=KG"));

			addInfo.AddInfoLine = "SCN=553321";
			AssertEquals("Hidden fields should still exist after AddInfoLine changed", "4", addInfo.ZA_PrinterNumber_Hidden);
			AssertEquals("ISS should have been cleared", 0m, addInfo.ZA_ISS);
			AssertEquals("SCN should have not cleared", "553321", addInfo.ZA_SCN);
		}

		public void TestReadOnlyPropertiesExcludedFromAddInfoLineString()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoiceHeader.AddInfo.AddInfoLine = "ORG=AUST*TILV=222*ADJ=220USD";
			invoiceHeader.AddInfo.Validation.ValidateAddInfoLine();
			Assert("AddInfo should have an error", invoiceHeader.AddInfo.AddInfoLineInfo.HasNotifications());
		}

		public void TestAddInfoLineForAddInfoForm()
		{
			addInfo.ZA_DCX = "DCX";
			addInfo.ZA_PST = "PST";

			AssertEquals("AddInfoLineForAddInfoForm is equal to AddInfoLine", true, addInfo.AddInfoLineForAddInfoForm == addInfo.AddInfoLine);

			addInfo.AddInfoLine += "*PRT=PRT";
			AssertEquals("AddInfoLineForAddInfoForm is equal to AddInfoLine", true, addInfo.AddInfoLineForAddInfoForm == addInfo.AddInfoLine);

			AssertEquals("Add Info Line has errors", true, addInfo.AddInfoLineInfo.HasNotifications());
			AssertEquals("AddInfoLineForAddInfoForm has no errors", false, addInfo.AddInfoLineForAddInfoFormInfo.HasNotifications());

			addInfo.AddInfoLineForAddInfoForm += "*POC=IT";
			AssertEquals("AddInfoLineForAddInfoForm is equal to AddInfoLine", true, addInfo.AddInfoLineForAddInfoForm == addInfo.AddInfoLine);
		}

		public void TestAddInfoMessageType()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("AddInfo IsExport", true, testDeclaration.AddInfo.IsExport);
			AssertEquals("AddInfo IsImport", false, testDeclaration.AddInfo.IsImport);

			testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("AddInfo IsImport", true, testDeclaration.AddInfo.IsImport);
			AssertEquals("AddInfo IsExport", false, testDeclaration.AddInfo.IsExport);

			AssertEquals("AU State", testDeclaration.AddInfo.ZA_AUState_HiddenInfo.HumanReadableName);
			AssertEquals("Customs Weight", testDeclaration.AddInfo.ZA_AQISCustomsWt_HiddenInfo.HumanReadableName);
		}

		public void TestHeaderLevelAddInfo()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			var addInfo = new AUAddInfo(header);
			AssertEquals("This field is editible", false, addInfo.ZA_AMBInfo.ReadOnly);
			AssertEquals("This field is editible", false, addInfo.ZA_GSTEInfo.ReadOnly);
			AssertEquals("This field is editible", false, addInfo.ZA_ORGInfo.ReadOnly);
			AssertEquals("This field is editible", false, addInfo.ZA_PRFInfo.ReadOnly);
			AssertEquals("This field is editible", false, addInfo.ZA_SCNInfo.ReadOnly);
			AssertEquals("This field is editible", false, addInfo.ZA_VANInfo.ReadOnly);
			AssertEquals("This field is editible", false, addInfo.ZA_WRNInfo.ReadOnly);

			AssertEquals("This field is editible", false, addInfo.ZA_TILVInfo.ReadOnly);
			AssertEquals("This field should not be editable", true, addInfo.ZA_ADJInfo.ReadOnly);

			AssertEquals("AU State", addInfo.ZA_AUState_HiddenInfo.HumanReadableName);
			AssertEquals("Customs Weight", addInfo.ZA_AQISCustomsWt_HiddenInfo.HumanReadableName);
		}

		public void TestGetJobDeclaration()
		{
			AssertEquals("Declaration in AddInfo", DefaultData.Declaration, DefaultData.InvoiceLine.AddInfo.JobDeclaration);
		}

		public void TestMergeAddInfoStringDoesntHaveQT2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_QT2 = 20;

			Assert("MergeAddInfoLine shouldn't have QT2", invoiceLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_QT2).IsEmpty);
		}

		public void TestMergeAddInfoStringDoenstHaveTILV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_TILV = "20AUD";

			Assert("MergeAddInfoLine shouldn't have TILV", invoiceLine.AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_TILV).IsEmpty);
		}

		public void TestSettingAddInfoPropertyProxiesNotificationsAutomatically()
		{
			addInfo.AddInfoLine = "AMB=Y";//Invalid code
			AssertEquals("Error expected", true, addInfo.AddInfoLineInfo.HasNotifications());
		}

		public void TestIsImportCMR()
		{
			JobComInvoiceHeader header = Factory.New<JobComInvoiceHeader>();
			AssertEquals("Is Import CMR", false, header.AddInfo.IsImportCMR);

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("Is Import CMR", true, header.AddInfo.IsImportCMR);
		}

		public void TestIsAir()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("IsAIr", true, declaration.AddInfo.IsAir);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IsAIr", false, declaration.AddInfo.IsAir);
		}

		public void TestIsSea()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("IsSea", false, declaration.AddInfo.IsSea);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IsSea", true, declaration.AddInfo.IsSea);
		}

		public void TestIsFCL()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("IsFCL", true, declaration.AddInfo.IsFCL);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("IsFCL", false, declaration.AddInfo.IsFCL);
		}

		public void TestIsLCL()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("IsLCL", true, declaration.AddInfo.IsLCL);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("IsLCL", false, declaration.AddInfo.IsLCL);
		}

		public void TestEFD()
		{
			AssertEquals("EFD is empty", ZDateTime.Empty, addInfo.EFD);

			addInfo.ZA_EFD = "400204";
			AssertEquals("EFD is in the wrong format", ZDateTime.Empty, addInfo.EFD);

			addInfo.ZA_EFD = "300204";
			AssertEquals("EFD is in the wrong format", ZDateTime.Empty, addInfo.EFD);

			addInfo.ZA_EFD = "301304";
			AssertEquals("EFD is in the wrong format", ZDateTime.Empty, addInfo.EFD);

			addInfo.ZA_EFD = "300504";
			AssertEquals("EFD is in the right format", new ZDateTime(2004, 05, 30), addInfo.EFD);
		}

		public void TestFOD()
		{
			AssertEquals("FOD is empty", ZDateTime.Empty, addInfo.FOD);

			addInfo.ZA_FOD = "400204";
			AssertEquals("FOD is in the wrong format", ZDateTime.Empty, addInfo.FOD);

			addInfo.ZA_FOD = "300204";
			AssertEquals("FOD is in the wrong format", ZDateTime.Empty, addInfo.FOD);

			addInfo.ZA_FOD = "301304";
			AssertEquals("FOD is in the wrong format", ZDateTime.Empty, addInfo.FOD);

			addInfo.ZA_FOD = "300504";
			AssertEquals("FOD is in the right format", new ZDateTime(2004, 05, 30), addInfo.FOD);
		}

		public void TestEffectiveDutyDate()
		{
			AssertEquals("EFD is empty", ZDateTime.Today, addInfo.EffectiveDutyDate);

			header.AddInfo.ZA_EFD = "050505";
			AssertEquals("EFD is not empty", new ZDateTime(2005, 05, 05), addInfo.EffectiveDutyDate);
		}

		public void TestAggregatedZA_VALB_Hidden()
		{
			line.AddInfo.ZA_VALB_Hidden = "TV";
			AssertEquals("AggregatedZA_VALB_Hidden", "TV", line.AddInfo.AggregatedZA_VALB_Hidden);

			header.AddInfo.ZA_VALB_Hidden = "IG";
			AssertEquals("AggregatedZA_VALB_Hidden", "TV", line.AddInfo.AggregatedZA_VALB_Hidden);

			line.AddInfo.ZA_VALB_Hidden = "";
			AssertEquals("AggregatedZA_VALB_Hidden", "IG", line.AddInfo.AggregatedZA_VALB_Hidden);
		}

		public void TestLoadPropertiesFromString_DoesntCauseInvoiceLineToRequireValidation()
		{
			addInfo.ZA_POC = "NZ";
			ZString addInfoLine = addInfo.ToString();
			addInfo.ZA_POC = "AU";

			addInfo.InvoiceLine.MarkLightValidationAsValidForTesting();
			AssertEquals("InvoiceLine.IsValid true initially", true, addInfo.InvoiceLine.LightValidationIsValid);

			addInfo.LoadPropertiesFromString(addInfoLine);
			AssertEquals("POC loaded correctly", "NZ", addInfo.ZA_POC);
			AssertEquals("InvoiceLine.IsValid still true after a call to LoadPropertiesFromString", true, addInfo.InvoiceLine.LightValidationIsValid);
		}

		public void TestReportErrorWhenFailedToCastDeclaration()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = company.Branches.AddNew();
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			header.JobDeclaration.JE_GB = branch.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerFromNewFactory = newFactory.Load<JobComInvoiceHeader>(header.PK);

			var declaration = headerFromNewFactory.AddInfo.JobDeclaration;
			AssertEquals("AUAddInfo|GetJobDeclaration", ErrorReporter.LastKeyReported);
			AssertStartsWith("Error report message", "Unable to cast object of type 'Enterprise.Customs.US.Business.JobDeclaration' to type 'Enterprise.Customs.AU.Declaration.Business.JobDeclaration'.", ErrorReporter.LastMessageReported);
			AssertContains("Error report message", "Declaration country: US\r\nInvoice header country: US\r\nInvoice header stack trace:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestMergeInvoiceLinesWithISS()
		{
			JobDeclaration declaration = GetDeclaration();
			line1.AddInfo.ZA_ISS = 12;
			invoiceLine2.AddInfo.ZA_ISS = 15;
			declaration.DoMerge();

			AssertEquals("One Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two Entry Lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeInvoiceLinesWithLCP()
		{
			JobDeclaration declaration = GetDeclaration();
			line1.AddInfo.ZA_LCP = 12;
			declaration.DoMerge();

			AssertEquals("One Entry Header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two Entry Lines", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestSettingAdjustmentCurrency_Hidden()
		{
			AssertEquals("", addInfo.AddInfoLine);
			addInfo.AdjustmentDollarPercentage_Hidden = "$";
			addInfo.AdjustmentCurrency_Hidden = "AUDX";
			AssertEquals("Currencies are only 3 characters", "AUD", addInfo.AdjustmentCurrency_Hidden);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionOnADJIfInvalidDataEntered()
		{
			addInfo.AdjustmentDollarPercentage_Hidden = "$";
			addInfo.AdjustmentCurrency_Hidden = "AUD";
			addInfo.AdjustmentAmount_Hidden = 1234567890123456789012345.67m;
		}

		public void TestZA_ADJSplitting_Amount()
		{
			AssertEquals("", addInfo.AddInfoLine);
			addInfo.ZA_ADJ = "100AUD";
			AssertEquals(100.0m, addInfo.AdjustmentAmount_Hidden);
			addInfo.ZA_ADJ = "";
			AssertEquals(0m, addInfo.AdjustmentAmount_Hidden);
			AssertEquals("Zero amount should not create an ADJ on AddInfo Line", "", addInfo.AddInfoLine);
		}

		public void TestZA_ADJSplitting_Currency()
		{
			addInfo.ZA_ADJ = "100AUD";
			AssertEquals("AUD", addInfo.AdjustmentCurrency_Hidden);
			addInfo.ZA_ADJ = "";
			AssertEquals("", addInfo.AdjustmentCurrency_Hidden);
		}

		public void TestZA_AdjustmentCurrencyHiddenDefaultsToInvoiceCurrency()
		{
			JobComInvoiceLine line = addInfo.Parent as JobComInvoiceLine;
			JobComInvoiceHeader header = line.Master as JobComInvoiceHeader;
			header.JZ_RX_NKInvoice_Currency = "GBP";
			addInfo.ZA_ADJ = "100";
			AssertEquals("GBP", addInfo.AdjustmentCurrency_Hidden);
			AssertEquals("100GBP", addInfo.ZA_ADJ);
		}

		public void TestZA_ADJSplitting_DollarPercentageWithoutSymbol()
		{
			addInfo.ZA_ADJ = "100";
			AssertEquals("$", addInfo.AdjustmentDollarPercentage_Hidden);
			addInfo.ZA_ADJ = "";
			AssertEquals("", addInfo.AdjustmentDollarPercentage_Hidden);
		}

		public void TestZA_ADJSplitting_DollarPercentageWithCurrency()
		{
			addInfo.ZA_ADJ = "100AUD";
			AssertEquals("$", addInfo.AdjustmentDollarPercentage_Hidden);
			addInfo.ZA_ADJ = "";
			AssertEquals("", addInfo.AdjustmentDollarPercentage_Hidden);
		}

		public void TestZA_ADJSplitting_DollarPercentageWithPercentage()
		{
			addInfo.ZA_ADJ = "50%";
			AssertEquals("%", addInfo.AdjustmentDollarPercentage_Hidden);
			addInfo.ZA_ADJ = "";
			AssertEquals("", addInfo.AdjustmentDollarPercentage_Hidden);
		}

		public void TestZA_ADJMarksApportionmentDirty()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Pre-condition", false, declaration.ApportionmentDirty);
			invoiceLine.AddInfo.ZA_ADJ = "100AUD";
			AssertEquals("Apportionment now dirty", true, declaration.ApportionmentDirty);
		}

		public void TestISSIsNotCalculatedOnNature10()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			addInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			invoiceLine.JI_Tariff = "2203.00.69 20";
			invoiceLine.JI_CustomsQuantity = 1000.0m;
			invoiceLine.AddInfo.ZA_QT2 = 10000.0m;
			AssertEquals("ZA_ISS", 0m, invoiceLine.AddInfo.ZA_ISS);
		}

		public void TestISSRounding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "22042990 41";
			invoiceLine.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			invoiceLine.AddInfo.ZA_ISS = 3.615m;
			AssertEquals("ZA_ISS", 3.62m, invoiceLine.AddInfo.ZA_ISS);
		}

		public void TestReadOnlyForInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("ADJ", false, invoiceLine.AddInfo.ZA_ADJInfo.ReadOnly);
			AssertEquals("MD2", false, invoiceLine.AddInfo.ZA_MD2Info.ReadOnly);
			AssertEquals("TC2", false, invoiceLine.AddInfo.ZA_TC2Info.ReadOnly);
			AssertEquals("TFQ", false, invoiceLine.AddInfo.ZA_TFQInfo.ReadOnly);
			AssertEquals("WETQ", false, invoiceLine.AddInfo.ZA_WETQInfo.ReadOnly);
			AssertEquals("STD", false, invoiceLine.AddInfo.ZA_STDInfo.ReadOnly);
			AssertEquals("CON", false, invoiceLine.AddInfo.ZA_CONInfo.ReadOnly);
			AssertEquals("MLP", false, invoiceLine.AddInfo.ZA_MLPInfo.ReadOnly);
			AssertEquals("ISS", false, invoiceLine.AddInfo.ZA_ISSInfo.ReadOnly);
			AssertEquals("IDP", false, invoiceLine.AddInfo.ZA_IDPInfo.ReadOnly);
			AssertEquals("ICV", false, invoiceLine.AddInfo.ZA_ICVInfo.ReadOnly);
			AssertEquals("QSC", false, invoiceLine.AddInfo.ZA_QSCInfo.ReadOnly);
			AssertEquals("TAN", false, invoiceLine.AddInfo.ZA_TANInfo.ReadOnly);
			AssertEquals("DXP", false, invoiceLine.AddInfo.ZA_DXPInfo.ReadOnly);
			AssertEquals("WETE", false, invoiceLine.AddInfo.ZA_WETEInfo.ReadOnly);
			AssertEquals("LCTQ", false, invoiceLine.AddInfo.ZA_LCTQInfo.ReadOnly);
			AssertEquals("LCT", false, invoiceLine.AddInfo.ZA_LCTInfo.ReadOnly);
			AssertEquals("LCTE", false, invoiceLine.AddInfo.ZA_LCTEInfo.ReadOnly);
			AssertEquals("WUV", false, invoiceLine.AddInfo.ZA_WUVInfo.ReadOnly);
			AssertEquals("WRU", false, invoiceLine.AddInfo.ZA_WRUInfo.ReadOnly);
			AssertEquals("WRQ", false, invoiceLine.AddInfo.ZA_WRQInfo.ReadOnly);
			AssertEquals("QT2", false, invoiceLine.AddInfo.ZA_QT2Info.ReadOnly);
			AssertEquals("AQIS", false, invoiceLine.AddInfo.ZA_AQISInfo.ReadOnly);
			AssertEquals("PIQ", false, invoiceLine.AddInfo.ZA_PIQInfo.ReadOnly);
			AssertEquals("ODF", false, invoiceLine.AddInfo.ZA_ODFInfo.ReadOnly);
			AssertEquals("DSA", false, invoiceLine.AddInfo.ZA_DSAInfo.ReadOnly);
			AssertEquals("DSN", false, invoiceLine.AddInfo.ZA_DSNInfo.ReadOnly);
			AssertEquals("CSC", false, invoiceLine.AddInfo.ZA_CSCInfo.ReadOnly);
			AssertEquals("DTY", false, invoiceLine.AddInfo.ZA_DTYInfo.ReadOnly);
			AssertEquals("DRE", false, invoiceLine.AddInfo.ZA_DREInfo.ReadOnly);
			AssertEquals("DMP", false, invoiceLine.AddInfo.ZA_DMPInfo.ReadOnly);
			AssertEquals("CVD", false, invoiceLine.AddInfo.ZA_CVDInfo.ReadOnly);
			AssertEquals("CSA", false, invoiceLine.AddInfo.ZA_CSAInfo.ReadOnly);
			AssertEquals("RNO", false, invoiceLine.AddInfo.ZA_RNOInfo.ReadOnly);
			AssertEquals("QIN", false, invoiceLine.AddInfo.ZA_QINInfo.ReadOnly);
			AssertEquals("DCX", false, invoiceLine.AddInfo.ZA_DCXInfo.ReadOnly);
			AssertEquals("DRC", false, invoiceLine.AddInfo.ZA_DRCInfo.ReadOnly);
			AssertEquals("ICN", false, invoiceLine.AddInfo.ZA_ICNInfo.ReadOnly);
			AssertEquals("WER", false, invoiceLine.AddInfo.ZA_WETInfo.ReadOnly);
			AssertEquals("DXT", false, invoiceLine.AddInfo.ZA_DXTInfo.ReadOnly);
			AssertEquals("ELA", false, invoiceLine.AddInfo.ZA_ELAInfo.ReadOnly);
			AssertEquals("FOD", false, invoiceLine.AddInfo.ZA_FODInfo.ReadOnly);
			AssertEquals("TRN", false, invoiceLine.AddInfo.ZA_TRNInfo.ReadOnly);
			AssertEquals("LCP", false, invoiceLine.AddInfo.ZA_LCPInfo.ReadOnly);
			AssertEquals("ISC", false, invoiceLine.AddInfo.ZA_ISCInfo.ReadOnly);
			AssertEquals("WMC", false, invoiceLine.AddInfo.ZA_WMCInfo.ReadOnly);
			AssertEquals("PRI", false, invoiceLine.AddInfo.ZA_PRIInfo.ReadOnly);
			AssertEquals("TCI", false, invoiceLine.AddInfo.ZA_TCIInfo.ReadOnly);
			AssertEquals("LCTI", false, invoiceLine.AddInfo.ZA_LCTIInfo.ReadOnly);
			AssertEquals("MLPI", false, invoiceLine.AddInfo.ZA_MLPIInfo.ReadOnly);
			AssertEquals("SEC", false, invoiceLine.AddInfo.ZA_SECInfo.ReadOnly);
			AssertEquals("PUP", false, invoiceLine.AddInfo.ZA_PUPInfo.ReadOnly);
			AssertEquals("TR2", false, invoiceLine.AddInfo.ZA_TR2Info.ReadOnly);
			AssertEquals("CL2", false, invoiceLine.AddInfo.ZA_CL2Info.ReadOnly);
			AssertEquals("VID", false, invoiceLine.AddInfo.ZA_VIDInfo.ReadOnly);
			AssertEquals("PRI_InstrumentType", false, invoiceLine.AddInfo.PRI_InstrumentTypeInfo.ReadOnly);
			AssertEquals("PRI_InstrumentNo", false, invoiceLine.AddInfo.PRI_InstrumentNoInfo.ReadOnly);
			AssertEquals("TI2_InstrumentType", false, invoiceLine.AddInfo.TI2_InstrumentTypeInfo.ReadOnly);
			AssertEquals("TI2_InstrumentNo", false, invoiceLine.AddInfo.TI2_InstrumentNoInfo.ReadOnly);
			AssertEquals("TCI_InstrumentType", false, invoiceLine.AddInfo.TCI_InstrumentTypeInfo.ReadOnly);
			AssertEquals("TCI_InstrumentNo", false, invoiceLine.AddInfo.TCI_InstrumentNoInfo.ReadOnly);

			AssertEquals("EFD", true, invoiceLine.AddInfo.ZA_EFDInfo.ReadOnly);
		}

		public void TestReadOnlyForInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("ADJ", true, invoiceHeader.AddInfo.ZA_ADJInfo.ReadOnly);
			AssertEquals("MD2", true, invoiceHeader.AddInfo.ZA_MD2Info.ReadOnly);
			AssertEquals("TC2", true, invoiceHeader.AddInfo.ZA_TC2Info.ReadOnly);
			AssertEquals("TFQ", true, invoiceHeader.AddInfo.ZA_TFQInfo.ReadOnly);
			AssertEquals("WETQ", true, invoiceHeader.AddInfo.ZA_WETQInfo.ReadOnly);
			AssertEquals("STD", true, invoiceHeader.AddInfo.ZA_STDInfo.ReadOnly);
			AssertEquals("CON", true, invoiceHeader.AddInfo.ZA_CONInfo.ReadOnly);
			AssertEquals("MLP", true, invoiceHeader.AddInfo.ZA_MLPInfo.ReadOnly);
			AssertEquals("ISS", true, invoiceHeader.AddInfo.ZA_ISSInfo.ReadOnly);
			AssertEquals("IDP", true, invoiceHeader.AddInfo.ZA_IDPInfo.ReadOnly);
			AssertEquals("ICV", true, invoiceHeader.AddInfo.ZA_ICVInfo.ReadOnly);
			AssertEquals("QSC", true, invoiceHeader.AddInfo.ZA_QSCInfo.ReadOnly);
			AssertEquals("TAN", true, invoiceHeader.AddInfo.ZA_TANInfo.ReadOnly);
			AssertEquals("DXP", true, invoiceHeader.AddInfo.ZA_DXPInfo.ReadOnly);
			AssertEquals("WETE", true, invoiceHeader.AddInfo.ZA_WETEInfo.ReadOnly);
			AssertEquals("LCTQ", true, invoiceHeader.AddInfo.ZA_LCTQInfo.ReadOnly);
			AssertEquals("LCT", true, invoiceHeader.AddInfo.ZA_LCTInfo.ReadOnly);
			AssertEquals("LCTE", true, invoiceHeader.AddInfo.ZA_LCTEInfo.ReadOnly);
			AssertEquals("WUV", true, invoiceHeader.AddInfo.ZA_WUVInfo.ReadOnly);
			AssertEquals("WRU", true, invoiceHeader.AddInfo.ZA_WRUInfo.ReadOnly);
			AssertEquals("WRQ", true, invoiceHeader.AddInfo.ZA_WRQInfo.ReadOnly);
			AssertEquals("UQ2", true, invoiceHeader.AddInfo.ZA_UQ2Info.ReadOnly);
			AssertEquals("QT2", true, invoiceHeader.AddInfo.ZA_QT2Info.ReadOnly);
			AssertEquals("AQIS", true, invoiceHeader.AddInfo.ZA_AQISInfo.ReadOnly);
			AssertEquals("PIQ", true, invoiceHeader.AddInfo.ZA_PIQInfo.ReadOnly);
			AssertEquals("ODF", true, invoiceHeader.AddInfo.ZA_ODFInfo.ReadOnly);
			AssertEquals("DSA", true, invoiceHeader.AddInfo.ZA_DSAInfo.ReadOnly);
			AssertEquals("DSN", true, invoiceHeader.AddInfo.ZA_DSNInfo.ReadOnly);
			AssertEquals("CSC", true, invoiceHeader.AddInfo.ZA_CSCInfo.ReadOnly);
			AssertEquals("DTY", true, invoiceHeader.AddInfo.ZA_DTYInfo.ReadOnly);
			AssertEquals("DRE", true, invoiceHeader.AddInfo.ZA_DREInfo.ReadOnly);
			AssertEquals("DMP", true, invoiceHeader.AddInfo.ZA_DMPInfo.ReadOnly);
			AssertEquals("CVD", true, invoiceHeader.AddInfo.ZA_CVDInfo.ReadOnly);
			AssertEquals("CSA", true, invoiceHeader.AddInfo.ZA_CSAInfo.ReadOnly);
			AssertEquals("RNO", true, invoiceHeader.AddInfo.ZA_RNOInfo.ReadOnly);
			AssertEquals("QIN", true, invoiceHeader.AddInfo.ZA_QINInfo.ReadOnly);
			AssertEquals("DCX", true, invoiceHeader.AddInfo.ZA_DCXInfo.ReadOnly);
			AssertEquals("DRC", true, invoiceHeader.AddInfo.ZA_DRCInfo.ReadOnly);
			AssertEquals("ICN", true, invoiceHeader.AddInfo.ZA_ICNInfo.ReadOnly);
			AssertEquals("WER", true, invoiceHeader.AddInfo.ZA_WETInfo.ReadOnly);
			AssertEquals("DXT", true, invoiceHeader.AddInfo.ZA_DXTInfo.ReadOnly);
			AssertEquals("ELA", true, invoiceHeader.AddInfo.ZA_ELAInfo.ReadOnly);
			AssertEquals("FOD", true, invoiceHeader.AddInfo.ZA_FODInfo.ReadOnly);
			AssertEquals("TRN", true, invoiceHeader.AddInfo.ZA_TRNInfo.ReadOnly);
			AssertEquals("LCP", true, invoiceHeader.AddInfo.ZA_LCPInfo.ReadOnly);
			AssertEquals("ISC", true, invoiceHeader.AddInfo.ZA_ISCInfo.ReadOnly);
			AssertEquals("WMC", true, invoiceHeader.AddInfo.ZA_WMCInfo.ReadOnly);
			AssertEquals("PRI", true, invoiceHeader.AddInfo.ZA_PRIInfo.ReadOnly);
			AssertEquals("TCI", true, invoiceHeader.AddInfo.ZA_TCIInfo.ReadOnly);
			AssertEquals("LCTI", true, invoiceHeader.AddInfo.ZA_LCTIInfo.ReadOnly);
			AssertEquals("MLPI", true, invoiceHeader.AddInfo.ZA_MLPIInfo.ReadOnly);
			AssertEquals("SEC", true, invoiceHeader.AddInfo.ZA_SECInfo.ReadOnly);
			AssertEquals("PUP", true, invoiceHeader.AddInfo.ZA_PUPInfo.ReadOnly);
			AssertEquals("TR2", true, invoiceHeader.AddInfo.ZA_TR2Info.ReadOnly);
			AssertEquals("CL2", true, invoiceHeader.AddInfo.ZA_CL2Info.ReadOnly);
			AssertEquals("VID", true, invoiceHeader.AddInfo.ZA_VIDInfo.ReadOnly);

			AssertEquals("PRI_InstrumentType", true, invoiceHeader.AddInfo.PRI_InstrumentTypeInfo.ReadOnly);
			AssertEquals("PRI_InstrumentNo", true, invoiceHeader.AddInfo.PRI_InstrumentNoInfo.ReadOnly);
			AssertEquals("TI2_InstrumentType", true, invoiceHeader.AddInfo.TI2_InstrumentTypeInfo.ReadOnly);
			AssertEquals("TI2_InstrumentNo", true, invoiceHeader.AddInfo.TI2_InstrumentNoInfo.ReadOnly);
			AssertEquals("TCI_InstrumentType", true, invoiceHeader.AddInfo.TCI_InstrumentTypeInfo.ReadOnly);
			AssertEquals("TCI_InstrumentNo", true, invoiceHeader.AddInfo.TCI_InstrumentNoInfo.ReadOnly);

			AssertEquals("EFD", false, invoiceHeader.AddInfo.ZA_EFDInfo.ReadOnly);
		}

		public void TestFieldsThatAreNotReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("POC", false, invoiceHeader.AddInfo.ZA_POCInfo.ReadOnly);
			AssertEquals("PST", false, invoiceHeader.AddInfo.ZA_PSTInfo.ReadOnly);
			AssertEquals("PRT", false, invoiceHeader.AddInfo.ZA_PRTInfo.ReadOnly);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("POC", false, invoiceLine.AddInfo.ZA_POCInfo.ReadOnly);
			AssertEquals("PST", false, invoiceLine.AddInfo.ZA_PSTInfo.ReadOnly);
			AssertEquals("PRT", false, invoiceLine.AddInfo.ZA_PRTInfo.ReadOnly);
		}

		public void TestUQ2Readonly()
		{
			AssertEquals("UQ2 readonly as when dumping is involved, UQ2 is needed, but Reference files do not indicate so.", false, addInfo.ZA_UQ2Info.ReadOnly);
		}

		public void TestDumpingExemptionType()
		{
			addInfo.AddInfoLine = "DXT=C";
			AssertEquals("Dumping Exemption Type", "C", addInfo.ZA_DXT);

			addInfo.ZA_DXT = "G";
			AssertEquals("Dumping Exemption Type", "G", addInfo.ZA_DXT);
		}

		public void TestELACNumber()
		{
			addInfo.AddInfoLine = "ELA=12345678";
			AssertEquals("ELAC Number", "12345678", addInfo.ZA_ELA);

			addInfo.ZA_ELA = "87654321";
			AssertEquals("ELAC", "87654321", addInfo.ZA_ELA);
		}

		public void TestFirmOrderDate()
		{
			addInfo.AddInfoLine = "FOD=050201";
			AssertEquals("Firm Order Date", "050201", addInfo.ZA_FOD);

			addInfo.ZA_FOD = "050303";
			AssertEquals("Firm Order Date", "050303", addInfo.ZA_FOD);
		}

		public void TestTreatmentRateNumber()
		{
			addInfo.AddInfoLine = "TRN=123";
			AssertEquals("Treatment Rate Number", "123", addInfo.ZA_TRN);

			addInfo.ZA_TRN = "555";
			AssertEquals("Treatment Rate Number", "555", addInfo.ZA_TRN);
		}

		public void TestLocalContentPercentage()
		{
			addInfo.AddInfoLine = "LCP=1.25";
			AssertEquals("Local Content Percentage", 1.25M, addInfo.ZA_LCP);

			addInfo.ZA_LCP = 2.25M;
			AssertEquals("Local Content Percentage", 2.25M, addInfo.ZA_LCP);
		}

		public void TestInstrumentSecurityCode()
		{
			addInfo.AddInfoLine = "ISC=123";
			AssertEquals("Instrument Security Code", "123", addInfo.ZA_ISC);

			addInfo.ZA_ISC = "987";
			AssertEquals("Instrument Security Code", "987", addInfo.ZA_ISC);
		}

		public void TestMultipleClearanceCode()
		{
			addInfo.AddInfoLine = "WMC=123";
			AssertEquals("Multiple Clearance Code", "123", addInfo.ZA_WMC);

			addInfo.ZA_WMC = "987";
			AssertEquals("Multiple Clearance Code", "987", addInfo.ZA_WMC);
		}

		public void TestPreferenceOriginCountryCode()
		{
			addInfo.AddInfoLine = "POC=AU";
			AssertEquals("Preference Origin Country/Region Code", "AU", addInfo.ZA_POC);

			addInfo.ZA_POC = "IT";
			AssertEquals("Preference Origin Country/Region Code", "IT", addInfo.ZA_POC);
		}

		public void TestPreferenceSchemeType()
		{
			addInfo.AddInfoLine = "PST=AAA";
			AssertEquals("Preference Scheme Type", "AAA", addInfo.ZA_PST);

			addInfo.ZA_PST = "BBB";
			AssertEquals("Preference Scheme Type", "BBB", addInfo.ZA_PST);
		}

		public void TestPreferenceRuleType()
		{
			addInfo.AddInfoLine = "PRT=AAA";
			AssertEquals("Preference Rule Type", "AAA", addInfo.ZA_PRT);

			addInfo.ZA_PRT = "BBB";
			AssertEquals("Preference Rule Type", "BBB", addInfo.ZA_PRT);
		}

		public void TestPreferenceInstrument()
		{
			AssertEquals("ZA_PRI", ZString.Empty, addInfo.ZA_PRI);

			addInfo.ZA_PRI = "PRI";
			AssertEquals("ZA_PRI", "PRI", addInfo.ZA_PRI);
		}

		public void TestTariffClassificationInstrument()
		{
			AssertEquals("ZA_TCI", ZString.Empty, addInfo.ZA_TCI);

			addInfo.ZA_TCI = "TCI";
			AssertEquals("ZA_TCI", "TCI", addInfo.ZA_TCI);
		}

		public void TestAQISCommodityCodeDeclaration_HiddenForInvoiceLine()
		{
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, declaration.AQISCommodityCodes.Count);
		}

		public void TestAQISCommodityCodeDeclaration_HiddenIsSavedForInvoiceLine()
		{
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISCommodityCodes.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("AQIS Commodity Code is not empty", false, loadedDeclaration.AddInfo.ZA_AQISCommCodes_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedDeclaration.AQISCommodityCodes.Count);
			AssertEquals("Values are the same", true, declaration.AddInfo.ZA_AQISCommCodes_Hidden == loadedDeclaration.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodeWithVariousValuesForInvoiceLine()
		{
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2222";
			AssertEquals("Two elements in collection", 2, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1,2222", declaration.AddInfo.ZA_AQISCommCodes_Hidden);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,22222";
			AssertEquals("One element in collection", 1, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISCommCodes_Hidden);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,,";
			AssertEquals("One element in collection", 1, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISCommCodes_Hidden);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1,2", declaration.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodeDeclaration_HiddenForInvoiceHeader()
		{
			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, header.AQISCommodityCodes.Count);

			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, header.AQISCommodityCodes.Count);

			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, header.AQISCommodityCodes.Count);

			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, header.AQISCommodityCodes.Count);

			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, header.AQISCommodityCodes.Count);
		}

		public void TestAQISCommodityCodeDeclaration_HiddenIsSavedForInvoiceHeader()
		{
			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, header.AQISCommodityCodes.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceHeader loadedHeader = factory2.Load<JobComInvoiceHeader>(header.PK);
			AssertEquals("AQIS Commodity Code is not empty", false, loadedHeader.AddInfo.ZA_AQISCommCodes_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedHeader.AQISCommodityCodes.Count);
			AssertEquals("Values are the same", true, header.AddInfo.ZA_AQISCommCodes_Hidden == loadedHeader.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodeWithVariousValuesForInvoiceHeader()
		{
			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,2222";
			AssertEquals("Two elements in collection", 2, header.AQISCommodityCodes.Count);
			AssertEquals("Value", "1,2222", header.AddInfo.ZA_AQISCommCodes_Hidden);

			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,22222";
			AssertEquals("One element in collection", 1, header.AQISCommodityCodes.Count);
			AssertEquals("Value", "1", header.AddInfo.ZA_AQISCommCodes_Hidden);

			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,,";
			AssertEquals("One element in collection", 1, header.AQISCommodityCodes.Count);
			AssertEquals("Value", "1", header.AddInfo.ZA_AQISCommCodes_Hidden);

			header.AddInfo.ZA_AQISCommCodes_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, header.AQISCommodityCodes.Count);
			AssertEquals("Value", "1,2", header.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodeDeclaration_HiddenForDeclaration()
		{
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISCommodityCodes.Count);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, declaration.AQISCommodityCodes.Count);
		}

		public void TestAQISCommodityCodeDeclaration_HiddenIsSavedForDeclaration()
		{
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISCommodityCodes.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("AQIS Commodity Code is not empty", false, loadedDeclaration.AddInfo.ZA_AQISCommCodes_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedDeclaration.AQISCommodityCodes.Count);
			AssertEquals("Values are the same", true, declaration.AddInfo.ZA_AQISCommCodes_Hidden == loadedDeclaration.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodeWithVariousValuesForDeclaration()
		{
			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,2222";
			AssertEquals("Two elements in collection", 2, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1,2222", declaration.AddInfo.ZA_AQISCommCodes_Hidden);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,22222";
			AssertEquals("One element in collection", 1, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISCommCodes_Hidden);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,,";
			AssertEquals("One element in collection", 1, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISCommCodes_Hidden);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, declaration.AQISCommodityCodes.Count);
			AssertEquals("Value", "1,2", declaration.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISEntityIds_HiddenLine()
		{
			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, line.AQISEntityIds.Count);

			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, line.AQISEntityIds.Count);

			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, line.AQISEntityIds.Count);

			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, line.AQISEntityIds.Count);

			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, line.AQISEntityIds.Count);
		}

		public void TestAQISEntityIds_HiddenIsSavedLine()
		{
			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, line.AQISEntityIds.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine loadedInvoiceLine = factory2.Load<JobComInvoiceLine>(line.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedInvoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedInvoiceLine.AQISEntityIds.Count);
			AssertEquals("Values are the same", true, line.AddInfo.ZA_AQISEntityIds_Hidden == loadedInvoiceLine.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIdsWithVariousValuesLine()
		{
			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,1234567890123456";
			AssertEquals("Two elements in collection", 2, line.AQISEntityIds.Count);
			AssertEquals("Value", "1,1234567890123456", line.AddInfo.ZA_AQISEntityIds_Hidden);

			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,12345678901234567";
			AssertEquals("One element in collection", 1, line.AQISEntityIds.Count);
			AssertEquals("Value", "1", line.AddInfo.ZA_AQISEntityIds_Hidden);

			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,,";
			AssertEquals("One element in collection", 1, line.AQISEntityIds.Count);
			AssertEquals("Value", "1", line.AddInfo.ZA_AQISEntityIds_Hidden);

			line.AddInfo.ZA_AQISEntityIds_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, line.AQISEntityIds.Count);
			AssertEquals("Value", "1,2", line.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIds_HiddenHeader()
		{
			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, header.AQISEntityIds.Count);

			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, header.AQISEntityIds.Count);

			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, header.AQISEntityIds.Count);

			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, header.AQISEntityIds.Count);

			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, header.AQISEntityIds.Count);
		}

		public void TestAQISEntityIds_HiddenIsSavedHeader()
		{
			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, header.AQISEntityIds.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceHeader loadedInvoiceHeader = factory2.Load<JobComInvoiceHeader>(header.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedInvoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedInvoiceHeader.AQISEntityIds.Count);
			AssertEquals("Values are the same", true, header.AddInfo.ZA_AQISEntityIds_Hidden == loadedInvoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIdsWithVariousValuesHeader()
		{
			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,1234567890123456";
			AssertEquals("Two elements in collection", 2, header.AQISEntityIds.Count);
			AssertEquals("Value", "1,1234567890123456", header.AddInfo.ZA_AQISEntityIds_Hidden);

			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,12345678901234567";
			AssertEquals("One element in collection", 1, header.AQISEntityIds.Count);
			AssertEquals("Value", "1", header.AddInfo.ZA_AQISEntityIds_Hidden);

			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,,";
			AssertEquals("One element in collection", 1, header.AQISEntityIds.Count);
			AssertEquals("Value", "1", header.AddInfo.ZA_AQISEntityIds_Hidden);

			header.AddInfo.ZA_AQISEntityIds_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, header.AQISEntityIds.Count);
			AssertEquals("Value", "1,2", header.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIds_HiddenDeclaration()
		{
			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISEntityIds.Count);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISEntityIds.Count);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, declaration.AQISEntityIds.Count);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISEntityIds.Count);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, declaration.AQISEntityIds.Count);
		}

		public void TestAQISEntityIds_HiddenIsSavedDeclaration()
		{
			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISEntityIds.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedDeclaration.AddInfo.ZA_AQISEntityIds_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedDeclaration.AQISEntityIds.Count);
			AssertEquals("Values are the same", true, declaration.AddInfo.ZA_AQISEntityIds_Hidden == loadedDeclaration.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIdsWithVariousValuesDeclaration()
		{
			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,1234567890123456";
			AssertEquals("Two elements in collection", 2, declaration.AQISEntityIds.Count);
			AssertEquals("Value", "1,1234567890123456", declaration.AddInfo.ZA_AQISEntityIds_Hidden);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,12345678901234567";
			AssertEquals("One element in collection", 1, declaration.AQISEntityIds.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISEntityIds_Hidden);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,,";
			AssertEquals("One element in collection", 1, declaration.AQISEntityIds.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISEntityIds_Hidden);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, declaration.AQISEntityIds.Count);
			AssertEquals("Value", "1,2", declaration.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAggreatedAQISPermitIds_HiddenLine()
		{
			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, line.AQISPermitIds.Count);

			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, line.AQISPermitIds.Count);

			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, line.AQISPermitIds.Count);

			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, line.AQISPermitIds.Count);

			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, line.AQISPermitIds.Count);
		}

		public void TestAggreatedAQISPermitIds_HiddenIsSavedLine()
		{
			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, line.AQISPermitIds.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine loadedInvoiceLine = factory2.Load<JobComInvoiceLine>(line.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedInvoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedInvoiceLine.AQISPermitIds.Count);
			AssertEquals("Values are the same", true, line.AddInfo.ZA_AQISPermitIds_Hidden == loadedInvoiceLine.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAggreatedAQISPermitIdsWithVariousValuesLine()
		{
			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,12345678901234567980123456879012345";
			AssertEquals("Two elements in collection", 2, line.AQISPermitIds.Count);
			AssertEquals("Value", "1,12345678901234567980123456879012345", line.AddInfo.ZA_AQISPermitIds_Hidden);

			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,123456789012345679801234568790123456";
			AssertEquals("One element in collection", 1, line.AQISPermitIds.Count);
			AssertEquals("Value", "1", line.AddInfo.ZA_AQISPermitIds_Hidden);

			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,,";
			AssertEquals("One element in collection", 1, line.AQISPermitIds.Count);
			AssertEquals("Value", "1", line.AddInfo.ZA_AQISPermitIds_Hidden);

			line.AddInfo.ZA_AQISPermitIds_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, line.AQISPermitIds.Count);
			AssertEquals("Value", "1,2", line.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAggreatedAQISPermitIds_HiddenDeclaration()
		{
			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISPermitIds.Count);

			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISPermitIds.Count);

			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, declaration.AQISPermitIds.Count);

			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISPermitIds.Count);

			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, declaration.AQISPermitIds.Count);
		}

		public void TestAggreatedAQISPermitIds_HiddenIsSavedDeclaration()
		{
			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISPermitIds.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedDeclaration.AddInfo.ZA_AQISPermitIds_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedDeclaration.AQISPermitIds.Count);
			AssertEquals("Values are the same", true, declaration.AddInfo.ZA_AQISPermitIds_Hidden == loadedDeclaration.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAggreatedAQISPermitIdsWithVariousValuesDeclaration()
		{
			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,12345678901234567980123456879012345";
			AssertEquals("Two elements in collection", 2, declaration.AQISPermitIds.Count);
			AssertEquals("Value", "1,12345678901234567980123456879012345", declaration.AddInfo.ZA_AQISPermitIds_Hidden);

			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,123456789012345679801234568790123456";
			AssertEquals("One element in collection", 1, declaration.AQISPermitIds.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISPermitIds_Hidden);

			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,,";
			AssertEquals("One element in collection", 1, declaration.AQISPermitIds.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISPermitIds_Hidden);

			declaration.AddInfo.ZA_AQISPermitIds_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, declaration.AQISPermitIds.Count);
			AssertEquals("Value", "1,2", declaration.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAggreatedAQISPermitIds_HiddenHeader()
		{
			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, header.AQISPermitIds.Count);

			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, header.AQISPermitIds.Count);

			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, header.AQISPermitIds.Count);

			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, header.AQISPermitIds.Count);

			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, header.AQISPermitIds.Count);
		}

		public void TestAggreatedAQISPermitIds_HiddenIsSavedHeader()
		{
			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, header.AQISPermitIds.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceHeader loadedInvoiceHeader = factory2.Load<JobComInvoiceHeader>(header.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedInvoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedInvoiceHeader.AQISPermitIds.Count);
			AssertEquals("Values are the same", true, header.AddInfo.ZA_AQISPermitIds_Hidden == loadedInvoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAggreatedAQISPermitIdsWithVariousValuesHeader()
		{
			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,12345678901234567980123456879012345";
			AssertEquals("Two elements in collection", 2, header.AQISPermitIds.Count);
			AssertEquals("Value", "1,12345678901234567980123456879012345", header.AddInfo.ZA_AQISPermitIds_Hidden);

			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,123456789012345679801234568790123456";
			AssertEquals("One element in collection", 1, header.AQISPermitIds.Count);
			AssertEquals("Value", "1", header.AddInfo.ZA_AQISPermitIds_Hidden);

			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,,";
			AssertEquals("One element in collection", 1, header.AQISPermitIds.Count);
			AssertEquals("Value", "1", header.AddInfo.ZA_AQISPermitIds_Hidden);

			header.AddInfo.ZA_AQISPermitIds_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, header.AQISPermitIds.Count);
			AssertEquals("Value", "1,2", header.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAQISProducerCodes_HiddenLine()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, declaration.AQISProducerCodes.Count);
		}

		public void TestAQISProducerCodes_HiddenIsSavedLine()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedDeclaration.AddInfo.ZA_AQISProducerCodes_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedDeclaration.AQISProducerCodes.Count);
			AssertEquals("Values are the same", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden == loadedDeclaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodesWithVariousValuesLine()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,12345678901234567980123456879012345";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1,12345678901234567980123456879012345", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,123456789012345679801234568790123456";
			AssertEquals("One element in collection", 1, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,,";
			AssertEquals("One element in collection", 1, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1,2", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodes_HiddenDeclaration()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, declaration.AQISProducerCodes.Count);
		}

		public void TestAQISProducerCodes_HiddenIsSavedDeclaration()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedDeclaration.AddInfo.ZA_AQISProducerCodes_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedDeclaration.AQISProducerCodes.Count);
			AssertEquals("Values are the same", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden == loadedDeclaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodesWithVariousValuesDeclaration()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,12345678901234567980123456879012345";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1,12345678901234567980123456879012345", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,123456789012345679801234568790123456";
			AssertEquals("One element in collection", 1, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,,";
			AssertEquals("One element in collection", 1, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1,2", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodes_HiddenHeader()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Three elements in collection", 2, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,22,3";
			AssertEquals("Three elements in collection", 3, declaration.AQISProducerCodes.Count);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2,";
			AssertEquals("Three elements in collection", 2, declaration.AQISProducerCodes.Count);
		}

		public void TestAQISProducerCodes_HiddenIsSavedHeader()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,2";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("AQIS Entity Ids is not empty", false, loadedDeclaration.AddInfo.ZA_AQISProducerCodes_Hidden.IsEmpty);
			AssertEquals("Two elements in collection", 2, loadedDeclaration.AQISProducerCodes.Count);
			AssertEquals("Values are the same", true, declaration.AddInfo.ZA_AQISProducerCodes_Hidden == loadedDeclaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodesWithVariousValuesHeader()
		{
			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,12345678901234567980123456879012345";
			AssertEquals("Two elements in collection", 2, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1,12345678901234567980123456879012345", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,123456789012345679801234568790123456";
			AssertEquals("One element in collection", 1, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,,";
			AssertEquals("One element in collection", 1, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "1,,2";
			AssertEquals("Two element in collection", 2, declaration.AQISProducerCodes.Count);
			AssertEquals("Value", "1,2", declaration.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestBlankImportOnlyWhenSavingExport()
		{
			DefaultData.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			DefaultData.Declaration.AddInfo.ZA_EFTReceiptPrinter_Hidden = "12345";

			AssertEquals("Value", "12345", DefaultData.Declaration.AddInfo.ZA_EFTReceiptPrinter_Hidden);
			Factory.Save();
			DefaultData.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Value", "12345", DefaultData.Declaration.AddInfo.ZA_EFTReceiptPrinter_Hidden);
			Factory.Save();
			AssertEquals("Value", ZString.Empty, DefaultData.Declaration.AddInfo.ZA_EFTReceiptPrinter_Hidden);
		}

		public void TestSettingZA_EFDSetsDutyDateOfInvoiceHeaderDutyDate()
		{
			DefaultData.Declaration.JE_DateOfFirstArrival = ZDateTime.Today;
			AssertEquals("Duty Date", DefaultData.Declaration.JE_DateOfFirstArrival, DefaultData.InvoiceHeader.EffectiveDutyDate);
			DefaultData.InvoiceHeader.AddInfo.AddInfoLine = "EFD=310104";
			AssertEquals("Duty Date", DefaultData.InvoiceHeader.AddInfo.EFD, DefaultData.InvoiceHeader.EffectiveDutyDate);
		}

		public void TestTILVInDifferentCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			declaration.JE_ExportDate = ZDateTime.Today;
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today, 0.5m, helper.USDCurrency);
			line.AddInfo.ZA_TILV = "100USD";
			Assert("Saved as it is", line.JI_AddInfo.IndexOf("TILV=") >= 0);
			AssertEquals("TILV in AUD", 200m, line.AddInfo.TILVInAUD);
		}

		public void TestEFDIsAvailableForHeaderOnly()
		{
			Assert("EFD is not available for jobcominvoiceline", addInfo.ZA_EFDInfo.ReadOnly);

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			Assert("EFD is available for jobcominvoiceheader", !invoiceHeader.AddInfo.ZA_EFDInfo.ReadOnly);
		}

		public void TestPRFAndORGStays()
		{
			addInfo.AddInfoLine = "PRF=X*ORG=US";
			AssertEquals("ZA_ORG", "US", addInfo.ZA_ORG);
			AssertEquals("ZA_PRF", "X", addInfo.ZA_PRF);
		}

		public void TestPSTAndORGStays()
		{
			addInfo.AddInfoLine = "PST=X*ORG=US";
			AssertEquals("ZA_ORG", "US", addInfo.ZA_ORG);
			AssertEquals("ZA_PST", "X", addInfo.ZA_PST);
		}

		public void TestAddInfoORGDefaultOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_ORG = "AU";

			AssertEquals("Invoice Line JI_CountryOfOrigin should be defaulted", "AU", invoiceLine.JI_CountryOfOrigin);
		}

		public void TestOriginOfGoodsWithFourChars()
		{
			addInfo.ZA_ORG = "USA";
			var expected = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			AssertEquals("Goods Origin", expected.RN_Code, addInfo.CountryOfOrigin.RN_Code);
		}

		public void TestZA_PRFList()
		{
			Assert("Instance exists with no parent", addInfo.Lookups.ZA_PRFList != null);
		}

		public void TestAddInfoFromString()
		{
			string testAddInfo1 = "ORG=INIA*ISS=23.2";
			addInfo.LoadPropertiesFromString(testAddInfo1);
			AssertEquals("Origin", "INIA", addInfo.ZA_ORG);
			AssertEquals("Invoice Spirit Strength", 23.2m, addInfo.ZA_ISS);

			string testAddInfo2 = "ORG=CHIN*ISS=45.2";
			addInfo.LoadPropertiesFromString(testAddInfo2);
			AssertEquals("Origin", "CHIN", addInfo.ZA_ORG);
			AssertEquals("Invoice Spirit Strength", 45.2m, addInfo.ZA_ISS);

			string testAddInfo3 = "DTY=23.24*AMB=DVTQPOC*LCTE=404*ICN=C23000H";
			addInfo.LoadPropertiesFromString(testAddInfo3);
			AssertEquals("Duty", 23.24m, addInfo.ZA_DTY);
			AssertEquals("Amber Processing", "DVTQPOC", addInfo.ZA_AMB);
			AssertEquals("Luxury Car Tax Exemption", "404", addInfo.ZA_LCTE);
			AssertEquals("Import Credit Number", "C23000H", addInfo.ZA_ICN);
			AssertEquals("Origin", "", addInfo.ZA_ORG);
			AssertEquals("Invoice Spirit Strength", 0m, addInfo.ZA_ISS);

			addInfo.LoadPropertiesFromString(testAddInfo1, false);
			AssertEquals("Origin", "INIA", addInfo.ZA_ORG);
			AssertEquals("Invoice Spirit Strength", 23.2m, addInfo.ZA_ISS);
			AssertEquals("Duty", 23.24m, addInfo.ZA_DTY);
			AssertEquals("Amber Processing", "DVTQPOC", addInfo.ZA_AMB);
			AssertEquals("Luxury Car Tax Exemption", "404", addInfo.ZA_LCTE);
			AssertEquals("Import Credit Number", "C23000H", addInfo.ZA_ICN);
		}

		public void TestRefCountryBusinessObject()
		{
			addInfo.ZA_ORG = "AU";
			AssertEquals("CountryOfOrigin", "AUSTRALIA", addInfo.CountryOfOrigin.RN_Desc.ToUpper());
			addInfo.ZA_ORG = "US";
			AssertEquals("CountryOfOrigin", "UNITED STATES", addInfo.CountryOfOrigin.RN_Desc.ToUpper());
		}

		public void TestGetDumpingExportPrice()
		{
			addInfo.AddInfoLine = "DXP=1000AUD";
			AssertEquals("Dumping Export Price", 1000m, addInfo.DumpingExportAmount);

			addInfo.ZA_DXP = "USD";
			AssertEquals("Dumping Export Price", 0m, addInfo.DumpingExportAmount);
		}

		public void TestGetDumpingExportCurrency()
		{
			addInfo.AddInfoLine = "DXP=1000USD";
			AssertEquals("Dumping Export Currency", "USD", addInfo.DumpingExportCurrency.RX_Code);

			addInfo.ZA_DXP = "1000 XXX";
			AssertEquals("Dumping Export Currency", null, addInfo.DumpingExportCurrency);

			addInfo.ZA_DXP = "1000";
			AssertEquals("Dumping Export Currency", "AUD", addInfo.DumpingExportCurrency.RX_Code);
		}

		public void TestUSAGetsNoPreference()
		{
			addInfo.ZA_ORG = "US";
			AssertEquals("Preference Count for USA", 0, addInfo.Lookups.ZA_PRFList.Count);
		}

		[ExpectNoExceptions]
		public void TestSetORGValueThatIsTooBigInAddInfoText()
		{
			addInfo.LoadPropertiesFromString("ORG=US");
			addInfo.LoadPropertiesFromString("ORG=ABCD");
			addInfo.LoadPropertiesFromString("ORG=ABCDE");
		}

		[ExpectNoExceptions]
		public void TestSetPRFValueThatIsTooBigInAddInfoText()
		{
			addInfo.LoadPropertiesFromString("PRF=A");
			addInfo.LoadPropertiesFromString("PRF=AA");
		}

		[ExpectNoExceptions]
		public void TestSetPSTValueThatIsTooBigInAddInfoText()
		{
			addInfo.LoadPropertiesFromString("PST=A");
			addInfo.LoadPropertiesFromString("PST=AA");
		}

		[ExpectNoExceptions]
		public void TestSetDRCValueThatIsTooBigInAddInfoText()
		{
			addInfo.LoadPropertiesFromString("DRC=US");
			addInfo.LoadPropertiesFromString("DRC=ABC");
		}

		public void TestORGMadeUppercase()
		{
			addInfo.ZA_ORG = "de";
			addInfo.ZA_POC = "nz";
			addInfo.ZA_PST = "us";
			addInfo.ZA_PRT = "p50";

			AssertEquals("ORG/POC/PST/PRT should always be upper case", "ORG=DE*POC=NZ*PRT=P50*PST=US", addInfo.AddInfoLine);
		}

		public void TestZA_DDN_Hidden()
		{
			var n10NDeclaration = Factory.New<JobDeclaration>();
			n10NDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			n10NDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			n10NDeclaration.JE_DeclarationReference = "B12345678";
			JobComInvoiceHeader n10Iinvoice = n10NDeclaration.Invoices.AddNew();
			JobComInvoiceLine n10InvoiceLine = n10Iinvoice.JobComInvoiceLines.AddNew();
			CusEntryHeader n10Entry = n10NDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryLine n10EntryLine = n10Entry.MergedLines.AddNew();
			n10EntryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			n10EntryLine.CL_LineNumber = 2;
			n10InvoiceLine.JI_CL = n10EntryLine.PK;
			n10Entry.EntryNumber = "AAABBBCCC";
			n10EntryLine.CL_AdValoremTariff = "1111.11.11 11";
			n10EntryLine.CL_CustomsValue = 1000.00m;
			n10InvoiceLine.JI_CustomsUnitQty = "NO";
			n10InvoiceLine.JI_CustomsQuantity = 12m;
			n10EntryLine.CL_DutyPercent = 5m;
			n10EntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 250m);
			n10EntryLine.CL_Description = "STUFF";

			Factory.Save();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			addInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			addInfo.ZA_DDN_Hidden = "AAABBBCCC-2";
			AssertEquals("AAABBBCCC", addInfo.ZA_DDN_Hidden);
			AssertEquals(2, addInfo.ZA_DDL_Hidden);
			AssertEquals("1111.11.11 11", line.JI_Tariff);
			AssertEquals(1000.00m, line.AddInfo.ZA_DCV_Hidden);
			AssertEquals(12m, line.JI_CustomsQuantity);
			AssertEquals("NO", line.JI_CustomsUnitQty);
			AssertEquals(5m, line.AddInfo.ZA_DTR_Hidden);
			AssertEquals(250m, line.AddInfo.ZA_DDT_Hidden);
			AssertEquals("STUFF", line.JI_Description);
		}

		public void TestDefaultingDrawbackMetthodAndEDN()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DeclarationReference = "B12345678";
			JobComInvoiceHeader invoiceHeader1 = testDec.Invoices.AddNew();
			JobComInvoiceHeader invoiceHeader2 = testDec.Invoices.AddNew();
			JobComInvoiceHeader invoiceHeader3 = testDec.Invoices.AddNew();
			invoiceHeader1.AddInfo.ZA_DAM_Hidden = "";
			invoiceHeader1.AddInfo.ZA_EDN_Hidden = "";
			invoiceHeader2.AddInfo.ZA_DAM_Hidden = "A";
			invoiceHeader2.AddInfo.ZA_EDN_Hidden = "EDN1";
			invoiceHeader3.AddInfo.ZA_DAM_Hidden = "B";
			invoiceHeader3.AddInfo.ZA_EDN_Hidden = "EDN2";
			JobComInvoiceLine invLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invLine1.AddInfo.ZA_DAM_Hidden = "";
			invLine1.AddInfo.ZA_EDN_Hidden = "";
			invLine2.AddInfo.ZA_DAM_Hidden = "A";
			invLine2.AddInfo.ZA_EDN_Hidden = "EDN1";
			invLine3.AddInfo.ZA_DAM_Hidden = "B";
			invLine3.AddInfo.ZA_EDN_Hidden = "EDN2";
			JobComInvoiceLine invLine4 = invoiceHeader2.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine5 = invoiceHeader2.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine6 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invLine4.AddInfo.ZA_DAM_Hidden = "";
			invLine4.AddInfo.ZA_EDN_Hidden = "";
			invLine5.AddInfo.ZA_DAM_Hidden = "A";
			invLine5.AddInfo.ZA_EDN_Hidden = "EDN1";
			invLine6.AddInfo.ZA_DAM_Hidden = "B";
			invLine6.AddInfo.ZA_EDN_Hidden = "EDN2";
			JobComInvoiceLine invLine7 = invoiceHeader3.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine8 = invoiceHeader3.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine9 = invoiceHeader3.JobComInvoiceLines.AddNew();
			invLine7.AddInfo.ZA_DAM_Hidden = "";
			invLine7.AddInfo.ZA_EDN_Hidden = "";
			invLine8.AddInfo.ZA_DAM_Hidden = "A";
			invLine8.AddInfo.ZA_EDN_Hidden = "EDN1";
			invLine9.AddInfo.ZA_DAM_Hidden = "B";
			invLine9.AddInfo.ZA_EDN_Hidden = "EDN2";

			testDec.DrawbackHeaderAssesmentMethod = "";
			testDec.AddInfo.ZA_EDN_Hidden = "";
			AssertEquals("No change", "", invoiceHeader1.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "", invoiceHeader1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invoiceHeader2.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invoiceHeader2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invoiceHeader3.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invoiceHeader3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "", invLine1.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "", invLine1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invLine2.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invLine2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine3.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "", invLine4.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "", invLine4.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invLine5.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invLine5.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine6.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine6.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "", invLine7.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "", invLine7.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invLine8.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invLine8.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine9.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine9.AddInfo.ZA_EDN_Hidden);

			testDec.DrawbackHeaderAssesmentMethod = "A";
			testDec.AddInfo.ZA_EDN_Hidden = "EDN1";
			AssertEquals("Changed", "A", invoiceHeader1.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN1", invoiceHeader1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invoiceHeader2.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invoiceHeader2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invoiceHeader3.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invoiceHeader3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("Changed", "A", invLine1.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN1", invLine1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invLine2.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invLine2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine3.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "", invLine4.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "", invLine4.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invLine5.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invLine5.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine6.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine6.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "", invLine7.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "", invLine7.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invLine8.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invLine8.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine9.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine9.AddInfo.ZA_EDN_Hidden);

			testDec.DrawbackHeaderAssesmentMethod = "B";
			testDec.AddInfo.ZA_EDN_Hidden = "EDN2";
			AssertEquals("Changed", "B", invoiceHeader1.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN2", invoiceHeader1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("Changed", "B", invoiceHeader2.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN2", invoiceHeader2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invoiceHeader3.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invoiceHeader3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("Changed", "B", invLine1.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN2", invLine1.AddInfo.ZA_EDN_Hidden);
			AssertEquals("Changed", "B", invLine2.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN2", invLine2.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine3.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine3.AddInfo.ZA_EDN_Hidden);
			AssertEquals("Changed", "B", invLine4.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN2", invLine4.AddInfo.ZA_EDN_Hidden);
			AssertEquals("Changed", "B", invLine5.AddInfo.ZA_DAM_Hidden);
			AssertEquals("Changed", "EDN2", invLine5.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine6.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine6.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "", invLine7.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "", invLine7.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "A", invLine8.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN1", invLine8.AddInfo.ZA_EDN_Hidden);
			AssertEquals("No change", "B", invLine9.AddInfo.ZA_DAM_Hidden);
			AssertEquals("No change", "EDN2", invLine9.AddInfo.ZA_EDN_Hidden);
		}

		public void TestFillEmptyLineLevelPropertiesFrom()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();

			invoice.AddInfo.ZA_WRN = "HeaderWRN";
			invoice.AddInfo.ZA_ORG = "FR";
			invoice.AddInfo.ZA_HeaderREL_Hidden = "Y";
			line.AddInfo.ZA_WRN = "LineWRN";

			line.AddInfo.FillEmptyPropertiesFrom(invoice.AddInfo);

			AssertEquals("Line WRN not overwritten", "LineWRN", line.AddInfo.ZA_WRN);
			AssertEquals("Line origin filled in from header", "FR", line.AddInfo.ZA_ORG);
			AssertEquals("Line header rel filled in", "Y", line.AddInfo.ZA_HeaderREL_Hidden);
		}

		public void TestGetEffectiveValue()
		{
			var addInfo = GetNewLineLevelAddInfo();
			var date = new ZDateTime(2006, 1, 1);
			addInfo.ZA_CPQuestionGenDate_Hidden = date;
			Customs.Business.IAddInfo iAddInfo = addInfo;
			AssertNull(iAddInfo.GetEffectiveValue(""));
			AssertNull(iAddInfo.GetEffectiveValue("ASD#@#"));
			AssertEquals(date, iAddInfo.GetEffectiveValue(addInfo.ZA_CPQuestionGenDate_HiddenInfo.Name));
		}

		public void TestDateDeserialisation()
		{
			var addInfo = GetNewLineLevelAddInfo();
			ZDateTime date = new ZDateTime(2006, 1, 1);
			addInfo.ZA_CPQuestionGenDate_Hidden = date;
			string toString = addInfo.ToString();
			var addInfo2 = GetNewLineLevelAddInfo();
			Assert(addInfo != addInfo2);
			addInfo2.LoadPropertiesFromString(toString);
			AssertEquals(date, addInfo2.ZA_CPQuestionGenDate_Hidden);
			addInfo2.LoadPropertiesFromString("CPQuestionGenDate_Hidden=1/1/06");
			AssertEquals(ZDateTime.Empty, addInfo2.ZA_CPQuestionGenDate_Hidden);
			addInfo2.LoadPropertiesFromString("CPQuestionGenDate_Hidden=01/01/2006");
			AssertEquals(date, addInfo2.ZA_CPQuestionGenDate_Hidden);

			addInfo2.LoadPropertiesFromString("ScheduledPaymentDate_Hidden=27/11/2023 19:51");
			AssertEquals("ScheduledPaymentDate_Hidden", new ZDateTime(2023, 11, 27, 19, 51, 00), addInfo2.ZA_ScheduledPaymentDate_Hidden);
		}

		public void TestDateSerialisation()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.ZA_CPQuestionGenDate_Hidden = new ZDateTime(2006, 1, 1);
			addInfo.ZA_ScheduledPaymentDate_Hidden = new ZDateTime(2023, 9, 8, 5, 4, 21);
			AssertEquals("CPQuestionGenDate_Hidden=01/01/2006*ScheduledPaymentDate_Hidden=08/09/2023 05:04", addInfo.ToString());
		}

		public void TestWRQAndWRUComeFromInvoiceQtyWhenNoCustomsUQNature20()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.InvoiceLine.JI_IsPackToBondForLine = true;
			CheckWRQAndWRUComeFromInvoiceQtyWhenNoCustomsUQ(addInfo);
		}

		public void TestWRQAndWRUComeFromInvoiceQtyWhenNoCustomsUQNature30()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			CheckWRQAndWRUComeFromInvoiceQtyWhenNoCustomsUQ(addInfo);
		}

		public void TestWRQAndWRUComeFromInvoiceQtyWhenNoCustomsUQWhenWEA()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			CheckWRQAndWRUComeFromInvoiceQtyWhenNoCustomsUQ(addInfo);
		}

		public void TestWRQAndWRUDoNotComeFromInvoiceQtyNature10()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.InvoiceLine.JI_InvoiceUQ = "KG";
			addInfo.InvoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("WRU", "", addInfo.ZA_WRU);
			AssertEquals("WRQ", 0m, addInfo.ZA_WRQ);
			Assert("Call through from invoice not serialised", !addInfo.InvoiceLine.JI_AddInfo.Contains("WRU"));
			Assert("Call through from invoice not serialised", !addInfo.InvoiceLine.JI_AddInfo.Contains("WRQ"));
		}

		public void TestWRQAndWRUDoNotComeFromInvoiceQtyWhenCustomsUQ()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.InvoiceLine.JI_IsPackToBondForLine = true;
			addInfo.InvoiceLine.JI_CustomsUnitQty = "NO";
			addInfo.InvoiceLine.JI_InvoiceUQ = "KG";
			addInfo.InvoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("WRU", "", addInfo.ZA_WRU);
			AssertEquals("WRQ", 0m, addInfo.ZA_WRQ);
			Assert("Call through from invoice not serialised", !addInfo.InvoiceLine.JI_AddInfo.Contains("WRU"));
			Assert("Call through from invoice not serialised", !addInfo.InvoiceLine.JI_AddInfo.Contains("WRQ"));
		}

		public void TestWRNCopyToWRL()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.ZA_WRL = 1;
			addInfo.ZA_WRN = "DDD3424XXY";
			AssertEquals("WRN", "DDD3424XXY", addInfo.ZA_WRN);
			AssertEquals("WRL", 1, addInfo.ZA_WRL);

			addInfo.ZA_WRN = "AAA3424XXY-43";
			AssertEquals("WRN", "AAA3424XXY", addInfo.ZA_WRN);
			AssertEquals("WRL", 43, addInfo.ZA_WRL);

			addInfo.ZA_WRN = "AAA3424XXY-5";
			AssertEquals("WRN", "AAA3424XXY", addInfo.ZA_WRN);
			AssertEquals("WRL", 5, addInfo.ZA_WRL);

			addInfo.ZA_WRN = "ZZZZZZZZ-AAA";
			AssertEquals("WRN", "ZZZZZZZZ-AAA", addInfo.ZA_WRN);
			AssertEquals("WRL", 5, addInfo.ZA_WRL);
		}

		public void TestCurrencyConverter()
		{
			var testDec = Factory.New<JobDeclaration>();
			AssertEquals("CurrencyConverter from dbo.JobDeclaration's AddInfo", ((ICurrencyConverterProvider)testDec).CurrencyConverter, testDec.AddInfo.CurrencyConverter);

			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			AssertEquals("CurrencyConverter from GroupHeader's AddInfo", groupHeader.CurrencyConverter, groupHeader.AddInfo.CurrencyConverter);

			JobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			AssertEquals("CurrencyConverter from Invoice's AddInfo", invoice.CurrencyConverter, invoice.AddInfo.CurrencyConverter);

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("CurrencyConverter from InvoiceLine's AddInfo", invoiceLine.CurrencyConverter, invoiceLine.AddInfo.CurrencyConverter);
		}

		public void TestNoStackOverlowFromWRQCalculation()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_AddInfo = "RNO=001*IsPackToBondForLine_Hidden=Y*WRQ=2000.00*WRU=NO*WUV=1*WRN=abcdef*WRL=5";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine reloadedLine = factory2.Load<JobComInvoiceLine>(invoiceLine.PK);

			// hit parent this caused overflow
			object x = reloadedLine.ParentLine;
			AssertEquals("WRQ gets loaded", 2000.00m, reloadedLine.AddInfo.ZA_WRQ);
			AssertEquals("WRN gets loaded", "abcdef", reloadedLine.AddInfo.ZA_WRN);
			AssertEquals("WRL gets loaded", 5, reloadedLine.AddInfo.ZA_WRL);
		}

		public void TestIsLoadingAddInfo()
		{
			var addInfo = GetNewLineLevelAddInfo();
			addInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			addInfo.ZA_POC = "NZ";
			addInfo.ZA_PRT = "P50";

			ZString addInfoLine = addInfo.ToString();
			addInfo.LoadPropertiesFromString(addInfoLine);
			AssertEquals("POC stays the same", "NZ", addInfo.ZA_POC);
			AssertEquals("PRT stays the same", "P50", addInfo.ZA_PRT);
		}

		public void TestGetNewValidationForEdificeDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			AUAddInfoValidation validationClass = declaration.AddInfo.Validation;
			AssertEquals("AddInfoValidation class", typeof(AUAddInfoValidation), validationClass.GetType());
		}

		public void TestGetNewValidationForCMRDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			AUAddInfoValidation validationClass = declaration.AddInfo.Validation;
			AssertEquals("CMRAddInfoDeclarationValidation class", typeof(CMRAddInfoDeclarationValidation), validationClass.GetType());
		}

		public void TestGetNewValidationForDrawback()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			AUAddInfoValidation validationClass = declaration.AddInfo.Validation;
			AssertEquals("DrawbackAddInfoDeclarationValidation class", typeof(DrawbackAddInfoDeclarationValidation), validationClass.GetType());
		}

		public void TestGetNewValidationForDrawbackLine()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AUAddInfoValidation validationClass = invoiceLine.AddInfo.Validation;
			AssertEquals("DrawbackAddinfoInvLineValidation class", typeof(DrawbackAddinfoInvLineValidation), validationClass.GetType());
		}

		public void TestGetNewValidationForInvoiceHeader()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AUAddInfoValidation validationClass = invoiceHeader.AddInfo.Validation;
			AssertEquals("CMRAddInfoHeaderValidation class", typeof(CMRAddInfoInvHeaderValidation), validationClass.GetType());
		}

		public void TestGetNewValidationForInvoiceLine()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AUAddInfoValidation validationClass = invoiceLine.AddInfo.Validation;
			AssertEquals("CMRAddInfoLineValidation class", typeof(CMRAddInfoInvLineValidation), validationClass.GetType());
		}

		public void TestGetNewValidationContainsPartAddInfoValidationOnlyWhenPartAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var invoiceAddInfo = invoiceLine.AddInfo;

			AssertEquals(false, invoiceAddInfo.Validation.ContainsPiggybackedValidation(typeof(AUAddInfoValidation)));

			AUOrgSupplierPart part = AUOrgSupplierPart.New(Factory);
			part.OP_PartNum = "~~~";

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.AddInfo.Validation.ContainsPiggybackedValidation(typeof(AUAddInfoValidation));
		}

		public void TestTILVCurrencyDoesNotReturnLocalCurrencyIfEmpty()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("TILV Currency", "AUD", invoiceLine.AddInfo.GetTILVCurrency("AUD").RX_Code);
			AssertEquals("TILV Currency", null, invoiceLine.AddInfo.GetTILVCurrency(""));
		}

		JobComInvoiceLine line1;
		JobComInvoiceLine invoiceLine2;
		JobDeclaration declaration;
		JobComInvoiceHeader header;
		JobComInvoiceLine line;
		AUAddInfo addInfo;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			line = header.JobComInvoiceLines.AddNew();
			line.JI_IsPackToBondForLine = true;
			addInfo = line.AddInfo;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().AddInfo;

		protected override List<string> ColumnsToClearValueAfterTested
		{
			get
			{
				var result = new List<string>();
				foreach (SchemaColumn column in AUAddInfoSchema.All)
				{
					result.Add(column.Name);
				}
				return result;
			}
		}

		DefaultDataRig defaultData;
		DefaultDataRig DefaultData => defaultData ?? (defaultData = new DefaultDataRig(Factory));

		JobDeclaration GetDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;

			return declaration;
		}

		void CheckWRQAndWRUComeFromInvoiceQtyWhenNoCustomsUQ(AUAddInfo addInfo)
		{
			JobComInvoiceLine invoiceLine = addInfo.InvoiceLine;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Invoice UQ", "KG", invoiceLine.JI_InvoiceUQ);
			AssertEquals("Invoice Qty", 10m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("WRU", "KG", addInfo.ZA_WRU);
			AssertEquals("WRQ", 10m, addInfo.ZA_WRQ);
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRQ=10"));
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRU=KG"));

			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 30m;
			AssertEquals("Invoice UQ", "KG", invoiceLine.JI_InvoiceUQ);
			AssertEquals("Invoice Qty", 30m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("WRU", "KG", addInfo.ZA_WRU);
			AssertEquals("WRQ", 30m, addInfo.ZA_WRQ);
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRQ=30"));
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRU=KG"));

			addInfo.ZA_WRQ = 100m;
			AssertEquals("Invoice UQ", "KG", invoiceLine.JI_InvoiceUQ);
			AssertEquals("Invoice Qty", 100m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("WRU", "KG", addInfo.ZA_WRU);
			AssertEquals("WRQ", 100m, addInfo.ZA_WRQ);
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRQ=100"));
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRU=KG"));

			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_InvoiceQuantity = 30m;
			AssertEquals("Invoice UQ", "NO", invoiceLine.JI_InvoiceUQ);
			AssertEquals("Invoice Qty", 30m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("WRU", "KG", addInfo.ZA_WRU);
			AssertEquals("WRQ", 100m, addInfo.ZA_WRQ);
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRQ=100"));
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRU=KG"));

			addInfo.ZA_WRQ = 50m;
			AssertEquals("WRU", "KG", addInfo.ZA_WRU);
			AssertEquals("WRQ", 50m, addInfo.ZA_WRQ);
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRQ=50"));
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRU=KG"));
			AssertEquals("Invoice UQ", "NO", invoiceLine.JI_InvoiceUQ);
			AssertEquals("Invoice Qty", 30m, invoiceLine.JI_InvoiceQuantity);

			addInfo.ZA_WRQ = 80;
			addInfo.ZA_WRU = "NO";
			AssertEquals("WRU", "NO", addInfo.ZA_WRU);
			AssertEquals("WRQ", 80m, addInfo.ZA_WRQ);
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRQ=80"));
			Assert("Serialised", invoiceLine.JI_AddInfo.Contains("WRU=NO"));
			AssertEquals("Invoice UQ", "NO", invoiceLine.JI_InvoiceUQ);
			AssertEquals("Invoice Qty", 80m, invoiceLine.JI_InvoiceQuantity);
		}

		AUAddInfo GetNewLineLevelAddInfo()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine.AddInfo;
		}
	}
}
