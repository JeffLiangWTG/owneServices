using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementModule))]
	class NctsMovementModuleTests : ZModuleBasherTest
	{
		public void TestGetBusinessObjectFromCountryCode()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.SetCountry("GB");
			var otherBranch = Factory.New<GlbBranch>();
			otherBranch.GB_RL_NKHomePort = "GBDVR";
			otherBranch.GB_GC = otherCompany.PK;
			otherBranch.GB_Code = "OTH";
			var nctsMovement = Factory.New<NctsHeader>();
			nctsMovement.SetMovementType(NctsMovementType.Codes.Departure);
			nctsMovement.BH_JobReference = "LRN";
			nctsMovement.BH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			using (var module = new NctsMovementModule())
			{
				var collection = module.GridCollection;
				((BusinessObjectCollection)collection).Load();
				AssertEquals(1, collection.Count);
			}
			nctsMovement.BH_GB = otherBranch.PK;
			Factory.Save();

			using (var module = new NctsMovementModule())
			{
				var collection = module.GridCollection;
				((BusinessObjectCollection)collection).Load();
				AssertEquals(0, collection.Count);
			}
		}

		public void TestOperationalActionsPlugin()
		{
			using (var module = new NctsMovementModule())
			{
				AssertNotNull("OperationalActions Plugin", module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestIOperationalActionSupportableMembers()
		{
			using (var module = new NctsMovementModule())
			{
				var operationalActionSupportable = module as IOperationalActionSupportable;
				AssertNotNull("NctsMovementModule as IOperationalActionSupportable", operationalActionSupportable);

				var operationalActionSupporter = operationalActionSupportable.OperationalActionSupporter;
				AssertType<NctsMovementOperationalActionSupporter>("OperationalActionSupporter Type", operationalActionSupporter);
				AssertSame("OperationalActionSupporter Cached", operationalActionSupporter, operationalActionSupportable.OperationalActionSupporter);
			}
		}

		public override void TestExceptionsFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestMilestonesFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestAutoAddedTaskStatusFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTasksFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		public override void TestTriggersFilter()
		{
			Assert("Not available on NctsMovement module", true);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.NctsMovementModule;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			collection.Add(header);
		}

		protected override bool HasController() => true;
	}
}
