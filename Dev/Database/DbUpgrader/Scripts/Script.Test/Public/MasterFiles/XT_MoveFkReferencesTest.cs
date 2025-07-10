using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(XT_MoveFkReferences))]
	class XT_MoveFkReferencesTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestComputedColumnsAreNotUpdated()
		{
			AssertSpecialColumnsHandled(ComputedColumnSql);
		}

		[ExpectNoExceptions]
		public void TestTemporaryColumnsHandled()
		{
			AssertSpecialColumnsHandled(TemporaryColumnSql);
		}

		const string TemporaryColumnSql = @"
ALTER TABLE [JobComInvoiceHeader]
ADD [CW!!USJobComInvoiceHeader_OA_InvoicerAddress] UNIQUEIDENTIFIER;

ALTER TABLE [JobComInvoiceHeader]
ADD CONSTRAINT FK_JobComInvoiceHeader_OrgAddress
FOREIGN KEY ([CW!!USJobComInvoiceHeader_OA_InvoicerAddress])
REFERENCES [OrgAddress] ([OA_PK]);
";

		const string ComputedColumnSql = @"
ALTER TABLE [JobComInvoiceHeader]
ADD [TestComputedColumn_OA] AS (JZ_OA_SupplierAddress) Persisted;

ALTER TABLE [JobComInvoiceHeader]
ADD CONSTRAINT FK_JobComInvoiceHeader_OrgAddress
FOREIGN KEY ([TestComputedColumn_OA])
REFERENCES [OrgAddress] ([OA_PK]);
";

		void AssertSpecialColumnsHandled(string sql)
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					using (var cmd = connection.Command(sql))
					{
						cmd.CommandTimeout = 900;
						cmd.ExecuteNonQuery();
					}

					using (var cmd = connection.Command("XT_MoveFkReferences"))
					{
						cmd.AddParameter("@NewParentPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
						cmd.AddParameter("@OldParentPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
						cmd.AddParameter("@ParentTableCode", SqlDbType.Char, "OA");
						cmd.AddParameter("@FkSystemLastEditUser", SqlDbType.VarChar, "E");
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandTimeout = 900;
						cmd.ExecuteNonQuery();
					}
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}
	}
}

