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
	[TestedType(typeof(UpdateGlowSavedColumnsToRenameETAETDArrivalDepartureDate))]
	class UpdateGlowSavedColumnsToRenameETAETDArrivalDepartureDateTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			moduleSDValueTestCases = new List<SDValueTestCase>
			{
				// Should convert ETA/ETD/ARRIVALDATE/DEPARTUREDATE to ETALOCAL/ETDLOCAL/ARRIVALDATELOCAL/DEPARTUREDATELOCAL
				new SDValueTestCase("ETA", "ETALOCAL"),
				new SDValueTestCase("ETD", "ETDLOCAL"),
				new SDValueTestCase("ARRIVALDATE", "ARRIVALDATELOCAL"),
				new SDValueTestCase("DEPARTUREDATE", "DEPARTUREDATELOCAL"),

				// Should convert ETAOFFSET/ETDOFFSET/ARRIVALDATEOFFSET to ETA/ETD/ARRIVALDATE
				new SDValueTestCase("ETAOFFSET", "ETA"),
				new SDValueTestCase("ETDOFFSET", "ETD"),
				new SDValueTestCase("ARRIVALDATEOFFSET", "ARRIVALDATE"),

				// Should not convert invalid columns
				new SDValueTestCase("ETAX"),
				new SDValueTestCase("XETA"),
				new SDValueTestCase("XETAX"),
				new SDValueTestCase("ETXA"),

				new SDValueTestCase("ETDX"),
				new SDValueTestCase("XETD"),
				new SDValueTestCase("XETDX"),
				new SDValueTestCase("ETXD"),

				new SDValueTestCase("ARRIVALDATEX"),
				new SDValueTestCase("XARRIVALDATE"),
				new SDValueTestCase("XARRIVALDATEX"),
				new SDValueTestCase("ARRIVALDXATE"),

				new SDValueTestCase("DEPARTUREDATEX"),
				new SDValueTestCase("XDEPARTUREDATE"),
				new SDValueTestCase("XDEPARTUREDATEX"),
				new SDValueTestCase("DEPARTUREDXATE"),

				new SDValueTestCase("ETAOFFSETX"),
				new SDValueTestCase("XETAOFFSET"),
				new SDValueTestCase("XETAOFFSETX"),
				new SDValueTestCase("ETAOFXFSET"),

				new SDValueTestCase("ETDOFFSETX"),
				new SDValueTestCase("XETDOFFSET"),
				new SDValueTestCase("XETDOFFSETX"),
				new SDValueTestCase("ETDOFXFSET"),

				new SDValueTestCase("ARRIVALDATEOFFSETX"),
				new SDValueTestCase("XARRIVALDATEOFFSET"),
				new SDValueTestCase("XARRIVALDATEOFFSETX"),
				new SDValueTestCase("ARRIVALDAXTEOFFSET"),
			};

			moduleSDValueTestCases.ForEach((testCase) => testCase.SetupModuleSDValue());
		}

		protected override void AssertTransformationResults()
		{
			var pkListString = string.Join("','", moduleSDValueTestCases.Select(tc => tc.StmDataPK));
			var sql = $@"SELECT SD_PK, dbo.CLRUncompressAsString(SD_BinaryValue) AS SD_Value FROM dbo.StmData WHERE SD_PK IN ('{pkListString}')";

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
				currentItem.AssertResult(row);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlowSavedColumnsToRenameETAETDArrivalDepartureDate();
		}

		List<SDValueTestCase> moduleSDValueTestCases;

		class SDValueTestCase
		{
			public SDValueTestCase(
				string columnName,
				string expectedColumnName = null)
			{
				this.sdValue = GetSDValue(columnName);
				if (expectedColumnName != null)
				{
					this.expectedSDValue = GetSDValue(expectedColumnName);
				}

				StmDataPK = Guid.NewGuid();
				DepartmentPK = Guid.NewGuid();
			}

			public Guid StmDataPK { get; }
			public Guid DepartmentPK { get; }

			readonly string sdValue;
			readonly string expectedSDValue;

			public void AssertResult(DataRow row)
			{
				AssertEquals(expectedSDValue ?? sdValue, row["SD_Value"]);
			}

			public void SetupModuleSDValue()
			{
				var createFilterQuery = @"
INSERT INTO [dbo].[StmData] ([SD_PK], [SD_Name], [SD_BinaryValue], [SD_Owner], [SD_DepartmentGuid], [SD_SystemCreateTimeUtc], [SD_SystemCreateUser], [SD_SystemLastEditTimeUtc], [SD_SystemLastEditUser])
VALUES (@pk, 'GridSettings_Search_IJobContainer', dbo.CLRCompressStringAsBytes(@sdValueXml), '117217ed-e095-46f0-b5c0-da3b74514c47', @departmentGuid, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

				using (var command = Db.Connection.Command(createFilterQuery))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, StmDataPK);
					command.AddParameter("@departmentGuid", SqlDbType.UniqueIdentifier, DepartmentPK);
					command.AddParameter("@sdValueXml", SqlDbType.VarChar, sdValue);
					command.ExecuteNonQuery();
				}
			}

			public string GetSDValue(string columnName)
			{
				return $@"<GridSettings xmlns=""http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces""
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
			<FieldName>{columnName}</FieldName>
			<Width>100</Width>
		</GridColumnDefinition>
	</VisibleColumns>
</GridSettings>
";
			}
		}
	}
}
