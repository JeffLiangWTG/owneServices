using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class MetaDataTest : TestCase
	{
		// <summary>
		// Index STATUS information (binary):
		//		Index is unique        : (status &    2) > 0
		//		SQL Server System Index: (status &   64) > 0
		//		PK Constraint Index    : (status & 2048) > 0
		//		UNIQUE Constraint Index: (status & 4096) > 0
		//	Index ID
		//	  1..254: Standard Index
		//       255: Entry for table heaps
		// </summary>

		[ExpectException(typeof(ArgumentException))]
		public void TestGetFKNameFromInvalidFKViolationErrorMessageThrowsException()
		{
			string testMessage = "An error message not containing a valid FK name.";
			MetaData.GetFKNameFromErrorMessage(testMessage);
		}

		public void TestGetTableNameFromErrorMessage()
		{
			string errorMesage = @"The INSERT statement conflicted with the CHECK constraint ""Constraint_Address1"". The conflict occurred in database ""Odyssey"", table ""OrgAddress"", column 'OA_Address1'.";
			string tableName = MetaData.GetTableNameFromErrorMessage(errorMesage);
			AssertEquals("Table Name", "OrgAddress", tableName);

			errorMesage = @"The INSERT statement conflicted with the CHECK constraint ""Constraint_Address1"". The conflict occurred in database ""Odyssey"", table ""dbo.OrgAddress"", column 'OA_Address1'.";
			tableName = MetaData.GetTableNameFromErrorMessage(errorMesage);
			AssertEquals("Table Name", "OrgAddress", tableName);
		}

		public void TestGetTableNameFromErrorMessageHandlesDboPrefixOnATempDb()
		{
			var errorMessage = "The INSERT statement conflicted with the CHECK constraint \"CHK_9285fbf6-3d7a-481c-a620-f95cb7fa19e4_Securable_Values\". The conflict occurred in database \"tempdb\", table \"dbo.#DATABASE_PROPOSED_PERMISSIONS______________________________________________________________________________________000000463B20\".The statement has been terminated.";
			var tableName = MetaData.GetTableNameFromErrorMessage(errorMessage);
			AssertEquals("#DATABASE_PROPOSED_PERMISSIONS______________________________________________________________________________________000000463B20", tableName);
		}

		public void TestGetTableNameFromErrorMessageHandlesAlternatePrefixOnATempDb()
		{
			var errorMessage = "The INSERT statement conflicted with the CHECK constraint \"CHK_9285fbf6-3d7a-481c-a620-f95cb7fa19e4_Securable_Values\". The conflict occurred in database \"tempdb\", table \"hrm.#DATABASE_PROPOSED_PERMISSIONS______________________________________________________________________________________000000463B20\".The statement has been terminated.";
			var tableName = MetaData.GetTableNameFromErrorMessage(errorMessage);
			AssertEquals("#DATABASE_PROPOSED_PERMISSIONS______________________________________________________________________________________000000463B20", tableName);
		}

		public void TestGetColumnNameFromErrorMessageWithoutColumn()
		{
			var errorMesage = @"The INSERT statement conflicted with the CHECK constraint ""Constraint_Address1"". The conflict occurred in database ""Odyssey"", table ""OrgAddress"".";
			var columnName = MetaData.GetColumnNameFromErrorMessage(errorMesage);
			AssertEquals("Column Name", "", columnName);
		}

		public void TestGetColumnNameFromErrorMessageWithColumn()
		{
			var errorMesage = @"The INSERT statement conflicted with the CHECK constraint ""Constraint_Address1"". The conflict occurred in database ""Odyssey"", table ""OrgAddress"", column 'OA_Address1'.";
			var columnName = MetaData.GetColumnNameFromErrorMessage(errorMesage);
			AssertEquals("Column Name", "OA_Address1", columnName);
		}

		public void TestGetConstraintNameFromErrorMessage()
		{
			string errorMesage = @"The INSERT statement conflicted with the CHECK constraint ""Constraint_Address1"". The conflict occurred in database ""Odyssey"", table ""dbo.OrgAddress"", column 'OA_Address1'.";
			string constraintName = MetaData.GetCheckConstraintNameFromErrorMessage(errorMesage);
			AssertEquals("Constraint Name", "Constraint_Address1", constraintName);
		}

		public void TestGetDuplicatedValueFromErrorMessage()
		{
			string errorMessage = @"The duplicate key value is (test1234, 12)";
			string duplicatedValue = MetaData.GetDuplicatedValueFromErrorMessage(errorMessage);
			AssertEquals("Duplicated value", "test1234, 12", duplicatedValue);
		}

		public void TestGetColumnNameFromIndexName()
		{
			var errorMessage = @"Cannot insert duplicate key row in object 'dbo.HVLVConsignment' with unique index 'NR_UX__HVC_WaybillNumber_HVC_ClusterKey'.";
			var columns = MetaData.GetColumnNamesByIndexNameAndTablePrefix(errorMessage, "HVC");
			CombineAssertions("Must retrieve 2 column names", () =>
			{
				AssertEquals(2, columns.Length);
				AssertEquals("Column HVC_WaybillNumber", "HVC_WaybillNumber", columns[0]);
				AssertEquals("Column HVC_ClusterKey", "HVC_ClusterKey", columns[1]);
			});
		}
	}
}
