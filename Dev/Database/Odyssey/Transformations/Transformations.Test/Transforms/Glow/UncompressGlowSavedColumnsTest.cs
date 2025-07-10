using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(UncompressGlowSavedColumns))]
	class UncompressGlowSavedColumnsTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			moduleSDValueTestCases = new List<SDValueTestCase>
			{
				new SDValueTestCase("GridSettings_Search_IJobContainer", isValueCompressedBefore: true, expectValueToBeCompressed: false),
				new SDValueTestCase("GridSettings_Search_IJobContainer", isValueCompressedBefore: false, expectValueToBeCompressed: false),
				new SDValueTestCase("GridSettings_Unaffected", isValueCompressedBefore: true, expectValueToBeCompressed: true),
				new SDValueTestCase("GridSettings_Unaffected", isValueCompressedBefore: false, expectValueToBeCompressed: false),
			};

			moduleSDValueTestCases.ForEach((testCase) => testCase.SetupModuleSDValue());
		}

		protected override void AssertTransformationResults()
		{
			var pkListString = string.Join("','", moduleSDValueTestCases.Select(tc => tc.StmDataPK));
			var sql = $@"SELECT SD_PK, dbo.CLRUncompressAsString(SD_BinaryValue) AS Uncompressed_SD_Value, CONVERT(VARCHAR(MAX), SD_BinaryValue) AS SD_Value FROM dbo.StmData WHERE SD_PK IN ('{pkListString}')";

			var schemeTable = new DataTable();
			using (var schemeCmd = Db.Connection.Command(sql))
			using (var schemeAdapter = schemeCmd.NewDataAdapter())
			{
				schemeAdapter.Fill(schemeTable);
			}

			AssertEquals("Expected all values to exist in DB", moduleSDValueTestCases.Count, schemeTable.Rows.Count);

			for (var i = 0; i < schemeTable.Rows.Count; i++)
			{
				var row = schemeTable.Rows[i];
				var currentItem = moduleSDValueTestCases.Single((f) => f.StmDataPK == (Guid)row["SD_PK"]);
				currentItem.AssertResult(row["Uncompressed_SD_Value"].ToString(), row["SD_Value"].ToString());
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UncompressGlowSavedColumns();
		}

		List<SDValueTestCase> moduleSDValueTestCases;

		class SDValueTestCase
		{
			public SDValueTestCase(
				string sdName,
				bool isValueCompressedBefore,
				bool expectValueToBeCompressed)
			{
				this.sdName = sdName;
				this.isValueCompressedBefore = isValueCompressedBefore;
				this.expectValueToBeCompressed = expectValueToBeCompressed;

				StmDataPK = Guid.NewGuid();
				DepartmentPK = Guid.NewGuid();
			}

			public Guid StmDataPK { get; }
			public Guid DepartmentPK { get; }

			readonly string sdName;
			readonly bool isValueCompressedBefore;
			readonly bool expectValueToBeCompressed;

			public void AssertResult(string uncompressedValue, string value)
			{
				if (expectValueToBeCompressed)
				{
					Assert(uncompressedValue != value);
				}
				else
				{
					Assert(uncompressedValue == value);
				}
				Assert(uncompressedValue == Value);
			}

			public void SetupModuleSDValue()
			{
				var createColumnsQuery = isValueCompressedBefore ? @"
INSERT INTO [dbo].[StmData] ([SD_PK], [SD_Name], [SD_BinaryValue], [SD_Owner], [SD_DepartmentGuid], [SD_SystemCreateTimeUtc], [SD_SystemCreateUser], [SD_SystemLastEditTimeUtc], [SD_SystemLastEditUser])
VALUES (@pk, @sdName, dbo.CLRCompressStringAsBytes(@sdValueXml), '117217ed-e095-46f0-b5c0-da3b74514c47', @departmentGuid, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
" : @"
INSERT INTO [dbo].[StmData] ([SD_PK], [SD_Name], [SD_BinaryValue], [SD_Owner], [SD_DepartmentGuid], [SD_SystemCreateTimeUtc], [SD_SystemCreateUser], [SD_SystemLastEditTimeUtc], [SD_SystemLastEditUser])
VALUES (@pk, @sdName, CONVERT(VARBINARY(MAX), @sdValueXml), '117217ed-e095-46f0-b5c0-da3b74514c47', @departmentGuid, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

				using (var command = Db.Connection.Command(createColumnsQuery))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, StmDataPK);
					command.AddParameter("@sdName", SqlDbType.VarChar, sdName);
					command.AddParameter("@sdValueXml", SqlDbType.VarChar, Value);
					command.AddParameter("@departmentGuid", SqlDbType.UniqueIdentifier, DepartmentPK);
					command.ExecuteNonQuery();
				}
			}

			const string Value = @"<GridSettings xmlns=""http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces""
	xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
	<AggregateDescriptions/>
	<GroupDescriptions/>
	<LastUpdateUtcTime>2023-10-12T00:59:49.5</LastUpdateUtcTime>
	<SortDescriptions>
		<GridSortDefinition>
			<Direction>desc</Direction>
			<FieldName>ORDERNO</FieldName>
			<SortName i:nil=""true""/>
		</GridSortDefinition>
	</SortDescriptions>
	<VisibleColumns>
		<GridColumnDefinition>
			<FieldName>ORDERNO</FieldName>
			<Width>150</Width>
		</GridColumnDefinition>
		<GridColumnDefinition>
			<FieldName>ETALOCAL</FieldName>
			<Width>100</Width>
		</GridColumnDefinition>
	</VisibleColumns>
</GridSettings>
";
		}
	}
}
