using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Customs.CA;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(CorrectDutyAndTaxInInvoiceLineAddInfo))]
	public class CorrectDutyAndTaxInInvoiceLineAddInfoTest : DataTransformationTestCase
	{
		public void TestIndex()
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();
			try
			{
				transformation.Run(s => logger.Add(s), new CancellationToken(canceled: true));
			}
			catch { }
			AssertIndex(1);
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertIndex(0);
		}

		public void TestLog()
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(new[]
				{
					"Batch processing starts. The start cluster key is '14'.",
					"Invoice lines with Cluster keys ranging from 1 to 14 have been processed.",
					"Batch processing ends.",
					"\tCompleted: Correct the duty and tax in InvoiceLine's addInfo according to CusAddInfo",
				}, logger);
			AssertIndex(0);
		}

		protected override void AssertTransformationResults()
		{
			AssertAddInfo(nameof(jiPK_NotUpdate), jiPK_NotUpdate, GetInvoiceLineAddInfo(1m, 1m, 1m, 1m));
			AssertAddInfo(nameof(jiPK_UpdateDTY), jiPK_UpdateDTY, GetInvoiceLineAddInfo(1.11m, 0m, 0m, 0m));
			AssertAddInfo(nameof(jiPK_UpdateEXS), jiPK_UpdateEXS, GetInvoiceLineAddInfo(1.11m, 2.22m, 0m, 0m));
			AssertAddInfo(nameof(jiPK_UpdateSIM), jiPK_UpdateSIM, GetInvoiceLineAddInfo(1.11m, 2.22m, 3.33m, 0m));
			AssertAddInfo(nameof(jiPK_UpdateSIM_SIM), jiPK_UpdateSIM_SIM, GetInvoiceLineAddInfo(1.11m, 2.22m, 3.33m, 0m));
			AssertAddInfo(nameof(jiPK_UpdateSIM_ADD), jiPK_UpdateSIM_ADD, GetInvoiceLineAddInfo(1.11m, 2.22m, 3.33m, 0m));
			AssertAddInfo(nameof(jiPK_UpdateSIM_CVD), jiPK_UpdateSIM_CVD, GetInvoiceLineAddInfo(1.11m, 2.22m, 3.33m, 0m));
			AssertAddInfo(nameof(jiPK_UpdateSIM_SUR), jiPK_UpdateSIM_SUR, GetInvoiceLineAddInfo(1.11m, 2.22m, 3.33m, 0m));
			AssertAddInfo(nameof(jiPK_UpdateGST), jiPK_UpdateGST, GetInvoiceLineAddInfo(1.11m, 2.22m, 3.33m, 4.44m));
			AssertAddInfo(nameof(jiPK_ClearAddInfo_DTY), jiPK_ClearAddInfo_DTY, GetInvoiceLineAddInfo(0m, 2.22m, 3.33m, 4.44m));
			AssertAddInfo(nameof(jiPK_ClearAddInfo_EXS), jiPK_ClearAddInfo_EXS, GetInvoiceLineAddInfo(1.11m, 0m, 3.33m, 4.44m));
			AssertAddInfo(nameof(jiPK_ClearAddInfo_SIM), jiPK_ClearAddInfo_SIM, GetInvoiceLineAddInfo(1.11m, 2.22m, 0m, 4.44m));
			AssertAddInfo(nameof(jiPK_ClearAddInfo_GST), jiPK_ClearAddInfo_GST, GetInvoiceLineAddInfo(1.11m, 2.22m, 3.33m, 0m));
			AssertAddInfo(nameof(jiPK_ClearAddInfo_ALL), jiPK_ClearAddInfo_ALL, GetInvoiceLineAddInfo(0m, 0m, 0m, 0m));
			AssertIndex(0);
		}

		void AssertIndex(int count)
		{
			var indexCountQuery = @"
SELECT COUNT(Name)
FROM sys.indexes 
WHERE name='CusAddInfo_Index_CorrectDutyAndTaxInInvoiceLineAddInfo' AND object_id = OBJECT_ID('dbo.CusAddInfo')";

			var indexCount = Db.Connection.ExecuteScalar(indexCountQuery);
			AssertEquals("Index count", count, indexCount);
		}

		void AssertAddInfo(string message, Guid jiPK, string expectAddInfo)
		{
			var actualAddInfo = Db.Connection.ExecuteScalar<string>($"SELECT JI_AddInfo FROM dbo.JobComInvoiceLine WHERE JI_PK = '{jiPK.ToString()}'");
			AssertEquals(message + "'s AddInfo is incorrect.", expectAddInfo, actualAddInfo.Trim('*'));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CorrectDutyAndTaxInInvoiceLineAddInfo(2);
		}

		protected override void PrepareTestData()
		{
			var sql = $@"INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser) VALUES ('{caCompanyPK.ToString()}', 'CCA', 'CA company', 'CA', GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES('{caBranchPK.ToString()}', '{caCompanyPK.ToString()}', 'BCA', GETDATE(), '~BP', GETDATE(), '~BP')";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}

			CreateTestData(jiPK_NotUpdate, 1, new DateTime(2024, 3, 23),
				1m, 1m, 1m, 1m, 1m, 1m, 1m,
				1m, 1m, 1m, 1m);
			CreateTestData(jiPK_UpdateDTY, 2, new DateTime(2024, 3, 25),
				1.11m, 0m, 0m, 0m, 0m, 0m, 0m,
				2.22m, 0m, 0m, 0m);
			CreateTestData(jiPK_UpdateEXS, 3, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0m, 0m, 0m, 0m, 0m,
				1.11m, 3.33m, 0m, 0m);
			CreateTestData(jiPK_UpdateSIM, 4, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0.11m, 0.22m, 0.33M, 2.67m, 0m,
				1.11m, 2.22m, 4.44m, 0m);
			CreateTestData(jiPK_UpdateSIM_SIM, 5, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 3.33m, 0m, 0m, 0m, 0m,
				1.11m, 2.22m, 4.44m, 0m);
			CreateTestData(jiPK_UpdateSIM_ADD, 6, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0m, 3.33m, 0m, 0m, 0m,
				1.11m, 2.22m, 4.44m, 0m);
			CreateTestData(jiPK_UpdateSIM_CVD, 7, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0m, 0m, 3.33m, 0m, 0m,
				1.11m, 2.22m, 4.44m, 0m);
			CreateTestData(jiPK_UpdateSIM_SUR, 8, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0m, 0m, 0m, 3.33m, 0m,
				1.11m, 2.22m, 4.44m, 0m);
			CreateTestData(jiPK_UpdateGST, 9, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0.11m, 0.22m, 0.33M, 2.67m, 4.44m,
				1.11m, 2.22m, 3.33m, 5.55m);
			CreateTestData(jiPK_ClearAddInfo_DTY, 10, new DateTime(2024, 3, 25),
				0m, 2.22m, 0.11m, 0.22m, 0.33M, 2.67m, 4.44m,
				1.11m, 2.22m, 3.33m, 5.55m);
			CreateTestData(jiPK_ClearAddInfo_EXS, 11, new DateTime(2024, 3, 25),
				1.11m, 0m, 0.11m, 0.22m, 0.33M, 2.67m, 4.44m,
				1.11m, 2.22m, 3.33m, 5.55m);
			CreateTestData(jiPK_ClearAddInfo_SIM, 12, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0m, 0m, 0M, 0m, 4.44m,
				1.11m, 2.22m, 3.33m, 5.55m);
			CreateTestData(jiPK_ClearAddInfo_GST, 13, new DateTime(2024, 3, 25),
				1.11m, 2.22m, 0.11m, 0.22m, 0.33M, 2.67m, 0m,
				1.11m, 2.22m, 3.33m, 5.55m);
			CreateTestData(jiPK_ClearAddInfo_ALL, 14, new DateTime(2024, 3, 25),
				0m, 0m, 0m, 0m, 0M, 0m, 0m,
				1.11m, 2.22m, 3.33m, 5.55m);
		}

		Guid caBranchPK = Guid.NewGuid();
		Guid caCompanyPK = Guid.NewGuid();
		Guid jiPK_NotUpdate = Guid.NewGuid();
		Guid jiPK_UpdateDTY = Guid.NewGuid();
		Guid jiPK_UpdateEXS = Guid.NewGuid();
		Guid jiPK_UpdateSIM = Guid.NewGuid();
		Guid jiPK_UpdateSIM_SIM = Guid.NewGuid();
		Guid jiPK_UpdateSIM_ADD = Guid.NewGuid();
		Guid jiPK_UpdateSIM_CVD = Guid.NewGuid();
		Guid jiPK_UpdateSIM_SUR = Guid.NewGuid();
		Guid jiPK_UpdateGST = Guid.NewGuid();
		Guid jiPK_ClearAddInfo_DTY = Guid.NewGuid();
		Guid jiPK_ClearAddInfo_EXS = Guid.NewGuid();
		Guid jiPK_ClearAddInfo_SIM = Guid.NewGuid();
		Guid jiPK_ClearAddInfo_GST = Guid.NewGuid();
		Guid jiPK_ClearAddInfo_ALL = Guid.NewGuid();

		void CreateTestData(Guid jiPK, int clusterKey , DateTime lastEditTime,
			decimal dtyAmount, decimal exsAmount, decimal simAmount, decimal addAmount, decimal cvdAmount, decimal surAmount, decimal gstAmount,
			decimal jiDTYAmount, decimal jiEXSAmount, decimal jiSIMAmount, decimal jiGSTAmount)
		{
			var invoiceLineAddInfo = GetInvoiceLineAddInfo(jiDTYAmount, jiEXSAmount, jiGSTAmount, jiGSTAmount);
			var insertCusAddInfo = GetInsertCusAddInfoScript(jiPK, dtyAmount, exsAmount, simAmount, addAmount, cvdAmount, surAmount, gstAmount);
			var sql = $@"DECLARE @JEPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@JEPK, '{caBranchPK.ToString()}', '{caCompanyPK.ToString()}', {clusterKey}, 'Ref{clusterKey}', 'CA', GETDATE(), '~BP', GETDATE(), '~BP');
DECLARE @JZPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_GB, JZ_ClusterKey, JZ_DataModel, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@JZPK, @JEPK, '{caBranchPK}', {clusterKey}, 'CA', GETDATE(), '~BP', GETDATE(), '~BP');
DECLARE @JIPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_DataModel, JI_ClusterKey, JI_SystemLastEditTimeUtc, JI_AddInfo, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditUser) VALUES ('{jiPK.ToString()}', @JZPK, 'CA', {clusterKey}, '{lastEditTime.ToString("yyyy-MM-dd HH:mm:ss")}', '{invoiceLineAddInfo}', GETDATE(), '~BP', '~BP')
{insertCusAddInfo}";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		string GetInvoiceLineAddInfo(decimal jiDTYAmount, decimal jiEXSAmount, decimal jiSIMAmount, decimal jiGSTAmount)
		{
			var result = "";
			if (jiDTYAmount != 0m)
			{
				result += $"DTYAmount={jiDTYAmount.ToString()}*";
			}
			if (jiEXSAmount != 0m)
			{
				result += $"EXSAmount={jiEXSAmount.ToString()}*";
			}
			if (jiSIMAmount != 0m)
			{
				result += $"SIMAmount={jiSIMAmount.ToString()}*";
			}
			if (jiGSTAmount != 0m)
			{
				result += $"GSTAmount={jiGSTAmount.ToString()}*";
			}
			return result.Trim('*');
		}

		string GetInsertCusAddInfoScript(Guid jiPK, decimal dtyAmount, decimal exsAmount, decimal simAmount, decimal addAmount, decimal cvdAmount, decimal surAmount, decimal gstAmount)
		{
			var result = "";
			if (dtyAmount != 0m)
			{
				result += GetCusAddInfoScript("DTY", dtyAmount);
			}
			if (exsAmount != 0m)
			{
				result += GetCusAddInfoScript("EXS", exsAmount);
			}
			if (simAmount != 0m)
			{
				result += GetCusAddInfoScript("SIM", simAmount);
			}
			if (addAmount != 0m)
			{
				result += GetCusAddInfoScript("ADD", addAmount);
			}
			if (cvdAmount != 0m)
			{
				result += GetCusAddInfoScript("CVD", cvdAmount);
			}
			if (surAmount != 0m)
			{
				result += GetCusAddInfoScript("SUR", surAmount);
			}
			if (gstAmount != 0m)
			{
				result += GetCusAddInfoScript("GST", gstAmount);
			}
			return result;

			string GetCusAddInfoScript(string taxType, decimal amount)
			{
				return $"INSERT INTO dbo.CusAddInfo(B7_PK, B7_ParentID, B7_ParentTableCode, B7_Type, B7_AddInfoData, B7_SystemCreateTimeUtc, B7_SystemCreateUser, B7_SystemLastEditTimeUtc, B7_SystemLastEditUser) VALUES (NEWID(), '{jiPK.ToString()}', 'JI', 'CDT', 'TaxType={taxType}*Amount={amount.ToString()}', GETDATE(), '~BP', GETDATE(), '~BP')\r\n";
			}
		}
	}
}
