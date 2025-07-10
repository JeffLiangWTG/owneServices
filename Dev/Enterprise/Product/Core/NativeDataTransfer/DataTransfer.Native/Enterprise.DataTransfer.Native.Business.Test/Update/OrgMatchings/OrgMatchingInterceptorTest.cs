using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings
{
	public class OrgMatchingInterceptorTest : TransactionedTestCase
	{
		public void TestUnmatchedOrg_ForOrganization()
		{
			EnableUnmatchedOrganization();
			var root = organization.Root;
			root["Code"] = "Random";
			handler.Invoke(organization);
			AssertNotEquals("Would not do Org Matching for Organization Entity Set", "UNMATCHED", root["Code"]);
		}

		public void TestUnmatchedOrg()
		{
			EnableUnmatchedOrganization();
			var orderSet = SetupOrderSet(sessionServices);
			handler.Invoke(orderSet);
			var buyer = orderSet.Root.Parents.Single(e => e.EntityName == "OrgHeader");
			var receivingAgent = orderSet.Root.Parents.Single(e => e.EntityName == "ReceivingAgent");

			AssertEquals("Buyer should be replace to unmatched when organization not existed", "UNMATCHED", buyer["Code"]);
			AssertEquals("ReceivingAgent should be replace to unmatched when organization not existed", "UNMATCHED", receivingAgent["Code"]);
		}

		public void TestCreateUnmatchNote()
		{
			var before = (int)connection.ExecuteScalar(string.Format("select count(*) from dbo.StmNote where ST_ParentID = '{0}'", orgHeader.InternalPK));
			var note = new UnmatchOrgRecord();
			orgHeader.Action = EntityAction.INSERT;
			orgHeader["Code"] = "What";
			orgHeader.AddNote(note);
			handler.Invoke(organization);
			var after = (int)connection.ExecuteScalar(string.Format("select count(*) from dbo.StmNote where ST_ParentID = '{0}'", orgHeader.InternalPK));
			AssertEquals(before + 1, after);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			connection = TestUtil.Connection;
			sessionServices = new AncillaryImportServices();
			orgHeader = TestUtil.PrepareOrgHeaderEntity(sessionServices);
			organization = new EntitySet("Organization") { Root = orgHeader };

			updateSetting = SetupSetting(sessionServices);
			handler = new OrgMatchingInterceptor(updateSetting, sessionServices) { Function = DummyMethod };
		}

		#region UnmatchOrg

		static void EnableUnmatchedOrganization()
		{
			var registry = OrganisationsDataRegistry.Instance;
			var unmatchedOrganisation = registry.UseUnmatchedOrganisationForMatching.Value;
			unmatchedOrganisation.IsEnabled = true;
			registry.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrganisation);
		}

		static IEntitySet SetupOrderSet(AncillaryImportServices sessionServices)
		{
			var entitySet = new EntitySet("Order");
			entitySet.Definition = TestUtil.GetEntitySetDefinition("Order");

			var addressDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader.BuyerAddress");
			var address = new Entity(addressDefinition, sessionServices);
			address["Code"] = "PST: XXXXXXXXXXXX";
			address["Address1"] = "XXXXXXXXXXXX";

			var buyerDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader.BuyerAddress.OrgHeader");
			var org = new Entity(buyerDefinition, sessionServices);
			org.ChildrenCollection.Add(address);

			var receivingAgentDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader.ReceivingAgent");
			var receivingAgent = new Entity(receivingAgentDefinition, sessionServices);
			receivingAgent["Code"] = "ADVINE1";

			var orderDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader");
			var order = new Entity(orderDefinition, sessionServices);
			order.Action = EntityAction.MERGE;
			order["OrderNumber"] = "99999";
			order["OrderNumberSplit"] = "12";
			order.ParentCollection.Add(org);
			order.ParentCollection.Add(receivingAgent);

			entitySet.Root = order;

			return entitySet;
		}

		#endregion

		static OrgMatchingSetting SetupSetting(AncillaryImportServices sessionServices)
		{
			var result = new OrgMatchingSetting();
			var context = new EntityContext(sessionServices, new FactoryProvider());
			result.Context = context;
			return result;
		}

		static void DummyMethod(IEntitySet entitySet)
		{
		}

		#endregion

		AncillaryImportServices sessionServices;
		Entity orgHeader;
		EntitySet organization;

		OrgMatchingInterceptor handler;
		OrgMatchingSetting updateSetting;

		DbConnection connection;
	}
}
