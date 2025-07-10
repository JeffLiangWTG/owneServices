using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsCanadaFTypeShipmentInvoice))]
	sealed class ExtensionsCanadaFTypeShipmentInvoiceTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var sqlText = $@"
DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
DECLARE @GcPkCa UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
DECLARE @GbPkCa UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkCa);

UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'CA', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkCa;

INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ClusterKey) VALUES
	('{decPk1}', 'CA', 'DEC01', 'LVX', @GbPkCa, @GcPkCa, '2014-08-01', 'US1', 1),
	('{decPk2}', 'CA', 'DEC02', 'LVS', @GbPkCa, @GcPkCa, '2014-07-31', 'US2', 2),
	('{decPk3}', 'SG', 'DEC03', 'LVS', @GbPkSg, @GcPkSg, '2014-08-01', 'US3', 3),
	('{decPk4}', 'CA', 'DEC04', 'LVS', @GbPkCa, @GcPkCa, '2014-08-01', 'US4', 4),
	('{decPk5}', 'CA', 'DEC05', 'LVS', @GbPkCa, @GcPkCa, '2014-08-01', 'US5', 5),
	('{decPk6}', 'CA', 'DEC06', 'LVS', @GbPkCa, @GcPkCa, '2014-08-01', 'US6', 6),
	('{decPk7}', 'CA', 'DEC07', 'LVX', @GbPkCa, @GcPkCa, '2014-08-01', 'US7', 7);

INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_GB, JZ_InvoiceNumber, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_GroupInvoice)
VALUES
	('{invoicePk11}', 'CA', '{decPk1}', 1, @GbPkCa, 'INV11', '2014-08-01', 'US1', 0),
	('{invoicePk21}', 'CA', '{decPk2}', 2, @GbPkCa, 'INV21', '2014-07-31', 'US2', 0),
	('{invoicePk31}', 'CA', '{decPk3}', 3, @GbPkCa, 'INV31', '2014-08-01', 'US3', 0),
	('{invoicePk41}', 'CA', '{decPk4}', 4, @GbPkCa, 'INV41', '2014-08-01', 'US4', 1),
	('{invoicePk42}', 'CA', '{decPk4}', 4, @GbPkCa, 'INV42', '2014-08-01', 'US4', 0),
	('{invoicePk61}', 'CA', '{decPk6}', 6, @GbPkCa, 'INV61', '2014-08-31', 'US6', 0),
	('{invoicePk71}', 'CA', '{decPk7}', 7, @GbPkCa, 'INV71', '2014-08-01', 'US7', 0);

INSERT INTO dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation1TableCode, XX_Relation2ID, XX_Relation2TableCode, XX_RelationType, XX_SystemCreateTimeUtc, XX_SystemCreateUser)
VALUES
	(NEWID(), '{invoicePk61}', 'JZ', '{decPk6}', 'JE', 'ZE', '2014-08-01', 'US6');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 2, transactions.Count());

				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2014, 8, 1), "US4", 1, "DEC04", "INV42");
				AssertRowMatchingRef1(transactions, "2", "DEM", "DEM", new DateTime(2014, 8, 31), "US6", 1, "DEC06", "INV61");
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 8);
			}
		}

		readonly Guid decPk1 = Guid.NewGuid();
		readonly Guid decPk2 = Guid.NewGuid();
		readonly Guid decPk3 = Guid.NewGuid();
		readonly Guid decPk4 = Guid.NewGuid();
		readonly Guid decPk5 = Guid.NewGuid();
		readonly Guid decPk6 = Guid.NewGuid();
		readonly Guid decPk7 = Guid.NewGuid();

		readonly Guid invoicePk11 = Guid.NewGuid();
		readonly Guid invoicePk21 = Guid.NewGuid();
		readonly Guid invoicePk31 = Guid.NewGuid();
		readonly Guid invoicePk41 = Guid.NewGuid();
		readonly Guid invoicePk42 = Guid.NewGuid();
		readonly Guid invoicePk61 = Guid.NewGuid();
		readonly Guid invoicePk71 = Guid.NewGuid();
	}
}
