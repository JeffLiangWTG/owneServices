using System.IO;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class TransportBookingTemplateDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new TransportBookingTemplateDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestTransportBookingTemplateDataFile()
		{
			DataHelpers.ClearTable("DtbBookingInstructionTmpl");
			DataHelpers.ClearTable("DtbBookingTmpl");

			var templatePK = new DtbBookingTmpl()
			{
				KT_Code = "XXXX",
				KT_Description = "Desc",
				KT_Direction = "EXP",
				KT_IsSystem = true
			}.InsertAndReturnObject(TestConnection).PK;

			var templateInstruction1PK = new DtbBookingInstructionTmpl()
			{
				K2_KT_BookingTmpl = templatePK,
				K2_Sequence = 1,
				K2_InstructionType = "DLV",
				K2_OrgType = "CNE",
				K2_DropMode = "MDE"
			}.InsertAndReturnObject(TestConnection).PK;

			var templateInstruction2PK = new DtbBookingInstructionTmpl()
			{
				K2_KT_BookingTmpl = templatePK,
				K2_Sequence = 2,
				K2_InstructionType = "PIC",
				K2_OrgType = "CNE",
				K2_DropMode = "MDE"
			}.InsertAndReturnObject(TestConnection).PK;

			var file = new TransportBookingTemplateDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 2, data.Tables.Count);
			AssertEquals(data.Tables["DtbBookingTmpl"].Rows.Count, 1);
			AssertEquals(data.Tables["DtbBookingInstructionTmpl"].Rows.Count, 2);
			Assert(data.Tables["DtbBookingTmpl"].Rows.Contains(templatePK));
			Assert(data.Tables["DtbBookingInstructionTmpl"].Rows.Contains(templateInstruction1PK));
			Assert(data.Tables["DtbBookingInstructionTmpl"].Rows.Contains(templateInstruction2PK));
		}
	}
}
