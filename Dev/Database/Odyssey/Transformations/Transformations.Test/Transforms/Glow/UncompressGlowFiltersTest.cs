using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(UncompressGlowFilters))]
	class UncompressGlowFiltersTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			moduleFilterTestCases = new List<FilterTestCase>
			{
				new FilterTestCase("Test_Uncompress_WhenDataIsCompressedBefore", "IEntityInfo_IJobShipment", isFilterDataCompressedBefore: true, expectFilterDataToBeCompressed: false),
				new FilterTestCase("Test_Uncompress_WhenDataIsNotCompressedBefore", "IEntityInfo_IJobShipment", isFilterDataCompressedBefore: false, expectFilterDataToBeCompressed: false),
				new FilterTestCase("Test_NotUncompress", "IEntityInfo_Unaffected", isFilterDataCompressedBefore: true, expectFilterDataToBeCompressed: true),
				new FilterTestCase("Test_NotUncompress", "IEntityInfo_Unaffected", isFilterDataCompressedBefore: false, expectFilterDataToBeCompressed: false),
			};

			moduleFilterTestCases.ForEach((testCase) => testCase.SetupModuleFilter());
		}

		protected override void AssertTransformationResults()
		{
			var pkListString = string.Join("','", moduleFilterTestCases.Select(tc => tc.FilterPK));
			var sql = $@"SELECT S9_PK, dbo.CLRUncompressAsString(S9_FilterData) AS UncompressedFilterData, CONVERT(VARCHAR(MAX), S9_FilterData) AS FilterData, S9_FilterName FROM dbo.StmModuleFilter WHERE S9_PK IN ('{pkListString}')";

			var schemeTable = new DataTable();
			using (var schemeCmd = Db.Connection.Command(sql))
			using (var schemeAdapter = schemeCmd.NewDataAdapter())
			{
				schemeAdapter.Fill(schemeTable);
			}

			AssertEquals("Expected all filters to exist in DB", moduleFilterTestCases.Count, schemeTable.Rows.Count);

			for (var i = 0; i < schemeTable.Rows.Count; i++)
			{
				var row = schemeTable.Rows[i];
				var currentItem = moduleFilterTestCases.Single((f) => f.FilterPK == (Guid)row["S9_PK"]);
				currentItem.AssertResult(row["UncompressedFilterData"].ToString(), row["FilterData"].ToString());
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UncompressGlowFilters();
		}

		List<FilterTestCase> moduleFilterTestCases;

		class FilterTestCase
		{
			public FilterTestCase(
				string filterName,
				string moduleId,
				bool isFilterDataCompressedBefore,
				bool expectFilterDataToBeCompressed)
			{
				this.filterName = filterName;
				this.moduleId = moduleId;
				this.isFilterDataCompressedBefore = isFilterDataCompressedBefore;
				this.expectFilterDataToBeCompressed = expectFilterDataToBeCompressed;

				FilterPK = Guid.NewGuid();
				RelatedEntityPK = Guid.NewGuid();
			}

			public Guid FilterPK { get; }
			public Guid RelatedEntityPK { get; }

			readonly string filterName;
			readonly string moduleId;
			readonly bool isFilterDataCompressedBefore;
			readonly bool expectFilterDataToBeCompressed;

			public void AssertResult(string uncompressedFilterData, string filterData)
			{
				if (expectFilterDataToBeCompressed)
				{
					Assert(uncompressedFilterData != filterData);
				}
				else
				{
					Assert(uncompressedFilterData == filterData);
				}
				Assert(uncompressedFilterData == FilterData);
			}

			public void SetupModuleFilter()
			{
				var createFilterQuery = isFilterDataCompressedBefore ? @"
INSERT INTO [dbo].[StmModuleFilter] ([S9_PK], [S9_ModuleID], [S9_FilterName], [S9_FilterData], [S9_RelatedEntityID], [S9_SystemCreateTimeUtc], [S9_SystemCreateUser], [S9_SystemLastEditTimeUtc], [S9_SystemLastEditUser])
VALUES (@pk, @moduleId, @filterName, dbo.CLRCompressStringAsBytes(@filterData), @relatedEntityPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
" : @"
INSERT INTO [dbo].[StmModuleFilter] ([S9_PK], [S9_ModuleID], [S9_FilterName], [S9_FilterData], [S9_RelatedEntityID], [S9_SystemCreateTimeUtc], [S9_SystemCreateUser], [S9_SystemLastEditTimeUtc], [S9_SystemLastEditUser])
VALUES (@pk, @moduleId, @filterName, CONVERT(VARBINARY(MAX), @filterData), @relatedEntityPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

				using (var command = Db.Connection.Command(createFilterQuery))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, FilterPK);
					command.AddParameter("@moduleId", SqlDbType.VarChar, moduleId);
					command.AddParameter("@filterName", SqlDbType.NVarChar, StmModuleFilterSchema.S9_FilterName.MaxLength, filterName);
					command.AddParameter("@filterData", SqlDbType.VarChar, FilterData);
					command.AddParameter("@relatedEntityPK", SqlDbType.UniqueIdentifier, RelatedEntityPK);
					command.ExecuteNonQuery();
				}
			}

			const string FilterData = @"<ArrayOfFilterGroup xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/glow/2014/07/16/filters.xsd"">
  <FilterGroup>
	<Filters>
	  <Filter>
		<FilterType>DateTimeOffsetFilter</FilterType>
		<Operation>wasinthepast</Operation>
		<PropertyPath>ETALOCAL</PropertyPath>
		<Values/>
	  </Filter>
	</Filters>
	<IsImplicit>false</IsImplicit>
  </FilterGroup>
</ArrayOfFilterGroup>
";
		}
	}
}
