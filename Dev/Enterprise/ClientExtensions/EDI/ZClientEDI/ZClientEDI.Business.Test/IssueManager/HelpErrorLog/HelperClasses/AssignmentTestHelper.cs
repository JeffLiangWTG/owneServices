using System;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public abstract class AssignmentTestHelper : TestCaseWithFactory
	{
		public string GetExceptionXml(string callElements)
		{
			return @"<EDI_Exception_Report>
				<ExceptionDetails>
					<StackTrace>" +
						callElements + @"
					</StackTrace>
				</ExceptionDetails>
			</EDI_Exception_Report>";
		}

		public void AddStackLineCount(string assembly, string stackLine, int count)
		{
			AddStackLineCount(Factory, assembly, stackLine, count);
		}

		public static void AddStackLineCount(BusinessObjectFactory factory, string assembly, string stackLine, int count)
		{
			var stackLineCount = factory.New<HelpErrorStackLineCount>();
			stackLineCount.HSL_Assembly = assembly;
			stackLineCount.HSL_StackLine = stackLine;
			stackLineCount.HSL_Count = count;
		}

		public static Guid AddPublishedAssembly(string assembly, string sourcePath)
		{
			var pk = Guid.NewGuid();
			const string insert = @"
				INSERT INTO PublishedAssemblies (PA_PK ,PA_AssemblyName ,PA_SourcePath)
				 VALUES (@AssemblyPk, @AssemblyName, @SourcePath)";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(insert))
				{
					command.AddParameter("@AssemblyPk", SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@AssemblyName", SqlDbType.NVarChar, assembly);
					command.AddParameter("@SourcePath", SqlDbType.NVarChar, sourcePath);
					command.ExecuteNonQuery();
				}
			}
			return pk;
		}

		public static void AddSourceTreeResponsibility(string path, string product, string productArea, string module)
		{
			string sourceTreeResponsibilityInsert = @"INSERT INTO SourceTreeResponsibility
				(ST_Path, ST_Product, ST_Product_Area, ST_Module)
				VALUES
				(@Path, @Product, @ProductArea, @Module)";

			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(sourceTreeResponsibilityInsert))
				{
					command.AddParameter("@Path", SqlDbType.VarChar, path);
					command.AddParameter("@Product", SqlDbType.VarChar, 3, product);
					command.AddParameter("@ProductArea", SqlDbType.VarChar, 3, productArea);
					command.AddParameter("@Module", SqlDbType.VarChar, 3, module);
					command.ExecuteNonQuery();
				}
			}
		}

		public static Guid AddPublishedClass(Guid assemblyPk, string className)
		{
			var pk = Guid.NewGuid();
			const string insert = @"
				INSERT INTO PublishedClasses (PC_PK ,PC_Assembly ,PC_ClassName)
				 VALUES	(@ClassPk, @AssemblyPk, @ClasssName)";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(insert))
				{
					command.AddParameter("@ClassPk", SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@AssemblyPk", SqlDbType.UniqueIdentifier, assemblyPk);
					command.AddParameter("@ClasssName", SqlDbType.NVarChar, className);
					command.ExecuteNonQuery();
				}
			}
			return pk;
		}

		public static Guid AddPublishedMethod(Guid classPk, string methodName)
		{
			return AddPublishedMethod(classPk, methodName, DateTimeOffset.Now);
		}

		public static Guid AddPublishedMethod(Guid classPk, string methodName, DateTimeOffset lastSeen)
		{
			var pk = Guid.NewGuid();
			const string insert = @"
				INSERT INTO PublishedMethods (PM_PK, PM_Class, PM_MethodName, PM_LastSeen)
				 VALUES	(@MethodPk, @ClassPk, @MethodName, @LastSeen)";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(insert))
				{
					command.AddParameter("@MethodPk", SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@ClassPk", SqlDbType.UniqueIdentifier, classPk);
					command.AddParameter("@MethodName", SqlDbType.NVarChar, methodName);
					command.AddParameter("@LastSeen", SqlDbType.DateTimeOffset, lastSeen);
					command.ExecuteNonQuery();
				}
			}
			return pk;
		}

		public EdiHelpErrorLog CreateLog(ZDateTime? firstProcessed = null)
		{
			var log = Factory.New<EdiHelpErrorLog>();
			log.HE_LogType = "EXC";
			log.HE_ExceptionType = typeof(OutOfMemoryException).ToString();
			log.HE_ExceptionSource = "CargoWiseOne.exe";
			log.HE_FirstReported = ZDateTime.Now;
			log.HE_FirstProcessed = firstProcessed ?? ZDateTime.Now;
			log.HE_LastReported = ZDateTime.Now;
			log.HE_IssueNumber = "1200000";
			return log;
		}

		public static void CreateLogOccurrence(EdiHelpErrorLog log, string exceptionContent, ZDateTime? exeDate = null, ZDateTime? exceptionDateTime = null)
		{
			var logOccurrence = log.Factory.New<HelpErrorLogOccurrence>();
			logOccurrence.HO_HE = log.PK;
			logOccurrence.HO_EXEDateTime = exeDate ?? ZDateTime.Now;
			if (exceptionDateTime is not null)
			{
				logOccurrence.HO_ExceptionDateTime = (ZDateTime)exceptionDateTime;
			}
			logOccurrence.HO_XMLData = exceptionContent;
		}

		static int DeleteAutoTesterData(string tableName, string columnName, Guid columnPK)
		{
			var rowsAffected = 0;
			const string delete = @"
				DELETE FROM {0}
				WHERE {1} = @ColumnPK";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(string.Format(delete, tableName, columnName, CultureInfo.InvariantCulture)))
				{
					command.AddParameter("@ColumnPK", SqlDbType.UniqueIdentifier, columnPK);
					rowsAffected = command.ExecuteNonQuery();
				}
			}
			return rowsAffected;
		}

		public static int DeletePublishedAssembly(Guid assemblyPK)
		{
			return DeleteAutoTesterData("PublishedAssemblies", "PA_PK", assemblyPK);
		}

		public static int DeleteSourceTreeResponsibility(string sourcePath)
		{
			var rowsAffected = 0;
			const string delete = @"
				DELETE FROM SourceTreeResponsibility
				WHERE ST_Path = @SourcePath";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(delete))
				{
					command.AddParameter("@SourcePath", SqlDbType.VarChar, sourcePath);
					rowsAffected = command.ExecuteNonQuery();
				}
			}
			return rowsAffected;
		}

		public static int DeletePublishedClass(Guid classPK)
		{
			return DeleteAutoTesterData("PublishedClasses", "PC_PK", classPK);
		}

		public static int DeletePublishedMethod(Guid methodPK)
		{
			return DeleteAutoTesterData("PublishedMethods", "PM_PK", methodPK);
		}
	}
}
