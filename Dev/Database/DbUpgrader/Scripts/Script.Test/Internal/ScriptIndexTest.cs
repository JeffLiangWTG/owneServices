using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing
{
	sealed class ScriptIndexTest : TestCase
	{
		public void TestNoNewScalarFunctions()
		{
			var scalarCount = (int)Db.Connection.ExecuteScalar(
@"select count(*) from sys.objects where type = 'FN' and name not in 
(
'AppendDateBetweenToWhere'
,'AppendDateToWhere'
,'AppendIntToWhere'
,'AppendUIToWhere'
,'AppendVARCHARToWhere'
,'BeginNest'
,'csfn_GetAssignedStaff'
,'csfn_ListStaffCodeForSelectRole'
,'csfn_ListStaffNameForSelectRole'
,'csfn_ListStaffPKForSelectRole'
,'EndNest'
,'FinishUpWhere'
,'GetAddressPksFilteredForOrg'
,'GetAddressPksForOrg'
,'GetComplianceSequenceVoidedFromNumber'
,'GetComplianceSequenceVoidedToNumber'
,'GetCurrentNestLevel'
,'GetDateInLocalTime'
--,'GetJD_OrderNumberAndSplit' --Customs
,'GetNextAvailableUniqueStaffCode'
,'GetNextDateAsDateTime' -- Used only with variables as input
,'GetNextDateAsSmallDateTime' -- Used only with variables as input
,'GetNextNestLevel'
,'GetNumberOfExamCampaignQuestions'
,'GetPeriodFromDate'
,'GetRoleDescription'
,'GetUpdateLastEditAuditColumns'
,'GetUpdateMask'
,'GetValidEndDate'
,'GetValidOperand'
,'GroupsForContact'
,'IsLocationPartOfRegion'
,'IsNestEmpty'
,'MainAddressPkForOrg'
,'OtherAtendeesList'
,'PLAppropriationAccount'
,'PLAppropriationAccountAcc'
,'PLAppropriationAccountAccBudget'
,'PLAppropriationAccountBudget'
,'RemoveFirstOperand'
,'RemoveLastOperand'
,'RetainedEarnings'
,'RetainedEarningsAcc'
,'RetainedEarningsAccBudget'
,'RetainedEarningsAccDep'
,'RetainedEarningsBudget'
,'TemplatesForMenuItem'
,'TrainListReportFooter'
,'ValidateAccountingWebServiceLogin'
,'WhsFormatLocationComponent'
,'WhsLocationFormatter'
,'GetAddInfoValueFromCodeAsSmallDateTime'
,'GetAddInfoValueFromCode'
,'GetAddInfoValueFromCodeAsBoolean'
,'csfn_FindPossibleOversizePackline'
,'ListContainsPK'
)");
			AssertEquals("You have just added a new scalar function - use an inline function instead. https://wisetechglobal.sharepoint.com/:p:/r/Development/Development%20Team%20Workspace/_layouts/15/WopiFrame.aspx?sourcedoc=%7BBC916138-4569-43A0-9682-68BEB8C9A8F5%7D&file=IC%207.5%20-%20User%20Defined%20Functions.pptx&action=default&DefaultItemOpen=1", 0, scalarCount);
		}

		public void TestDefaultClientMainSchemaDoesNotContainIncidentMain()
		{
			var result = Db.Connection.ExecuteScalar(@"SELECT Table_Name FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IncidentMain'");
			AssertNull("You have added the table IncidentMain to the main schema. ViewProcessHeader has been designed with Incidents being for EDI only, and if this is being changed, ViewProcessHeader will need to be changed as well. Currently, ViewProcessHeader excludes all from its selection by excluding all rows where FH_WorkflowType = 'INC'. For EDI, incidents are then unioned with the ViewProcessHeader by overriding ViewClientProcessHeader.", result);
		}
	}

	class ScriptIndexDependencyOrderNonTransactionalTest : TestCase
	{
		public void TestScriptsAreListedInOrderOfDependency()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					DropServiceBrokerObjects(connection);
					DropAllRoutines(connection);
					var allScriptsInIndexingOrder = CoreScriptIndex.GetScripts();

					foreach (IDbScript script in allScriptsInIndexingOrder)
					{
						try
						{
							connection.ExecuteNonQuery(script.Text);

							if (script is IIndexedViewDbScript)
							{
								connection.ExecuteNonQuery(((IIndexedViewDbScript)script).IndexCreateScript);
							}
						}
						catch (Exception ex)
						{
							string message = string.Format("\r\nScipt name: {0}\r\nException message: {1}\r\n", script.Name, ex.Message);
							throw new Exception(message, ex);
						}
					}
					AssertNoMissingDependencies(connection);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		void DropAllRoutines(DbConnection connection)
		{
			string sqlText = @"
declare @cmd varchar(max) = 'KeepGoing'
while (@cmd <> '') BEGIN
	select @cmd = '';
				SELECT
					@cmd += 'DROP TRIGGER [' + name + '] ON DATABASE'
				FROM
					sys.triggers
				WHERE
					parent_class = 0
					AND is_ms_shipped = 0
	if (@cmd <> '') exec (@cmd)
END

SET @cmd = 'KeepGoing'
while (@cmd <> '') BEGIN
	select @cmd = '';
				SELECT
					@cmd += 'DROP ' +
					case obj.type
						when 'V'  then 'VIEW'
						when 'P'  then 'PROCEDURE'
						when 'TR' then 'TRIGGER'
						else 'FUNCTION'
					end +
					' [' + usr.name + '].[' + obj.name + ']'
				FROM
					sys.objects obj
					LEFT JOIN sys.schemas usr ON usr.schema_id = obj.schema_id
					LEFT JOIN sys.sql_expression_dependencies AS dep ON dep.referenced_id = obj.object_id
				WHERE
					obj.type in ('V','P','FN','TF','IF','TR')
					AND obj.is_ms_shipped = 0
					AND dep.referenced_id is null
				ORDER BY
					case obj.type
						when 'TR' then 1
						else 2
					end ASC
	if (@cmd <> '') exec (@cmd)
END";

			connection.ExecuteNonQuery(sqlText);
		}

		void DropServiceBrokerObjects(DbConnection connection)
		{
			try
			{
				var sqlText = @"
declare @Commands nvarchar(max) = '';
select @Commands = @Commands + 'drop service [' + name + '];' from sys.services where service_id >= 2^16;
select @Commands = @Commands + 'drop contract [' + name + '];' from sys.service_contracts where service_Contract_id >= 2^16;
select @Commands = @Commands + 'drop QUEUE [' + name + '];' from sys.service_queues where is_ms_shipped = 0;
select @Commands = @Commands + 'drop message type [' + name + '];' from sys.service_message_types where message_type_id >= 2^16;
execute sp_executesql @Commands;";

				using (var cmd = connection.Command(sqlText))
				{
					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				string errorMessage = string.Format("Failed to drop Service Broker objects.\r\n{0}\r\n", e.Message);
				throw new Exception(errorMessage, e);
			}
		}

		void AssertNoMissingDependencies(DbConnection connection)
		{
			string sqlText = string.Format(@"
				DECLARE @MissingDependencies varchar(max);

				SELECT
					@MissingDependencies = ISNULL(@MissingDependencies + ',' + char(13), '') + '[' + obj.name + '] => missing reference [' + sed.referenced_entity_name + ']'
				FROM
					sys.sql_expression_dependencies sed
					INNER JOIN sys.objects obj ON obj.object_id = sed.referencing_id
					LEFT JOIN (
						sys.schemas refObjSch
						INNER JOIN sys.objects refObj ON refObj.schema_id = refObjSch.schema_id
					) ON refObjSch.name = sed.referenced_schema_name AND refObj.name = sed.referenced_entity_name
					LEFT JOIN (
						sys.schemas refTypSch
						INNER JOIN sys.types refTyp ON refTyp.schema_id = refTypSch.schema_id
					) ON refTypSch.name = sed.referenced_schema_name AND refTyp.name = sed.referenced_entity_name
				WHERE
					sed.referenced_database_name is null
					AND sed.referenced_schema_name not in ('{0}')
					AND REPLACE(sed.referenced_entity_name, 'Queue', 'Procedure') <> obj.name
					AND refObj.object_id is null
					AND refTyp.system_type_id is null
					AND sed.referenced_entity_name not in ('STIsEmpty', 'STEquals', 'query', 'value')

				SELECT ISNULL(@MissingDependencies, '');",
				string.Join("', '", Db.SqlReservedSchemas));

			string missingDependencies = connection.ExecuteScalar(sqlText).ToString();
			Assert(
					missingDependencies,
					string.IsNullOrEmpty(missingDependencies));
		}
	}
}
