using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(UpdateGlowAuditIndexFields))]
	class UpdateGlowAuditIndexFieldsTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateGlowAuditIndexFields();

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				//	CREATINGUSER -> CREATEUSER
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>CREATEUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>createUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEuser</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER2</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEUSER</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_1));

				//	any starts with -> exact
				//	any exact -> exact
				//	all exact -> exact
				//	none exact -> not equal
				//	no starts with -> not equal
				//	is blank -> is blank
				//	is not blank ->  is not blank
				AssertEquals($"" +
					$"<FilterLayoutValuesSerializer>" +
					$"<ModuleFilters>" +
					$"<ModuleFilter><Comparer>exact</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>exact</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>any starts with</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>exact</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>not equal</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>any starts with</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>not equal</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>is blank</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>is not blank</Comparer><Property>WY</Property></ModuleFilter>" +
					$"</ModuleFilters>" +
					$"</FilterLayoutValuesSerializer>", GetXMLDataFromStmModuleFilterUserData(S9_1_S0));

				//	SYSTEMCREATETIMEUTC -> CREATETIME
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>CREATETIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>createTIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATETIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEtime</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER2</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATETIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATETIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATETIME</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_2));

				//	no changes for non-index search layout
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>creatingUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_3));

				//	no changes for non-index search layout
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>systemCREATETIMEUTC</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>createTIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_4));

				//	no changes for SearchProperty
				AssertEquals($"" +
					$"<FilterLayoutValuesSerializer>" +
					$"<ModuleFilters>" +
					$"<ModuleFilter><SearchProperty>Today</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>This Week</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>any starts with</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Yesterday</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Last Week</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>any starts with</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Today</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>This Week</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Yesterday</SearchProperty></ModuleFilter>" +
					$"</ModuleFilters>" +
					$"</FilterLayoutValuesSerializer>", GetXMLDataFromStmModuleFilterUserData(S9_2_S0));

				//	CREATINGUSER -> CREATEUSER
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_1));

				//	REATINGUSER -> CREATEUSER
				//	StartsWith->Is
				//	Contains->Is
				//	Is->Is
				//	IsNot->IsNot
				//	NotStarting->IsNot
				//	IsBlank->IsBlank
				//	IsNotBlank->IsNotBlank
				AssertEquals($"" +
					$"<ArrayOfFilterGroup>" +
					$"<FilterGroup>" +
					$"<Filters>" +
					$"<Filter><FilterType>SimpleLookupFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>SimpleLookupFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER1</PropertyPath><Operation>IsInThisMonth</Operation></Filter>" +
					$"<Filter><FilterType>SimpleLookupFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>SimpleLookupFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>IsNot</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER2</PropertyPath><Operation>WasYesterday</Operation></Filter>" +
					$"<Filter><FilterType>SimpleLookupFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>IsNot</Operation></Filter>" +
					$"<Filter><FilterType>SimpleLookupFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>IsBlank</Operation></Filter>" +
					$"<Filter><FilterType>SimpleLookupFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>IsNotBlank</Operation></Filter>" +
					$"</Filters>" +
					$"</FilterGroup>" +
					$"</ArrayOfFilterGroup>", GetXMLDataFromStmModuleFilter(S9_5));

				//	SYSTEMCREATETIMEUTC -> CREATETIME
				//	no changes for date filter
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_2));
				AssertEquals($"" +
					$"<ArrayOfFilterGroup>" +
					$"<FilterGroup>" +
					$"<Filters>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>istoday</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>IsInThisWeek</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER1</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>IsInThisMonth</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>WasYesterday</Operation></Filter>" +
					$"</Filters>" +
					$"</FilterGroup>" +
					$"</ArrayOfFilterGroup>", GetXMLDataFromStmModuleFilter(S9_6));

				//	CREATEDDATE -> CREATETIME
				//	no changes for date filter
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_3));

				//	CREATEDDATE -> CREATETIME
				//	no changes for date filter
				AssertEquals($"" +
					$"<ArrayOfFilterGroup>" +
					$"<FilterGroup>" +
					$"<Filters>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>IsToday</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>IsInThisWeek</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER1</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>IsInThisMonth</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>wasyesterday</Operation></Filter>" +
					$"</Filters>" +
					$"</FilterGroup>" +
					$"</ArrayOfFilterGroup>", GetXMLDataFromStmModuleFilter(S9_7));

				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_4));

				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_5));
			});
		}

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			//	For CW1
			//	transform FilterDescription-[CREATINGUSER to CREATEUSER, SYSTEMCREATETIMEUTC to CREATETIME] in StmModuleFilter
			//	transform Comparer-[any starts with -> exact, any exact -> exact, all exact -> exact, none exact -> not equal, no starts with -> not equal, is blank -> is blank, is not blank ->  is not blank] in StmModuleFilterUserData
			//	do nothing for time fields in StmModuleFilterUserData
			//	is index search layout
			S9_1 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterSql(S9_1, "DtbConsignment", CreateStmModuleFilterXMLForCW1(["creatingUSER", "createUSER", "PLACEHOLDER1", "CREATINGuser", "CREATEuser", "PLACEHOLDER2", "CREATINGUSER", "CREATEUSER", "CREATINGUSER"]), 1));
			S9_1_S0 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterUserDataSql(S9_1_S0, S9_1, CreateStmModuleFilterUserDataForUser(["any starts with", "any exact", "any starts with", "all exact", "none exact", "any starts with", "no starts with", "is blank", "is not blank"])));

			//	is index search layout
			S9_2 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterSql(S9_2, "ShipmentReceival", CreateStmModuleFilterXMLForCW1(["systemCREATETIMEUTC", "createTIME", "PLACEHOLDER1", "SYSTEMCREATETIMEutc", "CREATEtime", "PLACEHOLDER2", "SYSTEMCREATETIMEUTC", "CREATETIME", "SYSTEMCREATETIMEUTC"]), 1));
			S9_2_S0 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterUserDataSql(S9_2_S0, S9_2, CreateStmModuleFilterUserDataForTime(["Today", "This Week", "any starts with", "Yesterday", "Last Week", "any starts with", "Today", "This Week", "Yesterday"])));

			//	non-index search layout
			S9_3 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterSql(S9_3, "DtbConsignment", CreateStmModuleFilterXMLForCW1(["creatingUSER", "PLACEHOLDER1"])));

			//	non-index search layout
			S9_4 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterSql(S9_4, "ShipmentReceival", CreateStmModuleFilterXMLForCW1(["systemCREATETIMEUTC", "createTIME", "PLACEHOLDER1"])));

			//	For Glow
			//	transform FieldName-[CREATINGUSER to CREATEUSER, SYSTEMCREATETIMEUTC to CREATETIME, CREATEDDATE -> CREATETIME] in StmData
			//	transform FilterType-[StringFilter -> SimpleLookupFilter] & Operation-[StartsWith -> Is, Contains -> Is, Is -> Is, IsNot -> IsNot, NotStarting -> IsNot, IsBlank -> IsBlank, IsNotBlank -> IsNotBlank] in StmModuleFilter
			//	do nothing for time fields in StmModuleFilter
			SD_1 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmDataSql(SD_1, "GridSettings_Search_IDtbConsignment_WYTEST", CreateStmData(["creatingUSER", "CREATEUSER", "PLACEHOLDER1", "CREATINGuser", "CREATEUSER"])));
			S9_5 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterSql(S9_5, "IEntityInfo_IDtbConsignment", CreateStmModuleFilterXMLForGlow([
				("stringfilter", "startsWith", "creatingUSER"),
				("StringFilter", "Contains", "CREATEUSER"),
				("StringFilter", "IsInThisMonth", "PLACEHOLDER1"),
				("StringFilter", "Is", "CREATINGUSER"),
				("StringFilter", "IsNot", "CREATEUSER"),
				("StringFilter", "WasYesterday", "PLACEHOLDER2"),
				("StringFilter", "NotStarting", "CREATINGUSER"),
				("StringFilter", "IsBlank", "CREATEUSER"),
				("StringFilter", "IsNotBlank", "CREATINGuser"),
			])));

			SD_2 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmDataSql(SD_2, "GridSettings_Search_IJobConsol_WYTEST", CreateStmData(["SYSTEMCREATETIMEUTC", "CREATETIME", "PLACEHOLDER1", "SYSTEMCREATETIMEUTC", "CREATETIME"])));
			S9_6 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterSql(S9_6, "IEntityInfo_IJobConsol", CreateStmModuleFilterXMLForGlow([
				("UtcDateTimeFilter", "istoday", "SYSTEMCREATETIMEUTC"),
				("UtcDateTimeFilter", "IsInThisWeek", "CREATETIME"),
				("StringFilter", "Is", "PLACEHOLDER1"),
				("UtcDateTimeFilter", "IsInThisMonth", "SYSTEMCREATETIMEUTC"),
				("UtcDateTimeFilter", "WasYesterday", "CREATETIME"),
			])));

			SD_3 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmDataSql(SD_3, "GridSettings_Search_IWhsItemCycleCountLocation_WYTEST", CreateStmData(["CREATEDDATE", "CREATETIME", "PLACEHOLDER1", "CREATEDDATE", "CREATETIME"])));
			S9_7 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmModuleFilterSql(S9_7, "IEntityInfo_IWhsItemCycleCountLocation", CreateStmModuleFilterXMLForGlow([
				("UtcDateTimeFilter", "IsToday", "CREATEDDATE"),
				("UtcDateTimeFilter", "IsInThisWeek", "CREATETIME"),
				("StringFilter", "Is", "PLACEHOLDER1"),
				("UtcDateTimeFilter", "IsInThisMonth", "CREATEDDATE"),
				("UtcDateTimeFilter", "wasyesterday", "CREATETIME"),
			])));

			SD_4 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmDataSql(SD_4, "GridSettings_Search_IWhsItemCycleCountLocation_WYTESTTEST", CreateStmData(["CREATEDDATE", "CREATETIME"], true, false)));

			SD_5 = Guid.NewGuid();
			sql.AppendLine(GetInsertStmDataSql(SD_5, "GridSettings_Search_IWhsItemCycleCountLocation_WYTESTTEST", CreateStmData(["CREATEDDATE", "CREATETIME"], false, true)));

			TestConnection.ExecuteNonQuery(sql.ToString());

			CombineAssertions(() =>
			{
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>creatingUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>createUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATINGuser</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEuser</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER2</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATINGUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATINGUSER</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_1));
				AssertEquals($"" +
					$"<FilterLayoutValuesSerializer>" +
					$"<ModuleFilters>" +
					$"<ModuleFilter><Comparer>any starts with</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>any exact</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>any starts with</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>all exact</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>none exact</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>any starts with</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>no starts with</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>is blank</Comparer><Property>WY</Property></ModuleFilter>" +
					$"<ModuleFilter><Comparer>is not blank</Comparer><Property>WY</Property></ModuleFilter>" +
					$"</ModuleFilters>" +
					$"</FilterLayoutValuesSerializer>", GetXMLDataFromStmModuleFilterUserData(S9_1_S0));
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>systemCREATETIMEUTC</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>createTIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>SYSTEMCREATETIMEutc</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATEtime</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER2</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>SYSTEMCREATETIMEUTC</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>CREATETIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>SYSTEMCREATETIMEUTC</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_2));
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>creatingUSER</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_3));
				AssertEquals($"" +
					$"<FilterLayoutSerializer>" +
					$"<FilterStrips>" +
					$"<FilterStrip><FilterDescription>systemCREATETIMEUTC</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>createTIME</FilterDescription></FilterStrip>" +
					$"<FilterStrip><FilterDescription>PLACEHOLDER1</FilterDescription></FilterStrip>" +
					$"</FilterStrips>" +
					$"</FilterLayoutSerializer>", GetXMLDataFromStmModuleFilter(S9_4));
				AssertEquals($"" +
					$"<FilterLayoutValuesSerializer>" +
					$"<ModuleFilters>" +
					$"<ModuleFilter><SearchProperty>Today</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>This Week</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>any starts with</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Yesterday</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Last Week</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>any starts with</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Today</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>This Week</SearchProperty></ModuleFilter>" +
					$"<ModuleFilter><SearchProperty>Yesterday</SearchProperty></ModuleFilter>" +
					$"</ModuleFilters>" +
					$"</FilterLayoutValuesSerializer>", GetXMLDataFromStmModuleFilterUserData(S9_2_S0));
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>creatingUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATINGuser</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>creatingUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATINGuser</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATEUSER</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_1));
				AssertEquals($"" +
					$"<ArrayOfFilterGroup>" +
					$"<FilterGroup>" +
					$"<Filters>" +
					$"<Filter><FilterType>stringfilter</FilterType><PropertyPath>creatingUSER</PropertyPath><Operation>startsWith</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>Contains</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER1</PropertyPath><Operation>IsInThisMonth</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>CREATINGUSER</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>IsNot</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER2</PropertyPath><Operation>WasYesterday</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>CREATINGUSER</PropertyPath><Operation>NotStarting</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>CREATEUSER</PropertyPath><Operation>IsBlank</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>CREATINGuser</PropertyPath><Operation>IsNotBlank</Operation></Filter>" +
					$"</Filters>" +
					$"</FilterGroup>" +
					$"</ArrayOfFilterGroup>", GetXMLDataFromStmModuleFilter(S9_5));
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>SYSTEMCREATETIMEUTC</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>SYSTEMCREATETIMEUTC</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>SYSTEMCREATETIMEUTC</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>SYSTEMCREATETIMEUTC</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_2));
				AssertEquals($"" +
					$"<ArrayOfFilterGroup>" +
					$"<FilterGroup>" +
					$"<Filters>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>SYSTEMCREATETIMEUTC</PropertyPath><Operation>istoday</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>IsInThisWeek</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER1</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>SYSTEMCREATETIMEUTC</PropertyPath><Operation>IsInThisMonth</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>WasYesterday</Operation></Filter>" +
					$"</Filters>" +
					$"</FilterGroup>" +
					$"</ArrayOfFilterGroup>", GetXMLDataFromStmModuleFilter(S9_6));
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>CREATEDDATE</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATEDDATE</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>CREATEDDATE</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>PLACEHOLDER1</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATEDDATE</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_3));
				AssertEquals($"" +
					$"<ArrayOfFilterGroup>" +
					$"<FilterGroup>" +
					$"<Filters>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATEDDATE</PropertyPath><Operation>IsToday</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>IsInThisWeek</Operation></Filter>" +
					$"<Filter><FilterType>StringFilter</FilterType><PropertyPath>PLACEHOLDER1</PropertyPath><Operation>Is</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATEDDATE</PropertyPath><Operation>IsInThisMonth</Operation></Filter>" +
					$"<Filter><FilterType>UtcDateTimeFilter</FilterType><PropertyPath>CREATETIME</PropertyPath><Operation>wasyesterday</Operation></Filter>" +
					$"</Filters>" +
					$"</FilterGroup>" +
					$"</ArrayOfFilterGroup>", GetXMLDataFromStmModuleFilter(S9_7));
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"<GridSortDefinition><FieldName>CREATEDDATE</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"<GridSortDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridSortDefinition>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_4));
				AssertEquals($"" +
					$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					$"<AggregateDescriptions />" +
					$"<GroupDescriptions />" +
					$"<SortDescriptions>" +
					$"</SortDescriptions>" +
					$"<VisibleColumns>" +
					$"<GridColumnDefinition><FieldName>CREATEDDATE</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"<GridColumnDefinition><FieldName>CREATETIME</FieldName><Width>243</Width><SortName i:nil=\"true\" /></GridColumnDefinition>" +
					$"</VisibleColumns>" +
					$"</GridSettings>", GetXMLDataFromStmData(SD_5));
			});
		}

		string GetXMLDataFromStmModuleFilter(Guid pk)
		{
			return TestConnection.ExecuteScalar<string>(@$"
SELECT
	dbo.CLRUncompressAsString(S9_FilterData) AS FilterData
FROM
	dbo.StmModuleFilter
WHERE S9_PK = '{pk}'
");
		}

		string GetXMLDataFromStmData(Guid pk)
		{
			return TestConnection.ExecuteScalar<string>(@$"
SELECT
	dbo.CLRUncompressAsString(SD_BinaryValue) AS FilterData
FROM
	dbo.StmData
WHERE SD_PK = '{pk}'
");
		}

		string GetXMLDataFromStmModuleFilterUserData(Guid pk)
		{
			return TestConnection.ExecuteScalar<string>(@$"
SELECT
	dbo.CLRUncompressAsString(S0_FilterDataValues) AS FilterDataValues
FROM
	dbo.StmModuleFilterUserData
WHERE S0_PK = '{pk}'
");
		}

		string GetInsertStmModuleFilterSql(Guid pk, string moduleID, string filterData, int isIndexSearch = 0)
		{
			return $@"
INSERT INTO dbo.StmModuleFilter(S9_PK, S9_ModuleID, S9_FilterData, S9_FilterName, S9_IsIndexSearch, S9_SystemCreateTimeUtc, S9_SystemCreateUser, S9_SystemLastEditTimeUtc, S9_SystemLastEditUser)
VALUES('{pk}', '{moduleID}', dbo.CLRCompressStringAsBytes('{filterData}'), '{nameof(UpdateGlowAuditIndexFieldsTest)}_{recordIndex++}', {isIndexSearch}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
		}

		string GetInsertStmModuleFilterUserDataSql(Guid s0, Guid s0_s9, string filterDataValues)
		{
			return $@"
INSERT INTO dbo.StmModuleFilterUserData(S0_PK, S0_RelatedEntityID, S0_S9, S0_FilterDataValues, S0_SystemCreateTimeUtc, S0_SystemCreateUser, S0_SystemLastEditTimeUtc, S0_SystemLastEditUser)
VALUES('{s0}', NEWID(), '{s0_s9}', dbo.CLRCompressStringAsBytes('{filterDataValues}'), GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
		}

		string GetInsertStmDataSql(Guid pk, string sdName, string sdValue)
		{
			return $@"
INSERT INTO dbo.StmData(SD_PK, SD_Owner, SD_NAME, SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
VALUES ('{pk}', NEWID(), '{sdName}', dbo.CLRCompressStringAsBytes('{sdValue}'), GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
		}

		string CreateStmModuleFilterXMLForCW1(IList<string> filterDescriptions)
		{
			var nodeData = filterDescriptions.Select(
				x => $"" +
				$"<FilterStrip>" +
				$"<FilterDescription>{x}</FilterDescription>" +
				$"</FilterStrip>"
			);
			return $"" +
				$"<FilterLayoutSerializer>" +
				$"<FilterStrips>" +
				string.Join("", nodeData) +
				$"</FilterStrips>" +
				$"</FilterLayoutSerializer>";
		}

		string CreateStmModuleFilterUserDataForUser(IList<string> comparers)
		{
			var nodeData = comparers.Select(
				x => $"" +
				$"<ModuleFilter>" +
				$"<Comparer>{x}</Comparer>" +
				$"<Property>WY</Property>" +
				$"</ModuleFilter>"
			);
			return $"" +
				$"<FilterLayoutValuesSerializer>" +
				$"<ModuleFilters>" +
				string.Join("", nodeData) +
				$"</ModuleFilters>" +
				$"</FilterLayoutValuesSerializer>";
		}

		string CreateStmModuleFilterUserDataForTime(IList<string> searchProperties)
		{
			var nodeData = searchProperties.Select(
				x => $"" +
				$"<ModuleFilter>" +
				$"<SearchProperty>{x}</SearchProperty>" +
				$"</ModuleFilter>"
			);
			return $"" +
				$"<FilterLayoutValuesSerializer>" +
				$"<ModuleFilters>" +
				string.Join("", nodeData) +
				$"</ModuleFilters>" +
				$"</FilterLayoutValuesSerializer>";
		}

		string CreateStmModuleFilterXMLForGlow(IList<(string filterType, string operation, string propertyPath)> nodeDataInfo)
		{
			var nodeData = nodeDataInfo.Select(
				x => $"" +
				$"<Filter>" +
				$"<FilterType>{x.filterType}</FilterType>" +
				$"<PropertyPath>{x.propertyPath}</PropertyPath>" +
				$"<Operation>{x.operation}</Operation>" +
				$"</Filter>"
			);
			return $"" +
				$"<ArrayOfFilterGroup>" +
				$"<FilterGroup>" +
				$"<Filters>" +
				string.Join("", nodeData) +
				$"</Filters>" +
				$"</FilterGroup>" +
				$"</ArrayOfFilterGroup>";
		}

		string CreateStmData(IList<string> fieldNames, bool needsSort = true, bool needsDefinition = true)
		{
			var sortNodeData = fieldNames.Select(
				x => $"" +
				$"<GridSortDefinition>" +
				$"<FieldName>{x}</FieldName>" +
				$"<Width>243</Width>" +
				$"<SortName i:nil=\"true\" />" +
				$"</GridSortDefinition>"
			);

			var definitionNodeData = fieldNames.Select(
				x => $"" +
				$"<GridColumnDefinition>" +
				$"<FieldName>{x}</FieldName>" +
				$"<Width>243</Width>" +
				$"<SortName i:nil=\"true\" />" +
				$"</GridColumnDefinition>"
			);

			return $"" +
				$"<GridSettings xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.Glow.Infrastructure.Interfaces\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
				$"<AggregateDescriptions />" +
				$"<GroupDescriptions />" +
				$"<SortDescriptions>" +
				(needsSort ? string.Join("", sortNodeData) : "") + 
				$"</SortDescriptions>" +
				$"<VisibleColumns>" +
				(needsDefinition ? string.Join("", definitionNodeData) : "") +
				$"</VisibleColumns>" +
				$"</GridSettings>";
		}

		int recordIndex;
		Guid S9_1, S9_1_S0, S9_2, S9_2_S0, S9_3, S9_4, S9_5, S9_6, S9_7, SD_1, SD_2, SD_3, SD_4, SD_5;
	}
}
