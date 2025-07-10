using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(LPCOController))]
	class LPCOControllerTest : ZControllerBasherTest
	{
		public void TestGetForm_ReturnType()
		{
			var permit = Factory.NewWithValidTestData<CusLPCOHeader>();
			Factory.Save();
			using (var form = Controller.ShowEditForm(permit))
			{
				AssertType<LPCOForm>(form);
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new LPCOController();
			AssertEquals(Env.Security.BRLPCONew, controller.CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.BRLPCOView, controller.CheckPointForViewExposedForTest);
			AssertEquals(Env.Security.BRLPCOEdit, controller.CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.BRLPCODelete, controller.CheckPointForDeleteExposedForTest);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.BR.LPCO;
		}

		public override Type ControllerToBashType => typeof(LPCOController);

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var permit = Factory.NewWithValidTestData<CusLPCOHeader>();
			permit.CPH_SubType = "1";
			permit.CPH_Type = "LPC";
			permit.CPH_UnitOfMeasure = "KG";
			permit.CPH_StartDate = ZDate.Today;
			return permit;
		}
	}
}
