using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CSARevenueSummaryFormController))]
	sealed class CSARevenueSummaryFormControllerTest : StatementControllerTest
	{
		public override void TestCorrectForm()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			using (var form = Controller.ShowFormForNewEntity(statement))
			{
				AssertEquals("Correct Form Type", typeof(CSARevenueSummaryFormForm), form.GetType());
			}
		}

		public void TestAdditionalFilter()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_IsMonthlyStatement = true;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_IsMonthlyStatement = false;

			Factory.Save();

			var collection = new CSARevenueSummaryFormModuleCollection(Factory);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), collection.Select(x => x.PK));

			statement1.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			statement2.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			Factory.Save();

			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { statement1.PK, statement2.PK }, collection.Select(x => x.PK));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CACSARevenueSummaryForm;

		public override Type ControllerToBashType => typeof(CSARevenueSummaryFormController);
	}
}
