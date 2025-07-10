using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsUsaEBond))]
	sealed class ExtensionsUsaEBondTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JePk1 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk2 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk3 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk4 UNIQUEIDENTIFIER = newid();
				DECLARE @chPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @chPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @chPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @chPk4 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkUs UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkUs UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkUs);
				DECLARE @GdPK01 UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'US', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkUs;

				DECLARE @GcPkPr UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkPr UNIQUEIDENTIFIER = newid();
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
				(@GcPkPr, 'DEN', 'PR company', 'PR');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
				(@GbPkPr, 'DEB', @GcPkPr);

			INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ApplicationCode, JE_ClusterKey) VALUES
					(@JePk1, 'US', 'DEC01', 'IMP', @GbPkUs, @GcPkUs, '2020-05-01', 'US1', 'BLT', 1),
					(@JePk2, 'PR', 'DEC02', 'IMP', @GbPkPr, @GcPkPr, '2020-05-02', 'US1', 'BLT', 2),
					(@JePk3, 'PR', 'DEC03', 'EXP', @GbPkPr, @GcPkPr, '2020-05-03', 'US1', 'BLT', 3),
					(@JePk4, 'PR', 'DEC04', 'IMP', @GbPkPr, @GcPkPr, '2020-05-04', 'US1', 'BLT', 4);

			INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES
					(newid(), 'US_BondType', 'STR', '9', 'JE', @JePk1),
					(newid(), 'US_BondType', 'STR', '9', 'JE', @JePk2),
					(newid(), 'US_BondType', 'STR', '9', 'JE', @JePk3),
					(newid(), 'US_BondType', 'STR', '9', 'JE', @JePk4);

			INSERT INTO dbo.CusEntryNum(CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_EntryType, CE_Category, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @JePk1, 'JobDeclaration', '31197501', 'ENS', 'CUS', 'US', getutcdate(), '~BP', getutcdate(), '~BP'),
					(newid(), @JePk2, 'JobDeclaration', '31197502', 'UCR', 'CUS', 'US', getutcdate(), '~BP', getutcdate(), '~BP'),
					(newid(), @JePk3, 'JobDeclaration', '31197503', 'ENS', 'CUS', 'US', getutcdate(), '~BP', getutcdate(), '~BP'),
					(newid(), @JePk4, 'JobDeclaration', '31197504', 'ENS', 'CUS', 'US', getutcdate(), '~BP', getutcdate(), '~BP');
			INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES
					(@chPk1, 'US', @JePk1, 'BGM Reference 1', 'ENS', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
					(@chPk2, 'PR', @JePk2, 'BGM Reference 2', 'ENS', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
					(@chPk3, 'PR', @JePk3, 'BGM Reference 3', 'ENS', 3, getutcdate(), '~BP', getutcdate(), '~BP'),
					(@chPk4, 'PR', @JePk4, 'BGM Reference 4', 'ENS', 4, getutcdate(), '~BP', getutcdate(), '~BP');
			INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_ApplicationCode, EM_ReceiveTransmit, EM_Status, EM_MessageType, EM_MessageSubType, EM_MessageText, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_MessageNum, EM_ApplicationReference, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPkUs, @GdPK01, @chPk1, 'CusUnderbond', 'UXB', 'RCV', 'RCV', 'XDC', 'XUE', 'M02Bill01_', '2020-05-01', 'US1', '00000000261714000001', 'ACCEPTED', getutcdate(), '~BP'),
					(newid(), @GbPkPr, @GdPK01, @chPk2, 'CusUnderbond', 'UXB', 'RCV', 'RCV', 'XDC', 'XUE', 'M02Bill01_', '2020-05-02', 'US2', '00000000261714000001', 'ACCEPTED', getutcdate(), '~BP'),
					(newid(), @GbPkPr, @GdPK01, @chPk3, 'CusUnderbond', 'UXB', 'RCV', 'RCV', 'XDC', 'XUE', 'M02Bill01_', '2020-05-03', 'US3', '00000000261714000001', 'ACCEPTED', getutcdate(), '~BP'),
					(newid(), @GbPkPr, @GdPK01, @chPk4, 'CusUnderbond', 'UXB', 'RCV', 'RCV', 'XDC', 'XUE', 'M02Bill01_', '2020-05-03', 'US4', '00000000261714000001', 'ACCEPTED', getutcdate(), '~BP'),
					(newid(), @GbPkPr, @GdPK01, @chPk4, 'CusUnderbond', 'UXB', 'RCV', 'RCV', 'XDC', 'XUE', 'M02Bill01_', '2020-05-04', 'US5', '00000000261714000001', 'ACCEPTED', getutcdate(), '~BP');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 2, transactions.Count());

				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2020, 5, 1), "US1", 1, "DEC01", "US", "31197501");
				AssertRowMatchingRef1(transactions, "2", "DEN", "DEB", new DateTime(2020, 5, 3), "US4", 1, "DEC04", "PR", "31197504");
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2020, 5);
			}
		}
	}
}
