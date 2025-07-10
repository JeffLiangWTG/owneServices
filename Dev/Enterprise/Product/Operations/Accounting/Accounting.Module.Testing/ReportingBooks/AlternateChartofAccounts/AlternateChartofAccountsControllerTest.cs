using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateChartofAccountsController))]
	internal class AlternateChartofAccountsControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(AccAlternateChart);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AlternateChartofAccounts;
		}

		public void TestCheckPoint()
		{
			var accGeneralLedgerDataController_ForTest = new AlternateChartofAccountsControllerForTest();

			AssertEquals(Env.Security.AlternateChartofAccountsView, accGeneralLedgerDataController_ForTest.CheckPointForView_ForTest);
			AssertEquals(Env.Security.AlternateChartofAccountsNew, accGeneralLedgerDataController_ForTest.CheckPointForNew_ForTest);
			AssertEquals(Env.Security.AlternateChartofAccountsEdit, accGeneralLedgerDataController_ForTest.CheckPointForEdit_ForTest);
			AssertEquals(Env.Security.AlternateChartofAccountsDelete, accGeneralLedgerDataController_ForTest.CheckPointForDelete_ForTest);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AccAlternateChart), TestController.TypeOfTopLevelBusinessObject);
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.AlternateChartofAccounts, TestController.ModuleID);
		}

		AlternateChartofAccountsController fTestController;
		AlternateChartofAccountsController TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new AlternateChartofAccountsController();
				}
				return fTestController;
			}
		}

		class AlternateChartofAccountsControllerForTest : AlternateChartofAccountsController
		{
			public SecurityCheckpoint CheckPointForView_ForTest => CheckPointForView;

			public SecurityCheckpoint CheckPointForNew_ForTest => CheckPointForNew;

			public SecurityCheckpoint CheckPointForEdit_ForTest => CheckPointForEdit;

			public SecurityCheckpoint CheckPointForDelete_ForTest => CheckPointForDelete;
		}
	}
}
