using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentNote))]
	class IncidentNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNote()
		{
			IncidentNote testNote = Factory.New<IncidentNote>();

			AssertEquals("NoteType", "", testNote.ST_NoteType);
			AssertEquals("NoteType count", 2, testNote.ST_NoteType_List.Count);
			Assert("Does not allow private notes", !testNote.ST_NoteType_List.ContainsCode(StmNoteDescription.PrvDescriptive));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ProfessionalServicesQuote incident = Factory.New<ProfessionalServicesQuote>();
			return new IncidentNotes(incident).AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			IncidentNote note = (IncidentNote)base.GetNewBusinessObjectForDeleteTest(factory);
			note.ST_Table = "IncidentMain";
			return note;
		}

		#endregion
	}
}
