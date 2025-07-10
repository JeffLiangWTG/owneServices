using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ClaimsAndQueriesListingAPTest : ScriptTest
	{
		public void TestClaimsAndQueriesListingWithMoreThanOneStmNote()
		{
			APInvoice apHeader = Factory.NewWithValidTestData<APInvoice>();
			APAccQueryClaim aPQueryClaim = Factory.NewWithValidTestData<APAccQueryClaim>();
			aPQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aPQueryClaim.AY_AH = apHeader.PK;
			aPQueryClaim.AY_ShortDescriptionOfClaim = "AP Claim";
			Factory.Save();

			ZDBOnlyQuery stmNoteQuery = new ZDBOnlyQuery(typeof(StmNote));
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_ParentID, aPQueryClaim.PK);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_Table, aPQueryClaim.TableName);

			DataTable resultForSettlementGroup = RunScript();
			StmNote[] stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("No StmNote on APQueryClaim", 0, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			StmNote stmNote1 = CreateStmNote(aPQueryClaim, "", true);
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("One StmNote on APQueryClaim", 1, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			StmNote stmNote2 = CreateStmNote(aPQueryClaim, "", true);
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("Two StmNotes on APQueryClaim", 2, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			StmNote stmNote3 = CreateStmNote(aPQueryClaim, "DOC", true);
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("Three StmNotes on APQueryClaim", 3, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			AccQueryClaimLogAdder logAdder = new AccQueryClaimLogAdder(aPQueryClaim);
			logAdder.LogComment = "APQueryCliam Log Coment - This is not a custom description";
			logAdder.AddLogToParent();
			Factory.Save();
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("Four StmNotes on APQueryClaim", 4, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);
		}

		public void TestClaimsAndQueriesListingContainsAPOnly()
		{
			GlbBranch otherBranch = Factory.NewWithValidTestData<GlbBranch>();

			ARAccQueryClaim queryClaimWithNoInvoice = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimWithNoInvoice.AY_GB = GlbBranch.CurrentBranch.PK;
			queryClaimWithNoInvoice.AY_ShortDescriptionOfClaim = "AR Claim";

			ARInvoice arHeader = Factory.NewWithValidTestData<ARInvoice>();
			arHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			ARAccQueryClaim aRQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			aRQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aRQueryClaim.AY_AH = arHeader.PK;
			aRQueryClaim.AY_ShortDescriptionOfClaim = "AR Claim";

			APInvoice apHeader = Factory.NewWithValidTestData<APInvoice>();
			apHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			APAccQueryClaim aPQueryClaim = Factory.NewWithValidTestData<APAccQueryClaim>();
			aPQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aPQueryClaim.AY_AH = apHeader.PK;
			aPQueryClaim.AY_ShortDescriptionOfClaim = "AP Claim";

			AccQueryClaimLogAdder logAdder = new AccQueryClaimLogAdder(aPQueryClaim);
			logAdder.LogComment = "APQueryCliam Log Coment";
			logAdder.AddLogToParent();

			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Select("ShortDescription = 'AP Claim'").Length);
			AssertNotEquals("Record should contain Details column", null, resultForSettlementGroup.Rows[0]["Details"]);
			AssertEquals("Result should contain 0 record for AR Claim", 0, resultForSettlementGroup.Select("ShortDescription = 'AR Claim'").Length);
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_ClaimsAndQueriesAP(
'{0}'	
)  
",
			GlbCompany.CurrentCompany.PK
			));
		}

		StmNote CreateStmNote(APAccQueryClaim master, string noteType, bool isCustomDescription)
		{
			var stmNote = Factory.NewWithValidTestData<StmNote>();
			stmNote.Master = master;
			stmNote.ST_ParentID = master.PK;
			stmNote.ST_Table = AccQueryClaim.Schema.TableName;
			stmNote.ST_NoteType = noteType;
			stmNote.ST_IsCustomDescription = isCustomDescription;
			Factory.Save();

			return stmNote;
		}
	}
}

