using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	class XmlColumnElementRenameTransformationTest : TransactionedTestCase
	{
		public void TestReplacesTagsWithSimilarTextCorrectly()
		{
			const string startingXml = @"
<xml>
	<SectionFilterPK>Something</SectionFilterPK>
	<FilterPK>Something</FilterPK>
</xml>";
			const string expectedXml = @"
<xml>
	<SectionFilterPK>Something</SectionFilterPK>
	<FilteroonyPK>Something</FilteroonyPK>
</xml>";

			var pk = Guid.NewGuid();
			InsertRecord(pk, startingXml);

			new DummyXmlColumnElementRenameTransformation
			{
				ElementsToRenameOverride = new[] { new XmlColumnElementRenameTransformation.ElementRenameDetails("FilterPK", "FilteroonyPK") }
			}.Run();

			AssertXmlColumnValue(pk, expectedXml);
		}

		public void TestReplacesSelfClosingTagsWithSimilarTextCorrectly()
		{
			const string startingXml = @"
<xml>
	<SectionFilterPK>Something</SectionFilterPK>
	<FilterPK />
</xml>";
			const string expectedXml = @"
<xml>
	<SectionFilterPK>Something</SectionFilterPK>
	<FilteroonyPK />
</xml>";

			var pk = Guid.NewGuid();
			InsertRecord(pk, startingXml);

			new DummyXmlColumnElementRenameTransformation
			{
				ElementsToRenameOverride = new[] { new XmlColumnElementRenameTransformation.ElementRenameDetails("FilterPK", "FilteroonyPK") }
			}.Run();

			AssertXmlColumnValue(pk, expectedXml);
		}

		#region Implementation

		static void InsertRecord(Guid pk, string xml)
		{
			var sql = string.Format(@"
				INSERT dbo.BMBoardSection (
					MS_PK, MS_LayoutData, MS_SystemCreateTimeUtc, MS_SystemCreateUser, MS_SystemLastEditTimeUtc, MS_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
				)
				", pk, xml);

			Db.Connection.ExecuteNonQuery(sql);
		}

		static void AssertXmlColumnValue(Guid pk, string expectedXmlContent)
		{
			var sql = string.Format(@"
				SELECT MS_LayoutData FROM dbo.BMBoardSection
				WHERE MS_PK = '{0}'", pk);

			string result;

			using (var command = Db.Connection.Command(sql))
			{
				result = (string)command.ExecuteScalar();
			}

			AssertMultilineASCIIEquals("", DbTextFormatTestHelper.FormatXmlDocSafely(expectedXmlContent), DbTextFormatTestHelper.FormatXmlDocSafely(result));
		}

		#endregion
	}
}
