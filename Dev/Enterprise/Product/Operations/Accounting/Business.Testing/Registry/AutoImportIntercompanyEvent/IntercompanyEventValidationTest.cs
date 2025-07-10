using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class IntercompanyEventValidationTest : TestCaseWithFactory
	{
		public void TestValidateStmEventCode()
		{
			var config = new IntercompanyEventConfiguration();

			var settings1 = config.IntercompanyEventSettingCollection.AddNew();

			settings1.Validation.ValidateStmEventCode();
			AssertHasError("Must have code", settings1.StmEventCodeInfo, "Please enter an Event Code.");

			settings1.StmEventCode = "ZXCV";
			settings1.Validation.ValidateStmEventCode();
			AssertHasError("Events must be valid", settings1.StmEventCodeInfo, "Enter a valid Event.");

			settings1.StmEventCode = Events.AddedARecordToTheSystem.Code;
			settings1.Validation.ValidateStmEventCode();
			AssertNoErrors("Correct code", settings1.StmEventCodeInfo);

			var settings2 = config.IntercompanyEventSettingCollection.AddNew();
			settings2.StmEventCode = Events.AddedARecordToTheSystem.Code;

			settings1.Validation.ValidateStmEventCode();
			AssertHasError("No duplicates allowed", settings1.StmEventCodeInfo, "Same Event not allowed more than once. Please select another Event.");
			settings2.Validation.ValidateStmEventCode();
			AssertHasError("No duplicates allowed", settings2.StmEventCodeInfo, "Same Event not allowed more than once. Please select another Event.");

			var settings3 = config.IntercompanyEventSettingCollection.AddNew();
			settings3.StmEventCode = Events.IncidentClosed.Code;
			settings3.Validation.ValidateStmEventCode();
			AssertNoError("This is not a duplicate", settings3.StmEventCodeInfo, "Same Event not allowed more than once. Please select another Event.");
		}

		public void TestValidateStartDate()
		{
			var config = new IntercompanyEventConfiguration();

			var settings1 = config.IntercompanyEventSettingCollection.AddNew();
			settings1.Validation.ValidateStartDate();
			AssertHasError("Compulsory", settings1.StartDateInfo, "Please enter a Start Date.");

			settings1.StartDate = ZDate.Today;
			settings1.Validation.ValidateStartDate();
			AssertNoErrors("valid date time", settings1.StartDateInfo);

			settings1.StartDate = ZDate.Invalid;
			settings1.Validation.ValidateStartDate();
			AssertHasError("not valid", settings1.StartDateInfo, "Please enter a valid date.");
		}
	}
}
