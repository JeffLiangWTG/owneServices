using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteValidationTest : BusinessObjectValidationTestCase //a
	{
		public void TestST_IsCustomDescription()
		{
			var mockMaster = Factory.NewMoq<DummyEnterpriseBusinessObject>();

			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes);

			mockMaster.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			var note1 = mockMaster.Object.Notes.AddNew();

			note1.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note1.ST_IsCustomDescriptionInfo);

			note1.ST_IsCustomDescription = true;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertHasError(note1.ST_IsCustomDescriptionInfo,
				string.Format("{0} is not a custom note. Please uncheck 'Custom Desc.' for the {0} note.", PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description));

			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			note1.Validation.ValidateST_IsCustomDescription();
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note1.ST_IsCustomDescriptionInfo);

			note1.Factory.Save();

			noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);

			note1.Validation.ValidateAll();
			AssertEquals(true, note1.ST_IsCustomDescription);
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note1.ST_IsCustomDescriptionInfo);
			AssertHasWarning(note1.ST_IsCustomDescriptionInfo,
				string.Format("{0} is not a custom note. Please uncheck 'Custom Desc.' for the {0} note.", PredefinedNoteTypes.Instance.AgentNotes.Description));
		}

		public void TestValidatePredefinedDescriptionIsUnique()
		{
			var mockMaster = Factory.NewMoq<DummyEnterpriseBusinessObject>();

			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);

			mockMaster.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			var note1 = mockMaster.Object.Notes.AddNew();
			var note2 = mockMaster.Object.Notes.AddNew();

			note1.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note2.ST_DescriptionInfo);

			note2.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertHasError(note2.ST_DescriptionInfo, "There is already another note with the description 'A/R Account Management Notes' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");

			note2.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note2.ST_DescriptionInfo);

			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			AssertHasError(note1.ST_DescriptionInfo, "There is already another note with the description 'Agent Notes' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");
			AssertNoErrors(note2.ST_DescriptionInfo);

			note1.ST_NoteContext = "GAH";
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note2.ST_DescriptionInfo);

			mockMaster.VerifyAll();
		}

		public void TestSkipValidationForReadonlyNotesWithPredefinedDescription()
		{
			var mockMaster = Factory.NewMoq<DummyEnterpriseBusinessObject>();
			var noteType = PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes;

			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(noteType);

			mockMaster.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			var note1 = mockMaster.Object.Notes.AddNew();
			var note2 = mockMaster.Object.Notes.AddNew();

			Assert("Pre-condition: note type is readonly after add", noteType.IsReadOnlyAfterAdd);

			note1.ReadOnly = false;
			note1.ST_Description = noteType.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note2.ST_DescriptionInfo);

			note2.ReadOnly = false;
			note2.ST_Description = noteType.Description;
			note1.Validation.ValidateAll();
			AssertHasError(note1.ST_DescriptionInfo, "There is already another note with the description 'Unrecognized Additional Reference Types' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");
			AssertHasError(note2.ST_DescriptionInfo, "There is already another note with the description 'Unrecognized Additional Reference Types' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");

			note2.ReadOnly = true;
			note1.Validation.ValidateAll();
			note2.Validation.ValidateAll();
			AssertHasError(note1.ST_DescriptionInfo, "There is already another note with the description 'Unrecognized Additional Reference Types' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");
			AssertHasError(note2.ST_DescriptionInfo, "There is already another note with the description 'Unrecognized Additional Reference Types' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");

			note1.ReadOnly = true;
			note1.Validation.ValidateAll();
			note2.Validation.ValidateAll();
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note2.ST_DescriptionInfo);

			mockMaster.VerifyAll();
		}

		public void TestValidatePredefinedDescriptionIsUniqueWhenEdittingSimultaneously()
		{
			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes);

			var parent = Factory.NewMoq<DummyEnterpriseBusinessObject>();
			parent.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			Factory.Save();
			Factory.RefreshEnabled = false;

			var note1 = parent.Object.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var parentInFactory2 = factory2.LoadMoq<DummyEnterpriseBusinessObject>(parent.Object.PK);
			parent.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			var note2 = parentInFactory2.Object.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;
			AssertNoErrors(note2.ST_DescriptionInfo);

			Factory.Save();
			note2.RunPreSaveValidation();
			AssertHasError(note2.ST_DescriptionInfo, "There is already another note with the description 'A/R Account Management Notes' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");

			parent.VerifyAll();
		}

		public void TestValidatePredefinedDescriptionIsUniqueWhenEdittingSimultaneously_DeletedNote()
		{
			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes);

			var parent = Factory.NewMoq<DummyEnterpriseBusinessObject>();
			parent.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			Factory.Save();
			Factory.RefreshEnabled = false;

			var note1 = parent.Object.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var parentInFactory2 = factory2.LoadMoq<DummyEnterpriseBusinessObject>(parent.Object.PK);
			parentInFactory2.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			var note1InFactory2 = factory2.Load<StmNote>(note1.PK);
			note1InFactory2.Delete();
			var note2 = parentInFactory2.Object.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;
			AssertNoErrors(note2.ST_DescriptionInfo);

			note2.RunPreSaveValidation();
			AssertNoErrors(note2.ST_DescriptionInfo);

			parent.VerifyAll();
			parentInFactory2.VerifyAll();
		}

		public void TestValidatePredefinedDescriptionIsUniqueChecksAutoratingLogPerCompany()
		{
			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);

			var parent = Factory.NewMoq<DummyEnterpriseBusinessObject>();
			parent.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			Factory.Save();
			Factory.RefreshEnabled = false;

			var autoratingNote1 = parent.Object.Notes.AddNew();
			autoratingNote1.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
			autoratingNote1.ST_GC_RelatedCompany = EnvProxy.Instance.CurrentCompany.PK;

			AssertNoErrors(autoratingNote1.ST_DescriptionInfo);
			Factory.Save();

			var autoratingNote2 = parent.Object.Notes.AddNew();
			autoratingNote2.ST_GC_RelatedCompany = EnvProxy.Instance.CurrentCompany.PK;
			autoratingNote2.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;

			AssertHasErrors("Should not allow this note for different Companies", autoratingNote2.ST_DescriptionInfo);

			var anotherCompany = (BusinessObject)Factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, EnvProxy.Instance.CurrentCompany.PK));
			AssertNotNull("Precondition: should find another company", anotherCompany);

			autoratingNote2.ST_GC_RelatedCompany = anotherCompany.PK;
			autoratingNote2.RunPreSaveValidation();

			AssertNoErrors(autoratingNote2.ST_DescriptionInfo);

			parent.VerifyAll();
		}

		public void TestDescriptionIsAutoRatingAuditLogWarning()
		{
			var noteType = PredefinedNoteTypes.Instance.AutoRatingAuditLog;
			var mockNoteMaster = Factory.NewMoq<DummyEnterpriseBusinessObject>();
			var noteTypes = new NoteTypeCollection { noteType };
			mockNoteMaster.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			var note = mockNoteMaster.Object.Notes.AddNew();
			note.ST_Description = noteType.Description;

			AssertHasWarning(note.ST_DescriptionInfo, "It is not recommended to manually enter AutoRating Log Notes because it will be recreated and overridden on next AutoRating session.");
			note.Delete();

			using (IMockResourceStringCache mockLanguageData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.German).UseMockData())
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
			{
				var resKey = ((ResourceString)noteType.MultilingualDescription).ResourceKey;
				mockLanguageData.Put(resKey, new ResourceStringData(resKey, "testing123"));

				var foreignNote = mockNoteMaster.Object.Notes.AddNew();
				foreignNote.ST_Description = "testing123";

				AssertEquals(noteType.Description, foreignNote.ST_Description);
				AssertEquals(noteType.MultilingualDescription.GetUnresolvedString(), foreignNote.ST_DescriptionInDatabase);
				AssertHasWarning(foreignNote.ST_DescriptionInfo, "It is not recommended to manually enter AutoRating Log Notes because it will be recreated and overridden on next AutoRating session.");

				foreignNote.ReadOnly = true;
				foreignNote.Validation.ValidateST_Description();

				AssertNoWarnings(foreignNote.ST_DescriptionInfo);
			}

			mockNoteMaster.VerifyAll();
		}

		public void TestEmptyNoteWillCauseErrorOrWarning()
		{
			var noteNotInDatabase = Factory.New<StmNote>();
			noteNotInDatabase.ST_Table = "DummyBizo";
			noteNotInDatabase.ST_NoteData = ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs20\par}");
			noteNotInDatabase.Validation.ValidateST_NoteData();
			Assert("Precondition", !noteNotInDatabase.IsInDatabase);
			AssertHasError(noteNotInDatabase.ST_NoteDataInfo, "A note without a description also has no note details.");

			var noteInDatabase = Factory.New<StmNote>();
			noteInDatabase.ST_Table = "DummyBizo";
			noteInDatabase.ST_NoteData = ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs20\par}");
			Factory.Save();
			noteInDatabase.Validation.ValidateST_NoteData();
			Assert("Precondition", noteInDatabase.IsInDatabase);
			AssertHasWarning(noteInDatabase.ST_NoteDataInfo, "A note without a description also has no note details.");

			var noteWithNotNullOrginalValue = Factory.New<StmNote>();
			noteWithNotNullOrginalValue.ST_Table = "DummyBizo";
			noteWithNotNullOrginalValue.ST_NoteData = ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs20 Some Random Text\par}");
			Factory.Save();
			noteWithNotNullOrginalValue.Validation.ValidateST_NoteData();
			Assert("Precondition", noteWithNotNullOrginalValue.IsInDatabase);
			noteWithNotNullOrginalValue.ST_NoteData = ZBlob.Empty;
			AssertHasError(noteWithNotNullOrginalValue.ST_NoteDataInfo, "A note without a description also has no note details.");
		}

		public void TestOldNotesWhichContainFileDonotCauseAnyWarning()
		{
			var oldNoteInDatabaseAndContainFile = Factory.New<StmNote>();
			oldNoteInDatabaseAndContainFile.ST_Table = "DummyBizo";
			oldNoteInDatabaseAndContainFile.ST_NoteData = ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs17{\object\objemb{\*\objclass Package}\objw1440\objh1125{\*\objdata \par}}}");
			Factory.Save();
			oldNoteInDatabaseAndContainFile.Validation.ValidateST_NoteData();
			Assert("Precondition", oldNoteInDatabaseAndContainFile.IsInDatabase);
			AssertEquals(false, oldNoteInDatabaseAndContainFile.ST_NoteDataInfo.HasWarning("A note without a description also has no note details"));
		}

		public void TestCheckST_NoteDataIsValidZBlobSize()
		{
			const string sizeValidationErrorMessage =
				"This note is too large to store in the database. Reduce its size by removing any large images or files.\r\n\r\n   - Current Size:  1025 KB\r\n   - Maximum Size:  1024 KB\r\n\r\n";

			var note = Factory.New<StmNote>();
			note.ST_Table = "DummyBizo";
			note.ST_NoteData = new ZBlob(new byte[1025 * 1024]);
			note.Validation.ValidateST_NoteData();
			AssertHasError(note.ST_NoteDataInfo, sizeValidationErrorMessage);

			note.ST_NoteData_ReadOnly = true;
			note.Validation.ValidateST_NoteData();
			AssertHasError("Size validation for new object", note.ST_NoteDataInfo, sizeValidationErrorMessage);

			Factory.Save();
			note.Validation.ValidateST_NoteData();
			AssertNoErrors("No size validation for readonly field of saved object", note.ST_NoteDataInfo);

			note.ST_NoteData_ReadOnly = false;
			note.Validation.ValidateST_NoteData();
			AssertHasError("Size validation for non-readonly field", note.ST_NoteDataInfo, sizeValidationErrorMessage);

			note.ST_Description = PredefinedNoteTypes.Instance.MessageInterpretation.Description;
			note.Validation.ValidateST_NoteData();
			AssertNoError("Size validation for non-readonly field", note.ST_NoteDataInfo, sizeValidationErrorMessage);
		}

		public void TestValidatePredefinedDescriptionWithSpace()
		{
			var mockMaster = Factory.NewMoq<DummyEnterpriseBusinessObject>();

			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes);

			mockMaster.Protected()
				.Setup<NoteTypeCollection>("NoteTypesCore")
				.Returns(noteTypes);

			var note = mockMaster.Object.Notes.AddNew();

			note.ST_Description = " " + PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description;

			AssertNoErrors(note.ST_DescriptionInfo);

			mockMaster.VerifyAll();
		}

		public void TestValidatePredefinedDescriptionWithNonEnglishLanguage()
		{
			using (var grmMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Russian).UseMockData())
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.Russian))
			{
				var resKey = ((ResourceString)PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.MultilingualDescription).ResourceKey;
				grmMockData.Put(resKey, new ResourceStringData(resKey, "testing123"));

				var mockMaster = Factory.NewMoq<DummyEnterpriseBusinessObject>();

				var noteTypes = new NoteTypeCollection();
				noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes);

				mockMaster.Protected()
					.Setup<NoteTypeCollection>("NoteTypesCore")
					.Returns(noteTypes);

				var note = mockMaster.Object.Notes.AddNew();
				note.ST_Description = "testing123";

				AssertEquals(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.Description, note.ST_Description);
				AssertEquals(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes.MultilingualDescription.GetUnresolvedString(), note.ST_DescriptionInDatabase);
				AssertNoErrors(note.ST_DescriptionInfo);

				var note2 = mockMaster.Object.Notes.AddNew();
				note2.ST_Description = "testing123";
				AssertHasError(note2.ST_DescriptionInfo, "There is already another note with the description 'testing123' for Context Module '" + note2.ST_NoteContextModuleCaption.Substring(3) + "', Direction '" + note2.ST_NoteContextDirectionCaption.Substring(3) + "', Freight Mode '" + note2.ST_NoteContextFreightModeCaption.Substring(3) + "'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");

				mockMaster.VerifyAll();
			}
		}

		public void TestValidatePredefinedDescriptionOrderUpdateHistory()
		{
			var shipment = Factory.New<ICommonShipment>();

			var note = Factory.NewWithValidTestData<StmNote>();
			note.Master = (IStmNoteParent)shipment;

			note.ST_Description = PredefinedNoteTypes.Instance.OrderUpdateHistory.Description;

			AssertHasError(note.ST_DescriptionInfo, "A note type with the description \"Order Update History\" is not a valid note type.");
		}
	}
}
