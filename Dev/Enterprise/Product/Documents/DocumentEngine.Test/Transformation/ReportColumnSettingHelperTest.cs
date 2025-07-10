using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ReportColumnSettingHelperTest : TestCaseWithFactory
	{
		public void TestUpdateReportColumnSettingSheetNames()
		{
			var reportId1 = Guid.NewGuid();
			var reportId2 = Guid.NewGuid();
			var dataId1 = Guid.NewGuid();
			var dataId2 = Guid.NewGuid();
			var configName1 = ReportColumnSettingRegistryPrefixHelper.RegistryKeyPrefix + "TestConfig1";
			var configName2 = ReportColumnSettingRegistryPrefixHelper.RegistryKeyPrefix + "TestConfig2";

			var settings1 = new ReportColumnSettings();
			settings1.Worksheets.AddNew("OldSheet1");
			settings1.Worksheets.AddNew("OldSheet2");
			settings1.Worksheets[0].ColumnHeadings.Add(new ColumnHeading("Column1", "Description1", "Heading1", 1, 1, 100, false));
			settings1.Worksheets[1].ColumnHeadings.Add(new ColumnHeading("Column2", "Description2", "Heading2", 1, 1, 100, false));

			var settings2 = new ReportColumnSettings();
			settings2.Worksheets.AddNew("AnotherOldSheet1");
			settings2.Worksheets.AddNew("AnotherOldSheet2");
			settings2.Worksheets[0].ColumnHeadings.Add(new ColumnHeading("Column1", "Description1", "Heading1", 1, 1, 100, false));
			settings2.Worksheets[1].ColumnHeadings.Add(new ColumnHeading("Column2", "Description2", "Heading2", 1, 1, 100, false));

			PrepareTestData(reportId1, dataId1, configName1, settings1);
			PrepareTestData(reportId2, dataId2, configName2, settings2);

			var helper = ObjectFactory.Get<IReportColumnSettingHelper>();
			var parameters = new List<ReportColumnSettingSheetNameUpdateParameter>();
			parameters.Add(new ReportColumnSettingSheetNameUpdateParameter(reportId1,
				new Dictionary<string, string>() { { "OldSheet1", "NewSheet1" } }));

			helper.UpdateReportColumnSettingSheetNames(parameters);
			AssertUpdateResult("Only correctly mapped sheet name be updated", reportId1, dataId1, new List<string>() { "NewSheet1", "OldSheet2" });
			AssertUpdateResult("Only correctly mapped sheet name be updated", reportId2, dataId2, new List<string>() { "AnotherOldSheet1", "AnotherOldSheet2" });
		}

		void PrepareTestData(Guid reportId, Guid pk, string name, ReportColumnSettings settings)
		{
			var dataType = new StringRegistryDataType();
			var xml = ColumnHeadingListSerialiser.Serialise(settings);
			var binaryData = dataType.Serialise(xml);
			using (var command = Db.Connection.Command(InsertConfigDataSQL))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, StmDataSchema.PK);
				command.AddParameterBasedOnDbColumn("@Name", name, StmDataSchema.SD_Name);
				command.AddParameterBasedOnDbColumn("@Owner", reportId, StmDataSchema.SD_Owner);
				command.AddParameterBasedOnDbColumn("@BinaryValue", binaryData, StmDataSchema.SD_BinaryValue);
				command.ExecuteNonQuery();
			}
		}

		void AssertUpdateResult(string message, Guid reportId, Guid pk, List<string> expectedSheetNames)
		{
			var dataType = new StringRegistryDataType();
			using (var command = Db.Connection.Command(GetConfigDataSQL))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, StmDataSchema.PK);
				var binaryData = command.ExecuteScalar() as byte[];
				var xml = dataType.Deserialise(binaryData);
				var manager = new ColumnConfigurationsManager(reportId, false);
				var parameters = new ColumnSettingXMLVersionUpgraderCurrentVersionParameters(manager);
				var reportColumnSetting = ColumnHeadingListSerialiser.Deserialise(xml, parameters);

				var actualSheetNames = new List<string>();
				foreach (Worksheet sheet in reportColumnSetting.Worksheets)
				{
					actualSheetNames.Add(sheet.Name);
				}
				AssertArrayEqualsByElements(message, expectedSheetNames.ToArray(), actualSheetNames.ToArray());
			}
		}

		const string InsertConfigDataSQL = @"
insert into dbo.StmData
(SD_PK, SD_Name, SD_Owner, SD_BinaryValue)
Values
(@PK, @Name, @Owner, @BinaryValue)
";

		const string GetConfigDataSQL = @"
select SD_BinaryValue 
from dbo.StmData
where SD_PK = @PK
";
	}
}
