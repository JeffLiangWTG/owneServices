using CargoWise.Types;
using Enterprise.Customs.JP.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestImporterAndSupplierFilter()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);

			var filter = (ModuleGuidsFilterForOrg)filterBusinessObject.ModuleFilters["Importer/Supplier"];
			filter.IsActive = true;

			AssertEquals("Importer(Consignee)/Shipper(Exporter)", filter.MultilingualDescription);
		}

		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestBondedDate()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var dateFilter = (ModuleSingleDateFilter)filterBusinessObject.ModuleFilters[nameof(Business.JobComInvoiceLine.JI_BondedDate)];
			dateFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_BondedDate = ZDate.Today;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.Dates, dateFilter.Category);
				dateFilter.Property1 = ZDate.Today;
				AssertEquals("Filter In", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				dateFilter.Property1 = ZDate.BrettsBirthday;
				AssertEquals("Filter Out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestTradeType()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_TradeType)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_TradeType = "123";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				textFilter.Property = "123";
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "456";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestPreInspectedCargoType()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_PreInspectedCargoType)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_PreInspectedCargoType = PreInspectedCargoTypeList.Codes.A;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				AssertType<PreInspectedCargoTypeList>(textFilter.List);
				textFilter.Property = PreInspectedCargoTypeList.Codes.A;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = PreInspectedCargoTypeList.Codes.B;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestApprovalCertificateCategory()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_ApprovalCertificateCategory)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_ApprovalCertificateCategory = ApprovalCertificateCategoryCodeList.Codes.E2;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				AssertType<ApprovalCertificateCategoryCodeList>(textFilter.List);
				textFilter.Property = ApprovalCertificateCategoryCodeList.Codes.E2;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = ApprovalCertificateCategoryCodeList.Codes.N2;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestCommercialValueType()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_CommercialValueType)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_CommercialValueType = CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				AssertType<CommercialValueTypeList>(textFilter.List);
				textFilter.Property = CommercialValueTypeList.Codes.ImportApprovalNoCommercialValue;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = CommercialValueTypeList.Codes.ImportApprovalHasCommercialOrCombination;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestFoodHygieneCertType()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_FoodHygieneCertificateType)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_FoodHygieneCertificateType = IDACertificateIdList.Codes.Yes;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				AssertType<IDACertificateIdList>(textFilter.List);
				textFilter.Property = IDACertificateIdList.Codes.Yes;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = IDACertificateIdList.Codes._8;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestPlantProtectionCertType()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_PlantProtectionCertificateType)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_PlantProtectionCertificateType = IDACertificateIdList.Codes.Yes;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				AssertType<IDACertificateIdList>(textFilter.List);
				textFilter.Property = IDACertificateIdList.Codes.Yes;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = IDACertificateIdList.Codes._8;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestAnimalQuarantineCertType()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_AnimalQuarantineCertificateType)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_AnimalQuarantineCertificateType = IDACertificateIdList.Codes.Yes;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				AssertType<IDACertificateIdList>(textFilter.List);
				textFilter.Property = IDACertificateIdList.Codes.Yes;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = IDACertificateIdList.Codes._8;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestDeclarationCargoType()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_DeclarationCargoType)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.B;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.ModesAndTypes, textFilter.Category);
				AssertType<DeclarationCargoTypeList>(textFilter.List);
				textFilter.Property = DeclarationCargoTypeList.Codes.B;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = DeclarationCargoTypeList.Codes.G;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestInspectionWitness()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var orgFilter = (ModuleGuidFilter)filterBusinessObject.ModuleFilters[nameof(JobDeclaration.InspectionWitness)];
			orgFilter.IsActive = true;

			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			declaration1.InspectionWitness.E2_OA_Address = newOrg.MainAddress.PK;
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.Organisations, orgFilter.Category);
				orgFilter.Property = newOrg.PK;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				AssertEquals("Non-InspectionWitness", false, declaration2.MatchesFilter(filterBusinessObject.Filter));
				orgFilter.Property = ZGuid.BrettsGuid;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestCustomsNotes()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.JP_CustomsNotes)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration1.CustomsEntryInstructions.AddNew();
			instruction.JP_CustomsNotes = "Customs Notes";
			instruction.JP_BrokersNotes = "Brokers Notes";
			instruction.JP_OwnersNotes = "Owners Notes";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
				textFilter.Property = "Customs Notes";
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "Brokers Notes";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestBrokerNotes()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.JP_BrokersNotes)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration1.CustomsEntryInstructions.AddNew();
			instruction.JP_CustomsNotes = "Customs Notes";
			instruction.JP_BrokersNotes = "Brokers Notes";
			instruction.JP_OwnersNotes = "Owners Notes";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
				textFilter.Property = "Brokers Notes";
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "Owners Notes";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestOwnerNotes()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.JP_OwnersNotes)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration1.CustomsEntryInstructions.AddNew();
			instruction.JP_CustomsNotes = "Customs Notes";
			instruction.JP_BrokersNotes = "Brokers Notes";
			instruction.JP_OwnersNotes = "Owners Notes";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
				textFilter.Property = "Owners Notes";
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "Customs Notes";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestMarksAndNumbers()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.JP_MarksAndNumbers)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration1.CustomsEntryInstructions.AddNew();
			instruction.JP_OwnersNotes = "Owners Notes";
			instruction.JP_MarksAndNumbers = "Marks And Numbers";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
				textFilter.Property = "Marks And Numbers";
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "Owners Notes";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestLoadingConfirmationIsRequired()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var flagFilter = (ModuleFlagsFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_LoadingConfirmationIsRequired)];
			flagFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_LoadingConfirmationIsRequired = true;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, flagFilter.Category);
				flagFilter.Property0 = true;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				flagFilter.Property0 = false;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestContentInspectionResult()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_ContentInspectionResult)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_ContentInspectionResult = ContentInspectionResultList.Codes.ConfirmationRequest;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
				AssertType<ContentInspectionResultList>(textFilter.List);
				textFilter.Property = ContentInspectionResultList.Codes.ConfirmationRequest;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "XXX";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestBeforePermitApplicationReason()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_BeforePermitApplicationReason)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_BeforePermitApplicationReason = BeforePermitApplicationReasonList.Codes.E2;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
				AssertType<BeforePermitApplicationReasonList>(textFilter.List);
				textFilter.Property = BeforePermitApplicationReasonList.Codes.E2;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "XXX";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestCommonControlNumber()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var textFilter = (ModuleTextFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_CommonControlNumber)];
			textFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_CommonControlNumber = "Common Control Number";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
				textFilter.Property = "Common Control Number";
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				textFilter.Property = "XXX";
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}

		public void TestDutyDrawback()
		{
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			filterBusinessObject.QueryObjectType = typeof(JobDeclaration);
			var flagsFilter = (ModuleFlagsFilter)filterBusinessObject.ModuleFilters[nameof(CusEntryInstruction.CEI_DutyDrawback)];
			flagsFilter.IsActive = true;
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.CustomsEntryInstructions.AddNew().CEI_DutyDrawback = Customs.Business.YesNoList.Codes.Yes;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(FilterCategories.NumbersAndReferences, flagsFilter.Category);
				flagsFilter.Property0 = true;
				AssertEquals("Filter in", true, declaration1.MatchesFilter(filterBusinessObject.Filter));
				flagsFilter.Property0 = false;
				AssertEquals("Filter out", false, declaration1.MatchesFilter(filterBusinessObject.Filter));
			});
		}
	}
}
