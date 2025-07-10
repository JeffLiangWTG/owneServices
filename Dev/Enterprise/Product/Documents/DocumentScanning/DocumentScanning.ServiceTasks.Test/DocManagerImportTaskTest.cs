using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[TestedType(typeof(DocManagerImportTask))]
	class DocManagerImportTaskTest : ServiceTaskTestCase<DocManagerImportTask>
	{
		public void TestEnsureHostedServiceBusinessObjectBindingAttributesAdded()
		{
			int matchCount = 0;
			foreach (var attribute in AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>())
			{
				if (attribute.ServiceTaskCode == "DMI" && attribute.Table == MailDBItemsSchema.Constants.TableName)
				{
					matchCount++;
				}
			}
			AssertEquals(2, matchCount);
		}

		public void TestInvalidXmlInImportDirectoryPath()
		{
			TestConnection.ExecuteNonQuery(@"
IF EXISTS (select 1 from StmData where SD_Name = 'DocManagerBatchProcessorImportOptions')
BEGIN
	UPDATE StmData
SET SD_BinaryValue = CAST(N'<?xml version=""1.0"" encoding=""utf-16""?><DirectorySearch><DirectoryPath/>' AS varbinary(max))
WHERE SD_Name = 'DocManagerBatchProcessorImportOptions'
END
ELSE
BEGIN
	INSERT INTO StmData (SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled, SD_PreserveTestValue, SD_SystemCreateUser,
	SD_SystemCreateTimeUtc, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc)
	VALUES(NEWID(), 'DocManagerBatchProcessorImportOptions', 'BIN',
	CAST(N'<?xml version=""1.0"" encoding=""utf-16""?><DirectorySearch><DirectoryPath/>' AS varbinary(max)),
	1, 0, 1, 'E', GETDATE(), 'E', GETDATE())
END");

			var originalIsUserInteractive = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = originalIsUserInteractive))
			{
				Globals.IsUserInteractive = false;
				var importTask = new DocManagerImportTask();
				try
				{
					importTask.RunTask();
					Fail("Expected HostedServiceException to be thrown");
				}
				catch (HostedServiceException e)
				{
					AssertEquals($"Import directory is invalid, please check Directory Path in {SystemDataRegistry.Instance.DocManagerBatchProcessorImportOptions.GetLocation()}", e.Message);
				}
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
