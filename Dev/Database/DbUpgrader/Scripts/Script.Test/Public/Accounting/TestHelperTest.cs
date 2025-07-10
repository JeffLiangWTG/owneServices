using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	static class TestHelper
	{
		public static DateTime DefaultPostDate => new DateTime(2007, 01, 05);

		public static (Guid PK, string Name) LineGLAccount => (new Guid("A17ACD2B-4303-4F08-8E81-56C94353D270"), "GROSS FREIGHT REVENUE");
		public static (Guid PK, string Name) LineGLAccount2 => (new Guid("c9c886ad-611e-4c29-8b2b-85020e0f5e2a"), "GROSS DOCUMENTATION REVENUE");

		public static (Guid PK, string Name) HeaderGLAccount => (new Guid("24D26FE6-8824-4E63-B46E-8696108E5E31"), "BANK CHARGES");
		public static (Guid PK, string Name) HeaderGLAccount2 => (new Guid("ff43d16b-6683-4370-a450-362abaebe51a"), "FINANCE EXPENSES");

		public static (Guid PK, string Code) Branch => (new Guid("FDD429D2-648C-4895-8F9F-06E90DED2BE5"), "SYD");
		public static (Guid PK, string Code) Branch2 => (new Guid("27a55065-ac88-4ec3-8bed-e575e79172cb"), "BNE");

		public static (Guid PK, string Code) Department => (new Guid("2B67864D-42E9-4A43-A9C8-09D2083C4227"), "FIR");
		public static (Guid PK, string Code) Department2 => (new Guid("f5c72696-19ad-4759-879f-89c8532ff238"), "FEA");

		public static Guid InsertGLHeader(string accountNum, string description, string accountType, string statisticalUnits, string debitCredit = "CR", bool isControlAccount = true, string reportSection = "TS")
		{
			var pk = Guid.NewGuid();
			var controlAccount = isControlAccount ? "1" : "0";
			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.CurrentCulture, @"INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_StatisticalUnits, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column, AG_SystemCreateTimeUtc, AG_SystemCreateUser, AG_SystemLastEditTimeUtc, AG_SystemLastEditUser) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, NULL, 0, '{7}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", pk, accountNum, description, accountType, statisticalUnits, debitCredit, controlAccount, reportSection));
			return pk;
		}

		public static Guid InsertGLMappingAccount(string language, string localAccountNumber, Guid parentGLAccountNo, string description, string reportType, string reportCategory, string debitCredit = "CR", string countryOfCompliance = "")
		{
			Guid pK = Guid.NewGuid();
			string sQL = @"INSERT INTO dbo.AccGLAccountDescriptor
							(
								AJ_PK
								,AJ_Language
								,AJ_AccountDescription
								,AJ_LocalAccountNumber
								,AJ_ReportType
								,AJ_DebitCredit
								,AJ_RN_NKCountryOfCompliance
								,AJ_ReportCategory
								,AJ_SystemCreateTimeUtc
								,AJ_SystemCreateUser
								,AJ_SystemLastEditTimeUtc
								,AJ_SystemLastEditUser)
								VALUES
								(@PK
								,@Language
								,@AccountDescription
								,@LocalAccountNumber
								,@ReportType
								,@DebitCredit
								,@CountryOfCompliance
								,@ReportCategory
								,GetUtcDate()
								,'~BP'
								,GetUtcDate()
								,'~BP'
							)";
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pK);
			cmd.AddParameter("@Language", SqlDbType.VarChar, AccGLAccountDescriptorSchema.AJ_Language.MaxLength, language);
			cmd.AddParameter("@AccountDescription", SqlDbType.VarChar, AccGLAccountDescriptorSchema.AJ_AccountDescription.MaxLength, description);
			cmd.AddParameter("@LocalAccountNumber", SqlDbType.VarChar, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber.MaxLength, localAccountNumber);
			cmd.AddParameter("@ReportType", SqlDbType.VarChar, AccGLAccountDescriptorSchema.AJ_ReportType.MaxLength, reportType);
			cmd.AddParameter("@DebitCredit", SqlDbType.VarChar, AccGLAccountDescriptorSchema.AJ_DebitCredit.MaxLength, debitCredit);
			cmd.AddParameter("@CountryOfCompliance", SqlDbType.VarChar, AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance.MaxLength, countryOfCompliance);
			cmd.AddParameter("@ReportCategory", SqlDbType.VarChar, AccGLAccountDescriptorSchema.AJ_ReportCategory.MaxLength, reportCategory);
			cmd.ExecuteNonQuery();
			if (parentGLAccountNo != Guid.Empty)
			{
				string sqlText = @"INSERT INTO dbo.AccGLDescriptorPivot VALUES (NEWID(), @YJ_AG, @AJ_PK, @YJ_SystemCreateTimeUtc, @YJ_SystemCreateUser, @YJ_SystemLastEditTimeUtc, @YJ_SystemLastEditUser)";
				DbCommand command = Db.Connection.Command(sqlText);
				command.AddParameter("@YJ_AG", SqlDbType.UniqueIdentifier, parentGLAccountNo);
				command.AddParameter("@AJ_PK", SqlDbType.UniqueIdentifier, pK);
				command.AddParameter("@YJ_SystemCreateTimeUtc", SqlDbType.SmallDateTime, AccGLDescriptorPivotSchema.YJ_SystemCreateTimeUtc.MaxLength, DateTime.Now);
				command.AddParameter("@YJ_SystemCreateUser", SqlDbType.VarChar, AccGLDescriptorPivotSchema.YJ_SystemCreateUser.MaxLength, "AAA");
				command.AddParameter("@YJ_SystemLastEditTimeUtc", SqlDbType.SmallDateTime, AccGLDescriptorPivotSchema.YJ_SystemLastEditTimeUtc.MaxLength, DateTime.Now);
				command.AddParameter("@YJ_SystemLastEditUser", SqlDbType.VarChar, AccGLDescriptorPivotSchema.YJ_SystemLastEditUser.MaxLength, "AAA");
				command.ExecuteNonQuery();
			}

			return pK;
		}

		public static Guid InsertTransactionLine(
			string lineType,
			Guid? al_ah = null,
			decimal lineAmount = 250,
			DateTime? postDate = null,
			DateTime? reverseDate = null,
			bool useAnotherBranch = false,
			bool useAnotherDepartment = false,
			bool useAnotherGLAccount = false,
			bool useNextPeriodPostDate = false,
			bool useNextPeriodReverseDate = false,
			bool useCommentChargeCode = false,
			bool postToGL = false,
			bool reverseToGL = false)
		{
			var postDateToSet = useNextPeriodPostDate ? DefaultPostDate.AddMonths(1) : (postDate ?? DefaultPostDate);
			var reverseDateToSet = useNextPeriodReverseDate ? DefaultPostDate.AddMonths(1) : (reverseDate ?? DefaultPostDate);

			return new TestDbHelper(Db.Connection).InsertTransactionLine(
				al_ah,
				jobPK: null,
				chargeCodePK: useCommentChargeCode ? new Guid("6fa0ced0-ef5b-4e13-8a77-eb998da8a522") : new Guid("476D9CA0-9004-418C-98CA-3025CC231934"),
				glAccountPK: useAnotherGLAccount ? LineGLAccount2.PK : LineGLAccount.PK,
				branchPK: useAnotherBranch ? Branch2.PK : Branch.PK,
				departmentPK: useAnotherDepartment ? Department2.PK : Department.PK,
				orgHeaderPK: null,
				useCommentChargeCode ? 0 : lineAmount,
				lineType,
				postDateToSet == DateTime.MinValue ? null : postDateToSet,
				reverseDateToSet == DateTime.MinValue ? null : reverseDateToSet,
				taxAmount: useCommentChargeCode || lineAmount == 0 ? 0 : 10,
				taxRecoverable: 0.5m,
				postToGL: postToGL,
				reverseToGL: reverseToGL
				);
		}

		public static void UpdateTransactionLineReveseDate(Guid pk, DateTime reverseDate)
		{
			var updateLineCommand = Db.Connection.Command(@"
					UPDATE dbo.AccTransactionLines
					SET AL_ReverseDate = @AL_ReverseDate,
						AL_SystemLastEditTimeUtc = GETUTCDATE(),
						AL_SystemLastEditUser = 'TST'
					WHERE AL_PK = @AL_PK");
			updateLineCommand.AddParameterBasedOnDbColumn("@AL_PK", pk, AccTransactionLinesSchema.PK);
			updateLineCommand.AddParameterBasedOnDbColumn("@AL_ReverseDate", reverseDate, AccTransactionLinesSchema.AL_ReverseDate);
			updateLineCommand.ExecuteNonQuery();
		}

		public static Guid InsertTransactionHeader(
			string ledger,
			string transactionType,
			Guid? account = null,
			decimal invoiceAmount = 0m,
			DateTime? transactionDate = null,
			string transactionNumber = null,
			bool useAnotherBranch = false,
			bool useAnotherDepartment = false,
			bool useAnotherGLAccount = false,
			bool useNextPeriodPostDate = false,
			bool postToGL = false)
		{
			var transactionNumberToSet = transactionNumber ?? Guid.NewGuid().ToString("n");
			var postDate = useNextPeriodPostDate ? DefaultPostDate.AddMonths(1) : (transactionDate ?? DefaultPostDate);
			var invoiceDate = transactionDate.HasValue && transactionDate != DateTime.MinValue ? transactionDate : new DateTime(2007, 01, 01, 01, 01, 01);

			return new TestDbHelper(Db.Connection).InsertTransactionHeader(
				ledger,
				transactionType,
				transactionNumberToSet,
				invoiceAmount,
				postDate: postDate == DateTime.MinValue ? null : postDate,
				invoiceDate: invoiceDate,
				branchPK: useAnotherBranch ? Branch2.PK : Branch.PK,
				departmentPK: useAnotherDepartment ? Department2.PK : Department.PK,
				bankAccountPK: account,
				glAccountPK: useAnotherGLAccount ? HeaderGLAccount2.PK : HeaderGLAccount.PK,
				postToGL: postToGL
				);
		}

		public static void AssertProcedureMustRunInTransaction(string procedureName, Action<DbCommand> addParameters)
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assertion.AssertExceptionThrown(
					"Attempt to execute the procedure outside of a transaction context",
					typeof(SqlException),
					"This procedure must be executed in a transaction.",
					() => connection.ExecuteNonQuery(procedureName, addParameters)
				);
			}
		}

		public static void AssertTransactionCountIsTheSameBeforeAndAfterExecution(DbConnection testConnection, string procedureName, string tableToLockAndForceTimeout, Action<DbCommand> addParameters)
		{
			Assertion.AssertEquals("Test connection must be in a transaction to execute this test. Please use a TransactionedTestCase.", 1, testConnection.AppTransactionCount);

			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				// Ensure procedure is compiled before putting an Sch-M lock on the table used by it.
				// Otherwise it will timeout at compilation, without even executing it.
				EnsureProcedureIsCompiled(otherConnection, procedureName, addParameters);

				// Adding an extended property to the table puts a Sch-M lock and forces a timeout when running the procedure.
				testConnection.ExecuteNonQuery($"EXEC sp_addextendedproperty @name = N'{Guid.NewGuid().ToString()}', @value = '.', @level0type = N'Schema', @level0name = 'dbo', @level1type = N'Table', @level1name = '{tableToLockAndForceTimeout}';");

				using (otherConnection.BeginTransactionWithManager())
				{
					int tranCountBefore = Convert.ToInt32(otherConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
					Assertion.AssertEquals("Transaction count before execution", 1, tranCountBefore);

					try
					{
						using (var cmd = otherConnection.Command(procedureName, 3))
						{
							addParameters(cmd);
							cmd.ExecuteNonQuery();
						}
					}
					catch (SqlException ex)
					{
						Assertion.AssertEquals("Expected forced error", DbErrorType.TimeoutExpired, new DbErrorMatch(ex).ExceptionType);
						int tranCountAfter = Convert.ToInt32(otherConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
						Assertion.AssertEquals("Transaction count after forced error", tranCountBefore, tranCountAfter);
					}
				}
			}
		}

		public static string CreateGLAggregate(TestDbHelper dBHelper, string categoryPrefix, int categoryGroupCount, decimal amount, int period, Guid glAccountPK, Guid departmentPK, Guid branchPK, Guid companyPK)
		{
			var categotyGroupBuilder = new StringBuilder();
			for (int index = 0; index < categoryGroupCount; index++)
			{
				var category = string.IsNullOrEmpty(categoryPrefix) ? categoryPrefix : $"{categoryPrefix}{index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
				dBHelper.InsertGLAggregate(amount, category, period, glAccountPK, branchPK, departmentPK, companyPK);
				categotyGroupBuilder.Append($"{category},");
			}
			return categotyGroupBuilder.ToString().TrimEnd(',');
		}

		public static string CreateTransactionHeaderByTransactionType(string categoryPrefix, int categoryGroupCount, string transactionType, string transactionNumPrefix, Action<string, string, string> func)
		{
			var categotyGroupBuilder = new StringBuilder();

			for (int index = 0; index < categoryGroupCount; index++)
			{
				var numberString = index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
				var category = $"{categoryPrefix}{numberString}";
				var transactionNum = $"{transactionNumPrefix}{numberString}";
				func(transactionType, transactionNum, category);
				categotyGroupBuilder.Append($"{category},");
			}
			return categotyGroupBuilder.ToString().TrimEnd(',');
		}

		static void EnsureProcedureIsCompiled(DbConnection connection, string procedureName, Action<DbCommand> addParameters)
		{
			try
			{
				connection.ExecuteNonQuery(procedureName, addParameters);
			}
			catch (SqlException ex)
			{
				if (ex.Message != "This procedure must be executed in a transaction.")
				{
					throw;
				}
			}
		}

		internal static void AddParameterSystemLastEditUser(DbCommand command)
		{
			command.CommandType = CommandType.StoredProcedure;
			command.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, "TST");
		}
	}
}
