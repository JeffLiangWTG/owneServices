using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class TransportBookingTemplateUpgradeTaskTest : TransactionedTestCase
	{
		public void TestDuplicateIsDeleted()
		{
			var task = new TransportBookingTemplateUpgradeTask();
			task.DeleteTransportBookingTemplateAndItsInstructions("EFCR"); //remove system EFCR

			var newEFCRTemplatePK = new DtbBookingTmpl()
			{
				KT_Code = "EFCR",
				KT_Description = "Desc",
				KT_Direction = "EXP",
				KT_IsSystem = false
			}.InsertAndReturnObject(TestConnection).PK;

			var newEFCRInstruction1PK = new DtbBookingInstructionTmpl()
			{
				K2_KT_BookingTmpl = newEFCRTemplatePK,
				K2_Sequence = 1,
				K2_InstructionType = "DLV",
				K2_OrgType = "CNE",
				K2_DropMode = "MDE"
			}.InsertAndReturnObject(TestConnection).PK;

			var newEFCRInstruction2PK = new DtbBookingInstructionTmpl()
			{
				K2_KT_BookingTmpl = newEFCRTemplatePK,
				K2_Sequence = 2,
				K2_InstructionType = "PIC",
				K2_OrgType = "CNE",
				K2_DropMode = "MDE"
			}.InsertAndReturnObject(TestConnection).PK;

			task.Run();

			var eFCRcmd = Db.Connection.Command(selectTemplateSql);
			eFCRcmd.AddParameter("@TemplateCode", SqlDbType.NVarChar, "EFCR");
			var eFCRCount = Convert.ToInt32(eFCRcmd.ExecuteScalar());
			AssertEquals("Searching for EFCR template", 1, eFCRCount);

			var eFCRInstructioncmd = Db.Connection.Command(selectInstructionsOfTemplate);
			eFCRInstructioncmd.AddParameter("@TemplateCode", SqlDbType.NVarChar, "EFCR");
			var eFCRInstructionsCount = Convert.ToInt32(eFCRInstructioncmd.ExecuteScalar());
			AssertEquals("Searching for EFCR template instructions", 2, eFCRInstructionsCount);
		}

		public void TestK2_DropModeColumnIsIgnoredForUpdate()
		{
			CreateTransportBookingTemplateAndItsAffiliates();
			var originalDepartmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GLBDEPARTMENT");
			var newDepartmentPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GLBDEPARTMENT WHERE GE_PK <> '" + originalDepartmentPK + "'");

			var task = new TransportBookingTemplateUpgradeTask();
			task.Run();

			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobType SET E3_GE = '" + originalDepartmentPK + "'");
			var count = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + originalDepartmentPK + "'");
			Assert("count should be greater than 0", count > 0);

			task.Run();

			var countAfterRun = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + originalDepartmentPK + "'");
			AssertEquals("No LocalCartageJobTypes should have been modified", count, countAfterRun);

			Db.Connection.ExecuteNonQuery("UPDATE dbo.LocalCartageJobType SET E3_GE = '" + newDepartmentPK + "'");
			count = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + newDepartmentPK + "'");
			Assert("count should be greater than 0", count > 0);

			task.Run();

			countAfterRun = (int)Db.Connection.ExecuteScalar("SELECT Count(*) FROM dbo.LocalCartageJobType WHERE E3_GE = '" + newDepartmentPK + "'");
			AssertEquals("No LocalCartageJobTypes should have been modified", count, countAfterRun);
		}

		public void TestDeleteCartageTypeAndItsAffiliates()
		{
			CreateTransportBookingTemplateAndItsAffiliates();

			var task = new TransportBookingTemplateUpgradeTask();
			task.DeleteTransportBookingTemplateAndItsInstructions(templatePK);

			CheckExistanceOfTheObjects(0);
		}

		public void TestTaskRun()
		{
			var task = new TransportBookingTemplateUpgradeTask();
			task.DeleteTransportBookingTemplateAndItsInstructions("EECR");
			task.DeleteTransportBookingTemplateAndItsInstructions("EECS");
			task.Run();

			var eECEcmd = Db.Connection.Command(selectTemplateSql);
			eECEcmd.AddParameter("@TemplateCode", SqlDbType.NVarChar, "EECR");
			var eECRCount = Convert.ToInt32(eECEcmd.ExecuteScalar());
			AssertEquals("Searching for EECR template", 1, eECRCount);

			var eECRInstructionscmd = Db.Connection.Command(selectInstructionsOfTemplate);
			eECRInstructionscmd.AddParameter("@TemplateCode", SqlDbType.NVarChar, "EECR");
			var eECRInstructionsCount = Convert.ToInt32(eECRInstructionscmd.ExecuteScalar());
			AssertEquals("Searching for instructions of EECR template", 2, eECRInstructionsCount);

			var eECScmd = Db.Connection.Command(selectTemplateSql);
			eECScmd.AddParameter("@TemplateCode", SqlDbType.NVarChar, "EECS");
			var eECSCount = Convert.ToInt32(eECScmd.ExecuteScalar());
			AssertEquals("Searching for EECS template", 1, eECSCount);

			var eECSInstructionscmd = Db.Connection.Command(selectInstructionsOfTemplate);
			eECSInstructionscmd.AddParameter("@TemplateCode", SqlDbType.NVarChar, "EECS");
			var eECSInstructionsCount = Convert.ToInt32(eECSInstructionscmd.ExecuteScalar());
			AssertEquals("Searching for instructions of EECS template", 2, eECSInstructionsCount);
		}

		#region Implementation

		void CreateTransportBookingTemplateAndItsAffiliates()
		{
			templatePK = new DtbBookingTmpl()
			{
				KT_Code = "XXXX",
				KT_Description = "Desc",
				KT_Direction = "EXP",
				KT_IsSystem = true
			}.InsertAndReturnObject(TestConnection).PK;

			templateInstruction1PK = new DtbBookingInstructionTmpl()
			{
				K2_KT_BookingTmpl = templatePK,
				K2_Sequence = 1,
				K2_InstructionType = "DLV",
				K2_OrgType = "CNE",
				K2_DropMode = "MDE"
			}.InsertAndReturnObject(TestConnection).PK;

			templateInstruction2PK = new DtbBookingInstructionTmpl()
			{
				K2_KT_BookingTmpl = templatePK,
				K2_Sequence = 2,
				K2_InstructionType = "PIC",
				K2_OrgType = "CNE",
				K2_DropMode = "MDE"
			}.InsertAndReturnObject(TestConnection).PK;

			CheckExistanceOfTheObjects(1);
		}

		void CheckExistanceOfTheObjects(int expectedCount)
		{
			var selectTemplateCmd = Db.Connection.Command(selectTemplateSql);
			selectTemplateCmd.AddParameter("@TemplateCode", SqlDbType.NVarChar, "XXXX");
			var templateCountBeforeUpgrade = Convert.ToInt32(selectTemplateCmd.ExecuteScalar());
			AssertEquals(expectedCount, templateCountBeforeUpgrade);

			var selectTemplateInstruction1Cmd = Db.Connection.Command(selectTemplateInstructionSql);
			selectTemplateInstruction1Cmd.AddParameter("@K2_PK", SqlDbType.UniqueIdentifier, templateInstruction1PK);
			var instructionCountBeforeUpgrade1 = Convert.ToInt32(selectTemplateInstruction1Cmd.ExecuteScalar());
			AssertEquals(expectedCount, instructionCountBeforeUpgrade1);

			var selectTemplateInstruction2Cmd = Db.Connection.Command(selectTemplateInstructionSql);
			selectTemplateInstruction2Cmd.AddParameter("@K2_PK", SqlDbType.UniqueIdentifier, templateInstruction2PK);
			var instructionCountBeforeUpgrade2 = Convert.ToInt32(selectTemplateInstruction2Cmd.ExecuteScalar());
			AssertEquals(expectedCount, instructionCountBeforeUpgrade2);
		}

		Guid templatePK;
		Guid templateInstruction1PK;
		Guid templateInstruction2PK;

		const string selectTemplateSql = @"
SELECT Count(*) From dbo.DtbBookingTmpl 
WHERE KT_Code = @TemplateCode";

		const string selectTemplateInstructionSql = @"
SELECT Count(*) From dbo.DtbBookingInstructionTmpl
WHERE K2_PK = @K2_PK";

		const string selectInstructionsOfTemplate = @"
SELECT Count(*) From dbo.DtbBookingInstructionTmpl Inner Join dbo.DtbBookingTmpl
On K2_KT_BookingTmpl = KT_PK
WHERE KT_Code = @TemplateCode";

		#endregion
	}
}
