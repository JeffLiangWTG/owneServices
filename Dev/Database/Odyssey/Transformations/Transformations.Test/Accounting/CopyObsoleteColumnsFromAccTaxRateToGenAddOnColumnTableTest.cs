using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(CopyObsoleteColumnsFromAccTaxRateToGenAddOnColumnTable))]
	class CopyObsoleteColumnsFromAccTaxRateToGenAddOnColumnTableTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new CopyObsoleteColumnsFromAccTaxRateToGenAddOnColumnTable();

		protected override void PrepareTestData()
		{
			var taxRatePK3 = Guid.NewGuid();
			var taxRatePK4 = Guid.NewGuid();
			var utcDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

			CreateColumnsIfNotExist();
			var sqlText = $@"
			INSERT INTO dbo.AccTaxRate(AT_PK, AT_Code, AT_Description, AT_Type, AT_ExtraTaxRateType, AT_RN_NKCountry, AT_IsActive, AT_PostingGroupId, AT_ReferenceExtraRateType, AT_ReferenceRateType, AT_RateSource, AT_TaxSystemCode, AT_RateObsolete, AT_ExtraTaxRateNumeratorObsolete, AT_ExtraTaxRateDenominatorObsolete, AT_SystemCreateTimeUtc,AT_SystemCreateUser,AT_SystemLastEditTimeUtc,AT_SystemLastEditUser)
			VALUES
			('{taxRatePK1}', 'TRATE1', 'TRATE1 Desc', 'RAT', '', 'AU', 1, 1, 'ZRAT', '', 'TID', '', 30, 35, 3, '{utcDateTime}', 'AA', '{utcDateTime}', 'BB'),
			('{taxRatePK2}', 'TRATE2', 'TRATE2 Desc', 'RAT', '', 'AU', 1, 1, 'ZRAT', '', 'TID', '', 32, 40, 4, '{utcDateTime}', 'AA', '{utcDateTime}', 'BB'),
			('{taxRatePK3}', 'TRATE4', 'TRATE4 Desc', 'RAT', '', 'AU', 1, 1, 'ZRAT', 'STD', 'TID', '', 33, 42, 5, '{utcDateTime}', 'AA', '{utcDateTime}', 'BB');

			INSERT INTO dbo.AccTaxRate(AT_PK, AT_Code, AT_Description, AT_Type, AT_ExtraTaxRateType, AT_RN_NKCountry, AT_IsActive, AT_PostingGroupId, AT_ReferenceExtraRateType, AT_ReferenceRateType, AT_RateSource, AT_TaxSystemCode, AT_SystemCreateTimeUtc,AT_SystemCreateUser,AT_SystemLastEditTimeUtc,AT_SystemLastEditUser)
			VALUES
			('{taxRatePK4}', 'TRATE3', 'TRATE3 Desc', 'RAT', '', 'AU', 1, 1, 'ZRAT', '', 'TID', '', '{utcDateTime}', 'AA', '{utcDateTime}', 'BB');";

			_ = Db.Connection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertTransformationResults()
		{
			var genAddOnColumnTableResult = GetGenAddOnColumnTableResult();

			CombineAssertions(() =>
			{
				AssertEquals("Result should have rows", 6, genAddOnColumnTableResult.Rows.Count);

				AssertGenAddOnColumnTableResultRow(genAddOnColumnTableResult, taxRatePK1, "RateObsolete", "DEC", "30.000");
				AssertGenAddOnColumnTableResultRow(genAddOnColumnTableResult, taxRatePK1, "ExtraTaxRateNumeratorObsolete", "INT", "35");
				AssertGenAddOnColumnTableResultRow(genAddOnColumnTableResult, taxRatePK1, "ExtraTaxRateDenominatorObsolete", "INT", "3");

				AssertGenAddOnColumnTableResultRow(genAddOnColumnTableResult, taxRatePK2, "RateObsolete", "DEC", "32.000");
				AssertGenAddOnColumnTableResultRow(genAddOnColumnTableResult, taxRatePK2, "ExtraTaxRateNumeratorObsolete", "INT", "40");
				AssertGenAddOnColumnTableResultRow(genAddOnColumnTableResult, taxRatePK2, "ExtraTaxRateDenominatorObsolete", "INT", "4");
			});
		}

		void AssertGenAddOnColumnTableResultRow(DataTable tableResult, Guid ratePK, string name, string type, object data)
		{
			var genAddOnColumnTableResultRow = tableResult.Rows.OfType<DataRow>().First(row =>
			new Guid(Convert.ToString(row["XA_ParentID"])).Equals(ratePK)
			&& row["XA_Name"].Equals(name));

			AssertEquals(name, genAddOnColumnTableResultRow["XA_Name"]);
			AssertEquals(type, genAddOnColumnTableResultRow["XA_Type"]);
			AssertEquals(data, genAddOnColumnTableResultRow["XA_Data"]);
			AssertEquals("AT", genAddOnColumnTableResultRow["XA_ParentTableCode"]);
			AssertEquals("E", genAddOnColumnTableResultRow["XA_SystemCreateUser"]);
			AssertEquals("E", genAddOnColumnTableResultRow["XA_SystemLastEditUser"]);
			AssertNotNull(genAddOnColumnTableResultRow["XA_SystemCreateTimeUtc"]);
			AssertNotNull(genAddOnColumnTableResultRow["XA_SystemLastEditTimeUtc"]);
		}

		void CreateColumnsIfNotExist()
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, AccTaxRateSchema.Constants.TableName, "AT_RateObsolete", "DECIMAL(9,3)");
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, AccTaxRateSchema.Constants.TableName, "AT_ExtraTaxRateNumeratorObsolete", "int");
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, AccTaxRateSchema.Constants.TableName, "AT_ExtraTaxRateDenominatorObsolete", "int");
		}

		DataTable GetGenAddOnColumnTableResult() => DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.GenAddOnColumn");

		Guid taxRatePK1 = Guid.NewGuid();
		Guid taxRatePK2 = Guid.NewGuid();
	}
}
