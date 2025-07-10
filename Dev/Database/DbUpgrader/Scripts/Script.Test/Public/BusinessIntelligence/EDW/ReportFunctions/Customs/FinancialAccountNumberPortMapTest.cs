using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Customs
{
	[TestedType(typeof(FinancialAccountNumberPortMap))]
	sealed class FinancialAccountNumberPortMapTest : BiCreateScriptTest
	{
		public void TestFunction()
		{
			var orgPk = Guid.NewGuid();
			var org2Pk = Guid.NewGuid();
			var ownerPk = Guid.NewGuid();
			string insertRegistryValues = $@"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<ArrayOfFinancialAccountNumberPortMap xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<FinancialAccountNumberPortMap>
		<OrganizationPK>{orgPk}</OrganizationPK>
		<CustomsOfficeCode>AAA</CustomsOfficeCode>
		<FinancialAccountNumber>8123564989</FinancialAccountNumber>
		<Cash>N</Cash>
		<ImporterPays>Y</ImporterPays>
		<CreditorPK>ce788e54-2d30-49eb-880c-b55e3df356bc</CreditorPK>
		<AccountStartDay>31</AccountStartDay>
		<DutyDefermentAmount>0.00</DutyDefermentAmount>
		<PaymentDay>1</PaymentDay>
		<AutoAllocationAllowed>N</AutoAllocationAllowed>
		<VatDefermentAmount>999999999.99</VatDefermentAmount>
	  </FinancialAccountNumberPortMap>
	<FinancialAccountNumberPortMap>
		<OrganizationPK>{org2Pk}</OrganizationPK>
		<CustomsOfficeCode>BBB</CustomsOfficeCode>
		<FinancialAccountNumber>9923564989</FinancialAccountNumber>
		<Cash>N</Cash>
		<ImporterPays>N</ImporterPays>
		<CreditorPK>499ba81b-378d-4cf2-a5b3-e46ebb551bd5</CreditorPK>
		<AccountStartDay>31</AccountStartDay>
		<DutyDefermentAmount>0.00</DutyDefermentAmount>
		<PaymentDay>1</PaymentDay>
		<AutoAllocationAllowed>N</AutoAllocationAllowed>
		<VatDefermentAmount>999999999.99</VatDefermentAmount>
	  </FinancialAccountNumberPortMap>
</ArrayOfFinancialAccountNumberPortMap>'

DECLARE @ZAFinancialAccountNumberPortMapsKey BIGINT
SELECT @ZAFinancialAccountNumberPortMapsKey = ISNULL(MAX(ZAFinancialAccountNumberPortMapsKey), 0) + 1
FROM {Db.EdwDatabaseName}.Customs.BAS__ZAFinancialAccountNumberPortMaps

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__ZAFinancialAccountNumberPortMaps(ZAFinancialAccountNumberPortMapsID, ZAFinancialAccountNumberPortMapsKey, OwnerID, CompanyKey, Value)
Values (newID(), @ZAFinancialAccountNumberPortMapsKey, '{ownerPk}', 1, CONVERT(varbinary(MAX), @registryRawValue))";

			TestConnection.ExecuteNonQuery(insertRegistryValues);
			var sql = $"SELECT * FROM [{ScriptDbName}].dbo.FinancialAccountNumberPortMap('{ownerPk}')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(2, result.Rows.Count);
			AssertEquals(orgPk, result.Rows[0]["OrganizationPK"]);
			AssertEquals("AAA", result.Rows[0]["CustomsOfficeCode"]);
			AssertEquals("8123564989", result.Rows[0]["FinancialAccountNumber"]);
			AssertEquals("Y", result.Rows[0]["ImporterPays"]);
			AssertEquals("ce788e54-2d30-49eb-880c-b55e3df356bc", result.Rows[0]["CreditorPK"].ToString());
			AssertEquals(org2Pk, result.Rows[1]["OrganizationPK"]);
			AssertEquals("BBB", result.Rows[1]["CustomsOfficeCode"]);
			AssertEquals("9923564989", result.Rows[1]["FinancialAccountNumber"]);
			AssertEquals("N", result.Rows[1]["ImporterPays"]);
			AssertEquals("499ba81b-378d-4cf2-a5b3-e46ebb551bd5", result.Rows[1]["CreditorPK"].ToString());
		}

		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
