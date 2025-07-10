using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	internal class ClientDeviceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHasEnterpriseCodeAsCode()
		{
			var header = Factory.New<ClientDeviceHeader>();
			var lookups = new ClientDeviceHeaderLookups(header);

			var ent = lookups.EnterpriseCodes.AddNew();
			AssertEquals("LE_EnterpriseCode", CodePropertyAttribute.CodePropertyNameFromType(ent.GetType()));
		}

		public void TestReturnsEmptyCollectionWhenNoEnterpriseCodeSet()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var header = Factory.New<ClientDeviceHeader>();

			var ent = Factory.New<LicenceEnterprise>();
			ent.LE_OH = org.PK;

			var svr = Factory.New<LicenceDatabase>();
			svr.LD_LE = ent.PK;
			svr.LD_ServerCode = "AAA";

			Factory.Save();

			var lookups = new ClientDeviceHeaderLookups(header);
			var serverCodes = lookups.ServerCodes;
			serverCodes.Load();
			AssertEquals(0, serverCodes.Count);
		}

		public void TestReturnsEmptyCollectionWhenNoMatchingEnterpriseCode()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var header = Factory.New<ClientDeviceHeader>();
			header.CDH_EnterpriseCode = "BBB";

			var ent = Factory.New<LicenceEnterprise>();
			ent.LE_EnterpriseCode = "CCC";
			ent.LE_OH = org.PK;

			var svr = Factory.New<LicenceDatabase>();
			svr.LD_LE = ent.PK;
			svr.LD_ServerCode = "AAA";

			Factory.Save();

			var lookups = new ClientDeviceHeaderLookups(header);
			var serverCodes = lookups.ServerCodes;
			serverCodes.Load();
			AssertEquals(0, serverCodes.Count);
		}

		public void TestReturnsMatchingServerCodes()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var header = Factory.New<ClientDeviceHeader>();
			header.CDH_EnterpriseCode = "BBB";

			var ent = Factory.New<LicenceEnterprise>();
			ent.LE_EnterpriseCode = "BBB";
			ent.LE_OH = org.PK;

			var svr = Factory.New<LicenceDatabase>();
			svr.LD_LE = ent.PK;
			svr.LD_ServerCode = "AAA";

			Factory.Save();

			var lookups = new ClientDeviceHeaderLookups(header);
			var serverCodes = lookups.ServerCodes;
			serverCodes.Load();
			AssertEquals(1, serverCodes.Count);
			AssertEquals(svr.PK, serverCodes[0].PK);
		}
	}
}