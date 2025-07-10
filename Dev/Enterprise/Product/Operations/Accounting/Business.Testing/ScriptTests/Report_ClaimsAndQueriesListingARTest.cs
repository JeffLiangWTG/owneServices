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
	class Report_ClaimsAndQueriesListingARTest : ScriptTest
	{
		public void TestClaimsAndQueriesListingWithMoreThanOneStmNote()
		{
			ARInvoice arHeader = Factory.NewWithValidTestData<ARInvoice>();
			ARAccQueryClaim aRQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			aRQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aRQueryClaim.AY_AH = arHeader.PK;
			aRQueryClaim.AY_ShortDescriptionOfClaim = "AR Claim";
			Factory.Save();

			ZDBOnlyQuery stmNoteQuery = new ZDBOnlyQuery(typeof(StmNote));
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_ParentID, aRQueryClaim.PK);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_Table, aRQueryClaim.TableName);

			DataTable resultForSettlementGroup = RunScript();
			StmNote[] stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("No StmNote on ARQueryClaim", 0, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			StmNote stmNote1 = CreateStmNote(aRQueryClaim, "", true);
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("One StmNote on ARQueryClaim", 1, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			StmNote stmNote2 = CreateStmNote(aRQueryClaim, "", true);
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("Two StmNotes on ARQueryClaim", 2, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			StmNote stmNote3 = CreateStmNote(aRQueryClaim, "DOC", true);
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("Three StmNotes on ARQueryClaim", 3, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);

			AccQueryClaimLogAdder logAdder = new AccQueryClaimLogAdder(aRQueryClaim);
			logAdder.LogComment = "ARQueryClaim Log Coment - This is not a custom description";
			logAdder.AddLogToParent();
			Factory.Save();
			resultForSettlementGroup = RunScript();
			stmNotes = Factory.Load(typeof(StmNote), stmNoteQuery) as StmNote[];
			AssertEquals("Four StmNotes on ARQueryClaim", 4, stmNotes.Length);
			AssertEquals("Result should contain 1 record", 1, resultForSettlementGroup.Rows.Count);
		}

		public void TestClaimsAndQueriesListingContainsAROnly()
		{
			GlbBranch otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;

			var contact = TestObjectCreator.CreateContact(TestObjectCreator.AALSHI);

			ARAccQueryClaim queryClaimWithNoInvoice = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimWithNoInvoice.AY_GB = GlbBranch.CurrentBranch.PK;
			queryClaimWithNoInvoice.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			queryClaimWithNoInvoice.AY_OC = contact.PK;
			queryClaimWithNoInvoice.AY_ShortDescriptionOfClaim = "AR Claim";

			queryClaimWithNoInvoice.AY_GB = GlbBranch.CurrentBranch.PK;

			AccQueryClaimLogAdder logAdder = new AccQueryClaimLogAdder(queryClaimWithNoInvoice);
			logAdder.LogComment = "QueryClaimWithNoInvoice Log Coment";
			logAdder.AddLogToParent();

			ARInvoice arHeader = Factory.NewWithValidTestData<ARInvoice>();
			arHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			ARAccQueryClaim aRQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			aRQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aRQueryClaim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			aRQueryClaim.AY_OC = contact.PK;
			aRQueryClaim.AY_AH = arHeader.PK;
			aRQueryClaim.AY_ShortDescriptionOfClaim = "AR Claim";

			logAdder = new AccQueryClaimLogAdder(aRQueryClaim);
			logAdder.LogComment = "ARQueryCliam Log Coment";
			logAdder.AddLogToParent();

			APInvoice apHeader = Factory.NewWithValidTestData<APInvoice>();
			apHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			APAccQueryClaim aPQueryClaim = Factory.NewWithValidTestData<APAccQueryClaim>();
			aPQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aPQueryClaim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			aPQueryClaim.AY_OC = contact.PK;
			aPQueryClaim.AY_AH = apHeader.PK;
			aPQueryClaim.AY_ShortDescriptionOfClaim = "AP Claim";

			APInvoice intercompanyAPHeader = Factory.NewWithValidTestData<APInvoice>();
			intercompanyAPHeader.AH_GB = otherBranch.PK;
			ARAccQueryClaim intercompanyAPQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			intercompanyAPQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			intercompanyAPQueryClaim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			intercompanyAPQueryClaim.AY_OC = contact.PK;
			intercompanyAPQueryClaim.AY_AH = intercompanyAPHeader.PK;
			intercompanyAPQueryClaim.AY_ShortDescriptionOfClaim = "AR Claim";

			logAdder = new AccQueryClaimLogAdder(intercompanyAPQueryClaim);
			logAdder.LogComment = "intercompanyAPQueryClaim Log Coment";
			logAdder.AddLogToParent();

			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result should contain 3 record", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result should contain 3 record", 3, resultForSettlementGroup.Select("ShortDescription = 'AR Claim'").Length);
			AssertNotEquals("Record should contain Details column", null, resultForSettlementGroup.Rows[0]["Details"]);
			AssertNotEquals("Record should contain Details column", null, resultForSettlementGroup.Rows[1]["Details"]);
			AssertNotEquals("Record should contain Details column", null, resultForSettlementGroup.Rows[2]["Details"]);

			AssertEquals("Debtor should be the debtor of the claim", "AALSHI", resultForSettlementGroup.Rows[0]["DebtorCode"].ToString().Trim());
			AssertEquals("Debtor should be the debtor of the claim", "AALSHI", resultForSettlementGroup.Rows[1]["DebtorCode"].ToString().Trim());
			AssertEquals("Debtor should be sister company org proxy", "ABIGAS", resultForSettlementGroup.Rows[2]["DebtorCode"].ToString().Trim());

			AssertEquals("Result should contain 0 record for AP Claim", 0, resultForSettlementGroup.Select("ShortDescription = 'AP Claim'").Length);
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_ClaimsAndQueriesAR(
'{0}'	
)  
",
			GlbCompany.CurrentCompany.PK
			));
		}

		StmNote CreateStmNote(ARAccQueryClaim master, string noteType, bool isCustomDescription)
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

