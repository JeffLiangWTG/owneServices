using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Organisations.OrgHeader.FindBoxCollections;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Organisation.Business.Test
{
	class EDIBrokerCollectionTestData : EDIBrokerCollection
	{
		public EDIBrokerCollectionTestData(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new ZQuery CreateAdditionalFilter()
		{
			return base.CreateAdditionalFilter();
		}
	}

	[TestedType(typeof(EDIBrokerCollection))]
	public class BrokerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new EDIBrokerCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Brokers.AddNew();
			AssertEquals("Broker is selected", true, org1.OH_IsBroker);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed = false;

			OrgHeader org2 = Brokers.AddNew();
			AssertEquals("Broker is not selected", false, org2.OH_IsBroker);
		}

		public void TestCreateAdditionalFilterIncludesCompetitor()
		{
			var collection = new EDIBrokerCollectionTestData(Factory);
			var query = collection.CreateAdditionalFilter();

			AssertEquals(true, query.LiteralTextADO.Contains("OH_IsCompetitor = 1 or"));
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Brokers.AddNew();
			organisation.OH_IsBroker = false;
			Brokers.ValidateEntityOnSaving(organisation);
			Assert("Error - Broker not selected", organisation.OH_IsBrokerInfo.HasErrors());

			organisation.OH_IsBroker = true;
			Brokers.ValidateEntityOnSaving(organisation);
			Assert("No error - Broker selected", !organisation.OH_IsBrokerInfo.HasErrors());
		}

		#region Implementation

		protected EDIBrokerCollection Brokers;

		protected override void SetUp()
		{
			base.SetUp();
			Brokers = new EDIBrokerCollection(Factory);
		}

		#endregion
	}
}
