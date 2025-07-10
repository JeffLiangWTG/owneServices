using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentNoteWithoutDescription))]
	public class IncidentNoteWithoutDescriptionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoDescriptionValidation()
		{
			IncidentNoteWithoutDescription note = Factory.New<IncidentNoteWithoutDescription>();

			note.ST_Description = "";
			AssertNoErrors(note.ST_DescriptionInfo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ProfessionalServicesQuote incident = Factory.New<ProfessionalServicesQuote>();
			return new IncidentNotesWithoutDescription(incident).AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			IncidentNoteWithoutDescription note = (IncidentNoteWithoutDescription)base.GetNewBusinessObjectForDeleteTest(factory);
			note.ST_Table = "IncidentMain";
			return note;
		}

		#endregion
	}
}
