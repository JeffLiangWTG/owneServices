using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class LogbookHelperTest : TestCaseWithFactory
	{
		public void TestSetLogbookEORIBranchSuffix_Null()
		{
			LogbookHelper.SetLogbookEORIBranchSuffix(null, "0000");
			AssertNull(LogbookEORIBranchGenAddOnColumn);
		}

		public void TestSetLogbookEORIBranchSuffix_Empty()
		{
			message.SetLogbookEORIBranchSuffix(ZString.Empty);
			AssertNull(LogbookEORIBranchGenAddOnColumn);
		}

		public void TestSetLogbookEORIBranchSuffix()
		{
			message.SetLogbookEORIBranchSuffix("0001");
			CombineAssertions(() =>
			{
				AssertNotNull("GenAddOn exists", LogbookEORIBranchGenAddOnColumn);
				AssertEquals("LogbookEORIBranchSuffix", message.GetLogbookEORIBranchSuffix(), "0001");
			});
		}

		public void TestSetLogbookRegistrationNumberValue_Empty()
		{
			message.SetLogbookRegistrationNumber(ZString.Empty);
			AssertNull(LogbookRegistrationNumberNote);
		}

		public void TestSetLogbookRegistrationNumberValue_Value()
		{
			message.SetLogbookRegistrationNumber("SINGLE");
			AssertEquals("SINGLE", message.GetLogbookRegistrationNumber());
		}

		public void TestSetLogbookRegistrationNumberValues_Null()
		{
			LogbookHelper.SetLogbookRegistrationNumber(null, Array.Empty<ZString>());
			AssertNull(LogbookRegistrationNumberNote);
		}

		public void TestSetLogbookRegistrationNumberValues_EmptyValues()
		{
			CombineAssertions(() =>
			{
				message.SetLogbookRegistrationNumber(new ZString[] { ZString.Empty, ZString.Empty });
				AssertNull("No Note", LogbookRegistrationNumberNote);

				message.SetLogbookRegistrationNumber(new ZString[] { "ENTERED", ZString.Empty, "ENTERED2" });
				AssertEquals("Note", "ENTERED, ENTERED2", message.GetLogbookRegistrationNumber());
			});
		}

		public void TestSetLogbookRegistrationNumberValues_DuplicateValues()
		{
			message.SetLogbookRegistrationNumber(new ZString[] { "TEST1", "TEST1", "TEST2" });
			AssertEquals("TEST1, TEST2", message.GetLogbookRegistrationNumber());
		}

		public void TestSetLogbookRegistrationNumber_StmNote()
		{
			message.SetLogbookRegistrationNumber("TEST");
			AssertStmNote(LogbookRegistrationNumberNote);
		}

		public void TestSetLogbookLocalReferenceNumber_Null()
		{
			LogbookHelper.SetLogbookLocalReferenceNumber(null, "TEST");
			AssertNull(LogbookLocalReferenceNumberNote);
		}

		public void TestSetLogbookLocalReferenceNumber_Empty()
		{
			message.SetLogbookLocalReferenceNumber(ZString.Empty);
			AssertNull(LogbookLocalReferenceNumberNote);
		}

		public void TestSetLogbookLocalReferenceNumber()
		{
			message.SetLogbookLocalReferenceNumber("TEST");
			AssertEquals("TEST", message.GetLogbookLocalReferenceNumber());
		}

		public void TestSetLogbookLocalReferenceNumber_StmNote()
		{
			message.SetLogbookLocalReferenceNumber("TEST");
			AssertStmNote(LogbookLocalReferenceNumberNote);
		}

		public void TestOverwriteNote()
		{
			CombineAssertions(() =>
			{
				message.SetLogbookLocalReferenceNumber("TEST");
				AssertEquals("Initial", "TEST", message.GetLogbookLocalReferenceNumber());
				message.SetLogbookLocalReferenceNumber("OVERWRITTEN");
				AssertEquals("Overwritten", "OVERWRITTEN", message.GetLogbookLocalReferenceNumber());
			});
		}

		void AssertStmNote(StmNote stmNote)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Type", nameof(StmNoteVisibility.INT), stmNote.ST_NoteType);
				AssertEquals("Text", "TEST", stmNote.ST_NoteText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<EDIMessage>();
		}
		EDIMessage message;

		StmNote LogbookRegistrationNumberNote => message.Notes.FindByDescription(LogbookHelper.LogbookRegistrationNumberNoteDescription).SingleOrDefault();

		StmNote LogbookLocalReferenceNumberNote => message.Notes.FindByDescription(LogbookHelper.LogbookLocalReferenceNumberNoteDescription).SingleOrDefault();

		GenAddOnColumn LogbookEORIBranchGenAddOnColumn
		{
			get
			{
				var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, message.PK);
				query.AddToFilter(GenAddOnColumnSchema.XA_Name, "LogbookEORIBranchSuffix");
				return Factory.Load<GenAddOnColumn>(query).SingleOrDefault();
			}
		}
	}
}
