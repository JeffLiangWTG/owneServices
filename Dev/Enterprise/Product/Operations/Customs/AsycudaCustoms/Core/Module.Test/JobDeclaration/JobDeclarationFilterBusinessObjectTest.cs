using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestManifestNumberFilterVisible()
		{
			var businessObject = new JobDeclarationFilterBusinessObject();
			AssertNotNull(businessObject.ModuleFilters[Customs.Module.DeclarationFilterConstants.NumberFilterTypes.ManifestNumber]);
		}

		public void TestContinuousGuaranteeQueryFilter()
		{
			var filterObj = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filterObj[JobDeclarationFilterBusinessObject.JobDeclarationFilterConstants.ContinuousGuarantee]);
			var declaration1 = SetupDeclarationWithGuarantees("X1", "CON", "CON", "LIN", 100m, null);
			var declaration2 = SetupDeclarationWithGuarantees("X2", "STB", "REL", "UNL", 200m, null);
			var declaration3_NoLinkedInstruction = Factory.New<JobDeclaration>();
			Factory.Save();
			var continuousGuaranteeQueryFilter = (ModuleFlagsFilter)filterObj[JobDeclarationFilterBusinessObject.JobDeclarationFilterConstants.ContinuousGuarantee];
			continuousGuaranteeQueryFilter.Property0 = true;
			continuousGuaranteeQueryFilter.Property1 = false;
			continuousGuaranteeQueryFilter.IsActive = true;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			continuousGuaranteeQueryFilter.Property0 = false;
			continuousGuaranteeQueryFilter.Property1 = true;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		public void TestLinkedGuaranteeQueryFilter()
		{
			var filterObj = new JobDeclarationFilterBusinessObject();
			var linkedGuaranteeQueryFilter = (ModuleGuidFilter)filterObj[JobDeclarationFilterBusinessObject.JobDeclarationFilterConstants.LinkedGuarantee];
			AssertNotNull(linkedGuaranteeQueryFilter);
			var guaranteeHeader1 = CreateCusGuarantee("TT1");
			var guaranteeHeader2 = CreateCusGuarantee("TT2");
			var declaration1 = SetupDeclarationWithGuarantees("X1", "CON", "CON", "LIN", 100m, guaranteeHeader1);
			var declaration2 = SetupDeclarationWithGuarantees("X2", "STB", "REL", "UNL", 200m, guaranteeHeader2);
			var declaration3_NoLinkedInstruction = Factory.New<JobDeclaration>();
			Factory.Save();
			linkedGuaranteeQueryFilter.Property = guaranteeHeader1.PK;
			linkedGuaranteeQueryFilter.IsActive = true;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			linkedGuaranteeQueryFilter.Property = ZGuid.NewZGuid();
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			linkedGuaranteeQueryFilter.Property = ZGuid.Empty;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			linkedGuaranteeQueryFilter.Property = guaranteeHeader1.PK;
			linkedGuaranteeQueryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		public void TestGuaranteeAmountQueryFilter()
		{
			var filterObj = new JobDeclarationFilterBusinessObject();
			var guaranteeAmountQueryFilter = (ModuleNumberRangeFilter)filterObj[JobDeclarationFilterBusinessObject.JobDeclarationFilterConstants.GuaranteeAmount];
			AssertNotNull(guaranteeAmountQueryFilter);
			var declaration1 = SetupDeclarationWithGuarantees("X1", "CON", "CON", "LIN", 100m, null);
			var declaration2 = SetupDeclarationWithGuarantees("X2", "STB", "REL", "UNL", 200m, null);
			guaranteeAmountQueryFilter.Property1 = 50m;
			guaranteeAmountQueryFilter.Property2 = 150m;
			guaranteeAmountQueryFilter.IsActive = true;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			guaranteeAmountQueryFilter.Property1 = 150m;
			guaranteeAmountQueryFilter.Property2 = 250m;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			guaranteeAmountQueryFilter.Property1 = 50m;
			guaranteeAmountQueryFilter.Property2 = 250m;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
		}

		public void TestGuaranteeActivityQueryFilter()
		{
			var filterObj = new JobDeclarationFilterBusinessObject();
			var guaranteeActivityQueryFilter = (ModuleTextFilter)filterObj[JobDeclarationFilterBusinessObject.JobDeclarationFilterConstants.GuaranteeActivity];
			AssertNotNull(guaranteeActivityQueryFilter);
			var declaration1 = SetupDeclarationWithGuarantees("X1", "CON", "CON", "LIN", 100m, null);
			var declaration2 = SetupDeclarationWithGuarantees("X2", "STB", "REL", "UNL", 200m, null);
			var declaration3_NoLinkedInstruction = Factory.New<JobDeclaration>();
			Factory.Save();
			guaranteeActivityQueryFilter.Property = "CON";
			guaranteeActivityQueryFilter.IsActive = true;
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.Property = "REL";
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeActivityQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		public void TestGuaranteeStatusQueryFilter()
		{
			var filterObj = new JobDeclarationFilterBusinessObject();
			var guaranteeStatusQueryFilter = (ModuleTextFilter)filterObj[JobDeclarationFilterBusinessObject.JobDeclarationFilterConstants.GuaranteeStatus];
			AssertNotNull(guaranteeStatusQueryFilter);
			var declaration1 = SetupDeclarationWithGuarantees("X1", "CON", "CON", "LIN", 100m, null);
			var declaration2 = SetupDeclarationWithGuarantees("X2", "STB", "REL", "UNL", 200m, null);
			var declaration3_NoLinkedInstruction = Factory.New<JobDeclaration>();
			Factory.Save();
			guaranteeStatusQueryFilter.Property = "LIN";
			guaranteeStatusQueryFilter.IsActive = true;
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.Property = "UNL";
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
			guaranteeStatusQueryFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3_NoLinkedInstruction.MatchesFilter(filterObj.Filter));
		}

		JobDeclaration SetupDeclarationWithGuarantees(ZString instructionStyle, ZString bondType, ZString activityCode, ZString status, ZDecimal amount, CusGuaranteeHeader guaranteeHeader)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = instructionStyle;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();
			var guarantee = entryInstruction.Guarantee;
			guarantee.PW_BondType = bondType;
			guarantee.PW_ActivityCode = activityCode;
			guarantee.PW_BondNumber2 = "REF";
			guarantee.PW_BondAmount = amount;
			guarantee.PW_CPH_Guarantee = guaranteeHeader?.PK ?? ZGuid.Empty;
			guarantee.PW_Status = status;
			Factory.Save();
			return declaration;
		}

		CusGuaranteeHeader CreateCusGuarantee(ZString number)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = number;
			guaranteeHeader.CPH_RN_NKCountryCode = MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			guaranteeHeader.CPH_ApplicationCode = "GUA";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "Entry Reference";
			transaction.CPL_TranValue = 1000m;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction.CPL_AppId = "Job Number";
			transaction.CPL_IsAggregated = ZBool.False;
			transaction.CPL_Comment = "Instruction Desc.";
			transaction.CPL_Procedure = "AAA";
			Factory.Save();
			return guaranteeHeader;
		}
	}
}
