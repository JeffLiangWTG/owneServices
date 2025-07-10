using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Organisations.OrgHeader.FindBoxCollections;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Organisation.Business.Test
{
	class EDILocalTransportCollectionTestData : EDILocalTransportCollection
	{
		public EDILocalTransportCollectionTestData(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new ZQuery CreateAdditionalFilter()
		{
			return base.CreateAdditionalFilter();
		}
	}

	[TestedType(typeof(EDILocalTransportCollection))]
	public class EDILocalTransportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new EDILocalTransportCollection(Factory, orgDefaults);
		}

		public void TestFilter()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgHeader localTransportProvider = Factory.New<OrgHeader>();

			carrier.OH_IsShippingProvider = true;

			localTransportProvider.OH_IsShippingProvider = true;
			localTransportProvider.OH_IsLocalTransport = true;

			LocalTransports.Load();
			Assert("Header should not be in list", !LocalTransports.Contains(header));
			Assert("Carrier should not be in list", !LocalTransports.Contains(carrier));
			Assert("Local transport provider should be in list", LocalTransports.Contains(localTransportProvider));
		}

		public void TestCreateAdditionalFilterIncludesCompetitor()
		{
			var collection = new EDILocalTransportCollectionTestData(Factory);
			var query = collection.CreateAdditionalFilter();

			AssertEquals(true, query.LiteralTextADO.Contains("OH_IsCompetitor = 1 or"));
		}

		public void TestNewChildDefaults()
		{
			OrgHeader organisation = LocalTransports.AddNew();
			Assert("ShippingProvider is selected", organisation.OH_IsShippingProvider);
			Assert("LocalTransport is selected", organisation.OH_IsLocalTransport);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = LocalTransports.AddNew();
			organisation.OH_IsShippingProvider = false;
			organisation.OH_IsLocalTransport = false;
			LocalTransports.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider has error", organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("LocalTransport has error", organisation.OH_IsLocalTransportInfo.HasErrors());

			organisation.OH_IsShippingProvider = true;
			LocalTransports.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("LocalTransport has error", organisation.OH_IsLocalTransportInfo.HasErrors());

			organisation.OH_IsLocalTransport = true;
			LocalTransports.ValidateEntityOnSaving(organisation);
			Assert("ShippingProvider does not have error", !organisation.OH_IsShippingProviderInfo.HasErrors());
			Assert("LocalTransport does not have error", !organisation.OH_IsLocalTransportInfo.HasErrors());
		}

		#region Implementation

		protected EDILocalTransportCollection LocalTransports;

		protected override void SetUp()
		{
			base.SetUp();
			LocalTransports = new EDILocalTransportCollection(Factory);
		}

		#endregion
	}
}
