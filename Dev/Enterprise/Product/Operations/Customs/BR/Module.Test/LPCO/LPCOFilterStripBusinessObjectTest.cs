using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LPCOFilterStripBusinessObject))]
	class LPCOFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestAvailableFilters()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.LPCOHolder]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.LPCONumber]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.StartDate]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.EndDate]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.LPCOMessageStatus]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.LPCOCustomsStatus]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.LPCORetroactiveDate]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.LPCOJobNumber]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.ReferenceDate]);
				AssertNotNull(filterBO[CusLPCOHeaderCollection.FilterConstants.UnitOfMeasure]);
			});
		}

		public void TestLPCONumber()
		{
			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today;
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(5);

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "4444";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(7);

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Equal, "11545411", new[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.LPCONumber);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Contains, "54", new[] { cusLpco1, cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCONumber);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.StartsWith, "58", new[] { cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCONumber);
			});
		}

		public void TestHolder()
		{
			var holder1 = Factory.NewWithValidTestData<OrgHeader>();
			holder1.OH_Code = "TEST1";

			var holder2 = Factory.NewWithValidTestData<OrgHeader>();
			holder2.OH_Code = "TEST2";

			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today;
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(10);
			cusLpco1.CPH_OH_PermitHolder = holder1.PK;

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "4444";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(12);
			cusLpco2.CPH_OH_PermitHolder = holder2.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleGuidFilterResult<CusLPCOHeader>(filterBO, holder1.PK, new[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.LPCOHolder);
				ModuleTestHelper.AssertModuleGuidFilterResult<CusLPCOHeader>(filterBO, holder2.PK, new[] { cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCOHolder);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestStartDate()
		{
			var holder1 = Factory.NewWithValidTestData<OrgHeader>();
			holder1.OH_Code = "TEST1";

			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today.AddDays(-10);
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(5);
			cusLpco1.CPH_OH_PermitHolder = holder1.PK;

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "4444";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(15);
			cusLpco2.CPH_OH_PermitHolder = holder1.PK;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<CusLPCOHeader>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(-5), new CusLPCOHeader[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.StartDate);
			ModuleTestHelper.AssertModuleDateFilterResult<CusLPCOHeader>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), new CusLPCOHeader[] { cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.StartDate);
		}

		[TestDate(2023, 1, 1)]
		public void TestEndDate()
		{
			var holder1 = Factory.NewWithValidTestData<OrgHeader>();
			holder1.OH_Code = "TEST1";

			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today.AddDays(-10);
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(5);
			cusLpco1.CPH_OH_PermitHolder = holder1.PK;

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "4444";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(15);
			cusLpco2.CPH_OH_PermitHolder = holder1.PK;

			Factory.Save();

			ModuleTestHelper.AssertModuleDateFilterResult<CusLPCOHeader>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-50), ZDateTime.Today.AddDays(5), new CusLPCOHeader[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.EndDate);
			ModuleTestHelper.AssertModuleDateFilterResult<CusLPCOHeader>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(20), new CusLPCOHeader[] { cusLpco1, cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.EndDate);
		}

		public void TestLPCOPermitReference()
		{
			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today;
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(5);

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "44442222";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(7);

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Equal, "2222", new[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.LPCOJobNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Contains, "22", new[] { cusLpco1, cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCOJobNumber);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.StartsWith, "44", new[] { cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCOJobNumber);
			});
		}

		public void TestLPCOMessageStatus()
		{
			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today;
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(5);
			cusLpco1.CPH_MessageStatus = "1";

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "44442222";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(7);
			cusLpco2.CPH_MessageStatus = "2";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Equal, "1", new[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.LPCOMessageStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.NotEqual, "22", new[] { cusLpco1, cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCOMessageStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.StartsWith, "2", new[] { cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCOMessageStatus);
			});
		}

		public void TestLPCOPermitStatus()
		{
			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today;
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(5);
			cusLpco1.CPH_CustomsStatus = "1";

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "44442222";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(7);
			cusLpco2.CPH_CustomsStatus = "2";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Equal, "1", new[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.LPCOCustomsStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.NotEqual, "22", new[] { cusLpco1, cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCOCustomsStatus);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.StartsWith, "2", new[] { cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCOCustomsStatus);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestLPCORetroactiveDate()
		{
			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_JobNumber = "2222";
			cusLpco1.CPH_Number = "11545411";
			cusLpco1.CPH_StartDate = ZDate.Today.AddDays(-10);
			cusLpco1.CPH_EndDate = ZDate.Today.AddDays(5);
			cusLpco1.CPH_RetroactiveDate = ZDateTimeOffset.Today.AddDays(20);

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_JobNumber = "4444";
			cusLpco2.CPH_Number = "58775451";
			cusLpco2.CPH_StartDate = ZDate.Today;
			cusLpco2.CPH_EndDate = ZDate.Today.AddDays(15);
			cusLpco2.CPH_RetroactiveDate = ZDateTimeOffset.Today;

			Factory.Save();
			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleDateFilterResult<CusLPCOHeader>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20), new CusLPCOHeader[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.LPCORetroactiveDate);
				ModuleTestHelper.AssertModuleDateFilterResult<CusLPCOHeader>(filterBO, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(20), new CusLPCOHeader[] { cusLpco1, cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.LPCORetroactiveDate);
			});
		}

		public void TestUnitOfMeasure()
		{
			var cusLpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco1.CPH_UnitOfMeasure = "KG";

			var cusLpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			cusLpco2.CPH_UnitOfMeasure = "G";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Equal, "KG", new[] { cusLpco1 }, CusLPCOHeaderCollection.FilterConstants.UnitOfMeasure);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.Contains, "G", new[] { cusLpco1, cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.UnitOfMeasure);
				ModuleTestHelper.AssertModuleTextFilterResult<CusLPCOHeader>(filterBO, SQLComparisonOperator.StartsWith, "G", new[] { cusLpco2 }, CusLPCOHeaderCollection.FilterConstants.UnitOfMeasure);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new LPCOFilterStripBusinessObject();

		LPCOFilterStripBusinessObject filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (LPCOFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}

