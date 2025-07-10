using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Integration.Testing
{
	public class CalculationLogsLoaderTest : TestCaseWithFactory
	{
		public void TestNoteDescription()
		{
			AssertEquals("Expected constant note description", "AutoRating Calculation Log Costing", CalculationLogsLoader.NoteDescriptionCosting);
			AssertEquals("Expected constant note description", "AutoRating Calculation Log Revenue", CalculationLogsLoader.NoteDescriptionRevenue);
		}

		public void TestLoad()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			AssertNull("No calculation logs note", CalculationLogsLoader.Load(bizo));

			StmNote note = bizo.GetNotes().Factory.New<HiddenStmNote>();
			note.ST_ParentID = bizo.PK;
			note.ST_Description = CalculationLogsLoader.NoteDescriptionCosting;
			note.ST_NoteData = ZBlob.FromUTF8("random text");
			AssertNull("Can't deserialize from ST_NoteData", CalculationLogsLoader.Load(bizo));

			CalculationLogsWrapper logsWrapper = new CalculationLogsWrapper();
			note.ST_NoteData = ZBlob.FromUTF8(logsWrapper.Serialize());

			CalculationLogsWrapper logsWrapperLoaded = CalculationLogsLoader.Load(bizo);
			AssertNotNull("Calculation logs deserialized from ST_NoteData", logsWrapperLoaded);
			AssertEquals(note.ST_NoteData.ToUTF8(), logsWrapperLoaded.Serialize());
		}

		public void TestLoad_Auto()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			AssertNull("No calculation log notes", CalculationLogsLoader.Load(bizo));

			CreateNote(bizo, true);
			AssertEquals("Loading costing note", "COSTING", CalculationLogsLoader.Load(bizo).Logs[0].CalculatorCode);

			CreateNote(bizo, false);
			AssertEquals("Loading revenue note", "REVENUE", CalculationLogsLoader.Load(bizo).Logs[0].CalculatorCode);
		}

		public void TestLoad_Costing()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			AssertNull("No calculation log notes", CalculationLogsLoader.Load(bizo));

			CreateNote(bizo, false);
			AssertNull("No costing note", CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Costing));

			CreateNote(bizo, true);
			AssertEquals("Loading costing note", "COSTING", CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Costing).Logs[0].CalculatorCode);
		}

		public void TestLoad_Revenue()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			AssertNull("No calculation log notes", CalculationLogsLoader.Load(bizo));

			CreateNote(bizo, true);
			AssertNull("No revenue note", CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Revenue));

			CreateNote(bizo, false);
			AssertEquals("Loading revenue note", "REVENUE", CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Revenue).Logs[0].CalculatorCode);
		}

		public void TestSave()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();

			CalculationLogsLoader.Save(bizo, null);
			AssertEquals("No new notes created", 0, bizo.GetNotes().GetAllNotes().Count);

			CalculationLog calculationLog = new CalculationLog();
			calculationLog.CalculatorCode = "AAA";
			calculationLog.IsCosting = true;

			CalculationLogsWrapper logsWrapper = new CalculationLogsWrapper();
			logsWrapper.Logs.Add(calculationLog);

			CalculationLogsLoader.Save(bizo, logsWrapper);

			ZQuery notesQuery = new ZQuery(StmNoteSchema.ST_ParentID, bizo.PK);
			HiddenStmNote[] notes = bizo.GetNotes().Factory.Load<HiddenStmNote>(notesQuery);
			AssertEquals("New note created", 1, notes.Length);

			HiddenStmNote note = notes[0];
			AssertEquals(CalculationLogsLoader.NoteDescriptionCosting, note.ST_Description);
			AssertEquals("NoteType should be DOC for HiddenStmNote", nameof(StmNoteVisibility.DOC), note.ST_NoteType);
			AssertEquals("IMPORTANT: ST_IsCustomDescription should be true; overwise this note will cause problems in DocEngine", true, note.ST_IsCustomDescription);
			AssertEquals(logsWrapper.Serialize(), note.ST_NoteData.ToUTF8());

			calculationLog.CalculatorCode = "COSTING";
			CalculationLogsLoader.Save(bizo, logsWrapper);

			notes = bizo.GetNotes().Factory.Load<HiddenStmNote>(notesQuery);
			AssertEquals("New note not created, existing one was overriden", 1, notes.Length);

			note = notes[0];
			AssertEquals(CalculationLogsLoader.NoteDescriptionCosting, note.ST_Description);
			AssertEquals(logsWrapper.Serialize(), note.ST_NoteData.ToUTF8());

			calculationLog.CalculatorCode = "REVENUE";
			calculationLog.IsCosting = false;
			CalculationLogsLoader.Save(bizo, logsWrapper);

			notes = bizo.GetNotes().Factory.Load<HiddenStmNote>(notesQuery);
			AssertEquals("New revenue note created", 2, notes.Length);

			HiddenStmNote costingNote = notes.First(x => x.ST_Description == CalculationLogsLoader.NoteDescriptionCosting);
			AssertEquals("Costing note", "COSTING", CalculationLogsWrapper.Deserialize(costingNote.ST_NoteData.ToUTF8()).Logs[0].CalculatorCode);

			HiddenStmNote revenueNote = notes.First(x => x.ST_Description == CalculationLogsLoader.NoteDescriptionRevenue);
			AssertEquals("Revenue note", "REVENUE", CalculationLogsWrapper.Deserialize(revenueNote.ST_NoteData.ToUTF8()).Logs[0].CalculatorCode);
		}

		public void TestSave_MixedWrapper()
		{
			CalculationLog costingLog1 = CreateLog("COSTING1", true);
			CalculationLog costingLog2 = CreateLog("COSTING2", true);
			CalculationLog revenueLog1 = CreateLog("REVENUE1", false);
			CalculationLog revenueLog2 = CreateLog("REVENUE2", false);

			CalculationLogsWrapper logsWrapper = new CalculationLogsWrapper();
			logsWrapper.Logs.Add(costingLog1);
			logsWrapper.Logs.Add(costingLog2);
			logsWrapper.Logs.Add(revenueLog1);
			logsWrapper.Logs.Add(revenueLog2);

			AssertEquals("Precondition: wrapper with mixed logs, both costing and revenue", true, logsWrapper.Logs.Any(log => log.IsCosting) && logsWrapper.Logs.Any(log => !log.IsCosting));

			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			CalculationLogsLoader.Save(bizo, logsWrapper);

			HiddenStmNote[] notes = bizo.GetNotes().Factory.Load<HiddenStmNote>(new ZQuery(StmNoteSchema.ST_ParentID, bizo.PK));

			HiddenStmNote costingNote = notes.First(x => x.ST_Description == CalculationLogsLoader.NoteDescriptionCosting);
			CalculationLogsWrapper costingWrapper = CalculationLogsWrapper.Deserialize(costingNote.ST_NoteData.ToUTF8());
			AssertContainsExactElementsInAnyOrder("New costing-only wrapper created", new ZString[] { "COSTING1", "COSTING2" }, costingWrapper.Logs.Select(log => log.CalculatorCode));

			HiddenStmNote revenueNote = notes.First(x => x.ST_Description == CalculationLogsLoader.NoteDescriptionRevenue);
			CalculationLogsWrapper revenueWrapper = CalculationLogsWrapper.Deserialize(revenueNote.ST_NoteData.ToUTF8());
			AssertContainsExactElementsInAnyOrder("New revenue-only wrapper created", new ZString[] { "REVENUE1", "REVENUE2" }, revenueWrapper.Logs.Select(log => log.CalculatorCode));
		}

		public void TestDisable()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			AssertNull("Precondition: no logs", CalculationLogsLoader.Load(bizo));

			CalculationLogsLoader.Disable(bizo);
			AssertNull("Nothing happened: still no logs", CalculationLogsLoader.Load(bizo));

			CalculationLogsLoader.Save(bizo, new CalculationLogsWrapper());
			AssertEquals("Precondition: logs wrapper exist and enabled", false, CalculationLogsLoader.Load(bizo).IsDisabled);

			CalculationLogsLoader.Disable(bizo);
			AssertEquals("Logs wrapper disabled", true, CalculationLogsLoader.Load(bizo).IsDisabled);
		}

		public void TestDisable_Options()
		{
			AssertDisableOption(CalculationLogsLoader.DisableOption.Both, true, true);
			AssertDisableOption(CalculationLogsLoader.DisableOption.Costing, true, false);
			AssertDisableOption(CalculationLogsLoader.DisableOption.Revenue, false, true);
		}

		void AssertDisableOption(CalculationLogsLoader.DisableOption disableOption, bool disableCosting, bool disableRevenue)
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			CreateNote(bizo, true);
			CreateNote(bizo, false);

			AssertEquals("Precondition: costing logs enabled", false, CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Costing).IsDisabled);
			AssertEquals("Precondition: revenue logs enabled", false, CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Revenue).IsDisabled);

			CalculationLogsLoader.Disable(bizo, disableOption);
			AssertEquals("Costing logs", disableCosting, CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Costing).IsDisabled);
			AssertEquals("Revenue logs", disableRevenue, CalculationLogsLoader.Load(bizo, CalculationLogsLoader.LoadOption.Revenue).IsDisabled);
		}

		#region Implementation

		CalculationLog CreateLog(string calculatorCode, bool isCosting)
		{
			CalculationLog result = new CalculationLog();
			result.CalculatorCode = calculatorCode;
			result.IsCosting = isCosting;

			return result;
		}

		void CreateNote(BusinessObject bizo, bool isCosting)
		{
			StmNote note = bizo.GetNotes().Factory.New<HiddenStmNote>();
			note.ST_ParentID = bizo.PK;
			note.ST_Description = isCosting ? CalculationLogsLoader.NoteDescriptionCosting : CalculationLogsLoader.NoteDescriptionRevenue;

			CalculationLog log = new CalculationLog();
			log.IsCosting = isCosting;
			log.CalculatorCode = isCosting ? "COSTING" : "REVENUE";

			CalculationLogsWrapper logsWrapper = new CalculationLogsWrapper();
			logsWrapper.Logs.Add(log);
			note.ST_NoteData = ZBlob.FromUTF8(logsWrapper.Serialize());
		}

		#endregion
	}
}
