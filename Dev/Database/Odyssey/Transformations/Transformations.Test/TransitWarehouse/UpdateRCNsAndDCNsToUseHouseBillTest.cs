using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.TransitWarehouse
{
	[TestedType(typeof(UpdateRCNsAndDCNsToUseHouseBill))]
	public class UpdateRCNsAndDCNsToUseHouseBillTest : DataTransformationTestCase
	{
		#region TestIndexProvider

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Add CusEntryNum for relating HSB rows To House Bill Number Column in RCN And DCN_1] ON [dbo].[CusEntryNum] ([CE_ParentTable], [CE_EntryType], [CE_IsValid]) INCLUDE ([CE_EntryNum]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		#endregion

		#region Set HouseBill Number By Latest Created Time When Edit Time Is Same

		public void TestRCNSetHouseBillNumberByLatestCreatedTimeWhenEditTimeIsSame()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);

			var duplicateHSB1 = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test11", "HSB");
			duplicateHSB1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-10);
			duplicateHSB1.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-4);
			duplicateHSB1.AppendInsertAndReturnObject(sql);

			var duplicateHSB2 = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test12", "HSB");
			duplicateHSB2.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-10);
			duplicateHSB2.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-1);
			duplicateHSB2.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcn.PK)
				.ExpectEquals("The House Bill Number in rcn should be duplicateHSB2 as it has later created time when the edit time is same or null", r => r.WRC_HouseBillNumber, duplicateHSB2.CE_EntryNum)
				.VerifyAll();
		}

		public void TestDCNSetHouseBillNumberByLatestCreatedTimeWhenEditTimeIsSame()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var dcn = new WhsItemDispatchConsignment(whs, "DCN000001", "DCN000001", "STD").AppendInsertAndReturnObject(sql);

			var duplicateHSB1 = new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test11", "HSB");
			duplicateHSB1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-15);
			duplicateHSB1.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-9);
			duplicateHSB1.AppendInsertAndReturnObject(sql);

			var duplicateHSB2 = new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test12", "HSB");
			duplicateHSB2.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-15);
			duplicateHSB2.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-11);
			duplicateHSB2.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemDispatchConsignment.AssertFromDB(TestConnection, dcn.PK)
				.ExpectEquals("The House Bill Number in dcn should be duplicateHSB1 as it has later created time when the edit time is same or null", d => d.WDC_HouseBillNumber, duplicateHSB1.CE_EntryNum)
				.VerifyAll();
		}

		#endregion

		#region Set HouseBill Number By Latest Edit Time

		public void TestRCNSetHouseBillNumberByLatestEditTime()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);

			var duplicateHsb1 = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test1", "HSB");
			duplicateHsb1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-2);
			duplicateHsb1.AppendInsertAndReturnObject(sql);
			var duplicateHsb2 = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test2", "HSB");
			duplicateHsb2.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-1);
			duplicateHsb2.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcn.PK)
				.ExpectEquals("The House bill number assigned to rcn should be duplicateHsb2 as it has later edit time", r => r.WRC_HouseBillNumber, duplicateHsb2.CE_EntryNum)
				.VerifyAll();
		}

		public void TestDCNSetHouseBillNumberByLatestEditTime()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var dcn = new WhsItemDispatchConsignment(whs, "DCN000001", "DCN000001", "STD").AppendInsertAndReturnObject(sql);

			var duplicateHsb1 = new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test1", "HSB");
			duplicateHsb1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-4);
			duplicateHsb1.AppendInsertAndReturnObject(sql);
			var duplicateHsb2 = new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test2", "HSB");
			duplicateHsb2.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-6);
			duplicateHsb2.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemDispatchConsignment.AssertFromDB(TestConnection, dcn.PK)
				.ExpectEquals("The House bill number assigned to dcn should be duplicateHsb1 as it has later edit time", d => d.WDC_HouseBillNumber, duplicateHsb1.CE_EntryNum)
				.VerifyAll();
		}

		#endregion

		#region Set HouseBill Number By Highest EntryNum When All Time Is Same

		public void TestRCNSetHouseBillNumberByHighestEntryNumWhenAllTimeIsSame()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);

			var duplicateHsb1 = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test23", "HSB");
			duplicateHsb1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-10);
			duplicateHsb1.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-8);
			duplicateHsb1.AppendInsertAndReturnObject(sql);

			var duplicateHsb2 = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test24", "HSB");
			duplicateHsb2.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-10);
			duplicateHsb2.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-8);
			duplicateHsb2.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcn.PK)
				.ExpectEquals("The House Bill should be equal to duplicateHsb2 as it has the later entry num", r => r.WRC_HouseBillNumber, duplicateHsb2.CE_EntryNum)
				.VerifyAll();
		}

		public void TestDCNSetHouseBillNumberByHighestEntryNumWhenAllTimeIsSame()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var dcn = new WhsItemDispatchConsignment(whs, "DCN000001", "DCN000001", "STD").AppendInsertAndReturnObject(sql);

			var duplicateHsb1 = new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test5", "HSB");
			duplicateHsb1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-10);
			duplicateHsb1.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-8);
			duplicateHsb1.AppendInsertAndReturnObject(sql);

			var duplicateHsb2 = new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test3", "HSB");
			duplicateHsb2.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-10);
			duplicateHsb2.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-8);
			duplicateHsb2.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemDispatchConsignment.AssertFromDB(TestConnection, dcn.PK)
				.ExpectEquals("The House Bill should be equal to duplicateHsb1 as it has the later entry num", d => d.WDC_HouseBillNumber, duplicateHsb1.CE_EntryNum)
				.VerifyAll();
		}

		#endregion

		#region Set HouseBill Number To Empty When No HSB In EntryNum

		public void TestRCNSetHouseBillNumberToEmptyWhenNoHSBInEntryNum()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);

			sql.Append(new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test1", "DPT").GetInsertStatement());

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcn.PK)
				.ExpectEquals("The House Bill Number in rcn should be empty", r => r.WRC_HouseBillNumber, string.Empty)
				.VerifyAll();
		}

		public void TestDCNSetHouseBillNumberToEmptyWhenNoHSBInEntryNum()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var dcn = new WhsItemDispatchConsignment(whs, "DCN000001", "DCN000001", "STD").AppendInsertAndReturnObject(sql);

			sql.Append(new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test1", "DPT").GetInsertStatement());

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemDispatchConsignment.AssertFromDB(TestConnection, dcn.PK)
				.ExpectEquals("The House Bill Number in dcn should be empty", d => d.WDC_HouseBillNumber, string.Empty)
				.VerifyAll();
		}

		#endregion

		#region Set HouseBill Number To Empty Even With MasterBill Number Present

		public void TestSetHouseBillNumberToEmptyEvenWithMasterBillNumberPresent()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);

			var mab = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test1", "MAB").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcn.PK)
				.ExpectEquals("The House Bill Number in rcn should be empty even though rcn has Master Bill Number", r => r.WRC_HouseBillNumber, string.Empty)
				.VerifyAll();
		}

		#endregion

		#region Should not Set HouseBill Number where there is an existing one

		public void TestRCNShouldNotSetHouseBillNumberIfExists()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			rcn.WRC_HouseBillNumber = "HSB001";
			rcn.AppendInsertAndReturnObject(sql);

			var duplicateHSB1 = new CusEntryNum(rcn.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test11", "HSB");
			duplicateHSB1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-10);
			duplicateHSB1.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-4);
			duplicateHSB1.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcn.PK)
				.ExpectEquals("The House Bill Number in rcn should remain if there is an existing one", r => r.WRC_HouseBillNumber, "HSB001")
				.VerifyAll();
		}

		public void TestDCNShouldNotSetHouseBillNumberIfExists()
		{
			var sql = new SqlQueryBuilder();
			var whs = InsertBranchAndWarehouse();

			var dcn = new WhsItemDispatchConsignment(whs, "DCN000001", "DCN000001", "STD");
			dcn.WDC_HouseBillNumber = "HSB001";
			dcn.AppendInsertAndReturnObject(sql);

			var duplicateHSB1 = new CusEntryNum(dcn.PK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test11", "HSB");
			duplicateHSB1.CE_SystemLastEditTimeUtc = DateTime.Now.AddDays(-15);
			duplicateHSB1.CE_SystemCreateTimeUtc = DateTime.Now.AddDays(-9);
			duplicateHSB1.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RunTransformation();

			WhsItemDispatchConsignment.AssertFromDB(TestConnection, dcn.PK)
				.ExpectEquals("The House Bill Number in dcn should remain if there is an existing one", d => d.WDC_HouseBillNumber, "HSB001")
				.VerifyAll();
		}

		#endregion

		#region Implementation

		#region Override

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateRCNsAndDCNsToUseHouseBill();

		Guid dcnPK;
		Guid rcnPK;

		string hsbEntryNum;
		string hsb1EntryNum;

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			rcnPK = rcn.PK;
			var hsb = new CusEntryNum(rcnPK, WhsItemReceiveConsignmentSchema.Constants.TableName, "Test1", "HSB").AppendInsertAndReturnObject(sql);

			var dcn = new WhsItemDispatchConsignment(whs, "DCN000001", "DCN000001", "STD").AppendInsertAndReturnObject(sql);
			dcnPK = dcn.PK;
			var hsb1 = new CusEntryNum(dcnPK, WhsItemDispatchConsignmentSchema.Constants.TableName, "Test2", "HSB").AppendInsertAndReturnObject(sql);

			hsbEntryNum = hsb.CE_EntryNum;
			hsb1EntryNum = hsb1.CE_EntryNum;

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			var instance = GetNewTestTransformationInstance();
			instance.Run();

			WhsItemReceiveConsignment.AssertFromDB(TestConnection, rcnPK)
				.ExpectEquals("The house bill number in rcn should be equal to hsbEntryNum", r => r.WRC_HouseBillNumber, hsbEntryNum)
				.VerifyAll();
			WhsItemDispatchConsignment.AssertFromDB(TestConnection, dcnPK)
				.ExpectEquals("The house bill number in dcn should be equal to hsb1EntryNum", d => d.WDC_HouseBillNumber, hsb1EntryNum)
				.VerifyAll();
		}

		#endregion

		WhsWarehouse InsertBranchAndWarehouse(string branchCode = "BR1", string warehouseCode = "WH1")
		{
			var branch = new GlbBranch(branchCode).InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse(warehouseCode, branch.PK).WithDockDoor(TestConnection);

			return whs;
		}

		#endregion
	}
}
