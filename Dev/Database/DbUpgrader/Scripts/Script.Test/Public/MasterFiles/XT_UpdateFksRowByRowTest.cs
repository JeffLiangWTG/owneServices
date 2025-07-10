using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(XT_UpdateFksRowByRow))]
	class XT_UpdateFksRowByRowTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestTemporaryComputedColumnsHandled()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					using (var cmd = connection.Command(TemporaryComputedColumnsSql))
					{
						cmd.CommandTimeout = 900;
						cmd.ExecuteNonQuery();
					}

					using (var cmd = connection.Command("XT_UpdateFksRowByRow"))
					{
						cmd.AddParameter("@FkSchema", SqlDbType.VarChar, "dbo");
						cmd.AddParameter("@FkTable", SqlDbType.VarChar, "JobComInvoiceHeader");
						cmd.AddParameter("@FkColumn", SqlDbType.VarChar, "CW!!USJobComInvoiceHeader_OA_InvoicerAddress");
						cmd.AddParameter("@FkTablePkCol", SqlDbType.VarChar, "JZ_PK");
						cmd.AddParameter("@NewParentPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
						cmd.AddParameter("@OldParentPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
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

		const string TemporaryComputedColumnsSql = @"
ALTER TABLE [JobComInvoiceHeader]
ADD [CW!!USJobComInvoiceHeader_OA_InvoicerAddress] UNIQUEIDENTIFIER;

ALTER TABLE [JobComInvoiceHeader]
ADD CONSTRAINT FK_JobComInvoiceHeader_OrgAddress
FOREIGN KEY ([CW!!USJobComInvoiceHeader_OA_InvoicerAddress])
REFERENCES [OrgAddress] ([OA_PK]);
";
	}
}

