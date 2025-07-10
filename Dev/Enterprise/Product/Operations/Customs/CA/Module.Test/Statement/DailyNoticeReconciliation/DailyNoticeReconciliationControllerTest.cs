using System;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(DailyNoticeReconciliationController))]
	sealed class DailyNoticeReconciliationControllerTest : StatementControllerTest
	{
		public override void TestCorrectForm()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = Controller.ShowFormForNewEntity(statement))
			{
				AssertEquals("Correct Form Type", typeof(StatementForm), form.GetType());
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CADailyNoticeReconciliation;

		public override Type ControllerToBashType => typeof(DailyNoticeReconciliationController);
	}
}
