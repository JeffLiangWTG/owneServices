using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.IncidentNotesWithoutDescription;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentNotesWithoutDescription))]
	internal class IncidentNotesWithoutDescriptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestElementType()
		{
			IncidentNotesWithoutDescription notes = (IncidentNotesWithoutDescription)GetNewBusinessObject();

			StmNote note = notes.AddNew();
			AssertEquals("Note.GetType()", typeof(IncidentNoteWithoutDescription), note.GetType());
			AssertEquals("VisibleNotes.TypeOfElements", typeof(IncidentNoteWithoutDescription), notes.VisibleNotes.TypeOfElements);
			AssertEquals("GetAllNotes().TypeOfElements", typeof(IncidentNoteWithoutDescription), notes.GetAllNotes().TypeOfElements);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ProfessionalServicesQuote incident = Factory.New<ProfessionalServicesQuote>();
			return new IncidentNotesWithoutDescription(incident);
		}

		#endregion
	}

	[TestedType(typeof(IncidentNoteWithoutDescriptionDependentCollection))]
	public class IncidentNoteWithoutDescriptionDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			return new IncidentNoteWithoutDescriptionDependentCollection(parent, Factory);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(IncidentNoteWithoutDescriptionDependentCollection);
		}
	}

	[TestedType(typeof(IncidentNoteWithoutDescriptionCollection))]
	public class IncidentNoteWithoutDescriptionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			return new IncidentNoteWithoutDescriptionCollection(parent);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(IncidentNoteWithoutDescriptionCollection);
		}
	}
}
