using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations
{
	[TestsSubclassesOf(typeof(SchemaChangeDataTransformation))]
	public abstract class DataCopyTestCase : TestWithTransformationDirectorCopyDb
	{
		[UseSnapshotProtection]
		public void TestRunAndAssertResults()
		{
			CreateOldTablesAndOrColumnsIfNotExist();

			PrepareOriginalData();
			TransformationToTest.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			DropCreatedOldTablesAndColumns();

			TransformationToTest.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertDataCopyResults();
		}

		public void TestSourceColumnsWithNoDefaultIfEmptyValueShouldBeAllowedExplicitly()
		{
			int columnsWithNoReplacementValueCount = 0;
			StringBuilder columnsWithNoReplacementValue = new StringBuilder();

			foreach (SourceTable table in TransformationToTest.SourceTables)
			{
				foreach (SourceColumn column in table.Columns)
				{
					string tableColumnKey = table.OriginalName + "." + column.Name;

					if (column.ReplacementValueIfNotExist == null
						&& !column.Name.EndsWith("_PK")
						&& !IAcknowledgeTheseSourceColumnsHaveNoReplacementValue.Contains(tableColumnKey)
						)
					{
						columnsWithNoReplacementValueCount++;
						columnsWithNoReplacementValue.AppendLine(tableColumnKey);
					}
				}
			}

			string failMessage = "\r\n\r\n"
				+ "When a source column doesn't exist in the original client database and a replacement value has not been specified \r\n"
				+ "in the transformation (GetSourceTables), the entire transformation is skipped (since there's no source data to be copied).\r\n"
				+ "That means other columns, even if they exist won't be copied either.\r\n\r\n"
				+ "Please provide REPLACEMENT values for the columns below,\r\n"
				+ "unless they are mandatory for the transformation (i.e. their absence make the transformation unnecessary).\r\n"
				+ "For the mandatory columns, please override and add them to IAcknowledgeTheseSourceColumnsHaveNoReplacementValue."
				+ "\r\n\r\n\r\n" + columnsWithNoReplacementValue.ToString() + "\r\n";

			AssertEquals(failMessage, 0, columnsWithNoReplacementValueCount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Only using BaseSourcePath for local testing, not on DAT")]
		public void TestIsMapped()
		{
			Type expectedType = TransformationToTest.GetType();

			if (expectedType == typeof(DataCopyForTesting))
			{
				Assert("DataCopyForTesting is a test only class, not supposed to be mapped.", true);
			}
			else
			{
				var isMapped = IsTransformationMapped(expectedType);

				if (string.IsNullOrWhiteSpace(ReasonNotToBeMapped))
				{
					const string text =
						"This transform is not mapped.<br/>" +
						"Please see the <a href=\"https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Data%20Transformation.aspx\">wiki</a> for instructions.";
					HtmlAssert(text, isMapped);
				}
				else
				{
					Assert($"This transformation is mapped but it shouldn't be (Reason: {ReasonNotToBeMapped})", !isMapped);
				}
			}
		}

		protected virtual string ReasonNotToBeMapped => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Only using BaseSourcePath for local testing, not on DAT")]
		static bool IsTransformationMapped(Type expectedTransformType)
		{
			var isMapped = false;

			if (!TestingState.IsRunningOnDAT)
			{
				string expectedTypeText = expectedTransformType.FullName;
				string mapperFile = Path.Combine(TestCase.BaseSourcePath, "Database", "Odyssey", "Transformations", "Transformations", "Transforms", "ShelfCheckinMapper.txt");

				using (TextReader reader = new StreamReader(mapperFile))
				{
					string line;

					while ((line = reader.ReadLine()) != null)
					{
						if (line == expectedTypeText)
						{
							isMapped = true;
							break;
						}
					}
				}
			}

			if (!isMapped)
			{
				IUpgradeManager manager = new DummyUpgradeManager();
				var mapper = new Mapper(manager);

				foreach (var transform in mapper.AllTransformations)
				{
					Type transformType = transform.GetType();

					if (transformType == expectedTransformType)
					{
						isMapped = true;
						break;
					}
				}
			}

			return isMapped;
		}

		/// <summary>
		/// Helper provides access to test data creation methods (mostly yanked from a shitful test).
		/// </summary>
		protected TransformationTestDataCreator TestDataCreator
		{
			get { return fTestDataCreator ?? (fTestDataCreator = new TransformationTestDataCreator()); }
		}

		TransformationTestDataCreator fTestDataCreator;

		/// <summary>
		/// NEW Instance of transformation to be tested
		/// </summary>
		protected abstract SchemaChangeDataTransformation GetNewTestTransformationInstance();
		/// <summary>
		/// Prepares test data before runing transformation
		/// </summary>
		protected abstract void PrepareOriginalData();

		/// <summary>
		/// Revert the transformation column type if it is changed in the PrepareOriginalData function
		/// </summary>
		protected virtual void RevertDatabaseColumnTypeChanges()
		{
			return;
		}

		/// <summary>
		/// Asserts data after transformation is run
		/// </summary>
		protected abstract void AssertDataCopyResults();

		/// <summary>
		/// List of columns in a TableName.ColumnName format
		/// The column listed here should be the ones that make the transformation unnecessary if they don't exist
		/// </summary>
		protected virtual List<string> IAcknowledgeTheseSourceColumnsHaveNoReplacementValue
		{
			get { return new List<string>(); }
		}

		/// <summary>
		/// Creates old tables/columns if they do not exist
		/// </summary>
		protected void CreateOldTablesAndOrColumnsIfNotExist()
		{
			foreach (SourceTable table in TransformationToTest.SourceTables)
			{
				((ISourceTableTestHelper)table).CreateOriginalTableAndOrColumnsIfNotExist();
			}
		}

		protected void DropCreatedOldTablesAndColumns()
		{
			RevertDatabaseColumnTypeChanges();

			foreach (SourceTable table in TransformationToTest.SourceTables)
			{
				((ISourceTableTestHelper)table).DropCreatedOriginalTableAndOrColumnsAfterCopyingData();
			}
		}

		#region TransformationToTest

		protected SchemaChangeDataTransformation TransformationToTest
		{
			get
			{
				if (fTransformationToTest == null)
				{
					fTransformationToTest = GetNewTestTransformationInstance();
				}

				return fTransformationToTest;
			}
		}

		protected SchemaChangeDataTransformation fTransformationToTest;

		#endregion
	}
}
