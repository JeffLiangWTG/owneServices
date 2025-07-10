using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(csfn_DisbursementChargeCodes))]
	class csfn_DisbursementChargeCodesTest : DbCreateScriptTest
	{
		public void TestDuplicateChargeCodes()
		{
			var gcPK = Guid.NewGuid();
			var acPK1 = Guid.NewGuid();
			var acPK2 = Guid.NewGuid();
			var stmDataValue = string.Format(@"<EntryChargeTypeSetting><ChargeType>DT1</ChargeType><AC_ChargeCode>{0}</AC_ChargeCode></EntryChargeTypeSetting><EntryChargeTypeSetting><ChargeType>DT2</ChargeType><AC_ChargeCode>{1}</AC_ChargeCode></EntryChargeTypeSetting></ArrayOfEntryChargeTypeSetting><EntryChargeTypeSetting><ChargeType>DT3</ChargeType><AC_ChargeCode>{2}</AC_ChargeCode></EntryChargeTypeSetting></ArrayOfEntryChargeTypeSetting>",
				acPK1.ToString(), acPK2.ToString(), acPK2.ToString());
			var sql = $@"
DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfEntryChargeTypeSetting xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<EntryChargeTypeSetting>
		<ChargeType>DT1</ChargeType>
		<AC_ChargeCode>{acPK1}</AC_ChargeCode>
	</EntryChargeTypeSetting>
	<EntryChargeTypeSetting>
		<ChargeType>DT2</ChargeType>
		<AC_ChargeCode>{acPK2}</AC_ChargeCode>
	</EntryChargeTypeSetting>
	<EntryChargeTypeSetting>
		<ChargeType>DT3</ChargeType>
		<AC_ChargeCode>{acPK2}</AC_ChargeCode>
	</EntryChargeTypeSetting>
</ArrayOfEntryChargeTypeSetting>'
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{gcPK}', 'DAN', 'CA company', 'CA', 'AUD');
INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_GC)
VALUES
	('{acPK1}', 'CUSDSB', 'DSB', '{gcPK}'),
	('{acPK2}', 'CUSDCA', 'DSB', '{gcPK}');
INSERT INTO dbo.StmData
	(SD_PK, SD_Name, SD_BinaryValue, SD_Type, SD_IsCancelled, SD_IsLogged, SD_Owner)
VALUES
	(newid(), 'EntryChargeTypesAndCodes', convert(varbinary(max), @registryRawValue), 'BIN', 0, 0, '{gcPK}');
";
			Db.Connection.ExecuteNonQuery(sql);

			using (var command = Db.Connection.Command($@"SELECT * FROM csfn_DisbursementChargeCodes('{gcPK}', 'CA')"))
			{
				using (var reader = command.ExecuteReader())
				{
					var guids = new List<Guid>();
					while (reader.Read())
					{
						guids.Add(reader.GetGuid(0));
					}
					AssertEquals("Should have two record", 2, guids.Count);
					AssertCollectionContains(acPK1, guids);
					AssertCollectionContains(acPK2, guids);
				}
			}
		}
	}
}
