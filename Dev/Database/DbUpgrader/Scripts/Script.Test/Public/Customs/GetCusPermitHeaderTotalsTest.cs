using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(GetCusPermitHeaderTotals))]
	class GetCusPermitHeaderTotalsTest : DbCreateScriptTest
	{
		public void TestCusPermitHeaderTotal()
		{
			var organisationPk = TestDataCreator.CreateOrganisation(new string('X', OrgHeaderSchema.OH_Code.MaxLength), new string('X', OrgHeaderSchema.OH_FullName.MaxLength), "CAYVR");
			var guaranteePK = AddGuarantee(organisationPk, "12345");

			AddTransaction(guaranteePK, "CON", true, 498);
			AddTransaction(guaranteePK, "CON", true, 242);

			AddTransaction(guaranteePK, "PND", false, 47);
			AddTransaction(guaranteePK, "PND", false, 3);

			AddTransaction(guaranteePK, "CON", false, 25);
			AddTransaction(guaranteePK, "CON", false, 20);

			//var spSql = "EXEC sp_GetCusPermitHeaderTotals @guaranteePK";
			using (DbCommand command = Db.Connection.Command("GetCusPermitHeaderTotals"))
			{
				command.CommandTimeout = 0; // No timeout
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@cph", SqlDbType.UniqueIdentifier, guaranteePK);
				command.AddOutputParameter("@confirmedBalance", SqlDbType.Decimal, 65535, 0, 0, 0);
				command.AddOutputParameter("@pendingTransactionsBalance", SqlDbType.Decimal, 65535, 0, 0, 0);
				command.AddOutputParameter("@calculationDate", SqlDbType.DateTime, 65535, 0, 0, 0);
				command.ExecuteNonQuery();
				var confirmedBalance = command.GetParameterValue("@confirmedBalance");
				var pendingBalance = command.GetParameterValue("@pendingTransactionsBalance");

				AssertEquals("Confirmed balance should be 5045", (decimal)5045, confirmedBalance);
				AssertEquals("Pending balance should be 50", (decimal)50, pendingBalance);
			}
		}

		public void TestCusPermitHeaderTotal_WhenAllAggregated()
		{
			var organisationPk = TestDataCreator.CreateOrganisation(new string('X', OrgHeaderSchema.OH_Code.MaxLength), new string('X', OrgHeaderSchema.OH_FullName.MaxLength), "CAYVR");
			var guaranteePK = AddGuarantee(organisationPk, "12345");

			AddTransaction(guaranteePK, "CON", true, 498);
			AddTransaction(guaranteePK, "CON", true, 242);

			//var spSql = "EXEC sp_GetCusPermitHeaderTotals @guaranteePK";
			using (DbCommand command = Db.Connection.Command("GetCusPermitHeaderTotals"))
			{
				command.CommandTimeout = 0; // No timeout
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@cph", SqlDbType.UniqueIdentifier, guaranteePK);
				command.AddOutputParameter("@confirmedBalance", SqlDbType.Decimal, 65535, 0, 0, 0);
				command.AddOutputParameter("@pendingTransactionsBalance", SqlDbType.Decimal, 65535, 0, 0, 0);
				command.AddOutputParameter("@calculationDate", SqlDbType.DateTime, 65535, 0, 0, 0);
				command.ExecuteNonQuery();
				var confirmedBalance = command.GetParameterValue("@confirmedBalance");
				var pendingBalance = command.GetParameterValue("@pendingTransactionsBalance");

				AssertEquals("Confirmed balance should be 5000", (decimal)5000, confirmedBalance);
				AssertEquals("Pending balance should be 0", (decimal)0, pendingBalance);
			}
		}

		Guid AddGuarantee(Guid orgPK, string referenceNumber)
		{
			var guaranteePK = Guid.NewGuid();

			var createSql = $@"
INSERT INTO dbo.CusPermitHeader
           (CPH_PK
           ,CPH_RN_NKCountryCode
           ,CPH_OH_PermitHolder
           ,CPH_Number
           ,CPH_StartDate
           ,CPH_EndDate
           ,CPH_SubType
           ,CPH_SystemCreateTimeUtc
           ,CPH_SystemCreateUser
           ,CPH_SystemLastEditTimeUtc
           ,CPH_SystemLastEditUser
           ,CPH_IsClosed
           ,CPH_OA_AppliesTo
           ,CPH_UnitOfMeasure
           ,CPH_QtyValIndicator
           ,CPH_Type
           ,CPH_PermitDescription
           ,CPH_AccessCodeOrPassword
           ,CPH_ApplicationCode
           ,CPH_Balance
           ,CPH_IsActive
           ,CPH_Provider)
VALUES(
'{guaranteePK}',
'FR',
'{orgPK}',
'{referenceNumber}',
GetDate(),
GetDate(),
'',
GetUTCDate(),
'E',
GetUTCDate(),
'E',
0,
NULL,
'EUR',
'VAL',
'GEN',
'',
'',
'GUA',
5000,
1,
''
)";

			using (var command = Db.Connection.Command(createSql))
			{
				command.ExecuteNonQuery();
			}

			return guaranteePK;
		}

		Guid AddTransaction(Guid guaranteePK, string status, bool isAggregated, decimal value)
		{
			var transactionPK = Guid.NewGuid();

			var createSql = $@"
INSERT INTO dbo.CusPermitLineTransaction
           (CPL_PK
           ,CPL_CPH_PermitHeader
           ,CPL_TranQty
           ,CPL_TranValue
           ,CPL_Reference
           ,CPL_Comment
           ,CPL_TransactionDate
           ,CPL_TransactionType
           ,CPL_TransactionCategory
           ,CPL_AppId
           ,CPL_TransactionStatus
           ,CPL_IsAggregated
           ,CPL_Procedure
           ,CPL_ReferenceNumberLine)
 VALUES(
'{transactionPK}',
'{guaranteePK}',
0,
{value},
'Ref',
'',
'{DateTime.Now.ToString("yyyy-MM-dd")}',
'TRA',
'VAL',
'',
'{status}',
{(isAggregated ? 1 : 0)},
'',
0)";

			using (var command = Db.Connection.Command(createSql))
			{
				command.ExecuteNonQuery();
			}

			return transactionPK;
		}
	}
}

