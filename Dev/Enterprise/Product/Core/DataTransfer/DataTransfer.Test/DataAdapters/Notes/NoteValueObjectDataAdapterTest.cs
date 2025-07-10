using System;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(NoteValueObjectDataAdapter))]
	sealed class NoteValueObjectDataAdapterTest : ValueObjectDataAdapterTest<StmNote, Xsd.NotesNote>
	{
		public void TestExportPredefinedDescriptionRtfNote()
		{
			StmNote populatedNote = Factory.New<StmNote>();
			populatedNote.ST_Description = PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description;
			populatedNote.ST_NoteDataAsText = "Predefined Description Note";
			populatedNote.ST_Table = "DummyBizo";

			Xsd.NotesNote xsdNote = new Xsd.NotesNote();
			Adapter.ExportToValueObject(populatedNote, xsdNote, new ValueObjectExportContext(Notify));
			AssertEquals("There should not be errors", false, Notify.HasErrors);

			AssertEquals("Note Type", Xsd.NotesNoteNoteType.ClientVisibleJobNotes, xsdNote.NoteType);
			AssertEquals("Note Description", ZString.Empty, xsdNote.CustomNoteTypeName);
			AssertEquals("Note Data", "Predefined Description Note", xsdNote.NoteData);
		}

		public void TestExportPredefinedDescriptionTextNote()
		{
			StmNote populatedNote = Factory.New<StmNote>();
			populatedNote.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			populatedNote.ST_NoteDataAsText = "Predefined Description Note";
			populatedNote.ST_Table = "DummyBizo";
			Xsd.NotesNote xsdNote = new Xsd.NotesNote();
			Adapter.ExportToValueObject(populatedNote, xsdNote, new ValueObjectExportContext(Notify));
			AssertEquals("There should not be errors", false, Notify.HasErrors);

			AssertEquals("Note Type", Xsd.NotesNoteNoteType.DetailedGoodsDescription, xsdNote.NoteType);
			AssertEquals("Note Description", ZString.Empty, xsdNote.CustomNoteTypeName);
			AssertEquals("Note Data", "Predefined Description Note", xsdNote.NoteData);
		}

		public void TestExportEmptyNote()
		{
			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);
			StmNote note1 = bizObjNotes.AddNew();
			note1.ST_NoteType = "PUB";

			Xsd.NotesNoteCollection xsdNotes = Adapter.ExportToXmlValueObjectCollection(bizObjNotes, new ValueObjectExportContext(Notify));

			AssertNotNull("should return an empty collection if notes is empty", xsdNotes);
			AssertEquals("Notes count", 0, xsdNotes.Count);
		}

		public void TestExportCustomDescriptionNote()
		{
			StmNote populatedNote = Factory.New<StmNote>();
			populatedNote.ST_IsCustomDescription = true;
			populatedNote.ST_Description = "Custom Description";
			populatedNote.ST_NoteDataAsText = "Custom Description Note";
			populatedNote.ST_Table = "DummyBizo";

			Xsd.NotesNote xsdNote = new Xsd.NotesNote();
			Adapter.ExportToValueObject(populatedNote, xsdNote, new ValueObjectExportContext(Notify));
			AssertEquals("There should not be errors", false, Notify.HasErrors);

			AssertEquals("Note Type", Xsd.NotesNoteNoteType.Custom, xsdNote.NoteType);
			AssertEquals("Note Description", "Custom Description", xsdNote.CustomNoteTypeName);
			AssertEquals("Note Data", "Custom Description Note", xsdNote.NoteData);
		}

		public void TestExportCustomDescriptionNoteWithFalseFlag()
		{
			StmNote populatedNote = Factory.New<StmNote>();
			OrgHeader org = Factory.New<OrgHeader>();
			populatedNote.Master = org;

			PredefinedNoteType newtype = new PredefinedNoteType((NoResString)"Custom Description", StmNoteVisibility.PUB, true, false, true, false);

			CustomNoteTypes noteTypes = new CustomNoteTypes();

			CustomNoteModuleAndCountry module = noteTypes.NoteModuleAndCountryList.AddNew();
			module.ModuleIDName = "ModuleID1";
			module.CountryCode = "UA";

			CustomNoteTypeItem item = module.CustomNoteTypesList.AddNew();
			item.NoteName = "Custom Description";
			item.DefaultVisibility = "PUB";

			CustomNotesProvider.Instance.AllCustomNoteTypes.Add(newtype);
			PredefinedNoteTypes.Instance.ClearCacheOfAllNotes();
			CustomNotesProvider.Instance.RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, noteTypes);

			populatedNote.ST_Description = "Custom Description";
			populatedNote.ST_NoteDataAsText = "Custom Description Note";
			Xsd.NotesNote xsdNote = new Xsd.NotesNote();
			Adapter.ExportToValueObject(populatedNote, xsdNote, new ValueObjectExportContext(Notify));
			AssertEquals("There should not be errors", false, Notify.HasErrors);

			AssertEquals("Note Type", Xsd.NotesNoteNoteType.Custom, xsdNote.NoteType);
			AssertEquals("Note Description", "Custom Description", xsdNote.CustomNoteTypeName);
			AssertEquals("Note Data", "Custom Description Note", xsdNote.NoteData);
		}

		public void TestExportToValueObjectCore_NoteCreatedDateTimeSpecified()
		{
			StmNote populatedNote = Factory.New<StmNote>();
			populatedNote.ST_Description = "TheNoteOfEternalSupriseAndFloundering";
			populatedNote.ST_NoteDataAsText = "String Of Data somehow relating to this test";
			populatedNote.ST_Table = "DummyBizo";
			populatedNote.Factory.Save();

			Assert("PreCondition: Note Created Date should be valid", populatedNote.ST_CreatedDateUtc.IsValid);

			var createdTimeStamp = populatedNote.ST_CreatedDateUtc;
			Xsd.NotesNote note = new Xsd.NotesNote();

			Adapter.ExportToValueObject(populatedNote, note, new ValueObjectExportContext(Notify));
			AssertEquals(true, note.NoteCreatedDateTime.IsValid);
			AssertEquals(createdTimeStamp, note.NoteCreatedDateTime);
		}

		public void TestExportToValueObjectCore_NoteCreatedDateTimeIfEmpty()
		{
			StmNote populatedNote = Factory.New<StmNote>();
			populatedNote.ST_Description = "TheNoteOfEternalSupriseAndFloundering";
			populatedNote.ST_NoteDataAsText = "String Of Data somehow relating to this test";
			populatedNote.ST_Table = "DummyBizo";

			Assert("PreCondition: Note Created Date should not be valid", !populatedNote.ST_CreatedDateUtc.IsValid);

			Xsd.NotesNote note = new Xsd.NotesNote();

			Adapter.ExportToValueObject(populatedNote, note, new ValueObjectExportContext(Notify));
			AssertEquals(false, note.NoteCreatedDateTime.IsValid);
		}

		public void TestExportNotesAttachedToBusinessObjectNotes_NotPublic()
		{
			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);
			StmNote note1 = bizObjNotes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.TranshipmentNotes.Description;
			note1.ST_NoteDataAsText = "hello hello";

			Xsd.NotesNoteCollection xsdNotes = Adapter.ExportToXmlValueObjectCollection(bizObjNotes, new ValueObjectExportContext(Notify));

			AssertNotNull("should return an empty collection (not null) if notes are private only", xsdNotes);
			AssertEquals("Notes count", 0, xsdNotes.Count);
		}

		public void TestExportNotesAttachedToBusinessObjectNotes_Public()
		{
			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);
			StmNote note1 = bizObjNotes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.TranshipmentNotes.Description;
			note1.ST_NoteDataAsText = "hello hello";
			note1.ST_NoteType = "PUB";

			Xsd.NotesNoteCollection xsdNotes = Adapter.ExportToXmlValueObjectCollection(bizObjNotes, new ValueObjectExportContext(Notify));
			AssertEquals(1, xsdNotes.Count);
		}

		public void TestExportToXmlValueObjectCollectionDoesNotReturnNull()
		{
			StmNote parent = Factory.New<StmNote>();
			Xsd.NotesNoteCollection notes = Adapter.ExportToXmlValueObjectCollection(new Notes(parent), new ValueObjectExportContext(Notify));

			AssertNotNull("XsdNotes should just be an empty collection, not null", notes);
			AssertEquals("Count of XsdNotes collection", 0, notes.Count);
		}

		public void TestImportNotesAndAttachToBusinessObjectNotes_ExistingBussinessObjects()
		{
			string oldNoteText = "hahaha";
			Notes businessObjectNotes = new Notes(Factory.New<StmNote>());
			StmNote note = businessObjectNotes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note.ST_NoteDataAsText = oldNoteText;

			Xsd.Notes xsdNotes = new Xsd.Notes();
			Xsd.NotesNote xsdNote = xsdNotes.Note.AddNew();
			xsdNote.NoteType = Xsd.NotesNoteNoteType.DetailedGoodsDescription;
			xsdNote.NoteData = "the new note data";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(businessObjectNotes, xsdNotes.Note, context);

			Assert(((StmNoteCollection)businessObjectNotes.GetAllNotes())[0].ST_NoteText != oldNoteText);
		}

		public void TestImportNotesWithCustomDescriptionAndAttachToBusinessObjectNotes_ExistingBussinessObjects()
		{
			string oldNoteText = "hahaha";
			Notes businessObjectNotes = new Notes(Factory.New<StmNote>());
			StmNote note = businessObjectNotes.AddNew();
			note.ST_Description = "YEAH";
			note.ST_NoteDataAsText = oldNoteText;

			Xsd.Notes xsdNotes = new Xsd.Notes();
			Xsd.NotesNote xsdNote = xsdNotes.Note.AddNew();
			xsdNote.CustomNoteTypeName = "YEAH";
			xsdNote.NoteData = "the new note data";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(businessObjectNotes, xsdNotes.Note, context);

			Assert(((StmNoteCollection)businessObjectNotes.GetAllNotes())[0].ST_NoteDataAsText != oldNoteText);
		}

		public void TestImportNotesWithCustomDescription()
		{
			string oldNoteText = "hahaha";
			Notes businessObjectNotes = new Notes(Factory.New<StmNote>());
			StmNote note = businessObjectNotes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note.ST_NoteDataAsText = oldNoteText;

			Xsd.Notes xsdNotes = new Xsd.Notes();
			Xsd.NotesNote xsdNote = xsdNotes.Note.AddNew();
			xsdNote.NoteType = Xsd.NotesNoteNoteType.DetailedGoodsDescription;
			xsdNote.CustomNoteTypeName = "YEAH";
			xsdNote.NoteData = "the new note data";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(businessObjectNotes, xsdNotes.Note, context);
			AssertEquals("Should match to the existing non custom note", 1, businessObjectNotes.GetAllNotes().Count);
			AssertEquals("Should have updated the note text", xsdNote.NoteData, ((StmNoteCollection)businessObjectNotes.GetAllNotes())[0].ST_NoteDataAsText);

			xsdNote.NoteType = Xsd.NotesNoteNoteType.Custom;
			xsdNote.CustomNoteTypeName = "YEAH";
			xsdNote.NoteData = "the new note data";

			Adapter.ImportNotesAndAttachToBusinessObjectNotes(businessObjectNotes, xsdNotes.Note, context);
			AssertEquals("Should create a new custom note", 2, businessObjectNotes.GetAllNotes().Count);
			AssertEquals(xsdNote.NoteData, ((StmNoteCollection)businessObjectNotes.GetAllNotes())[1].ST_NoteDataAsText);

			businessObjectNotes.RemoveAndDeleteAll();
			note = businessObjectNotes.AddNew();
			note.ST_Description = "YEAH";
			note.ST_NoteDataAsText = oldNoteText;

			xsdNote.NoteType = Xsd.NotesNoteNoteType.Custom;
			xsdNote.CustomNoteTypeName = "YEAH";
			xsdNote.NoteData = "the new note data";

			Adapter.ImportNotesAndAttachToBusinessObjectNotes(businessObjectNotes, xsdNotes.Note, context);
			AssertEquals("Should match to the existing custom note", 1, businessObjectNotes.GetAllNotes().Count);
			AssertEquals(xsdNote.NoteData, ((StmNoteCollection)businessObjectNotes.GetAllNotes())[0].ST_NoteDataAsText);
		}

		public void TestImportNotesAndAttachToBusinessObjectNotes_XsdNotesIsNull()
		{
			Xsd.NotesNoteCollection xsdNotes = null;

			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(bizObjNotes, xsdNotes, context);

			AssertEquals(0, bizObjNotes.DatabaseCount);
		}

		public void TestImportNotesAndAttachToBusinessObjectNotes_XsdNotesIsEmpty()
		{
			Xsd.NotesNoteCollection xsdNotes = new Xsd.NotesNoteCollection();
			Xsd.NotesNote note = xsdNotes.AddNew();
			note.NoteType = Xsd.NotesNoteNoteType.Custom;
			note.CustomNoteTypeName = "Consignee's Reference";
			note.NoteData = String.Empty;
			note.NoteCreatedDateTime = ZDateTime.UtcNow;

			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(bizObjNotes, xsdNotes, context);

			AssertEquals(0, bizObjNotes.DatabaseCount);
		}

		public void TestImportNotesAndAttachToBusinessObjectNotes_IsPublicallyVisible()
		{
			Xsd.NotesNoteCollection xsdNotes = new Xsd.NotesNoteCollection();
			Xsd.NotesNote note1 = xsdNotes.AddNew();
			note1.NoteData = "123";
			note1.NoteType = Xsd.NotesNoteNoteType.BookingNotes;

			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(bizObjNotes, xsdNotes, context);

			AssertEquals("PUB", (((StmNoteCollection)bizObjNotes.GetAllNotes())[0].ST_NoteType));
		}

		public void TestImportNotesAndAttachToBusinessObjectNotes_SameNoteExists()
		{
			Xsd.NotesNoteCollection notes = new Xsd.NotesNoteCollection();
			Xsd.NotesNote note = notes.AddNew();
			note.NoteType = Xsd.NotesNoteNoteType.BookingNotes;
			note.NoteData = "That Was a Popin' Summer!!";

			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);
			StmNote note1 = bizObjNotes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Description;
			note1.ST_NoteDataAsText = "That Was a Popin' Summer!!";
			note1.ST_NoteType = "PUB";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(bizObjNotes, notes, context);

			AssertEquals(1, bizObjNotes.GetAllNotes().Count);
		}

		public void TestImportNotesAndAttachToBusinessObjectNotes_NotSameNote()
		{
			Xsd.NotesNoteCollection notes = new Xsd.NotesNoteCollection();
			Xsd.NotesNote note = notes.AddNew();
			note.NoteType = Xsd.NotesNoteNoteType.CertificateOfOriginNote;
			note.NoteData = "That Was a Popin' Summer!!";

			StmNote dummyNote = Factory.New<StmNote>();
			Notes bizObjNotes = new Notes(dummyNote);
			StmNote note1 = bizObjNotes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Description;
			note1.ST_NoteDataAsText = "That Was a Popin' Summer!!";
			note1.ST_NoteType = "PUB";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(bizObjNotes, notes, context);

			AssertEquals(2, bizObjNotes.GetAllNotes().Count);
		}

		public void TestNoteDataShouldNotBeSetOnATextOnlyNote()
		{
			Xsd.NotesNoteCollection valueNotes = new Xsd.NotesNoteCollection();
			Xsd.NotesNote valueNote = valueNotes.AddNew();
			valueNote.NoteData = "splaty";
			valueNote.NoteType = Xsd.NotesNoteNoteType.Custom;
			valueNote.CustomNoteTypeName = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			Notes bizObjNotes = new Notes(orgHeader);
			StmNote bizObjNote = bizObjNotes.AddNew();
			bizObjNote.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			bizObjNote.ST_IsCustomDescription = false;
			bizObjNote.ST_NoteDataAsText = "VALUES";

			ErrorReporter.Clear();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(orgHeader.Notes, valueNotes, context);
			Assert("No Developer Reports should be sent", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			ErrorReporter.Clear();

			bool result = false;
			AssertEquals("Number of notes should be 2", 2, bizObjNotes.GetAllNotes().Count);
			AssertEquals("NoteData of existing note", "VALUES", bizObjNote.ST_NoteDataAsText);
			foreach (StmNote note in bizObjNotes.GetAllNotes())
			{
				if (note.ST_Description == valueNote.CustomNoteTypeName && note.ST_NoteDataAsText == "splaty")
				{
					result = true;
					break;
				}
			}
			Assert("Note Data", result);
		}

		public void TestImportOfRegistryDefinedCustomNoteType()
		{
			PredefinedNoteType newtype = new PredefinedNoteType((NoResString)"Custom Description", StmNoteVisibility.PUB, true, false, true, false);

			CustomNoteTypes noteTypes = new CustomNoteTypes();

			CustomNoteModuleAndCountry module = noteTypes.NoteModuleAndCountryList.AddNew();
			module.ModuleIDName = "ModuleID1";
			module.CountryCode = "UA";

			CustomNoteTypeItem item = module.CustomNoteTypesList.AddNew();
			item.NoteName = "Custom Description";
			item.DefaultVisibility = "PUB";

			CustomNotesProvider.Instance.AllCustomNoteTypes.Add(newtype);
			PredefinedNoteTypes.Instance.ClearCacheOfAllNotes();
			CustomNotesProvider.Instance.RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, noteTypes);

			Assert("System registry custom note defined", CustomNotesProvider.Instance.NoteTypeExistsByName("Custom Description"));

			StmNote note = Factory.New<StmNote>();
			note.ST_Table = "DummyBizo";
			Notes businessObjectNotes = new Notes(note);

			Xsd.Notes xsdNotes = new Xsd.Notes();
			Xsd.NotesNote xsdNote = xsdNotes.Note.AddNew();
			xsdNote.NoteType = Xsd.NotesNoteNoteType.Custom;
			xsdNote.CustomNoteTypeName = "Custom Description";
			xsdNote.NoteData = "the new note data";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportNotesAndAttachToBusinessObjectNotes(businessObjectNotes, xsdNotes.Note, context);
			Factory.Save();

			ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, "Custom Description").AddToFilter(StmNoteSchema.ST_Table, "StmNote");
			StmNote newNote = Factory.LoadTop1<StmNote>(filter);
			AssertEquals("System registry (custom) note imported.", "Custom Description", newNote.ST_Description);
			AssertEquals("Not user defined note type.", ZBool.False, newNote.ST_IsCustomDescription);
		}

		public void TestUpdatedOrCreatedNotificationNotShown()
		{
			NoteValueObjectDataAdapter adapter = (NoteValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			StmNote note = Factory.New<StmNote>();
			Xsd.NotesNoteCollection notesValue = new Xsd.NotesNoteCollection();
			Xsd.NotesNote noteValue = notesValue.AddNew();

			OrgHeader bizObj = Factory.New<OrgHeader>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			adapter.ImportNotesAndAttachToBusinessObjectNotes(bizObj.Notes, notesValue, context);
			AssertEquals("Should not contain created or updated message", false, Notify.ContainsNotificationType(WarningType.BusinessObjectCreatedOrUpdated));
		}

		public void TestJobDescriptionNoteDoesNotHaveEmptyData()
		{
			Notes businessObjectNotes = new Notes(Factory.New<StmNote>());
			StmNote note = businessObjectNotes.AddNew();
			note.FillWithValidTestData();
			note.ST_NoteText = "Data2";
			note.ST_Description = PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description;

			Xsd.Notes xsdNotes = new Xsd.Notes();
			Xsd.NotesNote xsdNote = xsdNotes.Note.AddNew();

			Adapter.ExportToValueObject(note, xsdNote, new ValueObjectExportContext(Notify));
			AssertEquals("Data2", xsdNote.NoteData);
		}

		#region Adapter

		NoteValueObjectDataAdapter Adapter
		{
			get
			{
				if (fAdapter == null)
				{
					fAdapter = new NoteValueObjectDataAdapter();
				}
				return fAdapter;
			}
		}
		NoteValueObjectDataAdapter fAdapter;

		#endregion

		#region Notify

		NotificationBuffer Notify
		{
			get
			{
				if (fNotify == null)
				{
					fNotify = new NotificationBuffer();
				}
				return fNotify;
			}
		}
		NotificationBuffer fNotify;

		#endregion

		#region Overrides for base test

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override ValueObjectDataAdapter<StmNote, Xsd.NotesNote> GetNewBizObjXmlDataAdapter()
		{
			return new NoteValueObjectDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyNote = Factory.NewWithValidTestData<StmNote>();
			emptyNote.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			emptyNote.ST_NoteText = "Some note text, apparently it's special";

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.Notes.Testing.EmptyNote.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyNote, expectedOutputFilename, ValidationKind.None, "Empty note");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedNote = Factory.New<StmNote>();
			populatedNote.ST_Description = "TheNoteOfEternalSupriseAndFloundering";
			populatedNote.ST_NoteDataAsText = "String Of Data somehow relating to this test";
			populatedNote.ST_IsCustomDescription = true;
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.Notes.Testing.PopulatedNote.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedNote, expectedOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated note");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override StmNote NewBusinessObject()
		{
			return Factory.New<StmNote>();
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Notes"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Note"; }
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get { return new string[] { "NoteCreatedDateTime" }; }
		}

		#endregion
	}
}
