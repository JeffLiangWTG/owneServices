using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestCargoStatusFilters()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.CH_CargoStatus = BRCargoStatusList.Codes.Stored;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.CH_CargoStatus = BRCargoStatusList.Codes.InTransit;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.CH_CargoStatus = BRCargoStatusList.Codes.CargoFullyExported;

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.CH_CargoStatus = ZString.Empty;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, BRCargoStatusList.Codes.Stored, new [] { declaration1 }, DeclarationFilterConstants.CargoStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, BRCargoStatusList.Codes.InTransit, new [] { declaration2 }, DeclarationFilterConstants.CargoStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, BRCargoStatusList.Codes.CargoFullyExported, new [] { declaration3 }, DeclarationFilterConstants.CargoStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, BRCargoStatusList.Codes.Stored, new [] { declaration2, declaration3, declaration4 }, DeclarationFilterConstants.CargoStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, BRCargoStatusList.Codes.InTransit, new [] { declaration1, declaration3, declaration4 }, DeclarationFilterConstants.CargoStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, BRCargoStatusList.Codes.CargoFullyExported, new [] { declaration1, declaration2, declaration4 }, DeclarationFilterConstants.CargoStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsBlank, ZString.Empty, new [] { declaration4 }, DeclarationFilterConstants.CargoStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new [] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.CargoStatus);
			});
		}

		public override void TestCombinedMessageStatus()
		{
			var declaration0 = Factory.New<JobDeclaration>();
			declaration0.JE_MessageStatus = "AWA";
			var entryHeader0 = declaration0.CustomsEntryHeaders.AddNew();
			entryHeader0.CH_Status = "AWA";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[filterBO.MessageStatusText];
			filter.IsActive = true;
			filter.Property = "AWA";
			var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(declaration0.PK, filteredDecs[0].PK);
		}

		public void TestAdministrativeStatusFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryHeaders.AddNew().CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Deferred;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew().CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.InProcess;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.CustomsEntryHeaders.AddNew();

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, BRAdministrativeStatusList.Codes.Deferred, new [] { declaration1 }, DeclarationFilterConstants.AdministrativeStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, BRAdministrativeStatusList.Codes.Deferred, new [] { declaration2, declaration3 }, DeclarationFilterConstants.AdministrativeStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsBlank, ZString.Empty, new [] { declaration3 }, DeclarationFilterConstants.AdministrativeStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new [] { declaration1, declaration2 }, DeclarationFilterConstants.AdministrativeStatus);
			});
		}

		public void TestUCRNumberFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.UniqueConsignmentReference = "20BR0000000001";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.UniqueConsignmentReference = "21BR0000000001";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.UniqueConsignmentReference = "20BR0000000002";

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.UniqueConsignmentReference = ZString.Empty;

			var declaration5 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration5.CustomsEntryInstructions.AddNew();
			instruction.UCRNumber = "20BR0000000001";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "20BR0000000001", new [] { declaration1, declaration5 }, DeclarationFilterConstants.UCRNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "20BR", new [] { declaration1, declaration3, declaration5 }, DeclarationFilterConstants.UCRNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "BR", new [] { declaration1, declaration2, declaration3, declaration5 }, DeclarationFilterConstants.UCRNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, "20BR0000000001", new [] { declaration2, declaration3 }, DeclarationFilterConstants.UCRNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.DoesNotStartWith, "20BR", new [] { declaration2 }, DeclarationFilterConstants.UCRNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotContains, "21BR", new [] { declaration1, declaration3, declaration5 }, DeclarationFilterConstants.UCRNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new [] { declaration1, declaration2, declaration3, declaration5 }, DeclarationFilterConstants.UCRNumber);
			});
		}

		public void TestLPCOFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			var lpco1 = invoiceLine1.LPCOJobComInvLineRefsCollection.AddNew();
			lpco1.JG_ReferenceNumber = "00000001";

			lpco1 = invoiceLine1.LPCOJobComInvLineRefsCollection.AddNew();
			lpco1.JG_ReferenceNumber = "99000004";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			var lpco2 = invoiceLine2.LPCOJobComInvLineRefsCollection.AddNew();
			lpco2.JG_ReferenceNumber = "99000002";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			var lpco3 = invoiceLine3.LPCOJobComInvLineRefsCollection.AddNew();
			lpco3.JG_ReferenceNumber = "00000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { declaration1 }, DeclarationFilterConstants.Lpco);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { declaration1, declaration2 }, DeclarationFilterConstants.Lpco);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "03", new [] { declaration3 }, DeclarationFilterConstants.Lpco);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "99,03", new [] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.Lpco);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestClearanceDateFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.CH_EntryReleaseDate = ZDateTime.Today;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.CH_EntryReleaseDate = ZDateTime.Today.AddDays(-45);

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.CH_EntryReleaseDate = ZDateTime.Today.AddDays(-15);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.CH_EntryReleaseDate = ZDateTime.Empty;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(-40), new [] { declaration2 }, DeclarationFilterConstants.ClearanceDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), new [] { declaration1 }, DeclarationFilterConstants.ClearanceDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-20), ZDateTime.Empty, new [] { declaration1, declaration3 }, DeclarationFilterConstants.ClearanceDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, new [] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.ClearanceDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, new [] { declaration4 }, DeclarationFilterConstants.ClearanceDate);
		}

		[TestDate(2023, 1, 1)]
		public void TestSubmittedDateFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.CH_EntrySubmittedDate = ZDateTime.Today;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.CH_EntrySubmittedDate = ZDateTime.Today.AddDays(-45);

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.CH_EntrySubmittedDate = ZDateTime.Today.AddDays(-15);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.CH_EntrySubmittedDate = ZDateTime.Empty;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(-40), new [] { declaration2 }, DeclarationFilterConstants.DateFilterTypes.Submitted);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), new [] { declaration1 }, DeclarationFilterConstants.DateFilterTypes.Submitted);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-20), ZDateTime.Empty, new [] { declaration1, declaration3 }, DeclarationFilterConstants.DateFilterTypes.Submitted);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, new [] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.DateFilterTypes.Submitted);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, new [] { declaration4 }, DeclarationFilterConstants.DateFilterTypes.Submitted);
		}

		public void TestNaladiNccaFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.NaladiNcca = "00000001";

			var invoiceLine2 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.NaladiNcca = "99000004";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.NaladiNcca = "99000002";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine4 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine4.NaladiNcca = "00000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { declaration1 }, DeclarationFilterConstants.NaladiNcca);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { declaration1, declaration2 }, DeclarationFilterConstants.NaladiNcca);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "03", new [] { declaration3 }, DeclarationFilterConstants.NaladiNcca);
			});
		}

		public void TestNaladiSHFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.NaladiHs = "00000001";

			var invoiceLine2 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.NaladiHs = "99000004";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.NaladiHs = "99000002";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine4 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine4.NaladiHs = "00000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { declaration1 }, DeclarationFilterConstants.NaladiHs);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { declaration1, declaration2 }, DeclarationFilterConstants.NaladiHs);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "03", new [] { declaration3 }, DeclarationFilterConstants.NaladiHs);
			});
		}

		public void TestImportLicenseFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.ImportLicenseNumber = "00000001";

			var invoiceLine2 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.ImportLicenseNumber = "99000004";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.ImportLicenseNumber = "99000002";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine4 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine4.ImportLicenseNumber = "00000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { declaration1 }, DeclarationFilterConstants.ImportLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { declaration1, declaration2 }, DeclarationFilterConstants.ImportLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "03", new [] { declaration3 }, DeclarationFilterConstants.ImportLicense);
			});
		}

		public void TestLinkedDocumentFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			var previuosDoc1 = invoiceLine1.PreviousDocuments.AddNew();
			previuosDoc1.CSI_ReferenceNumber = "00000001";
			previuosDoc1.CSI_LineNo = 1;

			previuosDoc1 = invoiceLine1.PreviousDocuments.AddNew();
			previuosDoc1.CSI_ReferenceNumber = "99000004";
			previuosDoc1.CSI_LineNo = 2;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			var previuosDoc2 = invoiceLine2.PreviousDocuments.AddNew();
			previuosDoc2.CSI_ReferenceNumber = "99000002";
			previuosDoc2.CSI_LineNo = 1;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			var previuosDoc3 = invoiceLine3.PreviousDocuments.AddNew();
			previuosDoc3.CSI_ReferenceNumber = "00000003";
			previuosDoc3.CSI_LineNo = 1;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { declaration1 }, DeclarationFilterConstants.LinkedDocument);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { declaration1, declaration2 }, DeclarationFilterConstants.LinkedDocument);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "03", new [] { declaration3 }, DeclarationFilterConstants.LinkedDocument);
			});
		}

		public void TestForeignDeclarationNumberFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			var mercosul1 = invoiceLine1.MercosulForeignDeclarations.AddNew();
			mercosul1.CSI_Description = "00000001";
			mercosul1.CSI_LineNo = 1;

			mercosul1 = invoiceLine1.MercosulForeignDeclarations.AddNew();
			mercosul1.CSI_Description = "99000004";
			mercosul1.CSI_LineNo = 2;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			var mercosul2 = invoiceLine2.MercosulForeignDeclarations.AddNew();
			mercosul2.CSI_Description = "99000002";
			mercosul2.CSI_LineNo = 1;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			var mercosul3 = invoiceLine3.MercosulForeignDeclarations.AddNew();
			mercosul3.CSI_Description = "00000003";
			mercosul3.CSI_LineNo = 1;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { declaration1 }, DeclarationFilterConstants.ForeignDeclaration);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { declaration1, declaration2 }, DeclarationFilterConstants.ForeignDeclaration);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "03", new [] { declaration3 }, DeclarationFilterConstants.ForeignDeclaration);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestIssueDateFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header1 = declaration1.CustomsEntryHeaders.AddNew();
			header1.EntryNumber = "TST1";
			header1.CusEntryNumber.CE_IssueDate = ZDateTime.Today;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header2 = declaration2.CustomsEntryHeaders.AddNew();
			header2.EntryNumber = "TST2";
			header2.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddDays(-45);

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header3 = declaration3.CustomsEntryHeaders.AddNew();
			header3.EntryNumber = "TST3";
			header3.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddDays(-15);

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration4.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header4 = declaration4.CustomsEntryHeaders.AddNew();
			header4.EntryNumber = "TST4";
			header4.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(-40), new [] { declaration2 }, DeclarationFilterConstants.IssueDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), new [] { declaration1 }, DeclarationFilterConstants.IssueDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-20), ZDateTime.Empty, new [] { declaration1, declaration3 }, DeclarationFilterConstants.IssueDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, new [] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.IssueDate);
			ModuleTestHelper.AssertModuleDateFilterResult<JobDeclaration>(filterBO, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, new [] { declaration4 }, DeclarationFilterConstants.IssueDate);
		}

		public void TestRiskChannel()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var header1 = declaration1.CustomsEntryHeaders.AddNew().CH_RiskChannel = RiskChannelList.Codes.Green;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var header2 = declaration2.CustomsEntryHeaders.AddNew().CH_RiskChannel = RiskChannelList.Codes.Yellow;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.CustomsEntryHeaders.AddNew();

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, RiskChannelList.Codes.Green, new [] { declaration1 }, DeclarationFilterConstants.RiskChannel);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, RiskChannelList.Codes.Yellow, new [] { declaration2 }, DeclarationFilterConstants.RiskChannel);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, RiskChannelList.Codes.Green, new [] { declaration2, declaration3 }, DeclarationFilterConstants.RiskChannel);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, RiskChannelList.Codes.Yellow, new [] { declaration1, declaration3 }, DeclarationFilterConstants.RiskChannel);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsBlank, ZString.Empty, new [] { declaration3 }, DeclarationFilterConstants.RiskChannel);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, RiskChannelList.Codes.Gray, new [] { declaration1, declaration2 }, DeclarationFilterConstants.RiskChannel);
			});
		}

		void AssertFilterName(string filterConstants)
		{
			var filter = (ModuleDateFilter)filterBO[filterConstants];
			Assert("No filter find with the name " + filterConstants, filter == null);
		}

		public void ValidateFiltersNames()
		{
			string[] filters = { "Administrative Status", "Cargo Status", "Unique Consignment Reference(UCR)", "Invoice Line LPCO", "Clearance Date", "NALADI/NCCA", "NALADI/HS", "Invoice Line Import License", "Linked Document Reference", "Invoice Line Foreign Dec. DE Number", "Issue Date" };
			var action = new Action<string>(AssertFilterName);
			Array.ForEach(filters, action);
		}

		public void TestExcludedLPCAndLICJobsAppears()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = BRJobMessageTypeList.Codes.ExWarehouse;
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();

			Assert(!dec1.MatchesFilter(filterObj.Filter));
			Assert(dec2.MatchesFilter(filterObj.Filter));
			Assert(dec3.MatchesFilter(filterObj.Filter));
			Assert(!dec4.MatchesFilter(filterObj.Filter));
		}

		public void TestPermitFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.Permits.AddNew().CSI_ReferenceNumber = "1234";
			invoiceLine1.Permits.AddNew().CSI_ReferenceNumber = "5567";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.Permits.AddNew().CSI_ReferenceNumber = "5589";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.Permits.AddNew().CSI_ReferenceNumber = "0003";
			invoiceLine3.Permits.AddNew().CSI_ReferenceNumber = "";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "1234", new[] { declaration1 }, DeclarationFilterConstants.Permit);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "55", new[] { declaration1, declaration2 }, DeclarationFilterConstants.Permit);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "03", new[] { declaration3 }, DeclarationFilterConstants.Permit);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, "1234", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.Permit);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.DoesNotStartWith, "55", new[] { declaration1, declaration3 }, DeclarationFilterConstants.Permit);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotContains, "3", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.Permit);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsBlank, ZString.Empty, new[] { declaration3 }, DeclarationFilterConstants.Permit);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.Permit);
			});
		}

		public void TestGoodsCatalogFilter()
		{
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_CatalogCode = "123";

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_CatalogCode = "123456";

			var catalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog3.CGC_CatalogCode = "789";

			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CGC_Catalog = catalog1.PK;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CGC_Catalog = catalog2.PK;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_CGC_Catalog = catalog3.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "123", new[] { declaration1 }, DeclarationFilterConstants.GoodsCatalog);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "123", new[] { declaration1, declaration2 }, DeclarationFilterConstants.GoodsCatalog);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "89", new[] { declaration3 }, DeclarationFilterConstants.GoodsCatalog);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, "1234", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.GoodsCatalog);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.DoesNotStartWith, "123", new[] { declaration3 }, DeclarationFilterConstants.GoodsCatalog);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotContains, "10", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.GoodsCatalog);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.GoodsCatalog);
			});
		}

		public void TestAuthorityIdentifierFilter()
		{
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_CatalogCode = "123";
			catalog1.CGC_AuthorityIdentifier = "123";

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_CatalogCode = "123456";
			catalog2.CGC_AuthorityIdentifier = "123456";

			var catalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog3.CGC_CatalogCode = "789";
			catalog3.CGC_AuthorityIdentifier = "789";

			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CGC_Catalog = catalog1.PK;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CGC_Catalog = catalog2.PK;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_CGC_Catalog = catalog3.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "123", new[] { declaration1 }, DeclarationFilterConstants.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "123", new[] { declaration1, declaration2 }, DeclarationFilterConstants.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "89", new[] { declaration3 }, DeclarationFilterConstants.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, "1234", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.DoesNotStartWith, "123", new[] { declaration3 }, DeclarationFilterConstants.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotContains, "10", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.AuthorityIdentifier);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.AuthorityIdentifier);
			});
		}

		public void TestAuthorityVersionFilter()
		{
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.CGC_CatalogCode = "123";
			catalog1.CGC_AuthorityVersion = "123";

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.CGC_CatalogCode = "123456";
			catalog2.CGC_AuthorityVersion = "123456";

			var catalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog3.CGC_CatalogCode = "789";
			catalog3.CGC_AuthorityVersion = "789";

			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CGC_Catalog = catalog1.PK;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CGC_Catalog = catalog2.PK;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration3.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_CGC_Catalog = catalog3.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Equal, "123", new[] { declaration1 }, DeclarationFilterConstants.AuthorityVersion);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.StartsWith, "123", new[] { declaration1, declaration2 }, DeclarationFilterConstants.AuthorityVersion);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.Contains, "89", new[] { declaration3 }, DeclarationFilterConstants.AuthorityVersion);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotEqual, "1234", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.AuthorityVersion);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.DoesNotStartWith, "123", new[] { declaration3 }, DeclarationFilterConstants.AuthorityVersion);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.NotContains, "10", new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.AuthorityVersion);
				ModuleTestHelper.AssertModuleTextFilterResult<JobDeclaration>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new[] { declaration1, declaration2, declaration3 }, DeclarationFilterConstants.AuthorityVersion);
			});
		}
	}
}
