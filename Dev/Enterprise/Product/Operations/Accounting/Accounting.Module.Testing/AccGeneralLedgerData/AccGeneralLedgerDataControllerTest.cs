using System;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccGeneralLedgerDataControllerForTest))]
	internal class AccGeneralLedgerDataControllerTest : ZSingletonControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(AccGeneralLedgerData);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccGeneralLedgerData;
		}

		public void TestCheckPoint()
		{
			var accGeneralLedgerDataController_ForTest = new AccGeneralLedgerDataControllerForTest();

			AssertEquals(Env.Security.None, accGeneralLedgerDataController_ForTest.CheckPointForView_ForTest);
			AssertEquals(Env.Security.None, accGeneralLedgerDataController_ForTest.CheckPointForNew_ForTest);
			AssertEquals(Env.Security.None, accGeneralLedgerDataController_ForTest.CheckPointForEdit_ForTest);
			AssertEquals(Env.Security.None, accGeneralLedgerDataController_ForTest.CheckPointForDelete_ForTest);
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
			AssertEquals("General Ledger Data don't have form", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert("General Ledger Data can't save", true);
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.AccGeneralLedgerData, TestController.ModuleID);
		}

		AccGeneralLedgerDataController fTestController;
		AccGeneralLedgerDataController TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new AccGeneralLedgerDataController();
				}
				return fTestController;
			}
		}

		class AccGeneralLedgerDataControllerForTest : AccGeneralLedgerDataController
		{
			public SecurityCheckpoint CheckPointForView_ForTest => CheckPointForView;

			public SecurityCheckpoint CheckPointForNew_ForTest => CheckPointForNew;

			public SecurityCheckpoint CheckPointForEdit_ForTest => CheckPointForEdit;

			public SecurityCheckpoint CheckPointForDelete_ForTest => CheckPointForDelete;
		}
	}
}
