using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(FixUcTransformationEncoding))]
	sealed class FixUcTransformationEncodingTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var filterData = Encoding.Unicode.GetBytes(
				"""
				﻿<?xml version="1.0" encoding="utf-16"?><CopyTemplateTree xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" N="JobComInvoiceLine" ConfigurationSource="SLT" NominatedRecordPk="00000000-0000-0000-0000-000000000000" Active="true"><E N="JobComInvoiceLine"><P N="AssignedNumber1" Do="Copy" /></E><ConfigurationName>T&amp;est2</ConfigurationName></CopyTemplateTree>
				""");

			var index = 0;
			testDataCreator.CreateCompany(gcNotTransformed, "~ZZ", "ZZ");
			foreach (var (countryCode, moduleIds) in FixUcTransformationEncoding.CountryModuleIds)
			{
				var gc = testDataCreator.CreateCompany($"~{countryCode}", countryCode);
				foreach (var moduleId in moduleIds)
				{
					index++;
					testDataCreator.CreateModuleFilter(moduleId, $"Test UC Template {index}", filterData, gc, compressFilterData: true);
					testDataCreator.CreateModuleFilter(moduleId, $"Test UC Template Not Transformed {index}", filterData, gcNotTransformed, compressFilterData: true);
				}
			}
		}

		protected override void AssertPreConditions()
		{
			CombineAssertions("Precondition - Filter Data is still valid XML", () =>
			{
				foreach (var (countryCode, moduleId, filterData) in GetAllModuleFilters())
				{
					Assert($"{countryCode} {moduleId} should be valid XML", !string.IsNullOrEmpty(filterData));
				}
			});

			foreach (var (countryCode, moduleIds) in FixUcTransformationEncoding.CountryModuleIds)
			{
				RunPreviousTransformationHelperAndAssertCount(countryCode, moduleIds);
			}

			CombineAssertions("Precondition - Filter Data is not valid XML after running previous transformation helper script", () =>
			{
				foreach (var (countryCode, moduleId, filterData) in GetAllModuleFilters())
				{
					if (countryCode == "ZZ")
					{
						continue;
					}

					Assert($"{countryCode} {moduleId} should be invalid XML", string.IsNullOrEmpty(filterData));
				}
			});
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions("After transformation", () =>
			{
				foreach (var (countryCode, moduleId, filterData) in GetAllModuleFilters())
				{
					Assert($"{countryCode} {moduleId} should be valid XML", !string.IsNullOrEmpty(filterData));
				}
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new FixUcTransformationEncoding();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override void SetUp()
		{
			base.SetUp();
			gcNotTransformed = Guid.NewGuid();
			selectAllFilterSql =
				$"""
				SELECT
					GC_RN_NKCountryCode,
					S9_ModuleId,
					ISNULL(TRY_CAST(CAST(dbo.CLRUncompressAsBytes(S9_FilterData) AS NVARCHAR(MAX)) AS XML), '') AS FilterData
				FROM dbo.StmModuleFilter
				JOIN dbo.GlbCompany ON GC_PK = S9_GC
				WHERE S9_GC = '{gcNotTransformed}' OR
				{FixUcTransformationEncoding.WhereClause}
				""";
		}

		static Guid gcNotTransformed;
		static string selectAllFilterSql;

		static IEnumerable<(string CountryCode, string ModuleId, string FilterData)> GetAllModuleFilters()
		{
			using var cmd = Db.Connection.Command(selectAllFilterSql);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var countryCode = (string)reader["GC_RN_NKCountryCode"];
				var moduleId = (string)reader["S9_ModuleId"];
				var filterData = (string)reader["FilterData"];
				yield return (countryCode, moduleId, filterData);
			}
		}

		static void RunPreviousTransformationHelperAndAssertCount(string countryCode, IReadOnlyCollection<string> moduleIDs)
		{
			var sql = $@"
UPDATE dbo.StmModuleFilter
	SET S9_FilterData = dbo.CLRCompressStringAsBytes(
		REPLACE(REPLACE(REPLACE(CAST(dbo.CLRUncompressAsString(S9_FilterData) AS VARCHAR(MAX)),
			'N=""' + @OldPrefix + '_', 'N=""' + @NewPrefix + '_'),
			'>' + @OldPrefix + '_', '>' + @NewPrefix + '_'),
			'&lt;' + @OldPrefix + '_', '&lt;' + @NewPrefix + '_')
	),
	S9_SystemLastEditTimeUtc = GETUTCDATE(),
	S9_SystemLastEditUser = '~BP'
WHERE
	S9_ModuleID IN ({string.Join(", ", moduleIDs.Select(x => $"'{x}'"))})
	AND S9_GC IN (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = @CountryCode)";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@OldPrefix", SqlDbType.VarChar, "Testing");
				cmd.AddParameter("@NewPrefix", SqlDbType.VarChar, "Testing");
				cmd.AddParameter("@CountryCode", SqlDbType.VarChar, countryCode);
				var affectedCount = cmd.ExecuteNonQuery();
				AssertEquals(moduleIDs.Count, affectedCount);
			}
		}
	}
}
