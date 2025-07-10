using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.TransitWarehouse
{
	[TestedType(typeof(PopulateConsignmentDestination))]
	public class PopulateConsignmentDestinationTest : DataTransformationTestCase
	{
		#region Implementation

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var dcn1 = new WhsItemDispatchConsignment(whs, "DCN000001", "DCN000001", "STD").AppendInsertAndReturnObject(sql);
			var dptd1_1 = new CusEntryNum(dcn1.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "AUSYD", "DPT");
			dptd1_1.CE_Category = "OTH";
			dptd1_1.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptd1_1.AppendInsertAndReturnObject(sql);
			var dptd1_2 = new CusEntryNum(dcn1.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "NZAKL", "DPT");
			dptd1_2.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-5);
			dptd1_2.CE_Category = "OTH";
			dptd1_2.AppendInsertAndReturnObject(sql);

			var dcn2 = new WhsItemDispatchConsignment(whs, "DCN000002", "DCN000002", "STD").AppendInsertAndReturnObject(sql);
			var dptd2_1 = new CusEntryNum(dcn2.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "AUSYD", "DPT");
			dptd2_1.CE_Category = "OTH";
			dptd2_1.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptd2_1.AppendInsertAndReturnObject(sql);
			var dptd2_2 = new CusEntryNum(dcn2.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "AUSYDAUSYD", "DPT");
			dptd2_2.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-5);
			dptd2_2.CE_Category = "OTH";
			dptd2_2.AppendInsertAndReturnObject(sql);

			var dcn3 = new WhsItemDispatchConsignment(whs, "DCN000003", "DCN000003", "STD").AppendInsertAndReturnObject(sql);
			var dptd3_1 = new CusEntryNum(dcn3.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "AUSYD", "DPT");
			dptd3_1.CE_Category = "OTH";
			dptd3_1.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptd3_1.AppendInsertAndReturnObject(sql);
			var dptd3_2 = new CusEntryNum(dcn3.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "MAB1", "MAB");
			dptd3_2.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-5);
			dptd3_2.CE_Category = "OTH";
			dptd3_2.AppendInsertAndReturnObject(sql);

			var dcn4 = new WhsItemDispatchConsignment(whs, "DCN000004", "DCN000004", "STD").AppendInsertAndReturnObject(sql);
			var dptd4 = new CusEntryNum(dcn4.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "AU", "DPT");
			dptd4.CE_Category = "OTH";
			dptd4.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptd4.AppendInsertAndReturnObject(sql);

			var rcn1 = new WhsItemReceiveConsignment(whs, "RCN000001", "RCN000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var dptr1_1 = new CusEntryNum(rcn1.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "AUSYD", "DPT");
			dptr1_1.CE_Category = "OTH";
			dptr1_1.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptr1_1.AppendInsertAndReturnObject(sql);
			var dptr1_2 = new CusEntryNum(rcn1.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "NZAKL", "DPT");
			dptr1_2.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-5);
			dptr1_2.CE_Category = "OTH";
			dptr1_2.AppendInsertAndReturnObject(sql);

			var rcn2 = new WhsItemReceiveConsignment(whs, "RCN000002", "RCN000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var dptr2_1 = new CusEntryNum(rcn2.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "AUSYD", "DPT");
			dptr2_1.CE_Category = "OTH";
			dptr2_1.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptr2_1.AppendInsertAndReturnObject(sql);
			var dptr2_2 = new CusEntryNum(rcn2.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "AUSYDAUSYD", "DPT");
			dptr2_2.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-5);
			dptr2_2.CE_Category = "OTH";
			dptr2_2.AppendInsertAndReturnObject(sql);

			var rcn3 = new WhsItemReceiveConsignment(whs, "RCN000003", "RCN000003", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var dptr3_1 = new CusEntryNum(rcn3.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "AUSYD", "DPT");
			dptr3_1.CE_Category = "OTH";
			dptr3_1.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptr3_1.AppendInsertAndReturnObject(sql);
			var dptr3_2 = new CusEntryNum(rcn3.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "MAB1", "MAB");
			dptr3_2.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-5);
			dptr3_2.CE_Category = "OTH";
			dptr3_2.AppendInsertAndReturnObject(sql);

			var rcn4 = new WhsItemReceiveConsignment(whs, "RCN000004", "RCN000004", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var dptr4 = new CusEntryNum(rcn4.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "AU", "DPT");
			dptr4.CE_Category = "OTH";
			dptr4.CE_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-10);
			dptr4.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			var dcn1 = WhsItemDispatchConsignment.ShallowLoadFromDB(TestConnection, sp => sp.WDC_ConsignmentID == "DCN000001").Single();
			dcn1.BuildAssertion(TestConnection)
				.ExpectEquals("DCN000001 should take the latest Destination.", d => d.WDC_RL_NKDestination, "NZAKL")
				.VerifyAll();

			var dcn2 = WhsItemDispatchConsignment.ShallowLoadFromDB(TestConnection, sp => sp.WDC_ConsignmentID == "DCN000002").Single();
			dcn2.BuildAssertion(TestConnection)
				.ExpectEquals("DCN000002 should take the valid Destination.", d => d.WDC_RL_NKDestination, "AUSYD")
				.VerifyAll();

			var dcn3 = WhsItemDispatchConsignment.ShallowLoadFromDB(TestConnection, sp => sp.WDC_ConsignmentID == "DCN000003").Single();
			dcn3.BuildAssertion(TestConnection)
				.ExpectEquals("DCN000003 should take the Destination from dptd type.", d => d.WDC_RL_NKDestination, "AUSYD")
				.VerifyAll();

			var rcn1 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, sp => sp.WRC_ConsignmentID == "RCN000001").Single();
			rcn1.BuildAssertion(TestConnection)
				.ExpectEquals("RCN000001 should take the latest Destination.", d => d.WRC_RL_NKDestination, "NZAKL")
				.VerifyAll();

			var rcn2 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, sp => sp.WRC_ConsignmentID == "RCN000002").Single();
			rcn2.BuildAssertion(TestConnection)
				.ExpectEquals("RCN000002 should take the valid Destination.", d => d.WRC_RL_NKDestination, "AUSYD")
				.VerifyAll();

			var rcn3 = WhsItemReceiveConsignment.ShallowLoadFromDB(TestConnection, sp => sp.WRC_ConsignmentID == "RCN000003").Single();
			rcn3.BuildAssertion(TestConnection)
				.ExpectEquals("RCN000003 should take the Destination from dptr type.", d => d.WRC_RL_NKDestination, "AUSYD")
				.VerifyAll();

			AssertEquals("All dptd Additional References should be deleted.", 0, CusEntryNum.CountInDB(TestConnection, t => t.CE_Category == "OTH" && t.CE_EntryType == "DPT"));

			AssertEquals("Other Additional References should not be deleted.", 2, CusEntryNum.CountInDB(TestConnection, t => !(t.CE_Category == "OTH" && t.CE_EntryType == "DPT")));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateConsignmentDestination();

		public override string[] expectedIndex => new string[] {
			"NONCLUSTERED INDEX [_WTG__Populate RCN and DCN Destination from AdditionalReference and Delete used AdditionalReference._1] ON [dbo].[CusEntryNum] ([CE_ParentTable], [CE_EntryType], [CE_Category], [CE_ParentID], [CE_SystemCreateTimeUtc] DESC) INCLUDE ([CE_EntryNum], [CE_EntryStatus]) WHERE (([CE_ParentTable] IN ('WhsItemReceiveConsignment', 'WhsItemDispatchConsignment')) AND [CE_Category]='OTH' AND [CE_EntryType]='DPT') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Populate RCN and DCN Destination from AdditionalReference and Delete used AdditionalReference._2] ON [dbo].[WhsItemReceiveConsignment] ([WRC_PK]) INCLUDE ([WRC_AutoVersion], [WRC_RL_NKDestination], [WRC_SystemLastEditTimeUtc], [WRC_SystemLastEditUser]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Populate RCN and DCN Destination from AdditionalReference and Delete used AdditionalReference._3] ON [dbo].[WhsItemDispatchConsignment] ([WDC_PK]) INCLUDE ([WDC_AutoVersion], [WDC_RL_NKDestination], [WDC_SystemLastEditTimeUtc], [WDC_SystemLastEditUser]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		#endregion
	}
}
