using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentLogs))]
	class IncidentLogsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddingAndRemovingLogs()
		{
			AssertEquals("ElementsNotInDBForTest.Count", 0, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 0, Logs.AddedLogs.Count);

			StmALog logA = Logs.AddLog(Events.AddedARecordToTheSystem, "A");
			StmALog logB = Logs.AddLog(Events.EditedARecord, "B");

			AssertEquals("ElementsNotInDBForTest.Count", 2, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 2, Logs.AddedLogs.Count);

			AssertEquals("AddedLogs[0]", logA, Logs.AddedLogs[0]);
			AssertEquals("AddedLogs[1]", logB, Logs.AddedLogs[1]);

			Logs.RemoveAddedLogs();
			AssertEquals("ElementsNotInDBForTest.Count", 0, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 0, Logs.AddedLogs.Count);

			Logs.AddLog(Events.AddedARecordToTheSystem, "A");
			Logs.AddLog(Events.EditedARecord, "B");

			Logs.StartAddingLogs();
			AssertEquals("ElementsNotInDBForTest.Count", 2, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 0, Logs.AddedLogs.Count);
		}

		public void TestRemoveAddedLogs()
		{
			AssertEquals("ElementsNotInDBForTest.Count", 0, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 0, Logs.AddedLogs.Count);

			var logA = Logs.AddLog(Events.AddedARecordToTheSystem, "A");
			var logB = Logs.AddLog(Events.EditedARecord, "B");
			var logC = Logs.AddLog(Events.EditedARecord, "C");

			AssertEquals("ElementsNotInDBForTest.Count", 3, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 3, Logs.AddedLogs.Count);

			Logs.RemoveAddedLogs(log => log.SL_Reference == "B");
			AssertEquals("ElementsNotInDBForTest.Count", 2, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 2, Logs.AddedLogs.Count);
			AssertEquals("A", Logs.AddedLogs[0].SL_Reference);
			AssertEquals("C", Logs.AddedLogs[1].SL_Reference);

			Logs.RemoveAddedLogs();
			AssertEquals("ElementsNotInDBForTest.Count", 0, Logs.ElementsNotInDBForTest.Count);
			AssertEquals("AddedLogs.Count", 0, Logs.AddedLogs.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new IncidentLogs(Factory.New<ProfessionalServicesQuote>());
		}

		IncidentLogs Logs
		{
			get
			{
				if (fLogs == null)
				{
					fLogs = (IncidentLogs)GetNewBusinessObject();
				}
				return fLogs;
			}
		}

		IncidentLogs fLogs;

		#endregion
	}
}
