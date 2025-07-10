using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Business.Update.OrgOpportunity;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Environment;

namespace Enterprise.DataTransfer.Native.Business.Update.Testing
{
	public class OrgOpportunityInterceptorTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestNewIdWithInsertAction()
		{
			Env.NumberFountains.SalesOpportunityID.SetNext(Db.Connection, 100000);
			var handler = PrepareOrgOpportunityInterceptor();

			var opportunityEntity = PrepareOpportunityEntitySet(EntityAction.INSERT, ZGuid.Empty, true, "IgnoreThisId");
			var opportunityEntityRoot = opportunityEntity.Root;
			handler.Invoke(opportunityEntity);

			AssertNotEquals("Opportunity Id should be reset", "IgnoreThisId", opportunityEntityRoot["OpportunityID"].ToString());
			AssertEndsWith("Opportunity Id number should end with the next number", "100000", opportunityEntityRoot["OpportunityID"].ToString());

			opportunityEntity = PrepareOpportunityEntitySet(EntityAction.INSERT, ZGuid.Empty, false);
			opportunityEntityRoot = opportunityEntity.Root;
			handler.Invoke(opportunityEntity);

			Assert("Opportunity Id has been set", opportunityEntityRoot.Properties.FirstOrDefault(p => p.Name == "OpportunityID") != null);
			AssertEndsWith("Opportunity Id number should end with the next number", "100001", opportunityEntityRoot["OpportunityID"].ToString());
		}

		[UseSnapshotProtection]
		public void TestNewIdWithMergeAction()
		{
			AssertNewIdWithSpecifiedAction(EntityAction.MERGE);
		}

		[UseSnapshotProtection]
		public void TestNewIdWithUpdateAction()
		{
			AssertNewIdWithSpecifiedAction(EntityAction.UPDATE);
		}

		void AssertNewIdWithSpecifiedAction(EntityAction action)
		{
			Env.NumberFountains.SalesOpportunityID.SetNext(Db.Connection, 100000);
			var opportunity = Factory.NewWithValidTestData<MasterFiles.Business.OrgOpportunity>();
			opportunity.P8_OpportunityID = "TestOpp1";
			Factory.Save();

			var handler = PrepareOrgOpportunityInterceptor();

			var opportunityEntitySet = PrepareOpportunityEntitySet(action, opportunity.PK, true, "IDShouldBeResetToRecordID");
			var opportunityEntityRoot = opportunityEntitySet.Root;
			handler.Invoke(opportunityEntitySet);

			AssertEquals("Opportunity ID should be the record's ID", "TestOpp1", opportunityEntityRoot["OpportunityID"].ToString());

			opportunityEntitySet = PrepareOpportunityEntitySet(action, ZGuid.NewZGuid(), true, "IDShouldBeResetToNextID");
			opportunityEntityRoot = opportunityEntitySet.Root;
			handler.Invoke(opportunityEntitySet);

			AssertEndsWith("Opportunity ID should be repopulated with a new ID (inserting record)", "100000", opportunityEntityRoot["OpportunityID"].ToString());

			opportunityEntitySet = PrepareOpportunityEntitySet(action, ZGuid.NewZGuid(), false);
			opportunityEntityRoot = opportunityEntitySet.Root;
			handler.Invoke(opportunityEntitySet);

			AssertEndsWith("Opportunity ID should be repopulated with a new ID (inserting record)", "100001", opportunityEntityRoot["OpportunityID"].ToString());

			opportunityEntitySet = PrepareOpportunityEntitySet(action, ZGuid.Empty, true, "IDShouldBeResetToNextID");
			opportunityEntityRoot = opportunityEntitySet.Root;
			handler.Invoke(opportunityEntitySet);

			AssertEndsWith("Opportunity ID should be populated with a new ID when the PK is not provided", "100002", opportunityEntityRoot["OpportunityID"].ToString());

			opportunityEntitySet = PrepareOpportunityEntitySet(action, ZGuid.Empty, false);
			opportunityEntityRoot = opportunityEntitySet.Root;
			handler.Invoke(opportunityEntitySet);

			AssertEndsWith("Opportunity ID should be populated with a new ID when the PK is not provided", "100003", opportunityEntityRoot["OpportunityID"].ToString());
		}

		protected EntitySet PrepareOpportunityEntitySet(EntityAction opportunityAction, ZGuid pk, bool setOpportunityId, string opportunityId = null)
		{
			var sessionServices = new AncillaryImportServices();
			var opportunityDefinition = TestUtil.FindEntityDefinition("Opportunity", "OrgOpportunity");
			var opportunity = new Entity(opportunityDefinition, sessionServices) { Action = opportunityAction };

			if (setOpportunityId)
			{
				opportunity["OpportunityID"] = opportunityId;
			}

			if (pk != ZGuid.Empty)
			{
				opportunity["PK"] = pk;
			}

			return new EntitySet("OrgOpportunity") { Root = opportunity };
		}

		protected OrgOpportunityInterceptor PrepareOrgOpportunityInterceptor()
		{
			var sessionServices = new AncillaryImportServices();
			var updateSetting = SetupSetting(sessionServices);
			var handler = new OrgOpportunityInterceptor(updateSetting, sessionServices) { Function = DummyMethod };

			return handler;
		}

		protected OrgOpportunitySetting SetupSetting(AncillaryImportServices sessionServices)
		{
			var result = new OrgOpportunitySetting();
			var context = new EntityContext(sessionServices, new TestFactoryProvider(Db.Connection));
			result.Context = context;
			return result;
		}

		static void DummyMethod(IEntitySet entitySet)
		{
		}
	}

	public class TestFactoryProvider : INativeFactoryProvider
	{
		public TestFactoryProvider(DbConnection testConnection)
		{
			setConnection = testConnection;
		}

		public BusinessObjectFactory GetNewFactory(DbConnection connection)
		{
			return new BusinessObjectFactory(setConnection)
			{
				NameForDebugging = "Native Xml"
			};
		}

		readonly DbConnection setConnection;
	}
}
