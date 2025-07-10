using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.UniversalCopy.Module.UniversalCopySchedule.Testing
{
	[TestedType(typeof(UniversalCopyScheduleModuleFilter))]
	public class UniversalCopyScheduleModuleFilterTest : ModuleFilterTestCase<UniversalCopyScheduleModuleFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get
			{
				{ return FilterCategories.Other; }
			}
		}

		public override void TestIsExpensiveQuery()
		{
			Assert(!Filter.IsExpensiveQuery);
		}

		protected override UniversalCopyScheduleModuleFilter GetNewModuleFilter()
		{
			return new UniversalCopyScheduleModuleFilter("Module");
		}
		public void TestModulePairList()
		{
			SetUpData();
			var filter = new UniversalCopyScheduleModuleFilter("Module");
			var list = filter.ModulesPair_List;
			AssertEquals("Operate > Forwarding > Shipments", list.GetDescriptionFromCode(ModuleIDs.JobShipment.Name));
			AssertEquals("Manage > Workflow && Process > Task List", list.GetDescriptionFromCode(ModuleIDs.ProcessTasks.Name));
			AssertNull("List should not contain report modules", list.GetDescriptionFromCode(ModuleIDs.ForwardingReport.Name));
		}

		protected override ZString ExpectedDescription
		{
			get { return "Module"; }
		}

		public void TestTHing()
		{
			var codeDescriptionPair = UniversalCopyScheduleModuleFilter.GetAllModulesPairsList()["GlbStaff"];
			var moduleId = ModuleIDs.AllExcludingClientModules.FirstOrDefault(id => id.Name == codeDescriptionPair.Code);

			using (var module = ZModuleFactory.Instance.Create(moduleId))
			{
				var bizoType = (module as ZFilterModule)?.TypeOfTopLevelBusinessObject;
				var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(bizoType);
				AssertEquals("GS_Code", codeProperty);
			}
		}

		public void TestGetQuery()
		{
			SetUpData();
			Filter.ModulesPairList = "Manage > Workflow && Process > Task List";
			if (!Filter.CopyObjectCode_ReadOnly)
			{
				AssertEquals(1, Factory.GetDatabaseCount(typeof(StmUniversalCopy), Filter.Query));
			}

			Filter.ModulesPairList = "Operate > Forwarding > Shipments";
			if (!Filter.CopyObjectCode_ReadOnly)
			{
				Filter.CopyObjectCode = "S00001000";
				AssertEquals(1, Factory.GetDatabaseCount(typeof(StmUniversalCopy), Filter.Query));
			}
		}

		void SetUpData()
		{
			var copyTemplate1 = Factory.New<UniversalCopyTemplate>();
			copyTemplate1.S9_ModuleID = ModuleIDs.JobShipment.Name + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			var copy1 = Factory.New<StmUniversalCopy>();
			copy1.SUC_CopyObjectTableCode = JobShipmentSchema.Constants.Prefix;
			copy1.SUC_S9_CopyTemplate = copyTemplate1.PK;

			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipment1["JS_UniqueConsignRef"] = "S00001000";
			copy1.SUC_CopyObjectId = shipment1.PK;

			var copyTemplate2 = Factory.New<UniversalCopyTemplate>();
			copyTemplate2.S9_ModuleID = ModuleIDs.ProcessTasks.Name + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			var copy2 = Factory.New<StmUniversalCopy>();
			copy2.SUC_CopyObjectTableCode = ProcessTasksSchema.Constants.Prefix;
			copy2.SUC_S9_CopyTemplate = copyTemplate2.PK;

			var copyTemplate3 = Factory.New<UniversalCopyTemplate>();
			copyTemplate3.S9_ModuleID = ModuleIDs.AccBankAccount.Name + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			var copy3 = Factory.New<StmUniversalCopy>();
			copy3.SUC_CopyObjectTableCode = AccBankAccountSchema.Constants.Prefix;
			copy3.SUC_S9_CopyTemplate = copyTemplate3.PK;

			Factory.Save();
		}

		#region TestQueryIsEmptyByDefault
		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("ModuleTextRangeFilter.Query is empty by default", true, Filter.IsEmpty);
		}

		#endregion
	}
}
