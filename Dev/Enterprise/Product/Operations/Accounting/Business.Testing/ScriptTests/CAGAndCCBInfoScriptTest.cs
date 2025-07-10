using System;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	class CAGAndCCBInfoScriptTest : ScriptTest
	{
		public void VerifyControllingCustomerAndAgentInfo(Func<string, ZGuid?, DataTable> runScript, ZGuid? jobParent1PK = null, ZGuid? jobParent2PK = null, bool revenueSide = false)
		{
			var org1 = TestObjectCreator.CreateOrgHeader("O1", true, true);
			org1.OH_FullName = "I am Org 1";

			var org1_1 = TestObjectCreator.CreateOrgHeader("O11", true, true);
			org1.OH_FullName = "I am Org 1_1";

			var org2 = TestObjectCreator.CreateOrgHeader("O12", true, true);
			org2.OH_FullName = "I am Org 1_2";

			TestObjectCreator.CreateRelatedParty(org1_1, org1, "MNG");

			var addOrg1 = TestObjectCreator.CreateAddress(org1_1, OrgAddressType.Office, true);
			var addOrg2 = TestObjectCreator.CreateAddress(org2, OrgAddressType.Office, true);
			var addAALSHI = TestObjectCreator.CreateAddress(TestObjectCreator.AALSHI, OrgAddressType.Office, true);
			var addABIGAS = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS, OrgAddressType.Office, true);

			Factory.Save();

			if (!jobParent1PK.HasValue && !jobParent2PK.HasValue)
			{
				var dt2 = runScript(string.Empty, null);
				AssertEquals(0, dt2.Rows.Count);
			}

			if (!jobParent1PK.HasValue)
			{
				var shipment = TestObjectCreator.CreateShipment("S001001");
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
				TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, !revenueSide ? 100m : 0m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, revenueSide ? 100m : 0m, TestObjectCreator.ABIGAS);
				jobParent1PK = shipment.PK;
			}
			TestObjectCreator.CreateJobDocAddress(jobParent1PK.Value, DocAddressTypes.Codes.ControllingAgent, addAALSHI.PK);
			TestObjectCreator.CreateJobDocAddress(jobParent1PK.Value, DocAddressTypes.Codes.ControllingCustomer, addOrg1.PK);

			if (!jobParent2PK.HasValue)
			{
				var shipment2 = TestObjectCreator.CreateShipment("S001002");
				var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
				TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, !revenueSide ? 100m : 0m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, revenueSide ? 100m : 0m, TestObjectCreator.ABIGAS);
				jobParent2PK = shipment2.PK;
			}
			TestObjectCreator.CreateJobDocAddress(jobParent2PK.Value, DocAddressTypes.Codes.ControllingAgent, addABIGAS.PK);
			TestObjectCreator.CreateJobDocAddress(jobParent2PK.Value, DocAddressTypes.Codes.ControllingCustomer, addOrg2.PK);

			Factory.Save();

			var dt = runScript(string.Empty, null);
			AssertEquals(2, dt.Rows.Count);

			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["CCBOrgName"].ToString() == org1_1.OH_FullName);
			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["CCBOrgName"].ToString() == org2.OH_FullName);

			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["CAGOrgName"].ToString() == TestObjectCreator.AALSHI.OH_FullName);
			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["CAGOrgName"].ToString() == TestObjectCreator.ABIGAS.OH_FullName);

			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["MNGName"].ToString() == org1.OH_FullName);
			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["MNGName"] == DBNull.Value);

			dt = runScript(string.Format(" WHERE CCB = '{0}'", org1_1.PK), null);
			AssertEquals(1, dt.Rows.Count);
			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["CCBOrgName"].ToString() == org1_1.OH_FullName);

			dt = runScript(string.Format(" WHERE CAG = '{0}'", TestObjectCreator.AALSHI.PK), null);
			AssertEquals(1, dt.Rows.Count);
			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["CAGOrgName"].ToString() == TestObjectCreator.AALSHI.OH_FullName);

			dt = runScript(string.Empty, org1.PK);
			AssertEquals(1, dt.Rows.Count);
			AssertCollectionContains(dt.Rows.OfType<DataRow>(), x => x["MNGName"].ToString() == org1.OH_FullName);
		}
	}
}
