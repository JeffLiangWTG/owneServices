using System;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	[TestsSubclassesOf(typeof(XmlColumnElementRenameTransformation),
			new Type[0],
			new Type[] { typeof(DummyXmlColumnElementRenameTransformation) })]
	abstract class XmlColumnElementRenameTransformationTestCase<T> : DataTransformationTestCase
			where T : XmlColumnElementRenameTransformation
	{
		#region DataTransformationTestCase Overrides

		protected sealed override DataTransformation GetNewTestTransformationInstance()
		{
			return (T)Activator.CreateInstance(typeof(T), new DummyUpgradeManager());
		}

		Guid row1PK, row2PK, row3PK;

		protected sealed override void PrepareTestData()
		{
			row1PK = Guid.NewGuid();
			row2PK = Guid.NewGuid();
			row3PK = Guid.NewGuid();

			var sql = new StringBuilder();
			sql.Append(GetValidInsertSql(row1PK, InvalidXml));
			sql.AppendLine();
			sql.Append(GetValidInsertSql(row2PK, ValidPreTransformedXml));
			sql.AppendLine();
			sql.Append(GetValidInsertSql(row3PK, ValidPostTransformedXml));

			using (var command = Db.Connection.Command(sql.ToString()))
			{
				command.ExecuteNonQuery();
			}

			AssertXmlColumnValue(row1PK, InvalidXml);
			AssertXmlColumnValue(row2PK, ValidPreTransformedXml);
			AssertXmlColumnValue(row3PK, ValidPostTransformedXml);
		}

		protected sealed override void AssertTransformationResults()
		{
			AssertXmlColumnValue(row1PK, InvalidXml);
			AssertXmlColumnValue(row2PK, ValidPostTransformedXml);
			AssertXmlColumnValue(row3PK, ValidPostTransformedXml);
		}

		#endregion

		#region Implementation

		void AssertXmlColumnValue(Guid pk, string expectedXmlContent)
		{
			var transform = (T)GetNewTestTransformationInstance();
			var sql = string.Format(@"
				SELECT {0} FROM {1}
				WHERE {2} = '{3}'",
				/*0*/ transform.XmlColumn.Name,
				/*1*/ transform.XmlColumn.TableName,
				/*2*/ transform.XmlColumn.TableSchema.PK.Name,
				/*3*/ pk);

			string result;

			using (var command = Db.Connection.Command(sql))
			{
				result = (string)command.ExecuteScalar();
			}

			AssertMultilineASCIIEquals("", DbTextFormatTestHelper.FormatXmlDocSafely(expectedXmlContent), DbTextFormatTestHelper.FormatXmlDocSafely(result));
		}

		protected abstract string GetValidInsertSql(Guid pk, string xmlColumnValue);
		protected abstract string ValidPreTransformedXml { get; }
		protected abstract string ValidPostTransformedXml { get; }

		static string InvalidXml
		{
			get { return "ExEmmEll..."; }
		}

		#endregion
	}
}
