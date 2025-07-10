using System;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified.Testing
{
	[TestedType(typeof(vw_ListingOfCustomisedReportsandDocuments))]
	class vw_ListingOfCustomisedReportsandDocumentsTest : DbCreateScriptTest
	{
		public void TestViewShouldReturnCustomizedMenuItemsOfTypeDocAndWeb()
		{
			var sql =
@"SELECT
   MenuName, IsPublished, AddingUser
FROM
   dbo.vw_ListingOfCustomisedReportsandDocuments
ORDER BY
   MenuName";

			var result = new StringBuilder();

			using (var command = Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						string isPublished = reader["IsPublished"].ToString();
						string info = reader["MenuName"] + " " + isPublished + (isPublished == "Y" ? "" : " " + reader["AddingUser"]);
						result.AppendLine(info);
					}
				}
			}

			AssertMultilineASCIIEquals("Query should return customized menu items of type DOC and WEB",
@"A. Menu DTT Y
D. Menu DFF N KLL
E. Menu WTT Y
H. Menu WFF N EGI",
				result.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateMenuItem("A. Menu DTT", "DOC", true, true, true, "");
			CreateMenuItem("B. Menu DTF", "DOC", true, false, true, "");
			CreateMenuItem("C. Menu DFT", "DOC", false, true, true, "");
			CreateMenuItem("D. Menu DFF", "DOC", false, false, false, "KLL");

			CreateMenuItem("E. Menu WTT", "WEB", true, true, true, "");
			CreateMenuItem("F. Menu WTF", "WEB", true, false, true, "");
			CreateMenuItem("G. Menu WFT", "WEB", false, true, true, "");
			CreateMenuItem("H. Menu WFF", "WEB", false, false, false, "EGI");

			CreateMenuItem("I. Menu ATT", "ACT", true, true, true, "");
			CreateMenuItem("J. Menu ATF", "ACT", true, false, true, "");
			CreateMenuItem("K. Menu AFT", "ACT", false, true, true, "");
			CreateMenuItem("L. Menu AFF", "ACT", false, false, true, "");
		}

		Guid CreateMenuItem(string menuName, string menuType, bool isClientSpecific, bool isSystemDefined, bool isPublished, string addingUser)
		{
			var result = Guid.NewGuid();
			var sql = string.Format(
@"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7})
VALUES (@SU_PK, @SU_MenuName, @SU_MenuType, @SU_IsClientSpecific, @SU_IsSystemDefined, @SU_IsPublished, @SU_GS_NKStaffCode)",
				StmMenuItemSchema.Constants.TableName,
				StmMenuItemSchema.Constants.PK,
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_MenuType,
				StmMenuItemSchema.Constants.SU_IsClientSpecific,
				StmMenuItemSchema.Constants.SU_IsSystemDefined,
				StmMenuItemSchema.Constants.SU_IsPublished,
				StmMenuItemSchema.Constants.SU_GS_NKStaffCode);

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SU_PK", result, StmMenuItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@SU_MenuName", menuName, StmMenuItemSchema.SU_MenuName);
				command.AddParameterBasedOnDbColumn("@SU_MenuType", menuType, StmMenuItemSchema.SU_MenuType);
				command.AddParameterBasedOnDbColumn("@SU_IsClientSpecific", isClientSpecific, StmMenuItemSchema.SU_IsClientSpecific);
				command.AddParameterBasedOnDbColumn("@SU_IsSystemDefined", isSystemDefined, StmMenuItemSchema.SU_IsSystemDefined);
				command.AddParameterBasedOnDbColumn("@SU_IsPublished", isPublished, StmMenuItemSchema.SU_IsPublished);
				command.AddParameterBasedOnDbColumn("@SU_GS_NKStaffCode", addingUser, StmMenuItemSchema.SU_GS_NKStaffCode);

				command.ExecuteNonQuery();
			}

			return result;
		}
	}
}

