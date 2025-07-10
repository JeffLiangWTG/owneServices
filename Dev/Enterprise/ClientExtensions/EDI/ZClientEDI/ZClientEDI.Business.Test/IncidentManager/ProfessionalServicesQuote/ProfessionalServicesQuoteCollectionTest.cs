using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ProfessionalServicesQuoteCollection))]
	public class ProfessionalServicesQuoteCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionMembers()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();

			Collection.Load();

			AssertEquals("Collection should not contain Incident.", false, Collection.Contains(incident));
			AssertEquals("Collection should contain Quote.", true, Collection.Contains(quote));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProfessionalServicesQuoteCollection(Factory);
		}

		#endregion
	}
}
