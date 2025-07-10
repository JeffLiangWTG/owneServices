using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.OpportunityManager.Business.Test
{
	[TestedType(typeof(EDIOrgOpportunityCollectionForIncident))]
	public class EDIOrgOpportunityCollectionForIncidentTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateAdditionalFilter_MatchesWhenOppHasNoParent()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();

			Factory.Save();

			var collection = new EDIOrgOpportunityCollectionForIncidentTestData(Factory, incident);
			Assert(opp.MatchesFilter(collection.CreateAdditionalFilter()));
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet_AlreadyHaveIncidentParent()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			incident1.RelatedChildActivityPivotCollection.AddNewPivot(opp);

			Factory.Save();

			var collection = new EDIOrgOpportunityCollectionForIncidentTestData(Factory, incident2);
			var error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(opp);

			Assert(!opp.MatchesFilter(collection.CreateAdditionalFilter()));
			AssertEquals($"{System.Environment.NewLine}{incident1.HumanReadableName} is already the parent of {opp.HumanReadableName}. {opp.HumanReadableName} can only have one parent.", error);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet_AlreadyHaveOtherParentType()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			opp1.RelatedChildActivityPivotCollection.AddNewPivot(opp2);

			Factory.Save();

			var collection = new EDIOrgOpportunityCollectionForIncidentTestData(Factory, incident);
			var error = collection.GetAllNotificationsWhenAdditionalFilterNotMet(opp2);

			Assert(!opp2.MatchesFilter(collection.CreateAdditionalFilter()));
			AssertEquals($"{System.Environment.NewLine}{opp1.HumanReadableName} is already the parent of {opp2.HumanReadableName}. {opp2.HumanReadableName} can only have one parent.", error);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EDIOrgOpportunityCollectionForIncident(Factory, Factory.New<SupportIncident>());
		}
	}

	class EDIOrgOpportunityCollectionForIncidentTestData : EDIOrgOpportunityCollectionForIncident
	{
		public EDIOrgOpportunityCollectionForIncidentTestData(BusinessObjectFactory factory, SupportIncident supportIncident) : base(factory, supportIncident)
		{
		}

		public new ZQuery CreateAdditionalFilter()
		{
			return base.CreateAdditionalFilter();
		}
	}
}
