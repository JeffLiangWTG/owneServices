using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZStmALogModuleTest;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZStmALogFilterBusinessObject))]
	class ZStmALogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ZStmALogFilterBusinessObject(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>());
		}

		public void TestFilters()
		{
			AssertFilters(LogsToShow.All, true);
			AssertFilters(LogsToShow.Operations, true);
			AssertFilters(LogsToShow.ChangeLogs, false);
		}

		void AssertFilters(LogsToShow logsToShow, bool estimatesAndCanceledExist)
		{
			ZStmALogFilterBusinessObject filterObject = new ZStmALogFilterBusinessObject(Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>());
			filterObject.LogsToShow = logsToShow;
			ModuleFilter f1 = filterObject[ZStmALogFilterBusinessObject.Schema.ShowCancelled];
			ModuleFilter f2 = filterObject[ZStmALogFilterBusinessObject.Schema.ShowEstimates];
			if (estimatesAndCanceledExist)
			{
				AssertEquals(FilterVisibility.AlwaysApplied, f1.Visibility);
				AssertEquals(FilterVisibility.AlwaysApplied, f2.Visibility);
			}
			else
			{
				AssertNull(f1);
				AssertNull(f2);
			}
			ModuleFilter f3 = filterObject[ZStmALogFilterBusinessObject.Schema.ShowFor];
			AssertEquals(FilterVisibility.AlwaysApplied, f3.Visibility);
			AssertNotNull(filterObject[ZStmALogFilterBusinessObject.Schema.EventTime]);
			AssertNotNull(filterObject[ZStmALogFilterBusinessObject.Schema.PostedTime]);
		}

		public void TestClone()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var control = new ZStmALogFilterBusinessObject(dummy);
			Assert(!control.LogsParentPK.IsEmpty);
			var control2 = (ZStmALogFilterBusinessObject)control.Clone();
			AssertEquals(control.LogsParentPK, control2.LogsParentPK);
		}

		public void TestShouldAddActiveStatusFilter()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			using var module = GetModuleForTest(dummy);
			var filterBo = module.FilterBusinessObject;

			AssertEquals(false, filterBo.ShouldAddActiveStatusFilter);
			AssertNull(filterBo[FilterDescriptions.ActiveStatus]);
		}

		public void TestIsCancelled()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			dummy.GetLogs().AddNew();
			dummy.GetLogs().AddNew();
			var log3 = dummy.GetLogs().AddNew();
			log3.Cancel();

			using (var module = GetModuleForTest(dummy))
			{
				AssertEquals("Precondition", 0, module.GridCollection.Count);
				module.Find();
				AssertEquals("should find 2 records as one is cancelled", 2, module.GridCollection.Count);
				((ModuleFlagsFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowCancelled]).Property0 = true;
				module.Find();
				AssertEquals("should find 3 records - together with cancelled", 3, module.GridCollection.Count);
			}
		}

		public void TestOrConditionOnShowFilterDoesNotRuinEverything()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log1 = dummy.GetLogs().AddNew();
			var dummy2 = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log2 = dummy2.GetLogs().AddNew();
			var filter = new ZStmALogFilterBusinessObject(dummy);
			var cancelledFilter = (ModuleFlagsFilter)filter[ZStmALogFilterBusinessObject.Schema.ShowCancelled];
			cancelledFilter.Property0 = true;
			cancelledFilter.OrCategory = FilterOrCategory.Aqua;
			var showFilter = (ModuleTextFilter)filter[ZStmALogFilterBusinessObject.Schema.ShowFor];
			showFilter.OrCategory = FilterOrCategory.Aqua;

			AssertEquals(true, showFilter.IsOrCategoryReadOnly);
			AssertEquals(true, showFilter.IsGroupOrCategoryReadOnly);
			AssertCollectionContains(log1, Factory.Load<StmALog>(filter.Filter));
			AssertCollectionNotContains(log2, Factory.Load<StmALog>(filter.Filter));
		}

		public void TestOrConditionOnShowFilterDoesNotRuinEverything_Groups()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log1 = dummy.GetLogs().AddNew();
			var dummy2 = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log2 = dummy2.GetLogs().AddNew();
			var filter = new ZStmALogFilterBusinessObject(dummy);

			var cancelledFilter = (ModuleFlagsFilter)filter[ZStmALogFilterBusinessObject.Schema.ShowCancelled];
			cancelledFilter.Property0 = true;
			cancelledFilter.OrCategory = FilterOrCategory.Aqua;
			var showFilter = (ModuleTextFilter)filter[ZStmALogFilterBusinessObject.Schema.ShowFor];
			showFilter.GroupName = "Group2";
			showFilter.GroupOrCategory = FilterOrCategory.Aqua;

			AssertEquals(true, showFilter.IsOrCategoryReadOnly);
			AssertEquals(true, showFilter.IsGroupOrCategoryReadOnly);
			AssertCollectionContains(log1, Factory.Load<StmALog>(filter.Filter));
			AssertCollectionNotContains(log2, Factory.Load<StmALog>(filter.Filter));
		}

		public void TestIsEstimate()
		{
			DummyEnterpriseBusinessObject dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			StmALog log1 = dummy.GetLogs().AddNew();
			StmALog log2 = dummy.GetLogs().AddNew();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			StmALog log3 = dummy.GetLogs().AddNew(Events.EditedARecord, ZDateTimeOffset.Empty, ZBool.True);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			using (var module = GetModuleForTest(dummy))
			{
				AssertEquals("Precondition", 0, module.GridCollection.Count);
				module.Find();
				AssertEquals("should find 2 records as one is an estimate", 2, module.GridCollection.Count);
				(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowEstimates] as ModuleFlagsFilter).Property0 = true;
				module.Find();
				AssertEquals("should find 3 records - together with estimate", 3, module.GridCollection.Count);
			}
		}

		public void TestDatePeriod()
		{
			AssertDates(ZDateTime.Today, ZDateTime.Today, "Today", 1);
			AssertDates(ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1), "Tomorrow", 1);
			AssertDates(ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(2), ModuleDateFilter.SpecifiedDateRange, 0);
			AssertDates(ZDateTime.Today, ZDateTime.Today.AddDays(1), ModuleDateFilter.SpecifiedDateRange, 2);
			AssertDates(ZDateTime.Today.AddDays(-5), ZDateTime.Today, ModuleDateFilter.SpecifiedDateRange, 1);
			AssertDates(ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(1), ModuleDateFilter.SpecifiedDateRange, 2);
			AssertDates(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-1), ModuleDateFilter.SpecifiedDateRange, 0);
		}

		void AssertDates(ZDateTime dateFrom, ZDateTime dateTo, ZString propertySearch, int count)
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			dummy.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: ZDateTimeOffset.Today));
			dummy.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: ZDateTimeOffset.Today.AddDays(1)));

			using (var module = GetModuleForTest(dummy))
			{
				AssertEquals("Precondition", 0, module.GridCollection.Count);
				module.Find();
				AssertEquals("should find all 2 records", 2, module.GridCollection.Count);
				((ModuleDateFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventTime]).Property1 = dateFrom;
				((ModuleDateFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventTime]).Property2 = dateTo;
				((ModuleDateFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventTime]).IsActive = true;
				((ModuleDateFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventTime]).PropertySearch = propertySearch;
				module.Find();
				AssertEquals("should find " + count + " record ", count, module.GridCollection.Count);
			}
		}

		public void TestShowFor()
		{
			DummyWithOverridenRelated dummy = Factory.NewWithValidTestData<DummyWithOverridenRelated>();
			StmALog log1 = dummy.Logs.AddNew();
			var filter = new ZStmALogFilterBusinessObject(dummy);
			AssertEquals("should find all 2 records", 2, Factory.Load<StmALog>(filter.Filter).Length);

			filter[ZStmALogFilterBusinessObject.Schema.ShowFor].IsActive = true;
			(filter[ZStmALogFilterBusinessObject.Schema.ShowFor] as ModuleTextFilter).Property = "DummyBizo";

			AssertEquals("should find all 1 records", 1, Factory.Load<StmALog>(filter.Filter).Length);
			AssertEquals("should be log of a master object", log1.PK, Factory.Load<StmALog>(filter.Filter)[0].PK);

			(filter[ZStmALogFilterBusinessObject.Schema.ShowFor] as ModuleTextFilter).Property = "DummyBizo [2]";

			AssertEquals("should find all 1 records", 1, Factory.Load<StmALog>(filter.Filter).Length);
			AssertEquals("should be log of a another object", dummy.AnotherLogPK, Factory.Load<StmALog>(filter.Filter)[0].PK);
		}

		public void TestShowFor_DontReturnAllTheLogsWhenItsBlank()
		{
			DummyWithOverridenRelated dummy = Factory.NewWithValidTestData<DummyWithOverridenRelated>();
			_ = dummy.AnotherDummy;
			StmALog log1 = dummy.Logs.AddNew();

			using (var module = GetModuleForTest(dummy))
			{
				AssertEquals("Precondition", 0, module.GridCollection.Count);
				module.Find();

				AssertEquals("should find all 2 records", 2, module.GridCollection.Count);

				(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowFor] as ModuleTextFilter).Property = "";
				(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowFor] as ModuleTextFilter).PropertyInfo.ValueChanged += (s, e) =>
				{
					if ((module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowFor] as ModuleTextFilter).Property != "")
					{
						(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowFor] as ModuleTextFilter).Property = "";
					}
				};
				module.Find();
				AssertEquals("should find all 2 record", 2, module.GridCollection.Count);
			}
		}

		public void TestShowFor_ShouldUseTableValuedParameters()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithOverridenRelated>();
			using (var module = GetModuleForTest(dummy))
			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					module.Find();
					var executedCommand = Db.Connection.ExecutedCommands.First(c => c.Contains("FROM dbo.StmALog"));
					AssertContains("Should use TVPs", "(SL_Parent in (SELECT Value FROM", executedCommand);
					Assert("Should not use explicit values", !Regex.IsMatch(executedCommand, @"\(SL_Parent in \((@(.*?),)*?@(.*?)\)"));
				}
			}
		}

		public void TestUserInitials()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var log1 = dummy.Logs.AddNew();
			var log2 = dummy.Logs.AddNew();
			log1.SL_GS_NKUser = "A";
			log2.SL_GS_NKUser = "B";

			using var module = GetModuleForTest(dummy);
			AssertEquals("Precondition", 0, module.GridCollection.Count);

			module.Find();
			AssertEquals("should find all 2 records", 2, module.GridCollection.Count);

			(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.User] as ModuleNkFilter).Property = "A";
			(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.User] as ModuleNkFilter).IsActive = true;
			module.Find();
			AssertEquals("should find 1 record", 1, module.GridCollection.Count);
			AssertEquals("should find 1 record", log1.PK, module.GridCollection.ToArray()[0].PK);

			(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.User] as ModuleNkFilter).Property = "B";
			module.Find();
			AssertEquals("should find 1 record", 1, module.GridCollection.Count);
			AssertEquals("should find 1 record", log2.PK, module.GridCollection.ToArray()[0].PK);
		}

		public void TestReference()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			StmALog log1 = dummy.Logs.AddNew();
			StmALog log2 = dummy.Logs.AddNew();

			using (log1.LockForUpdatingKeyFieldsForTesting())
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_Reference = "AAA";
				log2.SL_Reference = "BBB";
			}

			using (var module = GetModuleForTest(dummy))
			{
				AssertEquals("Precondition", 0, module.GridCollection.Count);
				module.Find();
				AssertEquals("should find 2 records", 2, module.GridCollection.Count);

				((ModuleTextFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.Reference]).Property = "AAA";
				((ModuleTextFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.Reference]).IsActive = true;
				module.Find();
				AssertEquals("should find 1 record", 1, module.GridCollection.Count);
				AssertEquals("should find 1 records", log1.PK, module.GridCollection.ToArray()[0].PK);

				(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.Reference] as ModuleTextFilter).Property = "CCC";
				module.Find();
				AssertEquals("should find 0 record", 0, module.GridCollection.Count);
			}
		}

		public void TestLogsToShowTypes()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			int count = 0;
			foreach (var evt in Events.ChangeLogs)
			{
				dummy.Logs.AddNew(evt);
				count++;
			}
			dummy.Logs.AddNew(Events.RecordAudited);

			using (var module = GetModuleForTest(dummy))
			{
				((ZStmALogFilterBusinessObject)module.FilterBusinessObject).LogsToShow = LogsToShow.All;
				module.Find();
				AssertEquals("should find all records", count + 1, module.GridCollection.Count);
			}

			using (var module = GetModuleForTest(dummy))
			{
				((ZStmALogFilterBusinessObject)module.FilterBusinessObject).LogsToShow = LogsToShow.ChangeLogs;
				module.Find();
				AssertEquals("should find all change logs records", count, module.GridCollection.Count);
			}

			using (var module = GetModuleForTest(dummy))
			{
				((ZStmALogFilterBusinessObject)module.FilterBusinessObject).LogsToShow = LogsToShow.Operations;
				module.Find();
				AssertEquals("should find one operation record", 1, module.GridCollection.Count);
			}
		}

		public void TestEventCode()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			StmALog log1 = dummy.Logs.AddNew();
			StmALog log2 = dummy.Logs.AddNew();

			using (log1.LockForUpdatingKeyFieldsForTesting())
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log1.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
				log2.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}

			using (var module = GetModuleForTest(dummy))
			{
				AssertEquals("Precondition", 0, module.GridCollection.Count);
				module.Find();
				AssertEquals("should find 2 records", 2, module.GridCollection.Count);

				((ModuleTextFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventCode]).Property = "ADD";
				((ModuleTextFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventCode]).IsActive = true;
				module.Find();
				AssertEquals("should find 1 record", 1, module.GridCollection.Count);
				AssertEquals("should find 1 record", log1.PK, module.GridCollection.ToArray()[0].PK);

				(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventCode] as ModuleTextFilter).Property = "SIV";
				module.Find();
				AssertEquals("should find 0 records", 0, module.GridCollection.Count);
			}
		}

		public void TestEventCode_ProductivityWiseEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			StmALog log1 = dummy.Logs.AddNew();
			StmALog log2 = dummy.Logs.AddNew();

			using (log1.LockForUpdatingKeyFieldsForTesting())
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log1.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
				log2.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}

			using (var module = GetModuleForTest(dummy))
			{
				AssertEquals("Precondition", 0, module.GridCollection.Count);
				module.Find();
				AssertEquals("should find 2 records", 2, module.GridCollection.Count);

				((ModuleTextFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventCode]).Property = "ADD";
				((ModuleTextFilter)module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventCode]).IsActive = true;
				module.Find();
				AssertEquals("should find 1 record", 1, module.GridCollection.Count);
				AssertEquals("should find 1 record", log1.PK, module.GridCollection.ToArray()[0].PK);

				(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventCode] as ModuleTextFilter).Property = "CAO";
				module.Find();
				AssertEquals("should find 0 records -- valid CW1 event, invalid PW event", 0, module.GridCollection.Count);

				(module.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.EventCode] as ModuleTextFilter).Property = "Z0Z"; // This is a code for a non-existent event for testing
				module.Find();
				AssertEquals("should find 0 records -- invalid CW1 event, invalid PW event", 0, module.GridCollection.Count);
			}
		}

		#region Test classses

		ZStmALogModuleForTest GetModuleForTest(IStmALogParent master)
		{
			var logsModule = new ZStmALogModuleForTest();
			logsModule.InitData(master, null);
			return logsModule;
		}

		public class DummyWithOverridenRelated : DummyEnterpriseBusinessObject
		{
			public DummyWithOverridenRelated(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override BusinessObject[] BusinessObjectsWithRelatedEvents
			{
				get { return new BusinessObject[] { this, AnotherDummy }; }
			}

			BusinessObject anotherDummy;
			public BusinessObject AnotherDummy
			{
				get
				{
					if (anotherDummy == null)
					{
						anotherDummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
						AnotherLogPK = anotherDummy.GetLogs().AddNew().PK;
					}
					return anotherDummy;
				}
			}

			public ZGuid AnotherLogPK { get; set; }
		}

		#endregion
	}
}
