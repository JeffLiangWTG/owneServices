using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentCollection))]
	public class SupportIncidentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			TestCaseHelper.ClearTable(IncidentMainSchema.Constants.TableName);

			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();
			SupportIncident incident = Factory.New<SupportIncident>();

			SupportIncidentCollection coll = new SupportIncidentCollection(Factory);
			coll.Load();
			AssertEquals(1, coll.Count);
			AssertNotNull(coll.FindByPK(incident.PK));
			AssertNull(coll.FindByPK(quote.PK));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SupportIncidentCollection(Factory);
		}
	}
}
