using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.XmlDefinition;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeOrgNoteValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestImportAndExportValueObjectCollection()
		{
			SysMergeOrgNoteValueObjectHelper orgNoteHelper = new SysMergeOrgNoteValueObjectHelper();
			INotifications notifications = new NotificationBuffer();
			OrgHeaderForDataTransfer organisation = Factory.New<OrgHeaderForDataTransfer>();
			AddNewNotesToOrganisation(organisation);
			AssertEquals("Notes records", 3, GetOrgNotes(organisation).Length);

			#region Export Test

			SysMergeOrgNoteCollection xsdOrgNotes = new SysMergeOrgNoteCollection();
			orgNoteHelper.ExportToValueObjectCollection(organisation, xsdOrgNotes, notifications);
			AssertEquals("Exported notes count", 3, xsdOrgNotes.Count);

			SysMergeOrgNote[] xsdOrgNotesArray = xsdOrgNotes.OfType<SysMergeOrgNote>().OrderBy(x => x.NoteType).ToArray();

			AssertEquals("Exported note 1 - NoteType", "AGV", xsdOrgNotesArray[0].NoteType);
			AssertEquals("Exported note 1 - NoteData", "Text3", xsdOrgNotesArray[0].NoteText);
			AssertEquals("Exported note 1 - NoteContext", "AAA", xsdOrgNotesArray[0].NoteContext);
			AssertEquals("Exported note 1 - Description", "Test", xsdOrgNotesArray[0].Description);
			Assert("Exported note 1 - IsCustomDescription", xsdOrgNotesArray[0].IsCustomDescription);
			Assert("Exported note 1 - ForceRead", xsdOrgNotesArray[0].ForceRead);

			AssertEquals("Exported note 2 - NoteType", "PRV", xsdOrgNotesArray[1].NoteType);
			AssertEquals("Exported note 2 - NoteData", string.Empty, xsdOrgNotesArray[1].NoteText);
			AssertEquals("Exported note 2 - NoteContext", "AER", xsdOrgNotesArray[1].NoteContext);
			AssertEquals("Exported note 2 - Description", PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, xsdOrgNotesArray[1].Description);
			Assert("Exported note 2 - IsCustomDescription", !xsdOrgNotesArray[1].IsCustomDescription);
			Assert("Exported note 2 - ForceRead", !xsdOrgNotesArray[1].ForceRead);

			AssertEquals("Exported note 3 - NoteType", "PUB", xsdOrgNotesArray[2].NoteType);
			AssertEquals("Exported note 3 - NoteData", "Text1", xsdOrgNotesArray[2].NoteText);
			AssertEquals("Exported note 3 - NoteContext", "AAA", xsdOrgNotesArray[2].NoteContext);
			AssertEquals("Exported note 3 - Description", PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, xsdOrgNotesArray[2].Description);
			Assert("Exported note 3 - IsCustomDescription", !xsdOrgNotesArray[2].IsCustomDescription);
			Assert("Exported note 3 - ForceRead", xsdOrgNotesArray[2].ForceRead);

			#endregion

			#region Import Test

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeaderForDataTransfer newOrganisation = newFactory.New<OrgHeaderForDataTransfer>();
			AssertEquals("New notes records", 0, GetOrgNotes(newOrganisation).Length);

			IValueObjectImportContext testContext = new ValueObjectImportContext(newOrganisation.Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), notifications);
			orgNoteHelper.ImportFromValueObjectCollection(xsdOrgNotes, newOrganisation, testContext);
			StmNote[] orgNotes = GetOrgNotes(newOrganisation).OrderBy(x => x.ST_NoteType).ToArray();

			AssertEquals("Notes after import", 3, orgNotes.Length);

			AssertEquals("Import note 1 - ST_Description", "Test", orgNotes[0].ST_Description);
			Assert("Import note 1 - ST_ForceRead", orgNotes[0].ST_ForceRead);
			Assert("Import note 1 - ST_IsCustomDescription", orgNotes[0].ST_IsCustomDescription);
			AssertEquals("Import note 1 - ST_NoteContext", "AAA", orgNotes[0].ST_NoteContext);
			AssertEquals("Import note 1 - ST_NoteData", "Text3", orgNotes[0].ST_NoteDataAsText);
			AssertEquals("Import note 1 - ST_NoteType", "AGV", orgNotes[0].ST_NoteType);

			AssertEquals("Import note 2 - ST_Description", PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, orgNotes[1].ST_Description);
			Assert("Import note 2 - ST_IsCustomDescription", !orgNotes[1].ST_IsCustomDescription);
			Assert("Import note 2 - ST_ForceRead", !orgNotes[1].ST_ForceRead);
			AssertEquals("Import note 2 - ST_NoteContext", "AER", orgNotes[1].ST_NoteContext);
			AssertEquals("Import note 2 - ST_NoteText", string.Empty, orgNotes[1].ST_NoteText);
			AssertEquals("Import note 2 - ST_NoteType", "PRV", orgNotes[1].ST_NoteType);

			AssertEquals("Import note 3 - ST_Description", PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, orgNotes[2].ST_Description);
			Assert("Import note 3 - ST_IsCustomDescription", !orgNotes[2].ST_IsCustomDescription);
			Assert("Import note 3 - ST_ForceRead", orgNotes[0].ST_ForceRead);
			AssertEquals("Import note 3 - ST_NoteContext", "AAA", orgNotes[2].ST_NoteContext);
			AssertEquals("Import note 3 - ST_NoteText", "Text1", orgNotes[2].ST_NoteText);
			AssertEquals("Import note 3 - ST_NoteType", "PUB", orgNotes[2].ST_NoteType);

			#endregion

		}

		public void TestDOCNoteNoErrorMessage()
		{
			var orgNoteHelper = new SysMergeOrgNoteValueObjectHelper();
			var notifications = new NotificationBuffer();
			var organisation = Factory.NewWithValidTestData<OrgHeaderForDataTransfer>();
			organisation.OH_Code = "JXK";
			var note = organisation.Factory.New<StmNote>();
			note.ST_Table = OrgHeaderSchema.Constants.TableName;
			note.ST_ParentID = organisation.PK;
			note.ST_Table = organisation.TableName;
			note.ST_Description = "";
			note.ST_NoteDataAsText = "The quick brown fox jumps over the lazy dog";
			note.ST_NoteContext = "AAA";
			note.ST_NoteType = "DOC";

			var xsdOrgNotes = new SysMergeOrgNoteCollection();
			orgNoteHelper.ExportToValueObjectCollection(organisation, xsdOrgNotes, notifications);
			AssertEquals("Exported notes count", 1, xsdOrgNotes.Count);

			var newFactory = new BusinessObjectFactory();
			var newOrganisation = newFactory.NewWithValidTestData<OrgHeaderForDataTransfer>();
			newOrganisation.OH_Code = "JXK";
			AssertEquals("New notes records", 0, GetOrgNotes(newOrganisation).Length);

			var testContext = new ValueObjectImportContext(newOrganisation.Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), notifications);
			orgNoteHelper.ImportFromValueObjectCollection(xsdOrgNotes, newOrganisation, testContext);
			var orgNotes = GetOrgNotes(newOrganisation).OrderBy(x => x.ST_NoteType).ToArray();

			AssertNullOrEmpty("No error occur", testContext.LastNotificationMessage);
			AssertEquals("Notes after import", 1, orgNotes.Length);
			newFactory.Save();
			AssertEquals(true, orgNotes[0].IsInDatabase);
			AssertEquals("Don't contains rich text", "The quick brown fox jumps over the lazy dog", orgNotes[0].ST_NoteData.ToAscii());
		}

		void AddNewNotesToOrganisation(OrgHeaderForDataTransfer organisation)
		{
			StmNote note = organisation.Factory.New<StmNote>();
			note.ST_Table = OrgHeaderSchema.Constants.TableName;
			note.ST_ParentID = organisation.PK;
			note.ST_Table = organisation.TableName;
			note.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note.ST_NoteText = "Text1";
			note.ST_NoteContext = "AAA";
			note.ST_NoteType = "PUB";

			note = organisation.Factory.New<StmNote>();
			note.ST_Table = OrgHeaderSchema.Constants.TableName;
			note.ST_ParentID = organisation.PK;
			note.ST_Table = organisation.TableName;
			note.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			note.ST_NoteContext = "AER";
			note.ST_ForceRead = false;
			note.ST_NoteType = "PRV";

			note = organisation.Factory.New<StmNote>();
			note.ST_Table = OrgHeaderSchema.Constants.TableName;
			note.ST_ParentID = organisation.PK;
			note.ST_Table = organisation.TableName;
			note.ST_IsCustomDescription = true;
			note.ST_Description = "Test";
			note.ST_NoteData = ORtfTextUtil.TextToRtfBytes("Text3");
			note.ST_NoteType = "AGV";
		}

		StmNote[] GetOrgNotes(OrgHeaderForDataTransfer org)
		{
			ZQuery query = new ZQuery(StmNoteSchema.ST_ParentID, org.PK).AddToFilter(StmNoteSchema.ST_Table, org.TableName);
			StmNote[] notesArray = org.Factory.Load<StmNote>(query);
			return notesArray;
		}
	}
}
