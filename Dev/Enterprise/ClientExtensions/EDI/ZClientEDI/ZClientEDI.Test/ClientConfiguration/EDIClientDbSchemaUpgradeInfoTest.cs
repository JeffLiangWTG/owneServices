using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(EDIClientDbSchemaUpgradeInfo))]
	public class EDIClientDbSchemaUpgradeInfoTest : ConstraintForClientSpecificSchema
	{
		readonly Regex TableDropRegex = new Regex(@"^\s*DROP\s+(TABLE\s+)(dbo\.)?(?<TableName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		readonly Regex IndexDropRegex = new Regex(@"^\s*DROP\s+(INDEX\s+)(dbo\.)?(?<TableName>\w+)(\.)(?<IndexName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		readonly Regex ConstraintDropRegex = new Regex(@"^\s*ALTER\s+(TABLE\s+)(?<TableName>\w+)(\s+DROP\s+)(CONSTRAINT\s+)(?<ConstraintName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		readonly Regex ProcedureDropRegex = new Regex(@"^\s*DROP\s+(PROCEDURE\s+)(dbo\.)?(?<ProcedureName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		readonly Regex FunctionDropRegex = new Regex(@"^\s*DROP\s+(FUNCTION\s+)(dbo\.)?(?<FunctionName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		readonly Regex TriggerDropRegex = new Regex(@"^\s*DROP\s+(TRIGGER\s+)(dbo\.)?(?<TriggerName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		readonly Regex ViewDropRegex = new Regex(@"^\s*DROP\s+(VIEW\s+)(dbo\.)?(?<ViewName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		[ExpectNoExceptions]
		public void TestDbSchemaUpgradeInfo()
		{
			Db.Connection.BeginTransaction();
			try
			{
				var scripts = new List<DatabaseObjectCreateScript>();
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts);
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);
				scripts.Reverse();
				foreach (var script in scripts)
				{
					var dropScriptExist = checkDropTable(script.DropScript) || checkDropIndex(script.DropScript) || checkDropConstraint(script.DropScript)
							|| checkDropProcedure(script.DropScript) || checkDropFunction(script.DropScript) || checkDropTrigger(script.DropScript)
							|| checkDropView(script.DropScript);
					if (dropScriptExist)
					{
						var tempList = script.DropScript.Split(' ');
						var temp = tempList[tempList.Length - 1].Trim();
						DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(Db.Connection, Db.SqlDbOwnerSchema, temp);
						ExecuteNonQuery(script.DropScript);
					}
				}

				scripts.Reverse();
				foreach (var script in scripts)
				{
					ExecuteNonQuery(script.CreateScript);
				}

				scripts.Reverse();
				foreach (var script in scripts)
				{
					ExecuteNonQuery(script.DropScript);
				}
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		bool checkDropView(string dropScript)
		{
			var scriptMatch = ViewDropRegex.Match(dropScript);

			if (!scriptMatch.Success)
			{
				return false;
			}

			var viewName = scriptMatch.Groups["ViewName"].Value;
			using (var cmd = Db.Connection.Command(string.Format("SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = N'{0}'", viewName)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read();
				}
			}
		}

		bool checkDropTrigger(string dropScript)
		{
			var scriptMatch = TriggerDropRegex.Match(dropScript);

			if (!scriptMatch.Success)
			{
				return false;
			}

			var triggerName = scriptMatch.Groups["TriggerName"].Value;
			using (var cmd = Db.Connection.Command(string.Format("SELECT 1 FROM SYSOBJECTS WHERE ID = OBJECT_ID(N'{0}') AND OBJECTPROPERTY (ID, N'IsTrigger')=1", triggerName)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read();
				}
			}
		}

		bool checkDropFunction(string dropScript)
		{
			var scriptMatch = FunctionDropRegex.Match(dropScript);

			if (!scriptMatch.Success)
			{
				return false;
			}

			var functionName = scriptMatch.Groups["FunctionName"].Value;
			using (var cmd = Db.Connection.Command(string.Format("SELECT 1 FROM SYS.OBJECTS WHERE NAME = '{0}' AND TYPE IN ('FN', 'IF', 'TF')", functionName)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read();
				}
			}
		}

		bool checkDropProcedure(string dropScript)
		{
			var scriptMatch = ProcedureDropRegex.Match(dropScript);

			if (!scriptMatch.Success)
			{
				return false;
			}

			var procedureName = scriptMatch.Groups["ProcedureName"].Value;
			using (var cmd = Db.Connection.Command(string.Format("SELECT 1 FROM SYS.OBJECTS WHERE NAME = '{0}' AND TYPE = 'P'", procedureName)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read();
				}
			}
		}

		bool checkDropConstraint(string dropScript)
		{
			var scriptMatch = ConstraintDropRegex.Match(dropScript);

			if (!scriptMatch.Success)
			{
				return false;
			}

			var tableName = scriptMatch.Groups["TableName"].Value;
			var constraintName = scriptMatch.Groups["ConstraintName"].Value;
			using (var cmd = Db.Connection.Command(string.Format("SELECT 1 FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'dbo.{0}') AND parent_object_id = OBJECT_ID(N'{1}')", constraintName, tableName)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read();
				}
			}
		}

		bool checkDropIndex(string dropScript)
		{
			var scriptMatch = IndexDropRegex.Match(dropScript);

			if (!scriptMatch.Success)
			{
				return false;
			}

			var tableName = scriptMatch.Groups["TableName"].Value;
			var indexName = scriptMatch.Groups["IndexName"].Value;
			using (var cmd = Db.Connection.Command(string.Format("SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('{0}', N'U') and NAME='{1}'", tableName, indexName)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read();
				}
			}
		}

		bool checkDropTable(string dropScript)
		{
			var scriptMatch = TableDropRegex.Match(dropScript);

			if (!scriptMatch.Success)
			{
				return false;
			}

			var tableName = scriptMatch.Groups["TableName"].Value;
			using (var cmd = Db.Connection.Command(string.Format("SELECT 1 FROM sys.objects WHERE name = '{0}'", tableName)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read();
				}
			}
		}

		public void TestClientStatisticsXMLArchiveMatchesClientStatisticsXML()
		{
			var originalScript = ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts.Single(script => script.ObjectName == "ClientStatisticsXML").CreateScript;
			originalScript = originalScript.Substring(0, originalScript.IndexOf(";"));
			var archiveScript = ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts.Single(script => script.ObjectName == "ClientStatisticsXMLArchive").CreateScript;
			archiveScript = archiveScript.Substring(0, archiveScript.IndexOf(";"));
			AssertEquals(originalScript.Replace("IM_", "IMA_").Replace("ClientStatisticsXML", "ClientStatisticsXMLArchive"), archiveScript);
		}

		void ExecuteNonQuery(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();
		}

		#region Constants Assertions
		public void Test_ClientLicenceFeeValue_AccChargeCode_MaxMatchesMasterFile() => AssertEquals(Enterprise.MasterFiles.Business.AutoAccChargeCode.Schema.AC_CodeMaxLength, ClientLicenceFeeSchema.L8_ChargeCode.MaxLength);
		public void Test_EdiTokenAuthOnBoardingData_TOD_VerificationUsername_MaxMatchesMasterFile() => AssertEquals(Enterprise.MasterFiles.Business.AutoGlbStaff.Schema.GS_EmailAddressMaxLength, EdiTokenAuthOnBoardingDataSchema.TOD_VerificationUsername.MaxLength);
		public void Test_EdiTokenAuthOnBoardingData_TOD_VerificationUserPassword_MaxMatchesMasterFile() => AssertEquals(Enterprise.MasterFiles.Business.GlbStaff.Schema.UserPasswordPlainTextMaxLength, EdiTokenAuthOnBoardingDataSchema.TOD_VerificationUserPassword.MaxLength);
		public void Test_ClientStaff_LS_FullName() => AssertEquals(Enterprise.MasterFiles.Business.AutoGlbStaff.Schema.GS_FullNameMaxLength, ClientStaffSchema.LS_FullName.MaxLength);
		public void Test_ClientStaff_LS_Email() => AssertEquals(Enterprise.MasterFiles.Business.AutoGlbStaff.Schema.GS_EmailAddressMaxLength, ClientStaffSchema.LS_Email.MaxLength);
		#endregion
	}
}
