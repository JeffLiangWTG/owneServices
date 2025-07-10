using System;
using System.Text;
using System.Xml;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(CopyEnableAddAndEditLogsToEnableAddEditAndDeleteLogsItems))]
	public class CopyEnableAddAndEditLogsToEnableAddEditAndDeleteLogsItemsWithOverrideDefaultTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CopyEnableAddAndEditLogsToEnableAddEditAndDeleteLogsItems();
		}

		readonly string TestData = @"<?xml version=""1.0"" encoding=""utf-16""?>
	<ArrayOfCodeDescriptionBoolDisallowNewCodeReadOnly xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
	xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<CodeDescriptionBoolDisallowNewCodeReadOnly><CodeMaxLength>50</CodeMaxLength><Code>ProcessTask</Code>
	<Description>ProcessTask</Description>
	<Bool>N</Bool>
	<SystemDefined>False</SystemDefined>
	</CodeDescriptionBoolDisallowNewCodeReadOnly></ArrayOfCodeDescriptionBoolDisallowNewCodeReadOnly>";
		readonly string SourceSDName = "EnableAddAndEditLogs";
		readonly string DestinationSDName = "EnableAddEditAndDeleteLogsItems";

		protected override void PrepareTestData()
		{
			Helper.DeleteStmDataRow(SourceSDName);
			Helper.InsertStmDataRow(SourceSDName, "BIN", Encoding.Unicode.GetBytes(TestData));
			Helper.DeleteStmDataRow(DestinationSDName);
			Helper.InsertStmDataRow(DestinationSDName, "BIN", Array.Empty<byte>());
		}

		protected override void AssertTransformationResults()
		{
			var value = Helper.GetStmDataValue(DestinationSDName);
			var xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(Encoding.Unicode.GetString(value));
			}
			catch (Exception)
			{
				return;
			}

			var xmlNode = xmlDocument.SelectSingleNode("ArrayOfEnableAddEditAndDeleteLogsItem");

			var enableAddEditAndDeleteLogsItem = xmlNode.SelectSingleNode("EnableAddEditAndDeleteLogsItem");
			AssertNotNull(enableAddEditAndDeleteLogsItem);

			var table = enableAddEditAndDeleteLogsItem.SelectSingleNode("Table");
			AssertNotNull(table);

			AssertEquals(ProcessTasksSchema.Constants.TableName, table.InnerText);
			AssertEquals("N", enableAddEditAndDeleteLogsItem.SelectSingleNode("EnableADDLogs").InnerText);
			AssertEquals("N", enableAddEditAndDeleteLogsItem.SelectSingleNode("EnableEDTLogs").InnerText);
			AssertEquals("N", enableAddEditAndDeleteLogsItem.SelectSingleNode("EnableDELLogs").InnerText);

			var sql = $"SELECT count(*) FROM dbo.StmData WHERE SD_Name = '{SourceSDName}'";
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				AssertEquals(0, (int)reader[0]);
			}
		}
	}

	[TestedType(typeof(CopyEnableAddAndEditLogsToEnableAddEditAndDeleteLogsItems))]
	public class CopyEnableAddAndEditLogsToEnableAddEditAndDeleteLogsItemsWithoutOverrideDefaultTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CopyEnableAddAndEditLogsToEnableAddEditAndDeleteLogsItems();
		}

		readonly string SourceSDName = "EnableAddAndEditLogs";
		readonly string DestinationSDName = "EnableAddEditAndDeleteLogsItems";

		protected override void PrepareTestData()
		{
		}

		protected override void AssertTransformationResults()
		{
			var sql = $"SELECT count(*) FROM dbo.StmData WHERE SD_Name = '{SourceSDName}'";
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				AssertEquals(0, (int)reader[0]);
			}

			sql = $"SELECT count(*) FROM dbo.StmData WHERE SD_Name = '{DestinationSDName}'";
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				AssertEquals(0, (int)reader[0]);
			}
		}
	}
}
