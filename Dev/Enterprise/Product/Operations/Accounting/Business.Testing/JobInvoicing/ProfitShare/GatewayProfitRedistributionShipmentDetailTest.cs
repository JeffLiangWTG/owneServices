using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	[TestedType(typeof(GatewayProfitRedistributionShipmentDetail))]
	class GatewayProfitRedistributionShipmentDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new GatewayProfitRedistributionShipmentDetail(shipment, Factory.New<OrgHeader>(), OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory, 0);
		}

		public void TestProfitShareCharges()
		{
			var shipment = profitShareTestHelper.CreateShipment("SHP1");
			var detail = new GatewayProfitRedistributionShipmentDetail(shipment, null, "", Factory, 10m);
			var charges = detail.ProfitShareCharges;

			AssertNotNull(charges);
			AssertEquals(1, charges.Count);
			AssertEquals(detail, charges[0].ShipmentDetail);
			AssertEquals(10m, charges[0].GrossRevenue);
			AssertEquals(10m, charges[0].ProfitShare);
			AssertEquals(10m, charges[0].Profit);
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Code, charges[0].CurrencyCode);
		}

		public void TestJob()
		{
			AssertJob();
		}

		public void TestJob_UsingADifferentFactory()
		{
			AssertJob(usingADifferentFactory: true);
		}

		void AssertJob(bool usingADifferentFactory = false)
		{
			var shipment = profitShareTestHelper.CreateShipment("SHP1");
			var detail = new GatewayProfitRedistributionShipmentDetail(shipment, null, "", usingADifferentFactory ? new BusinessObjectFactory() : Factory, 10m);

			AssertNull("should not create Job. GatewayProfitRedistributionMatcher handles Job creation, so it should not create it", detail.Job);

			AssertEquals("Pre-condition: Job should be created without any error", "", shipment.CreateShipmentJobHeaderWithMutex());
			AssertNotNull(detail.Job);
			AssertEquals(shipment.ShipmentJobHeader, detail.Job);

			shipment.ShipmentJobHeader.Dispose();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);
		}

		ProfitShareTestHelper profitShareTestHelper;
	}
}
