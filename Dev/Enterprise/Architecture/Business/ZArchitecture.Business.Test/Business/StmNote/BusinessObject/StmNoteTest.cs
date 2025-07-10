using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmNote))]
	sealed class StmNoteTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestSetNoteTextContainingExecuteAsReaderFlag()
		{
			var dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();
			note.ST_Description = "Detailed Goods Description";
			AssertEquals("Precondition - Note.ST_IsTextOnly should be true", true, note.ST_IsTextOnly);

			note.ST_NoteDataAsText = CargoWise.Data.DbCommand.ExecuteAsReaderFlagComments;
			Factory.Save();

			note.ST_NoteDataAsText = CargoWise.Data.DbCommand.ExecuteAsReaderFlagComments + CargoWise.Data.DbCommand.ExecuteAsReaderFlagComments;
			Factory.Save();

			note.Delete();
			Factory.Save();
		}

		public void TestSystemPreDefinedNoteShouldNotHaveValidationError()
		{
			var dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description;
			note.ST_NoteDataAsText = "-";
			Factory.Save();
			Assert(note.ReadOnly);
			Assert("note description should not have validation error", !note.ST_DescriptionInfo.HasErrors());
		}

		public void TestIsParentTemplateRecord()
		{
			var templateRecordProvider = Factory.New<DummyBizoWithITemplateRecordProvider>();
			var stmNote = templateRecordProvider.Notes.AddNew();
			Assert(!stmNote.IsParentTemplateRecord);

			templateRecordProvider.IsTemplateRecord = true;
			Assert(stmNote.IsParentTemplateRecord);
		}

		public void TestStmNoteHasMaster_OverrideValidationMasters()
		{
			var dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();
			AssertCollectionContains(note.Master, note.OverrideValidationMasters);
		}

		public void TestStmNoteHasMaster_JobDeclarationRowDeleted()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.Delete();
			var stmNote = Factory.NewWithValidTestData<StmNote>();
			stmNote.Master = (IStmNoteParent)declaration;
			AssertNotNull(stmNote.Master);
			AssertEquals(false, stmNote.HasMaster);
		}

		public void TestDefaultNoteContextFromCurrentlyLoggedInDepartment()
		{
			var department = Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbDepartment>(), GlbDepartmentSchema.GE_Code, "CIA");
			Factory.Save();

			DataRegistry.Instance.SetDefaultNoteContextFromCurrentlyLoggedInDepartment(true);
			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, EnvProxy.Instance.CurrentBranch.PK, department.PK.ToGuid()))
			{
				var org = Factory.NewWithValidTestData(ObjectFactory.GetType<IOrgHeader>());
				var orgNote = org.GetNotes().AddNew();

				AssertEquals("New notes for an organisation should default the context when the registry item is set", "DII", orgNote.ST_NoteContext);

				var notAnOrg = Factory.NewWithValidTestData<DummyBizOWithRelatedNotes>();
				var notOrgNote = notAnOrg.GetNotes().AddNew();

				AssertEquals("New notes for any bizo except Org shouldn't be effected by the registry item", "AAA", notOrgNote.ST_NoteContext);
			}
		}

		#region TestIsInternalOrClientVisibleOrAgentVisible

		public void TestIsInternalOrClientVisibleOrAgentVisible()
		{
			StmNote note = Factory.New<StmNote>();

			note.ST_NoteType = "PUB";
			AssertEquals(true, note.IsInternalOrClientVisibleOrAgentVisible);

			note.ST_NoteType = "PRV";
			AssertEquals(false, note.IsInternalOrClientVisibleOrAgentVisible);

			note.ST_NoteType = "INT";
			AssertEquals(true, note.IsInternalOrClientVisibleOrAgentVisible);

			note.ST_NoteType = "MOO";
			AssertEquals(false, note.IsInternalOrClientVisibleOrAgentVisible);

			note.ST_NoteType = "AGV";
			AssertEquals(true, note.IsInternalOrClientVisibleOrAgentVisible);
		}

		#endregion

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestDefaultNoteCompanyForOrganisationNotes()
		{
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbCompany>());
			var branch = (IGlbBranch)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbBranch>());
			branch.GB_GC = company.PK;

			Factory.Save();

			DataRegistry.Instance.SetDefaultNoteCompanyFromCurrentlyLoggedInCompany(true);
			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				var org = Factory.NewWithValidTestData(ObjectFactory.GetType<IOrgHeader>());
				var orgNote = org.GetNotes().AddNew();

				AssertEquals("New notes for an organisation should default to the currently logged in company when the registry item is set", company.PK, orgNote.ST_GC_RelatedCompany);

				var notAnOrg = Factory.NewWithValidTestData<DummyBizOWithRelatedNotes>();
				var notOrgNote = notAnOrg.GetNotes().AddNew();

				AssertEquals("New notes for any bizo except Org shouldn't be effected by the registry item", Guid.Empty, notOrgNote.ST_GC_RelatedCompany);
			}
		}

		[ExpectNoExceptions]
		public void TestMasterEvaluatedToNull()
		{
			try
			{
				StmNote note = Factory.New<StmNote>();
				AssertNull(note.Master);
				var list = note.ST_Description_List;
				AssertEquals("Note must have parent object.", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestNotesTypeChanged()
		{
			var predefinedNoteType = PredefinedNoteTypes.Instance.InternalWorkNotes;
			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = CreateNewNote(dummyBizOWithAutoLogs, predefinedNoteType.Description, "Test", "AAA", ZBool.False, nameof(StmNoteVisibility.INT));
			Factory.Save();
			AssertEquals("The Note should be RTF", false, note.ST_IsTextOnly);
		}

		public void TestRichTextMasqueradingAsDocumentNoteWhenSavingIsDeleted()
		{
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);

			var note = Factory.New<StmNote>();
			using (note.GetValidationSuspender())
			{
				Factory.SuspendValidation();
				note.ST_Table = "GlbStaff";
				note.ST_Description = "";
				note.ST_NoteType = nameof(StmNoteVisibility.DOC);
				((INeedRow)note).Row[StmNoteSchema.ST_NoteData.Name] = (byte[])ZBlob.FromAscii(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\lang2057\f0\fs20 <NewDataSet />\par }"); // Mock dirt record
				Factory.Save();
				AssertEquals("Deleted", true, note.IsDeleted);
				AssertEquals("Not saved to DB", null, new BusinessObjectFactory().Load<StmNote>(note.PK));
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertContains("Rich Text Note Masquerading as Document Note", ExceptionReporterTestListener.Instance[0].Message);
				ErrorReporter.Clear();
			}
		}

		public void TestRichTextMasqueradingAsDocumentNoteWillEncodeToText()
		{
			var note = Factory.New<StmNote>();
			using (note.GetValidationSuspender())
			{
				Factory.SuspendValidation();
				note.ST_Table = "GlbStaff";
				note.ST_Description = "";
				note.ST_NoteType = nameof(StmNoteVisibility.DOC);
				note.ST_NoteData = ZBlob.FromAscii(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\lang2057\f0\fs20 <NewDataSet />\par }");
				AssertEquals("<NewDataSet />", note.ST_NoteData.ToAscii());
				Factory.Save();
				AssertEquals("Saved success", true, note.IsInDatabase);
				AssertNotNull("Saved to DB", new BusinessObjectFactory().Load<StmNote>(note.PK));
			}
		}

		public void TestSetNoteTypeWillEncodeRichTextWhenNoteTypeIsDOC()
		{
			var note = Factory.New<StmNote>();
			using (note.GetValidationSuspender())
			{
				Factory.SuspendValidation();
				note.ST_Table = "GlbStaff";
				note.ST_Description = "";
				note.ST_NoteType = nameof(StmNoteVisibility.INT);
				note.ST_NoteData = ZBlob.FromAscii(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\lang2057\f0\fs20 <NewDataSet />\par }");
				AssertEquals("Contains rich text", true, note.ST_NoteData.ToAscii().StartsWith(@"{\rtf1"));

				note.ST_NoteType = nameof(StmNoteVisibility.DOC);
				AssertEquals("Don't contains rich text", "<NewDataSet />", note.ST_NoteData.ToAscii());

				Factory.Save();
				AssertEquals("Saved success", true, note.IsInDatabase);
				AssertNotNull("Saved to DB", new BusinessObjectFactory().Load<StmNote>(note.PK));
			}
		}

		public void TestSetNoteDataAsTextWillEncodeRichTextWhenNoteTypeIsDOC()
		{
			var note = Factory.New<StmNote>();
			using (note.GetValidationSuspender())
			{
				Factory.SuspendValidation();
				note.ST_Table = "GlbStaff";
				note.ST_Description = "";
				note.ST_NoteType = nameof(StmNoteVisibility.INT);
				note.ST_NoteDataAsText = "<NewDataSet />";
				AssertEquals("Contains rich text", true, note.ST_NoteData.ToAscii().StartsWith(@"{\rtf1"));

				note.ST_NoteType = nameof(StmNoteVisibility.DOC);
				AssertEquals("Don't contains rich text", "<NewDataSet />", note.ST_NoteData.ToAscii());

				note.ST_NoteDataAsText = "<AnotherDataSet />";
				AssertEquals("Don't contains rich text", "<AnotherDataSet />", note.ST_NoteData.ToAscii());

				Factory.Save();
				AssertEquals("Saved success", true, note.IsInDatabase);
				AssertNotNull("Saved to DB", new BusinessObjectFactory().Load<StmNote>(note.PK));
			}
		}

		public void TestEmptySTTableWhenSavingRaisesIssue()
		{
			StmNote note = Factory.New<StmNote>();
			AssertEquals(true, string.IsNullOrWhiteSpace(note.ST_Table));
			Factory.Save();
			AssertEquals("EmptySTTableInNewStmNote", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestDefaults()
		{
			StmNote note = Factory.New<StmNote>();
			AssertEquals("ST_NoteType", "INT", note.ST_NoteType);
			AssertEquals("ST_IsCustomDescription", ZBool.False, note.ST_IsCustomDescription);
		}

		public void TestStmNoteAttriburesGetterNoAdditionalDBHints()
		{
			var factory = new BusinessObjectFactory();
			IStmNoteParent dummyBizOWithAutoLogs = factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();
			note.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;
			note.ST_NoteDataAsText = "Test";
			factory.Save();

			var note1 = Factory.Load<StmNote>(note.PK);
			SqlEventTracker.Instance.Clear();

			_ = note1.ST_CreatedDateUtc;
			_ = note1.ST_CreatedByUserInitials;
			_ = note1.ST_CreatedByUserName;
			_ = note1.ST_LastModifiedByUserName;
			_ = note1.ST_LastModifiedDate;

			var addedLogSqlStatement = @"SELECT  TOP 1
SL_PK, SL_EventTime, SL_EventTimeUtc, SL_FireWorkflow, SL_GB_NKBranch,
SL_GE_NKDepartment, SL_GS_NKUser, SL_IsCancelled, SL_IsEstimate, SL_Parent,
SL_PostedTimeUtc, SL_Reference, SL_SE_NKEvent, SL_Table
	FROM dbo.StmALog
	WHERE SL_SE_NKEvent = @CWO1_ and SL_Parent = @CWO2_";
			var lastEditLogSqlStatement = @"SELECT  TOP 1
SL_PK, SL_EventTime, SL_EventTimeUtc, SL_FireWorkflow, SL_GB_NKBranch,
SL_GE_NKDepartment, SL_GS_NKUser, SL_IsCancelled, SL_IsEstimate, SL_Parent,
SL_PostedTimeUtc, SL_Reference, SL_SE_NKEvent, SL_Table
	FROM dbo.StmALog WITH (FORCESEEK, INDEX(NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime))
	WHERE SL_Parent = @CWO1_ and SL_SE_NKEvent = @CWO2_
	ORDER BY SL_PostedTimeUtc DESC";
			AssertEquals(false, SqlEventTracker.Instance.SqlEventList.Any(x => x.StartsWith(addedLogSqlStatement, StringComparison.OrdinalIgnoreCase)));
			AssertEquals(false, SqlEventTracker.Instance.SqlEventList.Any(x => x.StartsWith(lastEditLogSqlStatement, StringComparison.OrdinalIgnoreCase)));
		}

		public void TestCreatedOrUpdateByUser()
		{
			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			StmNote note = dummyBizOWithAutoLogs.Notes.AddNew();
			note.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;
			note.ST_NoteDataAsText = "Test";
			Factory.Save();

			AssertEquals("ST_CreatedByUserInitials should be set to current user before saving", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, note.ST_CreatedByUserInitials);
			AssertEquals("ST_CreatedByUserName should be set to current user before saving", StaticCurrentFetcher.Instance.CurrentUser.GS_FullName, note.ST_CreatedByUserName);
			AssertEquals("ST_SystemCreateUser should be set to current user before saving", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, note.ST_SystemCreateUser);
			AssertEquals("ST_LastModifiedByUserName should be set to current user before saving", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, note.ST_LastModifiedByUserName);
			AssertEquals("ST_SystemLastEditUser should be set to current user before saving", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, note.ST_SystemLastEditUser);
		}

		public void TestLogDuplicateNoteInfo_DoNotLogOnFailedSave()
		{
			ErrorReporter.Clear();

			var predefinedNoteType = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes;
			Assert("Pre-Condition: note type is not custom", !predefinedNoteType.IsCustomNoteType);
			Assert("Pre-Condition: note type does not allow duplicate", predefinedNoteType.IsOnlyOneAllowed);

			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			CreateNewNote(dummyBizOWithAutoLogs, predefinedNoteType.Description, "Test", "AAA", ZBool.False, nameof(StmNoteVisibility.INT));

			Factory.Save();

			try
			{
				var badNote = Factory.New<StmNote>();
				badNote.ST_GC_RelatedCompany = ZGuid.NewZGuid();

				CreateNewNote(dummyBizOWithAutoLogs, predefinedNoteType.Description, "Test", "AAA", ZBool.False, nameof(StmNoteVisibility.INT));
				Fail("Expected to trigger ZSaveException.");
			}
			catch (ZSaveException)
			{
				AssertEquals("Saving_Duplicate_Note has not been reported", false, ErrorReporter.HasBeenReported("Saving_Duplicate_Note"));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestDuplicateNoteNotSavedIntoDatabase()
		{
			ErrorReporter.Clear();

			var predefinedNoteType = PredefinedNoteTypes.Instance.DetailedGoodsDescription;
			Assert("Pre-Condition: note type is not custom", !predefinedNoteType.IsCustomNoteType);
			Assert("Pre-Condition: note type does not allow duplicate", predefinedNoteType.IsOnlyOneAllowed);

			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			CreateNewNote(dummyBizOWithAutoLogs, predefinedNoteType.Description, "Test", "AAA", ZBool.False, nameof(StmNoteVisibility.INT));

			var note = dummyBizOWithAutoLogs.Notes.AddNew();
			note.ST_Description = predefinedNoteType.Description;
			note.ST_NoteDataAsText = "test";
			note.ST_NoteContext = "AAA";
			note.ST_IsCustomDescription = false;
			note.ST_NoteType = nameof(StmNoteVisibility.INT);

			var exception = AssertExceptionThrown<ZSaveException>("Factory save should fail because of the duplication check in trigger", () => { Factory.Save(); });
			AssertContains(@"Failed to save a note into the database because there is already another note existing with the same description and Context Module for this job.", exception.Message);
		}

		public void TestDescriptionChangeFromCustomToDefined()
		{
			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = CreateNote(dummyBizOWithAutoLogs);

			note.ST_IsCustomDescription = true;
			note.ST_NoteType = nameof(StmNoteVisibility.INT);
			note.ST_Description = "A Custom Note";
			note.ST_NoteData = ZBlob.FromAscii(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\lang2057\f0\fs20 <NewDataSet />\par }");

			note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;

			AssertEquals("No Longer Custom", false, note.ST_IsCustomDescription);
			AssertEquals("Note Text Set", "<NewDataSet />", note.ST_NoteText);
		}

		public void TestReportDeveloperIfNoteDataOrNoteTextShouldNotBeSet()
		{
			ErrorReporter.Clear();
			TestPredefinedNoteTypes.Register();

			var textOnlyNoteType = TestPredefinedNoteTypes.Instance.TextOnlyNote;
			var richTextNoteType = TestPredefinedNoteTypes.Instance.RichTextNote;

			var dummy = Factory.New<DummyBizOWithAutoLogs>();
			var textOnlyNote = CreateNewNote(dummy, textOnlyNoteType, "Existing text", "AAA");
			var richTextNote = CreateNewNote(dummy, richTextNoteType, "Existing rich text", "AAA");

			Factory.Save();

			var richTextPossibleValue = "pretend this contains encoding within a RichTextBox" + new string('x', 200);
			var textOnlyPossibleValue = "i seriously have no clue how this could happen" + new string('x', 200);
			try
			{
				textOnlyNote.ST_NoteData = ORtfTextUtil.TextToRtfBytes(richTextPossibleValue);
				AssertEquals("Error reported", "DataOnTextNote", ErrorReporter.LastKeyReported);
				AssertContains("Note details are logged", "", ErrorReporter.LastMessageReported);

				AssertExceptionThrown<InvalidOperationException>(() => richTextNote.ST_NoteText = textOnlyPossibleValue);
			}
			finally
			{
				ErrorReporter.Clear();
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestChangeNoteTypeDetailsCausesIsPopupLogToUpdate()
		{
			ErrorReporter.Clear();
			TestPredefinedNoteTypes.Register();

			var popupLogType = TestPredefinedNoteTypes.Instance.PopupLogNote;

			var dummy = Factory.New<DummyBizOWithAutoLogs>();
			var popupLogNote = CreateNewNote(dummy, popupLogType, "Existing text", "AAA");

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				Assert(popupLogNote.ST_IsPopupLog);
				Assert(popupLogNote.ST_IsTextOnly);
			});

			TestPredefinedNoteTypes.Unregister();
			PredefinedNoteTypesSimulateChangePopupLogInRegistryTest.Register();
			Factory.Save();

			Assert("Is no longer a text-only note", !popupLogNote.ST_IsTextOnly);
			Assert("Therefore isPopupLog should now be false", !popupLogNote.ST_IsPopupLog);
		}

		public void TestEmptyNoteDataFallbackToText()
		{
			TestPredefinedNoteTypes.Register();

			var richTextNoteType = TestPredefinedNoteTypes.Instance.RichTextNote;
			var dummy = Factory.New<DummyBizOWithAutoLogs>();
			var richTextNote = CreateNewNote(dummy, richTextNoteType, "Rich text", "AAA");

			Factory.Save();

			var sql = @"UPDATE dbo.StmNote SET ST_NoteText = @NoteText, ST_NoteData = NULL WHERE ST_PK = @PK";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@NoteText", System.Data.SqlDbType.NVarChar, "Plain text for RTF Note");
				command.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, richTextNote.PK.ToGuid());
				command.ExecuteNonQuery();
			}
			var reloadedRichTextNote = new BusinessObjectFactory().Load<StmNote>(richTextNote.PK);

			AssertEquals(reloadedRichTextNote.ST_NoteData, (byte[])ZBlob.FromUTF8("Plain text for RTF Note"));
		}

		public void TestEmptyNoteTextFallbackToData()
		{
			TestPredefinedNoteTypes.Register();

			var plainTextNoteType = TestPredefinedNoteTypes.Instance.TextOnlyNote;
			var dummy = Factory.New<DummyBizOWithAutoLogs>();
			var plainTextNote = CreateNewNote(dummy, plainTextNoteType, "Plain text", "AAA");

			Factory.Save();

			var sql = @"UPDATE dbo.StmNote SET ST_NoteText = '', ST_NoteData = @NoteData WHERE ST_PK = @PK";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@NoteData", System.Data.SqlDbType.VarBinary, (byte[])ZBlob.FromUTF8("Data for plain text note"));
				command.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, plainTextNote.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			var reloadedPlainTextNote = new BusinessObjectFactory().Load<StmNote>(plainTextNote.PK);

			AssertEquals(reloadedPlainTextNote.ST_NoteText, "Data for plain text note");
		}

		public void TestTextOnlyDescriptionChanged()
		{
			var predefinedNoteType = PredefinedNoteTypes.Instance.OutturnNotes;
			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();

			AssertEquals("Note Text not set", string.Empty, note.ST_NoteText);
			note.ST_NoteData = ZBlob.FromAscii(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\lang2057\f0\fs20 <NewDataSet />\par }");
			note.ST_Description = predefinedNoteType.Description;

			AssertEquals("Note Text set to Text of Note Data", "<NewDataSet />", note.ST_NoteText);

			note.ST_NoteText = "Note Text Set directly";

			predefinedNoteType = PredefinedNoteTypes.Instance.PickupInstructionsNote;
			note.ST_Description = predefinedNoteType.Description;

			AssertEquals("Note Text not changed", "Note Text Set directly", note.ST_NoteText);
		}

		public void TestRichTextFormatDescriptionChanged()
		{
			var predefinedNoteType = PredefinedNoteTypes.Instance.PaymentHandlingInstructions;
			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();

			AssertNotNull(note.Master);
			AssertEquals("Note Data not set", ZBlob.Empty, note.ST_NoteData);
			note.ST_NoteText = "Note Text Set";
			note.ST_Description = predefinedNoteType.Description;

			AssertEquals("Note Data set to Text of Note Text", "Note Text Set", ORtfTextUtil.RtfToText(note.ST_NoteData));

			note.ST_NoteData = ORtfTextUtil.TextToRtfBytes("Note Data Set directly");

			predefinedNoteType = PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes;
			note.ST_Description = predefinedNoteType.Description;

			AssertEquals("Note Data not changed", "Note Data Set directly", ORtfTextUtil.RtfToText(note.ST_NoteData));
		}

		public void TestIsCustomChanged()
		{
			IStmNoteParent dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();

			AssertEquals("Note Data not set", ZBlob.Empty, note.ST_NoteData);
			note.ST_NoteText = "Note Text Set";
			note.ST_IsCustomDescription = true;

			AssertEquals("Note Data set to Text of Note Text", "Note Text Set", ORtfTextUtil.RtfToText(note.ST_NoteData));
		}

		StmNote CreateNewNote(IStmNoteParent parent, PredefinedNoteType noteType, string text, string context)
		{
			var note = parent.Notes.AddNew();
			note.ST_Description = noteType.Description;
			if (noteType.IsTextOnly)
			{
				note.ST_NoteText = text;
			}
			else
			{
				note.ST_NoteData = ORtfTextUtil.TextToRtfBytes(text);
			}
			note.ST_NoteContext = context;
			note.ST_IsCustomDescription = noteType.IsCustomNoteType;
			note.ST_NoteType = noteType.DefaultVisibility.ToString();
			Factory.Save();
			return note;
		}

		StmNote CreateNewNote(IStmNoteParent parent, string description, string text, string context, bool isCustomDescription, string noteType)
		{
			var note = parent.Notes.AddNew();
			note.ST_Description = description;
			note.ST_NoteDataAsText = text;
			note.ST_NoteContext = context;
			note.ST_IsCustomDescription = isCustomDescription;
			note.ST_NoteType = noteType;
			Factory.Save();
			return note;
		}

		public void TestUserMaxLengthsSameAsGlbStaff()
		{
			var note = BizO.Notes.AddNew();
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, note.ST_CreatedByUserInitialsInfo.MaxLength);
			AssertEquals(GlbStaffSchema.GS_FullName.MaxLength, note.ST_CreatedByUserNameInfo.MaxLength);
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, note.ST_LastModifiedByUserNameInfo.MaxLength);
		}

		public void TestST_NoteContextNotReadOnlyForOrgHeader()
		{
			BusinessObject orgHeader = (BusinessObject)Factory.New<IOrgHeader>();
			StmNote note = orgHeader.GetNotes().AddNew();
			AssertEquals("ST_NoteContext ReadOnly for OrgHeader", false, note.ST_NoteContextInfo.ReadOnly);
		}

		public void TestST_NoteContextReadOnlyForAllExceptOrgHeader()
		{
			StmNote note = BizO.Notes.AddNew();
			AssertEquals("ST_NoteContext ReadOnly for not OrgHeader", true, note.ST_NoteContextInfo.ReadOnly);
		}

		#region ST_NoteContext "wrappers" testing

		[ExpectNoExceptions]
		public void TestST_NoteContextModule()
		{
			StmNote bizO = Factory.New<StmNote>();
			bizO.ST_NoteContext = "___";
			AssertEquals("precondition", "___", bizO.ST_NoteContext);
			bizO.ST_NoteContextModule = "A";
			AssertEquals("ST_NoteContextModule should mirror first character of ST_NoteContext", "A", bizO.ST_NoteContextModule);
			AssertEquals("ST_NoteContextModule should not alter characters of ST_NoteContext except first one", "A__", bizO.ST_NoteContext);
			bizO.ST_NoteContext = "__";
			AssertEquals("ST_NoteContextModule should return question mark ('?') when ST_NoteContext.Length != 3", "?", bizO.ST_NoteContextModule);
			bizO.ST_NoteContextModule = "A";
			ErrorReporter.Clear();
		}

		public void TestST_NoteContextModuleCaption()
		{
			StmNote bizO = Factory.New<StmNote>();
			bizO.ST_NoteContext = "O__";
			AssertEquals("precondition", "O__", bizO.ST_NoteContext);
			bizO.ST_NoteContextModuleCaption = StmNoteCaptions.Module["A"];
			AssertEquals("ST_NoteContextModuleCaptions should return value from StmNoteCaptions.Module dictionary using ST_NoteContextModule as key", StmNoteCaptions.Module["A"], bizO.ST_NoteContextModuleCaption);
			AssertEquals("ST_NoteContextModuleCaptions should not alter characters of ST_NoteContext except first one (except when changing from or to Warehouse module)", "A__", bizO.ST_NoteContext);
			bizO.ST_NoteContextModuleCaption = StmNoteCaptions.Module["W"];
			AssertEquals("ST_NoteContextModuleCaptions should reset 2nd and 3rd characters of ST_NoteContext when changing from or to Warehouse module)", "WAA", bizO.ST_NoteContext);
			Assert(!StmNoteCaptions.Module.ContainsKey("B"));
		}

		public void TestST_NoteContextDirection()
		{
			StmNote bizO = Factory.New<StmNote>();
			bizO.ST_NoteContext = "AAA";
			AssertEquals("precondition", "AAA", bizO.ST_NoteContext);
			bizO.ST_NoteContextDirection = "I";
			AssertEquals("ST_NoteContextDirection should mirror second character of ST_noteContext", "I", bizO.ST_NoteContextDirection);
			AssertEquals("ST_NoteContextDirection should not alter characters of ST_noteContext except second one (except when ST_NoteContextModule is set to 'W' (Warehouse))", "AIA", bizO.ST_NoteContext);
			bizO.ST_NoteContext = "WR_";
			bizO.ST_NoteContextDirectionCaption = StmNoteCaptions.DirectionWarehouse["O"];
			AssertEquals("ST_NoteContextDirection value change should reset ST_NoteContextFreightMode value to default 'A' (All) when ST_NoteContextModule is set to 'W' (Warehouse)", "WOA", bizO.ST_NoteContext);
			bizO.ST_NoteContext = "__";
			AssertEquals("ST_NoteContextDirection should return question mark ('?') when ST_NoteContext.Length != 3", "?", bizO.ST_NoteContextDirection);
			ErrorReporter.Clear();
		}

		public void TestST_NoteContextDirectionCaption()
		{
			StmNote bizO = Factory.New<StmNote>();
			bizO.ST_NoteContext = "AAA";
			AssertEquals("precondition", "AAA", bizO.ST_NoteContext);
			bizO.ST_NoteContextDirectionCaption = StmNoteCaptions.Direction["I"];
			AssertEquals("ST_NoteContextDirectionCaptions should return value from StmNoteCaptions.Direction dictionary using ST_NoteContextDirection as key (except for warehouse notes)", StmNoteCaptions.Direction["I"], bizO.ST_NoteContextDirectionCaption);
			AssertEquals("ST_NoteContextDirectionCaptions should not alter characters of ST_NoteContext except second one (except when ST_NoteContextModule is set to 'W' (Warehouse))", "AIA", bizO.ST_NoteContext);
			bizO.ST_NoteContextModuleCaption = StmNoteCaptions.Module["W"];
			bizO.ST_NoteContextDirectionCaption = StmNoteCaptions.DirectionWarehouse["R"];
			AssertEquals("ST_NoteContextDirectionCaptions should return value from StmNoteCaptions.DirectionWarehouse dictionary using ST_NoteContextDirection as key (for warehouse notes)", StmNoteCaptions.DirectionWarehouse["R"], bizO.ST_NoteContextDirectionCaption);
		}

		public void TestST_NoteContextFreightMode()
		{
			StmNote bizO = Factory.New<StmNote>();
			bizO.ST_NoteContext = "___";
			AssertEquals("precondition", "___", bizO.ST_NoteContext);
			bizO.ST_NoteContextFreightMode = "A";
			AssertEquals("ST_NoteContextFreightMode should mirror third character of ST_noteContext", "A", bizO.ST_NoteContextFreightMode);
			AssertEquals("ST_NoteContextFreightMode should not alter characters of ST_noteContext except third one", "__A", bizO.ST_NoteContext);
			bizO.ST_NoteContext = "__";
			AssertEquals("ST_NoteContextFreightMode should return question mark ('?') when ST_NoteContext.Length != 3", "?", bizO.ST_NoteContextFreightMode);
			ErrorReporter.Clear();
		}

		public void TestST_NoteContextFreightModeCaption()
		{
			StmNote bizO = Factory.New<StmNote>();
			bizO.ST_NoteContext = "___";
			AssertEquals("precondition", "___", bizO.ST_NoteContext);
			bizO.ST_NoteContextFreightModeCaption = StmNoteCaptions.FreightMode["A"];
			AssertEquals("ST_NoteContextFreightModeCaptions should return value from StmNoteCaptions.FreightMode dictionary using ST_NoteContextFreightMode as key (except for warehouse notes)", StmNoteCaptions.Module["A"], bizO.ST_NoteContextFreightModeCaption);
			AssertEquals("ST_NoteContextFreightModeCaptions should not alter characters of ST_NoteContext except third one", "__A", bizO.ST_NoteContext);
			bizO.ST_NoteContextModuleCaption = StmNoteCaptions.Module["W"];
			bizO.ST_NoteContextDirectionCaption = StmNoteCaptions.DirectionWarehouse["R"];
			bizO.ST_NoteContextFreightModeCaption = StmNoteCaptions.FreightModeWarehouseR["R"];
			AssertEquals("ST_NoteContextDirectionCaptions should return value from StmNoteCaptions.FreightModeWarehouseR dictionary using ST_NoteContextFreightMode as key (for warehouse in notes)", StmNoteCaptions.FreightModeWarehouseR["R"], bizO.ST_NoteContextFreightModeCaption);
			bizO.ST_NoteContextDirectionCaption = StmNoteCaptions.DirectionWarehouse["O"];
			bizO.ST_NoteContextFreightModeCaption = StmNoteCaptions.FreightModeWarehouseO["O"];
			AssertEquals("ST_NoteContextDirectionCaptions should return value from StmNoteCaptions.FreightModeWarehouseO dictionary using ST_NoteContextFreightMode as key (for warehouse out notes)", StmNoteCaptions.FreightModeWarehouseO["O"], bizO.ST_NoteContextFreightModeCaption);
			bizO.ST_NoteContextDirectionCaption = StmNoteCaptions.DirectionWarehouse["I"];
			bizO.ST_NoteContextFreightModeCaption = StmNoteCaptions.FreightModeWarehouseI["T"];
			AssertEquals("ST_NoteContextDirectionCaptions should return value from StmNoteCaptions.FreightModeWarehouseI dictionary using ST_NoteContextFreightMode as key (for warehouse internal notes)", StmNoteCaptions.FreightModeWarehouseI["T"], bizO.ST_NoteContextFreightModeCaption);
		}

		public void TestST_Note_InvalidValue()
		{
			var bizO = Factory.New<StmNote>();
			bizO.ST_NoteContext = "";
			AssertEquals("ST_Note should not be less than three letters, new value is '', current value is 'AAA'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			bizO.ST_NoteContext = "AA";
			AssertEquals("ST_Note should not be less than three letters, new value is 'AA', current value is ''.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		public void TestST_NoteDataAsTextConcatenatedAndTrimmed()
		{
			StmNote note = BizO.Notes.AddNew();
			note.ST_NoteDataAsText =
@"This
is some test line
that spans across
multiple lines";

			AssertEquals("ST_NoteDataAsTextConcatenatedAndTrimmed", "This is some test line that spans across multiple lines", note.ST_NoteDataAsTextConcatenatedAndTrimmed);
		}

		public void TestST_NoteSource()
		{
			DummyBizOWithRelatedNotes dummyWithRelatedNotes = Factory.New<DummyBizOWithRelatedNotes>();

			// ST_NoteSource is only valid when the base bizO's notes are in an StmNoteCollectionView (as Note.IsRelated is dependent on it's parent view collection)
			StmNoteCollectionView noteView = new StmNoteCollectionView(dummyWithRelatedNotes);

			StmNote note = dummyWithRelatedNotes.Notes.AddNew();
			AssertEquals("Not a related note, and master implements INoteSource", "This " + dummyWithRelatedNotes.TableName, note.ST_NoteSource);

			StmNote relatedNote = dummyWithRelatedNotes.RelatedDummy.Notes.AddNew();
			AssertEquals("A related note, and master does *not* implement INoteSource", dummyWithRelatedNotes.TableName, relatedNote.ST_NoteSource);

			StmNote relatedNoteMaterImplementsINoteSource = dummyWithRelatedNotes.RelatedDummyImplementsINoteSource.Notes.AddNew();
			AssertEquals("A related note, and master does *not* implement INoteSource", "Sample Note Source", relatedNoteMaterImplementsINoteSource.ST_NoteSource);
		}

		public void TestValidateST_Description()
		{
			StmNote note1 = Factory.New<StmNote>();
			StmNote note2 = Factory.New<StmNote>();
			StmNote note3 = Factory.New<StmNote>();

			BizO.Notes.Add(note1);
			BizO.Notes.Add(note2);
			BizO.Notes.Add(note3);

			note1.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;
			Assert("BizO should have no errors.", !BizO.HasErrors);

			note2.ST_Description = DummyBizOWithRelatedNotes.WhiteNoteType.Description;
			Assert("BizO should have no errors.", !BizO.HasErrors);

			note3.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;
			Assert("BizO should have errors.", BizO.HasErrors);
		}

		public void TestValidateST_DescriptionDoesNotIncludeRelatedNotes()
		{
			DummyBizOWithRelatedNotes bizO2 = Factory.New<DummyBizOWithRelatedNotes>();
			StmNote note = CreateNote(BizO);
			StmNote relatedNote = CreateNote(bizO2);

			note.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;
			relatedNote.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;

			Assert("BizO should have no errors.", !BizO.HasErrors);
		}

		public void TestSettingIsCustomDescriptionMakesVisibilityReadonly()
		{
			StmNote note = CreateNote(BizO);
			Assert("Precondition: Visibility ReadOnly", note.ST_NoteTypeInfo.ReadOnly);
			Assert("Precondition: Visibility ReadOnly", note.ST_NoteType_DescriptiveTextInfo.ReadOnly);

			note.ST_IsCustomDescription = true;
			Assert("Visibility ReadOnly", !note.ST_NoteTypeInfo.ReadOnly);
			Assert("Visibility ReadOnly", !note.ST_NoteType_DescriptiveTextInfo.ReadOnly);

			note.ST_IsCustomDescription = false;
			Assert("Visibility ReadOnly", note.ST_NoteTypeInfo.ReadOnly);
			Assert("Visibility ReadOnly", note.ST_NoteType_DescriptiveTextInfo.ReadOnly);
		}

		public void TestST_NoteType_DescriptiveTextSetsST_NoteType()
		{
			StmNote note = CreateNote(BizO);
			note.ST_NoteType_DescriptiveText = "";
			Assert("Precondition - ST_NoteType_DescriptiveText should be empty", note.ST_NoteType_DescriptiveText.IsEmpty);
			Assert("Precondition - ST_NoteType should be empty", note.ST_NoteType.IsEmpty);

			note.ST_NoteType_DescriptiveText = StmNoteDescription.PubDescriptive;
			AssertEquals("Note.ST_NoteType_DescriptiveText", StmNoteDescription.PubDescriptive, note.ST_NoteType_DescriptiveText);
			AssertEquals("Note.ST_NoteType", "PUB", note.ST_NoteType);

			note.ST_NoteType_DescriptiveText = StmNoteDescription.IntDescriptive;
			AssertEquals("Note.ST_NoteType_DescriptiveText", StmNoteDescription.IntDescriptive, note.ST_NoteType_DescriptiveText);
			AssertEquals("Note.ST_NoteType", "INT", note.ST_NoteType);

			note.ST_NoteType_DescriptiveText = StmNoteDescription.PrvDescriptive;
			AssertEquals("Note.ST_NoteType_DescriptiveText", StmNoteDescription.PrvDescriptive, note.ST_NoteType_DescriptiveText);
			AssertEquals("Note.ST_NoteType", "PRV", note.ST_NoteType);

			note.ST_NoteType_DescriptiveText = StmNoteDescription.AgvDescriptive;
			AssertEquals("Note.ST_NoteType_DescriptiveText", StmNoteDescription.AgvDescriptive, note.ST_NoteType_DescriptiveText);
			AssertEquals("Note.ST_NoteType", "AGV", note.ST_NoteType);

			note.ST_NoteType_DescriptiveText = "xXx";
			AssertEquals("Note.ST_NoteType_DescriptiveText", "xXx", note.ST_NoteType_DescriptiveText);
			AssertEquals("Note.ST_NoteType", "xXx", note.ST_NoteType);
		}

		public void TestST_NoteTypeCore_List()
		{
			var note = CreateNote(BizO);

			Assert("The lists ST_NoteTypeCore_List and ST_NoteType_List should contain the same items", note.ST_NoteTypeCore_List.Count == note.ST_NoteType_List.Count);

			foreach (CodeDescriptionPair item in note.ST_NoteTypeCore_List)
			{
				Assert("ST_NoteType_List should contains " + item.Description, note.ST_NoteType_List.ContainsCode(item.Description));
			}
		}

		public void TestST_NoteDataAsText()
		{
			StmNote note = CreateNote(BizO);
			Assert("Precondition - Note.ST_NoteDataAsText should be empty", note.ST_NoteDataAsText.IsEmpty);
			AssertEquals("Precondition - Note.ST_IsTextOnly should be false", false, note.ST_IsTextOnly);

			note.ST_NoteDataAsText = "Lost Cities";
			AssertEquals("Note.ST_NoteData should be have been set", "Lost Cities", ORtfTextUtil.RtfToText(note.ST_NoteData));
			AssertEquals("Note.ST_NoteDataAsText should return the text with the RTF header stripped off", "Lost Cities", note.ST_NoteDataAsText);

			note.ST_NoteDataAsText = "";
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			Assert("Precondition - Note.ST_NoteDataAsText should be empty", note.ST_NoteDataAsText.IsEmpty);
			AssertEquals("Precondition - PredefinedNoteTypes.Instance.MarksAndNumbers should be text only", true, PredefinedNoteTypes.Instance.MarksAndNumbers.IsTextOnly);
			AssertEquals("Precondition - Note.ST_IsTextOnly should be true", true, note.ST_IsTextOnly);

			note.ST_NoteDataAsText = "Blue Moon";
			AssertEquals("Note.ST_NoteText should be have been set", "Blue Moon", note.ST_NoteText);
		}

		public void TestST_NoteText()
		{
			DummyBizOWithPredefinedUnmatchedNoteType bizOWithPredefinedUnmatchedNoteType = Factory.New<DummyBizOWithPredefinedUnmatchedNoteType>();
			StmNote note = CreateNote(bizOWithPredefinedUnmatchedNoteType);
			note.ST_NoteText = "abc";
			Factory.Save();
			AssertEquals("note text", "abc", note.ST_NoteText);

			note.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			note.ST_NoteText = "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Forwarder</OrganisationType><OrganisationSubType>Forwarder</OrganisationSubType><OwnerCode /><EDICode /><OrganisationName>TEST FORWARDER</OrganisationName><AddressLine1 /><AddressLine2 /><City /><PostCode /><StateOrProvince /><Country /></UnmatchOrgRecord></UnmatchOrgRecords>";

			ZString expectedNoteText = "Organisation Type: Forwarder\r\nOwner Code: \r\nEDI Code: \r\nOrganisation Name: TEST FORWARDER\r\nAddress Line 1: \r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: \r\n ";
			AssertEquals("serializable note text", expectedNoteText, note.ST_NoteText);
		}

		public void TestSettingIsCustomDescriptionTrueResetsVisibility()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;
			AssertEquals("Visibility", DummyBizOWithRelatedNotes.GreenNoteType.DefaultVisibility.ToString(), note.ST_NoteType);

			note.ST_IsCustomDescription = true;
			note.ST_NoteType = "EID";
			AssertEquals("Visibility", "EID", note.ST_NoteType);

			note.ST_IsCustomDescription = false;
			AssertEquals("Visibility", DummyBizOWithRelatedNotes.GreenNoteType.DefaultVisibility.ToString(), note.ST_NoteType);
		}

		public void TestValidateST_NoteText()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description; // text only note
			Assert("Precondition.", !note.ST_NoteTextInfo.HasErrors());

			note.RunPreSaveValidation();
			Assert("ST_NoteText should be mandatory if the note is text-only.", note.ST_NoteTextInfo.HasErrors());

			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description; // rtf note
			note.RunPreSaveValidation();
			Assert("ST_NoteText should *not* be mandatory if the note is rtf.", !note.ST_NoteTextInfo.HasErrors());
		}

		public void TestValidateST_NoteData()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description; // rtf note
			Assert("Precondition.", !note.ST_NoteDataInfo.HasErrors());

			note.RunPreSaveValidation();
			Assert("ST_NoteData should be mandatory if the note is text-only.", note.ST_NoteDataInfo.HasErrors());

			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description; // text only note
			note.RunPreSaveValidation();
			Assert("ST_NoteData should *not* be mandatory if the note is rtf.", !note.ST_NoteDataInfo.HasErrors());
		}

		public void TestValidateNoteContext()
		{
			var orgHeader = Factory.New<IOrgHeader>() as BusinessObject;
			var stmNote = orgHeader.GetNotes().AddNew();

			var originalRightForMdf = EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed;
			EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed = false;

			stmNote.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			stmNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
			stmNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);

			Assert("ST_NoteContextInfo should have errors", stmNote.ST_NoteContextInfo.HasErrors());

			stmNote.ST_NoteContextModule = nameof(StmNoteContextModule.C);
			Assert("ST_NoteContextInfo should have no errors", !stmNote.ST_NoteContextInfo.HasErrors());

			EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed = true;
			stmNote.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			Assert("ST_NoteContextInfo should have no errors with permission granted", !stmNote.ST_NoteContextInfo.HasErrors());

			EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed = originalRightForMdf;
		}

		public void TestValidateRelatedCompany()
		{
			var orgHeader = Factory.New<IOrgHeader>() as BusinessObject;
			var stmNote = orgHeader.GetNotes().AddNew();

			var originalRightForCompany = EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed;
			EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed = false;

			stmNote.ST_GC_RelatedCompany = ZGuid.Empty;
			stmNote.RunPreSaveValidation();
			Assert("ST_GC_RelatedCompanyInfo should have errors", stmNote.ST_GC_RelatedCompanyInfo.HasErrors());

			EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed = true;
			stmNote.ST_GC_RelatedCompany = ZGuid.Empty;
			stmNote.RunPreSaveValidation();
			Assert("ST_GC_RelatedCompanyInfo should have no errors with permission tranted", !stmNote.ST_GC_RelatedCompanyInfo.HasErrors());

			EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed = originalRightForCompany;
		}

		public void TestDocumentNoteShouldNotBeValidatedForContextAndCompany()
		{
			var orgHeader = Factory.New<IOrgHeader>() as BusinessObject;
			var documentNote = Factory.New<IDocumentNote>() as StmNote;
			orgHeader.GetNotes().Add(documentNote);

			var originalRightForMdf = EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed;
			var originalRightForCompany = EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed;
			EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed = false;

			documentNote.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			documentNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
			documentNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);

			Assert("ST_NoteContextInfo should not have errors for document note", !documentNote.ST_NoteContextInfo.HasErrors());

			EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed = false;
			documentNote.ST_GC_RelatedCompany = ZGuid.Empty;
			documentNote.RunPreSaveValidation();
			Assert("ST_GC_RelatedCompanyInfo should have no errors for document note", !documentNote.ST_GC_RelatedCompanyInfo.HasErrors());

			EnvProxy.Instance.Security.NotesNewWithAllMDFForOrg.IsAllowed = originalRightForMdf;
			EnvProxy.Instance.Security.NotesNewWithAllCompanyForOrg.IsAllowed = originalRightForCompany;
		}

		public void TestST_IsTextOnly()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description; // rtf note
			Assert("ST_IsCustomDescription = false, Predefined Rtf note should *not* be text only", !note.ST_IsTextOnly);

			note.ST_IsCustomDescription = true;
			Assert("ST_IsCustomDescription = true, Predefinted Rtf note should *not* be text only", !note.ST_IsTextOnly);

			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description; // text only note
			Assert("ST_IsCustomDescription = true, Predefined Text note should *not* be text only", !note.ST_IsTextOnly);

			note.ST_IsCustomDescription = false;
			Assert("ST_IsCustomDescription = false, Predefined Text note should be text only", note.ST_IsTextOnly);
		}

		public void TestST_NoteText_MaxLength()
		{
			TestPredefinedNoteTypes.Register();
			try
			{
				StmNote note = CreateNote(BizO);
				note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
				AssertEquals("Text only note max length should be taken from the predefined note", 100, note.ST_NoteTextInfo.MaxLength);

				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				DummyBizOWithRelatedNotes loadedBizO = Factory.Load<DummyBizOWithRelatedNotes>(BizO.PK);
				StmNote loadedNote = loadedBizO.Notes.FindByDescription(TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description)[0];
				AssertEquals("ST_NoteText.MaxLength should still come from the predefined note after re-loading the note", 100, loadedNote.ST_NoteTextInfo.MaxLength);
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestST_NoteText_MaxLength_TrimmedAppropriatelyIfNoteTypeChanges()
		{
			TestPredefinedNoteTypes.Register();
			try
			{
				StmNote note = CreateNote(BizO);
				note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength200.Description;
				AssertEquals("Text only note max length should be taken from the predefined note", 200, note.ST_NoteTextInfo.MaxLength);

				note.ST_NoteText = new string('x', 150);
				note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
				AssertEquals("ST_NoteText should be trimmed", new string('x', 100), note.ST_NoteText);
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestSettingBlobOnTextNoteThrowsException()
		{
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteData = ZBlob.FromAscii("Nikon 70-200 VR f2.8");
			AssertEquals("Setting ST_NoteData on a Text note should throw an exception", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSettingTextOnBlobNoteThrowsException()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			AssertExceptionThrown<InvalidOperationException>(() => note.ST_NoteText = "Nikon 70-200 VR f2.8");
		}

		public void TestSettingTextOnBlobNoteWhenCopyingDoesNotThrowException()
		{
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;

			((INeedRow)note).Row[StmNoteSchema.ST_NoteText.Name] = "Nikon 70-200 VR f2.8";
			((INeedRow)note).Row[StmNoteSchema.ST_NoteData.Name] = (byte[])ZBlob.FromAscii("Nikon 70-200 VR f2.8");

			StmNote noteClone = (StmNote)note.Clone();
			AssertEquals("Setting ST_NoteText on a Blob note during Copy should *not* throw an exception", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Nikon 70-200 VR f2.8", ((INeedRow)note).Row[StmNoteSchema.ST_NoteText.Name].ToString());
			ZBlob blob = (ZBlob)(byte[])((INeedRow)note).Row[StmNoteSchema.ST_NoteData.Name];
			AssertEquals("Nikon 70-200 VR f2.8", blob.ToAscii());
		}

		public void TestSettingBlobOnTextNoteWhenCopyingDoesNotThrowException()
		{
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;

			((INeedRow)note).Row[StmNoteSchema.ST_NoteText.Name] = "Nikon 70-200 VR f2.8";
			((INeedRow)note).Row[StmNoteSchema.ST_NoteData.Name] = (byte[])ZBlob.FromAscii("Nikon 70-200 VR f2.8");

			StmNote noteClone = (StmNote)note.Clone();
			AssertEquals("Setting ST_NoteData on a Text note during Copy should *not* throw an exception", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Nikon 70-200 VR f2.8", ((INeedRow)note).Row[StmNoteSchema.ST_NoteText.Name].ToString());
			ZBlob blob = (ZBlob)(byte[])((INeedRow)note).Row[StmNoteSchema.ST_NoteData.Name];
			AssertEquals("Nikon 70-200 VR f2.8", blob.ToAscii());
		}

		public void TestTextNoteClearsBlobOnSave()
		{
			StmNote note = CreateNote(BizO);
			note.ST_NoteData = ZBlob.FromAscii("Nikon 70-200 VR f2.8");
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			AssertEquals("ST_NoteData should be intact on TextOnly before save", ZBlob.FromAscii("Nikon 70-200 VR f2.8"), note.ST_NoteData);
			note.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("ST_NoteData should be cleared on TextOnly after save", ZBlob.Empty, note.ST_NoteData);
		}

		public void TestBlobNoteClearsTextOnSave()
		{
			StmNote note = CreateNote(BizO);
			note.ST_NoteText = "Nikon 70-200 VR f2.8";
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			AssertEquals("ST_NoteText should be intact on Blob before save", "Nikon 70-200 VR f2.8", note.ST_NoteText);
			note.RunPreSaveValidation();
			Factory.Save();
			Assert("ST_NoteText should be cleared on Blob after save", note.ST_NoteText.IsEmpty);
		}

		public void TestCannotSaveEmptyTextNote()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			Assert("Precondition - ST_NoteText should not have any errors", !note.ST_NoteTextInfo.HasErrors());

			note.ST_NoteText = "";
			Assert("ST_NoteText should be in error", note.ST_NoteTextInfo.HasErrors());

			note.ST_NoteText = "ole";
			Assert("ST_NoteText should not have any errors", !note.ST_NoteTextInfo.HasErrors());
		}

		public void TestCannotSaveEmptyBlobNote()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			Assert("Precondition - ST_NoteData should not have any errors", !note.ST_NoteDataInfo.HasErrors());

			note.ST_NoteData = ZBlob.Empty;
			Assert("ST_NoteData should be in error", note.ST_NoteDataInfo.HasErrors());

			note.ST_NoteDataAsText = "ole";
			Assert("ST_NoteData should not have any errors", !note.ST_NoteDataInfo.HasErrors());
		}

		public void TestValidateST_IsCustomDescription()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = DummyBizOWithRelatedNotes.GreenNoteType.Description;
			note.ST_IsCustomDescription = false;
			Assert("Precondition - ST_IsCustomDescription should not have any errors", !note.ST_IsCustomDescriptionInfo.HasErrors());

			note.ST_IsCustomDescription = true;
			Assert("ST_IsCustomDescription should be in error", note.ST_IsCustomDescriptionInfo.HasErrors());

			note.ST_Description = "ooga booga.";
			note.Validation.ValidateST_IsCustomDescription();
			Assert("ST_IsCustomDescription should not have any errors", !note.ST_IsCustomDescriptionInfo.HasErrors());
		}

		public void TestCopyPersistentValuesFrom()
		{
			StmNote note1 = CreateNote(BizO);
			note1.ST_Description = "Nikon 17-55 DX f2.8";

			StmNote note2 = Factory.New<StmNote>();
			note2.CopyPersistentValuesFrom(note1);

			AssertEquals("Nikon 17-55 DX f2.8", note2.ST_Description);
			AssertEquals(BizO.PK, note2.ST_ParentID);
			AssertEquals(BizO.TableName, note2.ST_Table);

			AssertEquals(note1.ST_Description, note2.ST_Description);
			AssertEquals(note1.ST_ParentID, note2.ST_ParentID);
			AssertEquals(note1.ST_Table, note2.ST_Table);
		}

		public void TestClone()
		{
			StmNote note1 = CreateNote(BizO);
			note1.ST_Description = "Nikon 17-55 DX f2.8";

			StmNote note2 = (StmNote)note1.Clone();

			AssertEquals("Nikon 17-55 DX f2.8", note2.ST_Description);
			AssertEquals(BizO.PK, note2.ST_ParentID);
			AssertEquals(BizO.TableName, note2.ST_Table);

			AssertEquals(note1.ST_Description, note2.ST_Description);
			AssertEquals(note1.ST_ParentID, note2.ST_ParentID);
			AssertEquals(note1.ST_Table, note2.ST_Table);
		}

		public void TestST_Table()
		{
			StmNote note = CreateNote(BizO);

			note.ST_Table = "StandardTableName";
			AssertEquals("StandardTableName", note.ST_Table);

			note.ST_Table = "SomeReferenceDatabase.dbo.ERFTableName";
			AssertEquals("ERFTableName", note.ST_Table);

			note.ST_Table = "SomeOtherReferenceDatabase..TRFTableName";
			AssertEquals("TRFTableName", note.ST_Table);
		}

		public void TestTypeDeciderForNew()
		{
			AssertEquals(typeof(StmNote), StmNote.TypeDecider.GetTypeForNew());
			AssertEquals(typeof(StmNote), Factory.New(typeof(StmNote)).GetType());
		}

		public void TestTypeDeciderForBinding()
		{
			AssertEquals(typeof(StmNote), StmNote.TypeDecider.GetTypeForBinding());
		}

		public void TestTypeDeciderForLoad()
		{
			Type historyNoteType = ObjectFactory.GetType<Enterprise.Integration.Freight.IOrderUpdateHistoryStmNote>();

			StmNote note = BizO.Notes.AddNew();
			StmNote historyNote = (StmNote)BizO.Notes.AddNew(historyNoteType);

			AssertEquals(typeof(StmNote), BizO.Notes.FindByPK(note.PK).GetType());
			AssertEquals(historyNoteType, BizO.Notes.FindByPK(historyNote.PK).GetType());
		}

		public void TestSupportsNotes()
		{
			AssertEquals(false, Factory.New<StmNote>().SupportsNotes);
		}

		public void TestST_IsPopupLog()
		{
			TestPredefinedNoteTypes.Register();
			try
			{
				StmNote note = CreateNote(BizO);
				AssertEquals("ST_IsPopupLog", false, note.ST_IsPopupLog);
				AssertEquals("ST_NoteTextInfo.ReadOnly", false, note.ST_NoteTextInfo.ReadOnly);

				note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;
				AssertEquals("ST_IsPopupLog", true, note.ST_IsPopupLog);
				AssertEquals("ST_NoteTextInfo.ReadOnly", true, note.ST_NoteTextInfo.ReadOnly);

				note.ST_IsCustomDescription = true;
				AssertEquals("ST_IsPopupLog", false, note.ST_IsPopupLog);
				AssertEquals("ST_NoteTextInfo.ReadOnly", false, note.ST_NoteTextInfo.ReadOnly);

				note.ST_IsCustomDescription = false;
				note.ST_Description = "x";
				AssertEquals("IsPopupLog", false, note.ST_IsPopupLog);
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestGetCloneForPopup_StandardNote()
		{
			StmNote note = CreateNote(BizO);

			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteText = "xxx";
			note.ReadOnly = false;

			StmNote clone = note.GetCloneForPopup();
			AssertEquals("ST_Description", PredefinedNoteTypes.Instance.MarksAndNumbers.Description, clone.ST_Description);
			AssertEquals("ST_NoteText", "xxx", clone.ST_NoteText);
			AssertEquals("ReadOnly", false, clone.ReadOnly);

			note.ST_Description = PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description;
			note.ST_NoteData = new byte[] { 1, 2, 3 };
			note.ReadOnly = true;

			clone = note.GetCloneForPopup();
			AssertEquals("ST_Description", PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description, clone.ST_Description);
			AssertEquals("ST_NoteData", new byte[] { 1, 2, 3 }, clone.ST_NoteData);
			AssertEquals("ReadOnly", true, clone.ReadOnly);
		}

		public void TestGetCloneForPopup_IsClonedForEdit()
		{
			EnvProxy.Instance.Security.NotesNew.IsAllowed = false;
			EnvProxy.Instance.Security.NotesEdit.IsAllowed = true;

			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteText = "xxx";
			note.ReadOnly = false;
			AssertEquals("Note not in DB", false, note.IsInDatabase);

			var cloneForAdd = note.GetCloneForPopup();
			cloneForAdd.ST_Description = "Aa";
			AssertEquals("clone.ST_IsClonedForEdit should be false", false, cloneForAdd.ST_IsClonedForEdit);

			AssertNotEquals(cloneForAdd.ST_IsCustomDescriptionInfo.ReadOnly, EnvProxy.Instance.Security.NotesNew.IsAllowed);

			Factory.Save();
			var cloneForEdit = note.GetCloneForPopup();
			cloneForEdit.ST_Description = "Bb";
			AssertEquals("clone2.ST_IsClonedForEdit should be true", true, cloneForEdit.ST_IsClonedForEdit);

			AssertNotEquals(cloneForEdit.ST_IsCustomDescriptionInfo.ReadOnly, EnvProxy.Instance.Security.NotesEdit.IsAllowed);
		}

		public void TestGetCloneForPopup_PopupLogNote()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				StmNote note = CreateNote(BizO);

				note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;
				note.ST_NoteText = "xxx";
				note.ReadOnly = true;

				StmNote clone = note.GetCloneForPopup();
				AssertEquals("ST_Description", TestPredefinedNoteTypes.Instance.PopupLogNote.Description, clone.ST_Description);
				AssertEquals("ST_NoteText", "", clone.ST_NoteText);
				AssertEquals("ReadOnly", true, clone.ReadOnly);
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestCopyChangesFromPopup_StandardNote()
		{
			StmNote note = CreateNote(BizO);

			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			StmNote clone = note.GetCloneForPopup();
			clone.ST_NoteText = "!!!";
			note.CopyChangesFromPopup(clone);
			AssertEquals("ST_NoteText", "!!!", note.ST_NoteText);

			note.ST_Description = PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description;
			clone = note.GetCloneForPopup();
			clone.ST_NoteData = new byte[] { 7, 8, 9 };
			note.CopyChangesFromPopup(clone);
			AssertEquals("ST_NoteData", new byte[] { 7, 8, 9 }, note.ST_NoteData);
		}

		public void TestCopyChangesFromPopup_MaxLength()
		{
			StmNote note = CreateNote(BizO);
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Code;
			note.ST_NoteText = "xxx";
			AssertEquals(50000, note.ST_NoteTextInfo.MaxLength);

			var cloneString = string.Concat(Enumerable.Repeat("X", 50001));
			StmNote clone = note.GetCloneForPopup();
			clone.NoteTextMaxLength = 50001;
			clone.ST_NoteText = cloneString;
			AssertEquals(50001, clone.ST_NoteTextInfo.MaxLength);
			AssertEquals("clone.ST_NoteText", clone.ST_NoteTextInfo.MaxLength, clone.ST_NoteText.Length);

			note.CopyChangesFromPopup(clone);
			AssertEquals("note.ST_NoteText", note.ST_NoteTextInfo.MaxLength, note.ST_NoteText.Length);
		}

		public void TestCopyChangesFromPopup_IsPopupLog_MaxLength()
		{
			var xmlForCustomNoteTypes = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CustomNoteTypes><NoteModuleAndCountryList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CustomNoteModuleAndCountry><ModuleIDName>ModuleID2</ModuleIDName><CountryCode>AU</CountryCode><CustomNoteTypesList><CustomNoteTypeItem><IsTextOnly>Y</IsTextOnly><IsAppendingNote>Y</IsAppendingNote><IsReadOnlyAfterAdd>N</IsReadOnlyAfterAdd><ForceRead>Y</ForceRead><DefaultVisibility>PUB</DefaultVisibility><NoteName>NoteType1</NoteName></CustomNoteTypeItem></CustomNoteTypesList></CustomNoteModuleAndCountry></NoteModuleAndCountryList></CustomNoteTypes>";
			var result = Encoding.Unicode.GetBytes(xmlForCustomNoteTypes);
			var item = new BinaryRegistryItem("CustomNotes", null, null, null, RegistryStorageFlags.System);
			var noteTypes = PredefinedNoteTypes.Instance;

			try
			{
				noteTypes.ClearCacheOfAllNotes();
				((IRegistryItemInternals)CustomNotesProvider.Instance.RegistryItem).ClearCache();
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, result);

				var note = CreateNote(BizO);
				note.ST_Description = "NoteType1";

				var clone = note.GetCloneForPopup();
				clone.ST_NoteText = new string('X', 50000);

				note.CopyChangesFromPopup(clone);
				AssertEquals("note.ST_NoteText", note.ST_NoteTextInfo.MaxLength, note.ST_NoteText.Length);
			}
			finally
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
				((IRegistryItemInternals)CustomNotesProvider.Instance.RegistryItem).ClearCache();
			}
		}

		[TestDate(2006, 10, 31, 12, 42, 18)]
		public void TestCopyChangesFromPopup_PopupLogNote()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				StmNote note = CreateNote(BizO);
				string initialsAndDateTime = EnvProxy.Instance.CurrentUser.InitialsAndDateTime;

				note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;

				StmNote clone = note.GetCloneForPopup();
				clone.ST_NoteText = " \r\n xxx \r\n ";
				note.CopyChangesFromPopup(clone);
				AssertEquals("ST_NoteText", initialsAndDateTime + "xxx", note.ST_NoteText);

				clone.ST_NoteText = " \r\n yyy \r\n ";
				note.CopyChangesFromPopup(clone);
				string expectedText = initialsAndDateTime + "yyy\r\n" + new string('-', 125) + "\r\n" + initialsAndDateTime + "xxx";
				AssertEquals("ST_NoteText", expectedText, note.ST_NoteText);
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestReadOnlySecurity()
		{
			bool oldNewNoteValue = EnvProxy.Instance.Security.NotesNew.IsAllowed;
			bool oldNewCustomNoteValue = EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed;
			bool oldEditNoteValue = EnvProxy.Instance.Security.NotesEdit.IsAllowed;
			bool oldDeleteNoteValue = EnvProxy.Instance.Security.NotesDelete.IsAllowed;

			try
			{
				EnvProxy.Instance.Security.NotesNew.IsAllowed = true;
				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = true;
				EnvProxy.Instance.Security.NotesEdit.IsAllowed = true;
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = false;

				StmNote savedNote = CreateNote(BizO);
				Factory.Save();

				StmNote unsavedNote = CreateNote(BizO);
				Assert(!unsavedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(!unsavedNote.ST_NoteTextInfo.ReadOnly);
				Assert(!unsavedNote.ST_DescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_NoteTextInfo.ReadOnly);
				Assert(!savedNote.ST_DescriptionInfo.ReadOnly);

				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = false;
				Assert(unsavedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(!unsavedNote.ST_NoteTextInfo.ReadOnly);
				Assert(!unsavedNote.ST_DescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_NoteTextInfo.ReadOnly);
				Assert(!savedNote.ST_DescriptionInfo.ReadOnly);

				EnvProxy.Instance.Security.NotesNew.IsAllowed = false;
				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = true;
				Assert(unsavedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(unsavedNote.ST_NoteTextInfo.ReadOnly);
				Assert(unsavedNote.ST_DescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_NoteTextInfo.ReadOnly);
				Assert(!savedNote.ST_DescriptionInfo.ReadOnly);

				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = false;
				Assert(unsavedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(unsavedNote.ST_NoteTextInfo.ReadOnly);
				Assert(unsavedNote.ST_DescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(!savedNote.ST_NoteTextInfo.ReadOnly);
				Assert(!savedNote.ST_DescriptionInfo.ReadOnly);

				EnvProxy.Instance.Security.NotesEdit.IsAllowed = false;
				Assert(unsavedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(unsavedNote.ST_NoteTextInfo.ReadOnly);
				Assert(unsavedNote.ST_DescriptionInfo.ReadOnly);
				Assert(savedNote.ST_IsCustomDescriptionInfo.ReadOnly);
				Assert(savedNote.ST_NoteTextInfo.ReadOnly);
				Assert(savedNote.ST_DescriptionInfo.ReadOnly);

				savedNote.Delete();
				Factory.Save();
				Assert(savedNote.IsDeleted);
			}
			finally
			{
				EnvProxy.Instance.Security.NotesNew.IsAllowed = oldNewNoteValue;
				EnvProxy.Instance.Security.NotesNewCustomNote.IsAllowed = oldNewCustomNoteValue;
				EnvProxy.Instance.Security.NotesEdit.IsAllowed = oldEditNoteValue;
				EnvProxy.Instance.Security.NotesDelete.IsAllowed = oldDeleteNoteValue;
			}
		}

		public void TestST_NoteContexValidation()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			{
				StmNote note = CreateNote(BizO);
				note.Validation.ValidateST_NoteContext();
				Assert("ST_NoteContextModuleCaption should accept other than English characters", !note.ST_NoteContextModuleCaptionInfo.HasError("StmNote|ST_NoteContextModuleCaption only accepts Western European languages characters."));
				Assert("ST_NoteContextFreightModeCaption should accept other than English characters", !note.ST_NoteContextFreightModeCaptionInfo.HasError("StmNote|ST_NoteContextFreightModeCaption only accepts Western European languages characters."));
				Assert("ST_NoteContextDirectionCaption should accept other than English characters", !note.ST_NoteContextDirectionCaptionInfo.HasError("StmNote|ST_NoteContextDirectionCaption only accepts Western European languages characters."));
			}
		}

		#region IStmNoteInternals

		public void TestIsNoteRead()
		{
			IStmNoteInternals note = CreateNote(BizO);

			AssertEquals("Note.IsNoteRead is initially false", false, note.IsNoteRead);

			note.IsNoteRead = true;
			AssertEquals("Note.IsNoteRead is set correctly", true, note.IsNoteRead);

			note.IsNoteRead = false;
			AssertEquals("Note.IsNoteRead is set correctly", false, note.IsNoteRead);
		}

		#endregion

		#region TestLocalDates

		[TestUtcOffset(7, 0, 0)]
		public void TestLocalDates()
		{
			StmNote note = CreateNote(BizO);

			AssertEquals(ZDateTime.Empty, note.ST_CreatedDateLocal);
			AssertEquals(ZDateTime.Empty, note.ST_LastModifiedDateLocal);
			AssertEquals(ZDateTime.Empty, note.ST_SystemCreateTimeUtc);
			AssertEquals(ZDateTime.Empty, note.ST_SystemLastEditTimeUtc);

			note.Factory.Save();

			AssertNotEquals(ZDateTime.Empty, note.ST_CreatedDateLocal);
			AssertNotEquals(ZDateTime.Empty, note.ST_LastModifiedDateLocal);
			AssertNotEquals(ZDateTime.Empty, note.ST_SystemCreateTimeUtc);
			AssertNotEquals(ZDateTime.Empty, note.ST_SystemLastEditTimeUtc);
		}

		#endregion

		public void TestReadOnlyForUnmatchedOrgNote()
		{
			DummyBizOWithPredefinedUnmatchedNoteType bizOWithPredefinedUnmatchedNoteType = Factory.New<DummyBizOWithPredefinedUnmatchedNoteType>();

			StmNote note = CreateNote(bizOWithPredefinedUnmatchedNoteType);
			note.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			Assert(note.ReadOnly);

			StmNote anotherNote = CreateNote(BizO); // BizO does not have UnmatchedOrgDetails among PredefinedTypes
			anotherNote.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			Assert(!anotherNote.ReadOnly);
		}

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			DummyBizOWithPredefinedUnmatchedNoteType bizOWithPredefinedUnmatchedNoteType = Factory.New<DummyBizOWithPredefinedUnmatchedNoteType>();

			StmNote note = CreateNote(bizOWithPredefinedUnmatchedNoteType);
			AssertEquals("Note", note.HumanReadableName);

			StmNote anotherNote = CreateNote(BizO); // BizO does not have UnmatchedOrgDetails among PredefinedTypes
			anotherNote.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			AssertEquals("Note - Unmatched Org Details", anotherNote.HumanReadableName);
		}

		#endregion

		#region MaxLength

		public void TestGetCloneForPopup_ShouldCopyMaxLength()
		{
			var note = Factory.New<StmNote>();
			note.NoteTextMaxLength = 50;
			var clone = note.GetCloneForPopup();
			AssertEquals(50, clone.NoteTextMaxLength);
		}

		public void TestNoteTextMaxLength_WhenNotSet_ShouldBeNegativeOne()
		{
			var note = Factory.New<StmNote>();
			AssertEquals(-1, note.NoteTextMaxLength);
		}

		public void TestST_NoteText_MaxLength_ShouldGetValueFromNoteTextMaxLengthProperty()
		{
			var note = Factory.New<StmNote>();
			var property = typeof(StmNote).GetProperty("ST_NoteText_MaxLength", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(property);
			AssertEquals(-1, property.GetValue(note, null));

			note.NoteTextMaxLength = 50;
			AssertEquals(50, property.GetValue(note, null));
		}

		#endregion

		public void TestMultilingualDescriptions()
		{
			var bizO = Factory.New<DummyBizOWithRelatedNotes>();
			StmNote greenNote;
			StmNote whiteNote;

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockCHS = Res.UseMockData())
			{
				mockCHS.Put("green", new ResourceStringData("green", "绿色是唯一的"));
				mockCHS.Put("white", new ResourceStringData("white", "白色是*不*独特"));

				greenNote = CreateNote(bizO);
				whiteNote = CreateNote(bizO);
				greenNote.ST_Description = "绿色是唯一的";
				AssertNoErrors(greenNote.ST_DescriptionInfo);
				whiteNote.ST_Description = "白色是*不*独特";
				AssertNoErrors(whiteNote.ST_DescriptionInfo);
				Factory.Save();

				greenNote.Reload();
				whiteNote.Reload();
				AssertEquals("绿色是唯一的", greenNote.ST_Description);
				AssertEquals("白色是*不*独特", whiteNote.ST_Description);
			}

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.French))
			using (var mockFRN = Res.UseMockData())
			{
				mockFRN.Put("green", new ResourceStringData("green", "vert est unique"));
				mockFRN.Put("white", new ResourceStringData("white", "blanc n'est pas unique"));

				greenNote.Reload();
				whiteNote.Reload();
				AssertEquals("vert est unique", greenNote.ST_Description);
				AssertEquals("blanc n'est pas unique", whiteNote.ST_Description);
			}

			AssertEquals("green is unique", greenNote.ST_Description);
			AssertEquals("white is *not* unique", whiteNote.ST_Description);
		}

		public void TestTextTemplateContextIDIsLanguageNeutral()
		{
			using (var mockCHS = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockCHS.Put("green", new ResourceStringData("green", "绿色是唯一的"));
				var bizO = Factory.New<DummyBizOWithRelatedNotes>();
				var note = CreateNote(bizO);
				note.ST_Description = "green is unique";
				string englishContext = note.GetTextTemplateContextID(note, new KBindingMemberInfo(StmNote.Schema.ST_NoteText));
				AssertContains("green", englishContext);
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("绿色是唯一的", note.ST_Description);
					AssertEquals(englishContext, note.GetTextTemplateContextID(note, new KBindingMemberInfo(StmNote.Schema.ST_NoteText)));
				}
			}
		}

		public void TestGetICustomTextTemplateFallbackContextId()
		{
			var bizO = Factory.New<DummyBizOWithRelatedNotes>();
			var note = CreateNote(bizO);
			note.ST_Description = "green is unique";
			AssertEquals("DummyBizo.StmNote.green is unique", note.GetTextTemplateContextID(note, new KBindingMemberInfo(StmNote.Schema.ST_NoteText)));
			AssertEquals("DummyBizo.StmNote", ((ICustomTextTemplateFallbackContext)note).GetFallbackTextTemplateContextID(note, new KBindingMemberInfo(StmNote.Schema.ST_NoteText)));
			note.ST_IsCustomDescription = true;
			AssertEquals("DummyBizo.StmNote", note.GetTextTemplateContextID(note, new KBindingMemberInfo(StmNote.Schema.ST_NoteText)));
			AssertEquals("DummyBizo.StmNote.green is unique", ((ICustomTextTemplateFallbackContext)note).GetFallbackTextTemplateContextID(note, new KBindingMemberInfo(StmNote.Schema.ST_NoteText)));
		}

		public void TestNoteTypeMadeUneditableLaterDoesntBecomeUnsaveable()
		{
			var bizO = Factory.New<DummyBizOWithRelatedNotes>();
			var note = CreateNote(bizO);
			note.ST_Description = "becomes uneditable later";
			note.ST_IsCustomDescription = true;
			note.ST_NoteDataAsText = "1234";
			AssertNoErrors(note.ST_IsCustomDescriptionInfo);
			Factory.Save();
			Assert("ST_IsCustomDescription is correct", note.ST_IsCustomDescription);
			//now set ST_Description to something it can't be in the DB
			Db.Connection.ExecuteNonQuery(string.Format("update dbo.StmNote set ST_Description = 'purple is not editable and text only', ST_NoteText = '' WHERE ST_PK = '{0}'", note.PK));
			//load in second factory
			var factory2 = new BusinessObjectFactory();
			var bizO2 = factory2.Load<DummyBizOWithRelatedNotes>(bizO.PK);
			var note2 = (StmNote)bizO2.Notes.GetAllNotes().First();
			AssertEquals("description is correct", "purple is not editable and text only", note2.ST_Description);
			Assert("ST_IsCustomDescription has been unset to avoid validation error", !note2.ST_IsCustomDescription);
			Assert(note2.ST_IsTextOnly);
			note2.RunPreSaveValidation();
			AssertEquals("1234", note2.ST_NoteText);
			factory2.Save();
			AssertNoErrors(note2);
			AssertEquals("1234", note2.ST_NoteText);
		}

		public void TestGetCloneForPopupMultilngual()
		{
			using (var mockCHS = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockCHS.Put("green", new ResourceStringData("green", "绿色是唯一的"));
				var bizO = Factory.New<DummyBizOWithRelatedNotes>();
				var note = CreateNote(bizO);
				note.ST_Description = "green is unique";
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("绿色是唯一的", note.ST_Description);
					AssertEquals("green is unique", note.ST_DescriptionInDatabase);
					AssertNoErrors(note.ST_DescriptionInfo);
					var clone = note.GetCloneForPopup();
					AssertEquals("绿色是唯一的", clone.ST_Description);
					AssertEquals("green is unique", clone.ST_DescriptionInDatabase);
					AssertNoErrors(clone.ST_DescriptionInfo);
				}
			}
		}

		public void TestGlbCompanyList()
		{
			StmNote note = CreateNote(BizO);
			foreach (var company in Factory.Load<IGlbCompany>(new ZQuery()))
			{
				Assert(note.GlbCompanyList.Contains(company.PK));
			}
		}

		public void TestSetCaptionsFromTranslatedDescription()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
			{
				var note = CreateNote(BizO);

				note.ST_NoteContextDirectionCaption = ((CodeDescriptionPair)note.ST_NoteContextDirection_List[1]).MultilingualCode;
				AssertEquals(note.ST_NoteContextDirectionCaption, ((CodeDescriptionPair)note.ST_NoteContextDirection_List[1]).MultilingualCode);

				note.ST_NoteContextModuleCaption = ((CodeDescriptionPair)note.ST_NoteContextModule_List[1]).MultilingualCode;
				AssertEquals(note.ST_NoteContextModuleCaption, ((CodeDescriptionPair)note.ST_NoteContextModule_List[1]).MultilingualCode);

				note.ST_NoteContextDirectionCaption = ((CodeDescriptionPair)note.ST_NoteContextDirection_List[1]).MultilingualCode;
				AssertEquals(note.ST_NoteContextDirectionCaption, ((CodeDescriptionPair)note.ST_NoteContextDirection_List[1]).MultilingualCode);

				note.ST_NoteContextFreightModeCaption = ((CodeDescriptionPair)note.ST_NoteContextFreightMode_List[1]).MultilingualCode;
				AssertEquals(note.ST_NoteContextFreightModeCaption, ((CodeDescriptionPair)note.ST_NoteContextFreightMode_List[1]).MultilingualCode);
			}
		}

		public void TestNotesTypeSetToNonEditableShouldBeReadOnlyAfterFirstSave()
		{
			TestPredefinedNoteTypes.Unregister(); // To clear any previous initialization
			TestPredefinedNoteTypes.Register();

			BizO.isSystemNoteForTesting = (x) => x.ST_Description == "GOODBYE WORLD";
			var note1 = CreateNote(BizO);
			note1.ST_Description = TestPredefinedNoteTypes.Instance.ReadOnlyAfterAddNote.Description;
			note1.ST_NoteDataAsText = "xxx";
			var note2 = CreateNote(BizO);
			note2.ST_IsCustomDescription = true;
			note2.ST_Description = "HELLO WORLD";
			note2.ST_NoteDataAsText = "xxx";
			var note3 = CreateNote(BizO);
			note3.ST_IsCustomDescription = true;
			note3.ST_Description = "GOODBYE WORLD";
			note3.ST_NoteDataAsText = "xxx";
			Factory.Save();
			Assert("Should be read only after save for the first time", note1.IsReadOnlyAfterAdd);

			var factory1 = new BusinessObjectFactory();
			var bizO1 = factory1.Load<DummyBizOWithRelatedNotes>(BizO.PK);
			note1 = (StmNote)bizO1.Notes.GetAllNotes().First(x => x.PK == note1.PK);
			AssertEquals("Note1 should still be read only after reload", true, note1.IsReadOnlyAfterAdd);
			AssertEquals("Note2 should still not be read only after reload", false, note2.IsReadOnlyAfterAdd);
			AssertEquals("Note3 should still be read only after reload", true, note3.IsReadOnlyAfterAdd);
		}

		public void TestNotesShouldCreateEventWhenCreatedAgainstAWorkflowProvider()
		{
			var masterType = ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment));
			var shipment = Factory.NewWithValidTestData(masterType);
			CreateNote(shipment as IStmNoteParent);
			Factory.Save();
			var loadedShipment = new BusinessObjectFactory().Load(masterType, shipment.PK);
			var createdLog = ((IStmALogProvider)loadedShipment).Logs.Find(log => log.SL_SE_NKEvent.Equals(AutoEvents.NoteAddedCode)).First();
			AssertNotNull(createdLog);
		}

		public void TestNotesShouldCreateEventWhenDeletedAgainstAWorkflowProvider()
		{
			var masterType = ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment));
			var shipment = Factory.NewWithValidTestData(masterType);
			var note = CreateNote(shipment as IStmNoteParent);
			Factory.Save();
			shipment.GetNotes().FindByPK(note.PK).Delete();
			Factory.Save();
			var loadedShipment = new BusinessObjectFactory().Load(masterType, shipment.PK);
			var createdLog = ((IStmALogProvider)loadedShipment).Logs.Find(log => log.SL_SE_NKEvent.Equals(AutoEvents.NoteDeletedCode)).First();
			AssertNotNull(createdLog);
		}

		public void TestNotesShouldCreateEventWhenModifiedAgainstAWorkflowProvider()
		{
			var masterType = ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment));
			var shipment = Factory.NewWithValidTestData(masterType);
			var note = CreateNote(shipment as IStmNoteParent);
			Factory.Save();
			shipment.GetNotes().FindByPK(note.PK).ST_Description = "new description";
			Factory.Save();
			var loadedShipment = new BusinessObjectFactory().Load(masterType, shipment.PK);
			var createdLog = ((IStmALogProvider)loadedShipment).Logs.Find(log => log.SL_SE_NKEvent.Equals(AutoEvents.NoteModifiedCode)).First();
			AssertNotNull(createdLog);
		}

		public void TestNoteShouldNotCreateEventWhenAddAndDeletePerformedBeforeSave()
		{
			var masterType = ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment));
			var shipment = Factory.NewWithValidTestData(masterType);
			var note = CreateNote(shipment as IStmNoteParent);
			note.Delete();
			Factory.Save();
			var loadedShipment = new BusinessObjectFactory().Load(masterType, shipment.PK);
			var createdLogs = ((IStmALogProvider)loadedShipment).Logs.Find(log => log.SL_SE_NKEvent.Equals(AutoEvents.NoteAddedCode) || log.SL_SE_NKEvent.Equals(AutoEvents.NoteDeletedCode)).Count();
			AssertEquals(0, createdLogs);
		}

		public void TestNoteShouldNotCreateModifiedEventWhenAddAndModifyPerformedBeforeSave()
		{
			var masterType = ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment));
			var shipment = Factory.NewWithValidTestData(masterType);
			var note = CreateNote(shipment as IStmNoteParent);
			note.ST_Description = "must make a note of this";
			Factory.Save();
			var loadedShipment = new BusinessObjectFactory().Load(masterType, shipment.PK);
			var stmALogs = ((IStmALogProvider)loadedShipment).Logs.Find(log => log.SL_SE_NKEvent.Equals(AutoEvents.NoteAddedCode) || log.SL_SE_NKEvent.Equals(AutoEvents.NoteModifiedCode)).ToList();
			AssertEquals(1, stmALogs.Count);
			AssertEquals(AutoEvents.NoteAddedCode, stmALogs.First().SL_SE_NKEvent);
		}

		public void TestNoteShouldNotEventWhenParentIsNotAWorkflowProvider()
		{
			var masterType = ObjectFactory.GetType(typeof(IEDIMessage));
			var message = Factory.NewWithValidTestData(masterType);
			var note = CreateNote(message as IStmNoteParent);
			note.ST_Description = "must make a note of this";
			Factory.Save();
			var loadedShipment = new BusinessObjectFactory().Load(masterType, message.PK);
			var stmALogs = ((IStmALogProvider)loadedShipment).Logs.Find(log => log.SL_SE_NKEvent.Equals(AutoEvents.NoteAddedCode) || log.SL_SE_NKEvent.Equals(AutoEvents.NoteModifiedCode)).ToList();
			AssertEquals(0, stmALogs.Count);
		}

		[ExpectNoExceptions]
		public void TestNoteTextAndNoteDataUpdateWhenTextOnlyChanges()
		{
			var masterType = ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment));
			var shipment = Factory.NewWithValidTestData(masterType);
			var note = CreateNote(shipment as IStmNoteParent);

			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_NoteText = "Hello World.";
			note.RunPreSaveValidation();
			Factory.Save();
			var note2 = new BusinessObjectFactory().LoadTop1<StmNote>(
				new ZQuery()
				.AddToFilter(StmNoteSchema.PK, note.PK)
				.AddToFilter(StmNoteSchema.ST_NoteText, SQLComparisonOperator.NotEqual, ZString.Empty)
				.AddToFilter(StmNoteSchema.ST_NoteData, DBNull.Value)
			);
			AssertEquals("ST_NoteText should not be empty, ST_NoteData should be empty, ST_IsTextOnly should be true", true, note2.ST_IsTextOnly);

			note.ST_Description = PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description;
			note.RunPreSaveValidation();
			Factory.Save();
			var note3 = new BusinessObjectFactory().LoadTop1<StmNote>(
				new ZQuery()
				.AddToFilter(StmNoteSchema.PK, note.PK)
				.AddToFilter(StmNoteSchema.ST_NoteText, ZString.Empty)
				.AddToFilter(StmNoteSchema.ST_NoteData, SQLComparisonOperator.NotEqual, DBNull.Value)
			);
			AssertEquals("ST_NoteText should be empty, ST_NoteData should not be empty, ST_IsTextOnly should be false", false, note3.ST_IsTextOnly);

			note.ST_NoteData = ORtfTextUtil.TextToRtfBytes("Hello World.Hello World.");
			note.RunPreSaveValidation();
			Factory.Save();
			var note4 = new BusinessObjectFactory().LoadTop1<StmNote>(
				new ZQuery()
				.AddToFilter(StmNoteSchema.PK, note.PK)
				.AddToFilter(StmNoteSchema.ST_NoteText, ZString.Empty)
				.AddToFilter(StmNoteSchema.ST_NoteData, SQLComparisonOperator.NotEqual, DBNull.Value)
			);
			AssertEquals("ST_NoteText should be empty, ST_NoteData should not be empty, ST_IsTextOnly should be false", false, note4.ST_IsTextOnly);

			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.RunPreSaveValidation();
			Factory.Save();
			var note5 = new BusinessObjectFactory().LoadTop1<StmNote>(
				new ZQuery()
				.AddToFilter(StmNoteSchema.PK, note.PK)
				.AddToFilter(StmNoteSchema.ST_NoteText, SQLComparisonOperator.NotEqual, ZString.Empty)
				.AddToFilter(StmNoteSchema.ST_NoteData, DBNull.Value)
			);
			AssertEquals("ST_NoteText should not be empty, ST_NoteData should be empty, ST_IsTextOnly should be true", true, note5.ST_IsTextOnly);
		}

		public void TestHtmlProperty()
		{
			var note = Factory.New<StmNote>();
			AssertEquals(ZBlob.Empty, note.ST_NoteData);
			AssertEquals(ZBlob.Empty, note.ST_NoteData_HTML);

			note.ST_NoteData_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(note.ST_NoteData.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", note.ST_NoteData_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var note = Factory.New<StmNote>();
			AssertEquals(ZBlob.Empty, note.ST_NoteData);
			AssertEquals(ZBlob.Empty, note.ST_NoteData_HTML);

			note.ST_NoteData = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", note.ST_NoteData_HTML.ToUTF8());

			note.ST_NoteData = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", note.ST_NoteData_HTML.ToUTF8());
		}

		#region Implementation

		DummyBizOWithRelatedNotes BizO;

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateNote(BizO);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BizO = Factory.New<DummyBizOWithRelatedNotes>();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (StmNote)Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			result.ST_Table = "DummyBizO";
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var stmNote = base.GetNewBusinessObjectForDeleteTest(factory) as StmNote;
			stmNote.ST_Table = "GlbStaff";

			return stmNote;
		}

		StmNote CreateNote(IStmNoteParent parent)
		{
			StmNote note = Factory.New<StmNote>();

			note.ST_ParentID = parent.NotesParentPK;
			note.ST_Table = parent.NotesParentTableName;
			parent.Notes.Add(note);

			return note;
		}

		#endregion
	}
}
