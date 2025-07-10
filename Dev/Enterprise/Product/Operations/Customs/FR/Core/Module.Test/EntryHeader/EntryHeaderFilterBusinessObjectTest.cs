using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.EntryHeader.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	public class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCorrelationIDFilter()
		{
			var cusEntry1 = Factory.New<CusEntryNumber>();
			cusEntry1.CE_EntryType = "LRN";
			cusEntry1.CE_EntryNum = "1234";
			cusEntry1.Parent = entryHeader1;

			var cusEntry2 = Factory.New<CusEntryNumber>();
			cusEntry2.CE_EntryType = "LRN";
			cusEntry2.CE_EntryNum = "4321";
			cusEntry2.Parent = entryHeader2;

			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.CorrelationID];
			filter.IsActive = true;
			AssertEquals("The default comparation operator is 'Contains'.", SQLComparisonOperator.Contains, filter.SqlComparisonOperator);

			filter.Property = "12";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "32";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));

			filter.Property = "1234";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "78";
			cusEntry2.CE_EntryNum = "6789";
			Factory.Save();
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "6789";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestTrigPointForValFilter()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader1.CH_TriggeringPointForValidation = "PAB";

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader2.CH_TriggeringPointForValidation = "";
			Factory.Save();

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			entryHeader3.CH_TriggeringPointForValidation = "";
			entryHeader3.CH_TriggeringPointForValidation = "NUL";
			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.TrigPointForVal];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PAB";

			Assert(entryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!entryHeader2.MatchesFilter(filterObj.Filter));
			Assert(!entryHeader3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Assert(!entryHeader1.MatchesFilter(filterObj.Filter));
			Assert(entryHeader2.MatchesFilter(filterObj.Filter));
			Assert(entryHeader3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Assert(!entryHeader1.MatchesFilter(filterObj.Filter));
			Assert(entryHeader2.MatchesFilter(filterObj.Filter));
			Assert(!entryHeader3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Assert(entryHeader1.MatchesFilter(filterObj.Filter));
			Assert(!entryHeader2.MatchesFilter(filterObj.Filter));
			Assert(entryHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestAssessmentDateFilter()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction.PK;
			var setDateTime = new ZDateTime(2021, 08, 27, 00, 00, 00);
			entryInstruction.CEI_DateForDuty = setDateTime;

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();

			Factory.Save();

			AssertEquals("Prerequisite: make sure entryHeader1.Assessment Date has a value.", setDateTime, entryHeader1.AssessmentDate);
			AssertEquals("Prerequisite: make sure entryHeader2.Assessment Date has no value.", ZDateTime.Empty, entryHeader2.AssessmentDate);

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.AssessmentDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			AssertEquals("First entry header should match for the filter has no boundaries.", true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should match for the filter has no boundaries.", true, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = setDateTime.AddDays(-10);
			filter.Property2 = setDateTime.AddDays(-1);
			AssertEquals("First entry header should not match because its Assessment Date is outside of the filter boundaries.", false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = setDateTime.AddDays(-1);
			filter.Property2 = setDateTime.AddDays(1);
			AssertEquals("First entry header should match because its Assessment Date is inside of the filter boundaries.", true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = setDateTime.AddDays(1);
			filter.Property2 = setDateTime.AddDays(10);
			AssertEquals("First entry header should not match because its Assessment Date is outside of the filter boundaries.", false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = setDateTime.AddDays(-10);
			filter.Property2 = ZDateTime.Empty;
			AssertEquals("First entry header should match because its Assessment Date is inside of the filter boundaries.", true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = setDateTime;
			filter.Property2 = ZDateTime.Empty;
			AssertEquals("First entry header should match because its Assessment Date is inside of the filter boundaries.", true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = setDateTime.AddDays(1);
			filter.Property2 = ZDateTime.Empty;
			AssertEquals("First entry header should not match because its Assessment Date is outside of the filter boundaries.", false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = setDateTime.AddDays(-1);
			AssertEquals("First entry header should not match because its Assessment Date is outside of the filter boundaries.", false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Today;
			AssertEquals("First entry header should match because its Assessment Date is inside of the filter boundaries.", true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Today.AddDays(1);
			AssertEquals("First entry header should match because its Assessment Date is inside of the filter boundaries.", true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match for the filter has boundaries but Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals("First entry header should match because its Assessment Date is not empty.", true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match because its Assessment Date is empty.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("First entry header should not match because its Assessment Date is not empty.", false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Second entry header should not match because it has no entry instruction.", false, entryHeader2.MatchesFilter(filterObj.Filter));

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine3 = entryHeader3.AllEntryLines.AddNew();
			var invoiceLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
			invoiceLine3.JI_CEI = entryInstruction3.PK;
			entryInstruction3.CEI_DateForDuty = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("Prerequisite: make sure entryHeader3.Assessment Date has no value.", ZDateTime.Empty, entryHeader3.AssessmentDate);
			AssertEquals("Third entry header should match because it has entry instruction and its Assessment Date is null.", true, entryHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestEntryNumberFilter()
		{
			declaration1.JE_MessageType = "IMP";
			declaration2.JE_MessageType = "EXP";

			entryHeader1.EntryNumber = "1234";
			entryHeader2.EntryNumber = "5678";

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = "IMP";
			cusEntryNumber.CE_EntryNum = "1234";
			cusEntryNumber.Parent = entryHeader2;

			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.Constants.EntryNumber];
			filter.IsActive = true;
			filter.Property = "1234";
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "5678";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));

			declaration1.JE_MessageType = "EXP";
			declaration2.JE_MessageType = "IMP";
			Factory.Save();
			filter.Property = "1234";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestDeltaAgreementFilter()
		{
			declaration2.JE_CustomsProfile = "1111";
			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.DeltaAgreement];

			filter.IsActive = true;
			filter.Property = "1111";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "2222";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestDeltaModeFilter()
		{
			declaration2.JE_DeltaMode = "G1";
			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.DeltaMode];

			filter.IsActive = true;
			filter.Property = "G1";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "G2";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestIsDeltaDOneStepSentFilter()
		{
			var declaration3 = Factory.New<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();

			declaration1.JE_DeltaMode = "G1";
			entryHeader1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			declaration2.JE_DeltaMode = "G2";
			entryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			declaration3.JE_DeltaMode = "G2";
			entryHeader3.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.IsDeltaDOneStepSent];

			filter.IsActive = true;
			filter.Property0 = true;
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader3.MatchesFilter(filterObj.Filter));

			filter.Property0 = false;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestIsDeltaDTwoStepSentFilter()
		{
			var declaration3 = Factory.New<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();

			declaration1.JE_DeltaMode = "G1";
			entryHeader1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			declaration2.JE_DeltaMode = "G2";
			entryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			declaration3.JE_DeltaMode = "G2";
			entryHeader3.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.IsDeltaDTwoStepSent];

			filter.IsActive = true;
			filter.Property0 = true;
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader3.MatchesFilter(filterObj.Filter));

			filter.Property0 = false;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestIsDeltaDStepTwoSentOKButZeroLiquidationFilter()
		{
			var declaration3 = Factory.New<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			var entryLine3 = entryHeader3.AllEntryLines.AddNew();
			var declaration4 = Factory.New<JobDeclaration>();
			var entryHeader4 = declaration4.CustomsEntryHeaders.AddNew();
			var entryLine4 = entryHeader4.AllEntryLines.AddNew();
			var declaration5 = Factory.New<JobDeclaration>();
			var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
			var entryLine5 = entryHeader4.AllEntryLines.AddNew();
			var declaration6 = Factory.New<JobDeclaration>();
			var entryHeader6 = declaration6.CustomsEntryHeaders.AddNew();
			var entryLine61 = entryHeader6.AllEntryLines.AddNew();
			var entryLine62 = entryHeader6.AllEntryLines.AddNew();
			var declaration7 = Factory.New<JobDeclaration>();
			var entryHeader7 = declaration7.CustomsEntryHeaders.AddNew();
			var entryLine71 = entryHeader7.AllEntryLines.AddNew();
			var entryLine72 = entryHeader7.AllEntryLines.AddNew();
			var declaration8 = Factory.New<JobDeclaration>();
			var entryHeader8 = declaration8.CustomsEntryHeaders.AddNew();
			var entryLine81 = entryHeader8.AllEntryLines.AddNew();
			var declaration9 = Factory.New<JobDeclaration>();
			var entryHeader9 = declaration9.CustomsEntryHeaders.AddNew();
			var entryLine91 = entryHeader9.AllEntryLines.AddNew();
			var declaration10 = Factory.New<JobDeclaration>();
			var entryHeader10 = declaration10.CustomsEntryHeaders.AddNew();
			var entryLine101 = entryHeader10.AllEntryLines.AddNew();

			declaration1.JE_DeltaMode = "G1";
			entryHeader1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;

			declaration2.JE_DeltaMode = "G2";
			entryHeader2.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var fee2 = entryLine2.Fees.AddNew();
			fee2.CF_ChargeAmount = 1m;

			declaration3.JE_DeltaMode = "G2";
			entryHeader3.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var fee3 = entryLine3.Fees.AddNew();
			fee3.CF_ChargeAmount = 0m;

			declaration4.JE_DeltaMode = "G2";
			entryHeader4.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;

			declaration5.JE_DeltaMode = "G2";
			entryHeader5.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;

			declaration6.JE_DeltaMode = "G2";
			entryHeader6.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var fee6 = entryLine61.Fees.AddNew();
			fee6.CF_ChargeAmount = 1m;

			declaration8.JE_DeltaMode = "G2";
			entryHeader8.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var fee81 = entryLine81.ConfirmedFees.AddNew();
			fee81.CF_ChargeAmount = 2m;
			var fee82 = entryLine81.ConfirmedFees.AddNew();
			fee82.CF_ChargeAmount = 0m;

			declaration9.JE_DeltaMode = "G2";
			entryHeader9.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var fee91 = entryLine91.ConfirmedFees.AddNew();
			fee91.CF_ChargeAmount = 0m;
			var fee92 = entryLine91.Fees.AddNew();
			fee92.CF_ChargeAmount = -1m;

			declaration10.JE_DeltaMode = "G2";
			entryHeader10.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES130;
			var fee101 = Factory.New<CusEntryLineFee>();
			fee101.CF_CL = entryLine101.PK;
			fee101.CF_ChargeAmount = 0m;
			fee101.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;

			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.IsDeltaDStepTwoSentOKButZeroLiquidation];

			filter.IsActive = true;
			filter.Property0 = true;
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("€0 will NOT be rounded up to €1, so if you add a fee with amount €0, you still get €0.", true, entryHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader8.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader9.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader10.MatchesFilter(filterObj.Filter));

			filter.Property0 = false;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader8.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader9.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader10.MatchesFilter(filterObj.Filter));
		}

		public void TestDeltaGFallbackNumberfilter()
		{
			var cusEntryNumber = CusEntryNumber.New(entryHeader2, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France);
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_EntryNum = "1234";
			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.DeltaGFallbackNumber];

			filter.IsActive = true;
			filter.Property = "1234";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "1111";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestDeltaGFallbackNumberStatus()
		{
			var cusEntryNumber = CusEntryNumber.New(entryHeader2, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France);
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_EntryStatus = "TST";
			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.DeltaGFallbackStatus];

			filter.IsActive = true;
			filter.Property = "TST";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));

			filter.Property = "CAN";
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestECSStatusFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.ExitedStatus];

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("filter list", Factory.GetCachedValue<ExportControlStatusList>(), filter.List);
				AssertEquals("filter column", CusEntryHeaderSchema.CH_ExitedStatus, filter.FilterColumn);
			});
		}

		public void TestExitTypeFilter()
		{
			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.ExportExitType];

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			declaration1.JE_ExportExitType = ExportExitTypeList.Codes.OTH;
			declaration2.JE_ExportExitType = ExportExitTypeList.Codes.TRA;
			declaration3.JE_ExportExitType = ZString.Empty;
			Factory.Save();

			filter.IsActive = true;
			filter.Property = ExportExitTypeList.Codes.OTH;

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals(true, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, entryHeader3.MatchesFilter(filterObj.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertEquals(false, entryHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, entryHeader3.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			declaration1 = Factory.New<JobDeclaration>();
			declaration2 = Factory.New<JobDeclaration>();
			entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryLine2 = (CusEntryLine)entryHeader2.AllEntryLines.AddNew();
		}
		JobDeclaration declaration1;
		JobDeclaration declaration2;
		CusEntryHeader entryHeader1;
		CusEntryHeader entryHeader2;
		CusEntryLine entryLine2;
	}
}
