using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.NO;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.NO.Testing
{
	[TestedType(typeof(NOMvaSettlementReport))]
	sealed class NOMvaSettlementReportTest : CustomsReportDbCreateScriptTest
	{
		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Parameters.Importer);
			yield return (SqlDbType.SmallDateTime, Parameters.PeriodFrom);
			yield return (SqlDbType.SmallDateTime, Parameters.Periodto);
		}

		public void TestColumns() => CombineAssertions(() =>
		{
			var reportSql = @"SELECT RowNumber, VATNumber, ClearanceDate, CustomsOffice, SequenceNum, JobNumber, YourReference, VatType, CifValue, CustomsCharges, ExciseDuties, CustomsValue, VATCharge FROM NOMvaSettlementReport(@importerPK, @periodFrom, @periodTo)";
			using var command = Db.Connection.Command(reportSql);
			_ = command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
			_ = command.AddParameter("@periodFrom", SqlDbType.DateTime, new DateTime(2025, 1, 1));
			_ = command.AddParameter("@periodTo", SqlDbType.DateTime, new DateTime(2025, 3, 31));

			AssertFunctionReturnExpectedValue("RowNumber", 1);
			AssertFunctionReturnExpectedValue("VATNumber", "123456789");
			AssertFunctionReturnExpectedValue("ClearanceDate", "20250302");
			AssertFunctionReturnExpectedValue("CustomsOffice", "ABCDEF");
			AssertFunctionReturnExpectedValue("SequenceNum", "1234567890");
			AssertFunctionReturnExpectedValue("JobNumber", "Reference1");
			AssertFunctionReturnExpectedValue("YourReference", "Reference2");
			AssertFunctionReturnExpectedValue("VatType", "MV1");
			AssertFunctionReturnExpectedValue("CIFValue", 11m);
			AssertFunctionReturnExpectedValue("CustomsCharges", 200m);
			AssertFunctionReturnExpectedValue("ExciseDuties", 400m);
			AssertFunctionReturnExpectedValue("CustomsValue", 800m);
			AssertFunctionReturnExpectedValue("VATCharge", 100m);
		});

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string messageType, string transportMode, string decReference, string ownerReference, int clusterKey)
		{
			var declarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_TransportMode, JE_DeclarationReference, JE_OwnerRef, JE_SystemCreateUser, JE_SystemLastEditUser, JE_ClusterKey)
VALUES (@declarationPK, 'NO', @messageType, @branchPK, @companyPK, @transportMode, @decReference, @ownerReference, '~bp', '~bp', @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				_ = command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				_ = command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType);
				_ = command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				_ = command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				_ = command.AddParameter("@transportMode", SqlDbType.VarChar, JobDeclarationSchema.JE_TransportMode.MaxLength, transportMode);
				_ = command.AddParameter("@decReference", SqlDbType.VarChar, JobDeclarationSchema.JE_DeclarationReference.MaxLength, decReference);
				_ = command.AddParameter("@ownerReference", SqlDbType.VarChar, JobDeclarationSchema.JE_OwnerRef.MaxLength, ownerReference);
				_ = command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				_ = command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusEntryInstruction(Guid declarationPK, DateTime dateForDuty, int clusterKey)
		{
			var entryInstructionPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@entryInstructionPK, 'NO', @declarationPK, @dateForDuty, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				_ = command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				_ = command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				_ = command.AddParameter("@dateForDuty", SqlDbType.SmallDateTime, CusEntryInstructionSchema.CEI_DateForDuty.MaxLength, dateForDuty);
				_ = command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				_ = command.ExecuteNonQuery();
			}
			return entryInstructionPK;
		}

		Guid CreateCusEntryHeader(Guid declarationPK, string entryStatus, DateTime releaseDate, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_EntryStatus, CH_EntryReleaseDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@entryHeaderPK, 'NO', @declarationPK, @entryStatus, @entryReleaseDate, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				_ = command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				_ = command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				_ = command.AddParameter("@entryStatus", SqlDbType.VarChar, CusEntryHeaderSchema.CH_EntryStatus.MaxLength, entryStatus);
				_ = command.AddParameter("@entryReleaseDate", SqlDbType.SmallDateTime, CusEntryHeaderSchema.CH_EntryReleaseDate.MaxLength, releaseDate);
				_ = command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				_ = command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		Guid CreateCusEntryLine(Guid entryHeaderPK, decimal customsValue, decimal valueForVAT, int clusterKey)
		{
			var entryLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_CustomsValue, CL_ValueForVAT, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@entryLinePK, 'NO', @entryHeaderPK, @customsValue, @valueForVAT, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				_ = command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				_ = command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				_ = command.AddParameter("@customsValue", SqlDbType.Decimal, customsValue);
				_ = command.AddParameter("@valueForVAT", SqlDbType.Decimal, valueForVAT);
				_ = command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				_ = command.ExecuteNonQuery();
			}
			return entryLinePK;
		}

		Guid CreateInvoiceHeader(Guid declarationPK, int clusterKey)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey)
VALUES (@invoiceLinePK, 'NO', @declarationPK, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				_ = command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				_ = command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				_ = command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				_ = command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}

		Guid CreateInvoiceLine(Guid invoiceHeaderPK, Guid entryInstructionPK, Guid entryLinePK, int clusterKey)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_CL, JI_CEI, JI_ClusterKey)
VALUES (@invoiceLinePK, 'NO', @invoiceHeaderPK, @entryLinePK, @entryInstructionPK, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				_ = command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				_ = command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				_ = command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				_ = command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				_ = command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				_ = command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var companyPK = TestDataCreator.CreateCompany("DUM", "NO", "NOK");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "OSL", "NOOSL", "NO");
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "ROA", "Reference1", "Reference2", 1);

			importerPK = TestDataCreator.CreateOrganisation("ABC", "Name");
			var importerOrgAddressPK = TestDataCreator.CreateAddress(importerPK, "ABC", "address1");
			_ = TestDataCreator.CreateDocAddress(importerOrgAddressPK, "Importer", declarationPK, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, addressOverride: true);

			_ = TestDataCreator.CreateOrgCusCode(importerPK, "MVA", "123456789", "NO");

			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2025, 03, 01), 1);
			var invoiceHeaderPK1 = CreateInvoiceHeader(declarationPK, 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", new DateTime(2025, 03, 02), 1);
			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 11, 800, 1);
			_ = CreateInvoiceLine(invoiceHeaderPK1, cusEntryInstructionPK, entryLinePK1, 1);
			_ = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "ABCDEF1234567890", "CER", "CUS", "NO");
			_ = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "0011223344556677", "MRN", "CUS", "NO");

			_ = TestDataCreator.CreateCusEntryLineFee(entryLinePK1, "MV1", 100, 1);
			_ = TestDataCreator.CreateCusEntryLineFee(entryLinePK1, "TL1", 200, 1);
			_ = TestDataCreator.CreateCusEntryLineFee(entryLinePK1, "FA200", 400, 1);
		}
		Guid importerPK;

		static class Parameters
		{
			public const string Importer = "@ImporterPK";
			public const string PeriodFrom = "@PeriodFrom";
			public const string Periodto = "@PeriodTo";
		}
	}
}
