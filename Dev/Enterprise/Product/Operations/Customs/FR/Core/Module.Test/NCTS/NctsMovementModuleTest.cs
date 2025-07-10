using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.NCTS.Testing
{
	[TestedType(typeof(NctsMovementModule))]
	class NctsMovementModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.NctsMovementModule;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			collection.Add(header);
		}
		public void TestActionMenuItemsContainsNCTSInboundInterchangeImporter()
		{
			using (var module = new NctsMovementModule())
			{
				_ = module.FormActionMenu;
				var actionMenuItems = module.ActionsMenuItem.MenuItems;
				var addNCTSResponseInterchangeActionMenuItem = actionMenuItems.FindByText(TP5InboundInterchangeImporter.TP5ResponseInterchangeAddActionText);
				AssertNotNull("The menu item of NCTS response interchange importer should exist in the actions menu.", addNCTSResponseInterchangeActionMenuItem);
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

		protected override bool HasController() => true;

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override string CountryCode => Core.Constants.CountryCodes.France;
	}
}
