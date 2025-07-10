using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.Statement.Testing
{
	[TestedType(typeof(StatementController))]
	class StatementControllerTest : Customs.Module.Testing.StatementControllerTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.EU.FR.CustomsStatement, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(CusStatementHeader), controller.TypeOfTopLevelBusinessObject);
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			AssertEquals(true, controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		public void TestCheckPointForView()
		{
			AssertEquals(Env.Security.FRCustomsStatement, controller.CheckPointForViewExposedForTest);
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.FRCustomsStatement, controller.CheckPointForNewExposedForTest);
		}

		public void TestCheckPointForEdit()
		{
			AssertEquals(Env.Security.FRCustomsStatement, controller.CheckPointForEditExposedForTest);
		}

		public void TestCheckPointForDelete()
		{
			AssertEquals("CusStatementHeader is not allowed to be deleted (see trigger trgCusStatementHeader_Del), hence no checkpoint for deletion.", Env.Security.None, controller.CheckPointForDeleteExposedForTest);
		}

		public override void TestDeleteForm()
		{
			Assert("CusStatementHeader is not allowed to be deleted (see trigger trgCusStatementHeader_Del).", true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			Factory.Save();
			return statement;
		}

		protected override void SetUp()
		{
			base.SetUp();
			controller = new StatementController();
		}

		StatementController controller;
	}
}
