using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Environment;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgSalesCall
{
	public class OrgSalesCallInterceptorTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestOrgSalesCallWithInsertAction()
		{
			Env.NumberFountains.CommunicationID.SetNext(Db.Connection, 100000);

			var handler = PrepareOrgSalesCallInterceptor();

			var entitySetWithoutID = PrepareCommunicationEntitySet(EntityAction.INSERT, ZGuid.Empty, null);
			var root = entitySetWithoutID.Root;
			handler.Invoke(entitySetWithoutID);
			var orgSalesCallEntityWithoutID = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgSalesCall") ?? root;

			AssertEquals("CommunicationID should be set if an ID is not provided", "CM00100000", orgSalesCallEntityWithoutID["CommunicationID"].ToString());

			var entitySetWithID = PrepareCommunicationEntitySet(EntityAction.INSERT, ZGuid.Empty, "IDShouldBeIgnored");
			var root2 = entitySetWithID.Root;
			handler.Invoke(entitySetWithID);
			var orgSalesCallEntityWithID = root2.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgSalesCall") ?? root2;

			AssertEquals("CommunicationID should be reset if an ID has been provided", "CM00100001", orgSalesCallEntityWithID["CommunicationID"].ToString());
		}

		[UseSnapshotProtection]
		public void TestOrgSalesCallWithUpdateOrMergeAction()
		{
			AssertOrgSalesCallWithAction(EntityAction.MERGE, "CM00000125", PrepareCommunicationEntitySet);
			AssertOrgSalesCallWithAction(EntityAction.UPDATE, "CM00000126", PrepareCommunicationEntitySet);

			void AssertOrgSalesCallWithAction(EntityAction action, string communicationId, Func<EntityAction, ZGuid, string, EntitySet> prepareEntitySet)
			{
				Env.NumberFountains.CommunicationID.SetNext(Db.Connection, 100000);

				var communication = Factory.New<MasterFiles.Business.OrgSalesCall>();
				communication.OQ_CommunicationID = communicationId;
				Factory.Save();

				var idToBeReset = "IDShouldBeResetToNextID";
				var handler = PrepareOrgSalesCallInterceptor();

				var entitySetWithInvalidID = prepareEntitySet(action, communication.PK, "IDShouldBeResetToRecordID");
				var root = entitySetWithInvalidID.Root;
				handler.Invoke(entitySetWithInvalidID);
				var orgSalesCallEntityWithExistingCommID = root.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgSalesCall") ?? root;

				AssertEquals("CommunicationID should be the record's ID", communicationId, orgSalesCallEntityWithExistingCommID["CommunicationID"].ToString());

				var entitySetWithRecordNotInDB = prepareEntitySet(action, ZGuid.NewZGuid(), idToBeReset);
				var root2 = entitySetWithRecordNotInDB.Root;
				handler.Invoke(entitySetWithRecordNotInDB);
				var orgSalesCallEntityWithNewID = root2.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgSalesCall") ?? root2;

				AssertEquals("Communication ID should be repopulated with a new ID (inserting record)", "CM00100000", orgSalesCallEntityWithNewID["CommunicationID"].ToString());

				var entitySetWithoutID = prepareEntitySet(action, ZGuid.NewZGuid(), null);
				var root3 = entitySetWithoutID.Root;
				handler.Invoke(entitySetWithoutID);
				var orgSalesCallEntityWithNewID2 = root3.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgSalesCall") ?? root3;

				AssertEquals("Communication ID should be populated with a new ID (inserting record)", "CM00100001", orgSalesCallEntityWithNewID2["CommunicationID"].ToString());

				var entitySetWithIDButNotPK = prepareEntitySet(action, ZGuid.Empty, idToBeReset);
				var root4 = entitySetWithIDButNotPK.Root;
				handler.Invoke(entitySetWithIDButNotPK);
				var orgSalesCallEntityWithNewID3 = root4.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgSalesCall") ?? root4;

				AssertEquals("Communication ID should be populated with a new ID when the PK is not provided", "CM00100002", orgSalesCallEntityWithNewID3["CommunicationID"].ToString());

				var entitySetWithoutPKAndID = prepareEntitySet(action, ZGuid.Empty, null);
				var root5 = entitySetWithoutPKAndID.Root;
				handler.Invoke(entitySetWithoutPKAndID);
				var orgSalesCallEntityWithNewID4 = root5.Children.FirstOrDefault(c => c.Definition.EntityName == "OrgSalesCall") ?? root5;

				AssertEquals("Communication ID should be populated with a new ID when the PK is not provided", "CM00100003", orgSalesCallEntityWithNewID4["CommunicationID"].ToString());
			}
		}

		protected EntitySet PrepareCommunicationEntitySet(EntityAction orgSalesCallAction, ZGuid pk, string communicationId)
		{
			var definition = TestUtil.FindEntityDefinition("Communication", "OrgSalesCall");
			var sessionServices = new AncillaryImportServices();
			var communication = new Entity(definition, sessionServices);
			SetEntityProperties(communication, orgSalesCallAction, pk, communicationId);

			return new EntitySet("Communication") { Root = communication };
		}

		void SetEntityProperties(IEntity entity, EntityAction orgSalesCallAction, ZGuid pk, string communicationId)
		{
			entity.Action = orgSalesCallAction;

			if (pk != ZGuid.Empty)
			{
				entity["PK"] = pk;
			}

			if (communicationId != null)
			{
				entity["CommunicationID"] = communicationId;
			}
		}

		protected OrgSalesCallInterceptor PrepareOrgSalesCallInterceptor()
		{
			var sessionServices = new AncillaryImportServices();
			var updateSetting = SetupSetting(sessionServices);
			var handler = new OrgSalesCallInterceptor(updateSetting, sessionServices) { Function = DummyMethod };

			return handler;
		}

		protected OrgSalesCallSetting SetupSetting(AncillaryImportServices sessionServices)
		{
			var result = new OrgSalesCallSetting();
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
