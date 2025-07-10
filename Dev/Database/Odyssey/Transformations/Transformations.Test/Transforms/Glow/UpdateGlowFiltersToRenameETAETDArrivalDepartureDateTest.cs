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
	[TestedType(typeof(UpdateGlowFiltersToRenameETAETDArrivalDepartureDate))]
	class UpdateGlowFiltersToRenameETAETDArrivalDepartureDateTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			moduleFilterTestCases = new List<FilterTestCase>
			{
				new FilterTestCase("Should_Convert_ETA_to_ETALOCAL", "ETA", "ETALOCAL"),
				new FilterTestCase("Should_Convert_ETD_to_ETDLOCAL", "ETD", "ETDLOCAL"),
				new FilterTestCase("Should_Convert_ARRIVALDATE_to_ARRIVALDATELOCAL", "ARRIVALDATE", "ARRIVALDATELOCAL"),
				new FilterTestCase("Should_Convert_DEPARTUREDATE_to_DEPARTUREDATELOCAL", "DEPARTUREDATE", "DEPARTUREDATELOCAL"),
				new FilterTestCase("Should_Convert_ETA_to_ETALOCAL_Without_Value", "ETA", "ETALOCAL", false),
				new FilterTestCase("Should_Convert_ETD_to_ETDLOCAL_Without_Value", "ETD", "ETDLOCAL", false),
				new FilterTestCase("Should_Convert_ARRIVALDATE_to_ARRIVALDATELOCAL_Without_Value", "ARRIVALDATE", "ARRIVALDATELOCAL", false),
				new FilterTestCase("Should_Convert_DEPARTUREDATE_to_DEPARTUREDATELOCAL_Without_Value", "DEPARTUREDATE", "DEPARTUREDATELOCAL", false),

				new FilterTestCase("Should_Convert_ETAOFFSET_to_ETA", "ETAOFFSET", "ETA"),
				new FilterTestCase("Should_Convert_ETDOFFSET_to_ETD", "ETDOFFSET", "ETD"),
				new FilterTestCase("Should_Convert_ARRIVALDATEOFFSET_to_ARRIVALDATE", "ARRIVALDATEOFFSET", "ARRIVALDATE"),
				new FilterTestCase("Should_Convert_ETAOFFSET_to_ETA_Without_Value", "ETAOFFSET", "ETA", false),
				new FilterTestCase("Should_Convert_ETDOFFSET_to_ETD_Without_Value", "ETDOFFSET", "ETD", false),
				new FilterTestCase("Should_Convert_ARRIVALDATEOFFSET_to_ARRIVALDATE_Without_Value", "ARRIVALDATEOFFSET", "ARRIVALDATE", false),

				new FilterTestCase("Should_Not_Convert_ETAX", "ETAX"),
				new FilterTestCase("Should_Not_Convert_XETA", "XETA"),
				new FilterTestCase("Should_Not_Convert_XETAX", "XETAX"),
				new FilterTestCase("Should_Not_Convert_ETXA", "ETXA"),

				new FilterTestCase("Should_Not_Convert_ETDX", "ETDX"),
				new FilterTestCase("Should_Not_Convert_XETD", "XETD"),
				new FilterTestCase("Should_Not_Convert_XETDX", "XETDX"),
				new FilterTestCase("Should_Not_Convert_ETXD", "ETXD"),

				new FilterTestCase("Should_Not_Convert_ARRIVALDATEX", "ARRIVALDATEX"),
				new FilterTestCase("Should_Not_Convert_XARRIVALDATE", "XARRIVALDATE"),
				new FilterTestCase("Should_Not_Convert_XARRIVALDATEX", "XARRIVALDATEX"),
				new FilterTestCase("Should_Not_Convert_ARRIVALDXATE", "ARRIVALDXATE"),

				new FilterTestCase("Should_Not_Convert_DEPARTUREDATEX", "DEPARTUREDATEX"),
				new FilterTestCase("Should_Not_Convert_XDEPARTUREDATE", "XDEPARTUREDATE"),
				new FilterTestCase("Should_Not_Convert_XDEPARTUREDATEX", "XDEPARTUREDATEX"),
				new FilterTestCase("Should_Not_Convert_DEPARTUREDXATE", "DEPARTUREDXATE"),

				new FilterTestCase("Should_Not_Convert_ETAOFFSETX", "ETAOFFSETX"),
				new FilterTestCase("Should_Not_Convert_XETAOFFSET", "XETAOFFSET"),
				new FilterTestCase("Should_Not_Convert_XETAOFFSETX", "XETAOFFSETX"),
				new FilterTestCase("Should_Not_Convert_ETAOFXFSET", "ETAOFXFSET"),

				new FilterTestCase("Should_Not_Convert_ETDOFFSETX", "ETDOFFSETX"),
				new FilterTestCase("Should_Not_Convert_XETDOFFSET", "XETDOFFSET"),
				new FilterTestCase("Should_Not_Convert_XETDOFFSETX", "XETDOFFSETX"),
				new FilterTestCase("Should_Not_Convert_ETDOFXFSET", "ETDOFXFSET"),

				new FilterTestCase("Should_Not_Convert_ARRIVALDATEOFFSETX", "ARRIVALDATEOFFSETX"),
				new FilterTestCase("Should_Not_Convert_XARRIVALDATEOFFSET", "XARRIVALDATEOFFSET"),
				new FilterTestCase("Should_Not_Convert_XARRIVALDATEOFFSETX", "XARRIVALDATEOFFSETX"),
				new FilterTestCase("Should_Not_Convert_ARRIVALDAXTEOFFSET", "ARRIVALDAXTEOFFSET"),
			};

			moduleFilterTestCases.ForEach((testCase) => testCase.SetupModuleFilter());
		}

		protected override void AssertTransformationResults()
		{
			var pkListString = string.Join("','", moduleFilterTestCases.Select(tc => tc.FilterPK));
			var sql = $@"SELECT S9_PK, dbo.CLRUncompressAsString(S9_FilterData) AS FilterData, S9_FilterName FROM dbo.StmModuleFilter WHERE S9_PK IN ('{pkListString}')";

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
				currentItem.AssertResult(row);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlowFiltersToRenameETAETDArrivalDepartureDate();
		}

		List<FilterTestCase> moduleFilterTestCases;

		class FilterTestCase
		{
			public FilterTestCase(
				string filterName,
				string filterColumn,
				string expectedFilterColumn = null,
				bool withValue = true)
			{
				this.filterName = filterName;
				this.filterData = GetFilterData(filterColumn, withValue);
				if (expectedFilterColumn != null)
				{
					this.expectedFilterData = GetFilterData(expectedFilterColumn, withValue);
				}

				FilterPK = Guid.NewGuid();
				RelatedEntityPK = Guid.NewGuid();
			}

			public Guid FilterPK { get; }
			public Guid RelatedEntityPK { get; }

			readonly string filterName;
			readonly string filterData;
			readonly string expectedFilterData;

			public void AssertResult(DataRow row)
			{
				AssertEquals(expectedFilterData ?? filterData, row["FilterData"]);
			}

			public void SetupModuleFilter()
			{
				var createFilterQuery = @"
INSERT INTO [dbo].[StmModuleFilter] ([S9_PK], [S9_ModuleID], [S9_FilterName], [S9_FilterData], [S9_RelatedEntityID], [S9_SystemCreateTimeUtc], [S9_SystemCreateUser], [S9_SystemLastEditTimeUtc], [S9_SystemLastEditUser])
VALUES (@pk, 'SEP_Index_IJobOrderHeader_417d399aff514143880c6e9e3fef4226_a6e4ff80-5cbe-426d-89ad-ae0b7fa55998', @filterName, dbo.CLRCompressStringAsBytes(@filterData), @relatedEntityPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

				using (var command = Db.Connection.Command(createFilterQuery))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, FilterPK);
					command.AddParameter("@relatedEntityPK", SqlDbType.UniqueIdentifier, RelatedEntityPK);
					command.AddParameter("@filterName", SqlDbType.NVarChar, StmModuleFilterSchema.S9_FilterName.MaxLength, filterName);
					command.AddParameter("@filterData", SqlDbType.VarChar, filterData);
					command.ExecuteNonQuery();
				}
			}

			public string GetFilterData(string columnName, bool withValue)
			{
				var valueElement = withValue
					? @"
<Values>
  <a:string>GEOFOOSYD</a:string>
</Values>" : "<Values/>";

				return $@"<ArrayOfFilterGroup xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/glow/2014/07/16/filters.xsd"">
  <FilterGroup>
    <Filters>
      <Filter>
        <FilterType>DateTimeOffsetFilter</FilterType>
        <Operation>wasinthepast</Operation>
        <PropertyPath>{columnName}</PropertyPath>
        <Values/>
      </Filter>
      <Filter>
        <FilterType>SimpleLookupFilter</FilterType>
        <Operation>is</Operation>
        <PropertyPath>CLIENT</PropertyPath>
        {valueElement}
      </Filter>
    </Filters>
    <IsImplicit>false</IsImplicit>
  </FilterGroup>
</ArrayOfFilterGroup>
";
			}
		}
	}
}
