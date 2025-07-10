using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(csfn_ValuationChargesInline))]
	class csfn_ValuationChargesInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_ValuationChargesInline()
		{
			var parentID = Guid.NewGuid();
			var jobComInvoiceHeaderChargeSql = @"
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_IsIncludedInITOT, J7_IsApportionedCharge,
J7_RX_NKCurrency, J7_SystemCreateTimeUtc, J7_SystemCreateUser, J7_SystemLastEditTimeUtc, J7_SystemLastEditUser)
VALUES
	(NEWID(), 'JI', @J7_ParentID, 1, 'OFT', 7.81, 0, 0, 'USD', GETDATE(), '~BP', GETDATE(), '~BP'),
	(NEWID(), 'JI', @J7_ParentID, 1, 'OFT', 0.29, 0, 0, 'AUD', GETDATE(), '~BP', GETDATE(), '~BP'),
	(NEWID(), 'JI', @J7_ParentID, 1, 'OFT', 0.43, 0, 0, 'USD', GETDATE(), '~BP', GETDATE(), '~BP')
";

			using (var command = Db.Connection.Command(jobComInvoiceHeaderChargeSql))
			{
				command.AddParameter("@J7_ParentID", SqlDbType.UniqueIdentifier, parentID);
				command.ExecuteNonQuery();
			}

			var amounts = "";
			Db.Connection.ExecuteReader($@"SELECT Amounts FROM csfn_ValuationChargesInline('{parentID}', 'OFT')", reader =>
			{
				amounts = reader.GetString(0);
			});

			AssertEquals("Amounts", "7.81USD, 0.29AUD, 0.43USD", amounts);
		}
	}
}

