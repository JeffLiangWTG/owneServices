using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgMiscServ))]
	public class EDIOrgMiscServTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return HeaderForTest.MiscServ;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return HeaderForTest.MiscServ;
		}

		public void TestNewEDIMiscServ()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			Assert(testHeader.MiscServ.GetType().Equals(typeof(EDIOrgMiscServ)));
			AssertEquals(testHeader.MiscServ.OM_OH, testHeader.PK);
		}

		EDIOrgHeader CreateHeader(BusinessObjectFactory inputFactory, string code, string uNLOCO)
		{
			var fHeaderForTest = inputFactory.NewWithValidTestData<EDIOrgHeader>();
			fHeaderForTest.OH_Code = code;
			fHeaderForTest.OH_RL_NKClosestPort = uNLOCO;
			return fHeaderForTest;
		}

		public EDIOrgHeader HeaderForTest
		{
			get { return CreateHeader(Factory, "TGBLOG", "AUBNE"); }
		}

		#endregion

		public void TestWarehousesIncludesCompetitors()
		{
			var org1 = Factory.New<EDIOrgHeader>();
			var org2 = Factory.New<EDIOrgHeader>();
			var org3 = Factory.New<EDIOrgHeader>();

			org2.OH_IsWarehouseClient = true;
			org3.OH_IsCompetitor = true;

			var warehouses = Factory.New<EDIOrgHeader>().MiscServ.Warehouses;
			warehouses.Load();

			Assert("Org1 should not be in list", !warehouses.Contains(org1));
			Assert("Org2 should be in list", warehouses.Contains(org2));
			Assert("Org3 should be in list", warehouses.Contains(org3));
		}

		public void TestLocalTransportsIncludesCompetitors()
		{
			var org1 = Factory.New<EDIOrgHeader>();
			var org2 = Factory.New<EDIOrgHeader>();
			var org3 = Factory.New<EDIOrgHeader>();

			org2.OH_IsLocalTransport = true;
			org2.OH_IsShippingProvider = true;
			org3.OH_IsCompetitor = true;

			var transports = Factory.New<EDIOrgHeader>().MiscServ.LocalTransports;
			transports.Load();

			Assert("Org1 should not be in list", !transports.Contains(org1));
			Assert("Org2 should be in list", transports.Contains(org2));
			Assert("Org3 should be in list", transports.Contains(org3));
		}

		public void TestBrokersIncludesCompetitors()
		{
			var org1 = Factory.New<EDIOrgHeader>();
			var org2 = Factory.New<EDIOrgHeader>();
			var org3 = Factory.New<EDIOrgHeader>();

			org2.OH_IsBroker = true;
			org3.OH_IsCompetitor = true;

			var brokers = Factory.New<EDIOrgHeader>().MiscServ.Brokers;
			brokers.Load();

			Assert("Org1 should not be in list", !brokers.Contains(org1));
			Assert("Org2 should be in list", brokers.Contains(org2));
			Assert("Org3 should be in list", brokers.Contains(org3));
		}
	}
}
