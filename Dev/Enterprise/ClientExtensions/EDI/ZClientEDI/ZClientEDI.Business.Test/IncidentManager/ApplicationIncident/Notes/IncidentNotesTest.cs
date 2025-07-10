using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[TestedType(typeof(IncidentNotes))]
	internal class IncidentNotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIncidentNotes()
		{
			DummyEnterpriseBusinessObject dummy1 = Factory.New<DummyEnterpriseBusinessObject>();
			IncidentNotes notes = new IncidentNotes(dummy1);
			StmNote note1 = notes.AddNew();

			AssertEquals("Note type", typeof(IncidentNote), note1.GetType());
			AssertEquals("Visible Note type", typeof(IncidentNote), notes.VisibleNotes.TypeOfElements);
			AssertEquals("All Note type", typeof(IncidentNote), notes.GetAllNotes().TypeOfElements);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ProfessionalServicesQuote incident = Factory.New<ProfessionalServicesQuote>();
			return new IncidentNotes(incident);
		}

		#endregion
	}

	[TestedType(typeof(IncidentNotes.IncidentNoteDependantCollection))]
	class IncidentNoteDependantCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<IncidentNote>();
			return new IncidentNotes.IncidentNoteDependantCollection(parent, Factory);
		}
	}

	[TestedType(typeof(IncidentNotes.IncidentNoteCollection))]
	class IncidentNoteCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<IncidentNote>();
			return new IncidentNotes.IncidentNoteCollection(parent);
		}
	}
}
