using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(DeleteInvalidUniversalJobLink))]
	public class DeleteInvalidUniversalJobLinkTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(StmUniversalJobLink.CountInDB(TestConnection, t => t.PK == validUniversalJobLink.PK), 1);
			AssertEquals(StmUniversalJobLink.CountInDB(TestConnection, t => t.PK == invalidUniversalJobLink.PK), 0);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteInvalidUniversalJobLink();

		protected override void PrepareTestData()
		{
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, StmUniversalJobLinkSchema.Constants.SqlSchemaName, StmUniversalJobLinkSchema.Constants.TableName, "Constraint_UCL_ParentTableCode_NoCheck"))
			{
				var sql = new StringBuilder();

				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
				var whs = new WhsWarehouse("WH1", branch1.PK) { WW_TransitSecurityProcessingRequired = false }.WithDockDoor(TestConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RCN1", "RCN1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
				var shipment = new JobShipment("S00000001");
				validUniversalJobLink = new StmUniversalJobLink(rcn.PK, "WRC", "S00000001", "ForwardingShipment").AppendInsertAndReturnObject(sql);
				invalidUniversalJobLink = new StmUniversalJobLink(shipment.PK, "JS", "RCN1", "TransitReceive").AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Delete Invalid Universal Job Link_1] ON [dbo].[StmUniversalJobLink] ([UCL_ParentID]) INCLUDE ([UCL_OH_Owner], [UCL_SystemCreateTimeUtc], [UCL_SystemLastEditTimeUtc]) WHERE ([UCL_ParentTableCode]<>'WDL' AND [UCL_ParentTableCode]<>'YDL' AND [UCL_ParentTableCode]<>'WRC' AND [UCL_ParentTableCode]<>'YPL' AND [UCL_ParentTableCode]<>'KM' AND [UCL_ParentTableCode]<>'WDC' AND [UCL_ParentTableCode]<>'WRP' AND [UCL_ParentTableCode]<>'WRH' AND [UCL_ParentTableCode]<>'WDH' AND [UCL_ParentTableCode]<>'YTU') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		StmUniversalJobLink validUniversalJobLink;
		StmUniversalJobLink invalidUniversalJobLink;
	}
}
