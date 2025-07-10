using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreExtractor
	{
		public MENTAgedScoreExtractor(MENTAgedScoreExtraction extraction, IVisualisationFactoryProvider factoryProvider)
		{
			Argument.NotNull(extraction, nameof(extraction));
			rootExtraction = extraction;
			this.factoryProvider = factoryProvider;
		}

		readonly MENTAgedScoreExtraction rootExtraction;
		readonly IVisualisationFactoryProvider factoryProvider;

		public MENTAgedScoreExtractorResult Extract()
		{
			return new MENTAgedScoreExtractorResult(this.ExtractAllExtractions());
		}

		IEnumerable<MENTAgedScoreExtractionResult> ExtractAllExtractions()
		{
			var results = new ConcurrentBag<MENTAgedScoreExtractionResult>();

			AsyncStrategy.Default.ParallelForEach(rootExtraction.AllExtractionsForExtractor.ToArray(), extractionToCombine =>
			{
				var extractionFactory = factoryProvider.GetNewEditFactory("MENTAgedScoreExtractor.ParallelExtractionFactory");

				var importedExtraction = (MENTAgedScoreExtraction)extractionFactory.ImportFromAnotherFactory(extractionToCombine);

				var querySqlAndParameters = new ExtractionSqlGenerator(importedExtraction).GenerateQueryAndParameters();
				var rows = ExecuteExtraction(importedExtraction, querySqlAndParameters.QueryText, querySqlAndParameters.Parameters);
				results.Add(new MENTAgedScoreExtractionResult(rows, importedExtraction.MEX_Name));

				if (importedExtraction.HasChanges)
				{
					extractionFactory.Save();
				}
			});

			return results.OrderBy(r => r.Name);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is a form of viewModel")]
		Collection<MENTDataRow> ExecuteExtraction(MENTAgedScoreExtraction extraction, string queryText, ZSqlParameterCollection queryParams)
		{
			Argument.NotNull(extraction, nameof(extraction));

			var dataSet = new Collection<MENTDataRow>();

			using (var providedConnectionWrapper = factoryProvider.GetSecondaryServerConnectionProvider().GetNewConnectionWrapper())
			{
				var readerConnection = providedConnectionWrapper.Connection;
				var command = new ZSqlConnectionInfo(readerConnection, null).GetNewDbCommandForSelect(queryText, queryParams.ToArray());

				try
				{
					if (extraction.IsInstantaneous)
					{
						((IDataCollectionStrategy)extraction.RelatedQuery.GetQueryable().CollectionStrategy).PerformPreQueryOperation(readerConnection);
					}

					using (var result = command.ExecuteReader())
					{
						var seriesColumns = GetRelevantColumnsForExtraction(extraction.SeriesColumns);
						var categoryColumns = GetRelevantColumnsForExtraction(extraction.CategoryColumns);
						var yColumn = new SQLColumnSpecification(MENTConstants.AgedScoreValueColumn, MENTConstants.DecimalColumn, MENTConstants.DecimalColumn, false);

						var allColumns = seriesColumns.Concat(categoryColumns.Append(yColumn)).ToArray();

						while (result.Read())
						{
							if (!AnyResults(result, allColumns))
							{
								var seriesColumnPairs = GetColumnPairsForErrorReporting(seriesColumns, result);
								var categoryColumnPairs = GetColumnPairsForErrorReporting(categoryColumns, result);

								var caption = Res.GetString("d51e28db-2799-4f03-b44d-8bd028884d2f", "MENT Query error");
								var message = Res.GetString("9bfb8431-573f-4e3f-8424-5a41c4e637eb", "The query returned a null score. Please correct the query. Details:\r\n\r\nSeries Columns:\r\n{0}\r\n\r\nCategory Columns:\r\n{1}",
									string.Join(System.Environment.NewLine,
										seriesColumnPairs.Select(p => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", p.Item1, p.Item2))),
									string.Join(System.Environment.NewLine,
										categoryColumnPairs.Select(p => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", p.Item1, p.Item2))));
								Globals.Message.Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Error); // This is a form of viewModel
								dataSet.Clear();
								break;
							}

							MENTDataRow rowToAdd = TranslateColumnValuesIntoRow(result, allColumns, seriesColumns, categoryColumns);
							dataSet.Add(rowToAdd);
						}
						result.NextResult();
					}
				}
				catch (SqlException ex)
				{
					if (!extraction.IsInstantaneous)
					{
						ErrorReporter.ReportOnce("SQL Run resulted in an exception", string.Format(CultureInfo.InvariantCulture, "The system generated SQL: \r\n {0} \r\n\r\n had the following error: {1}", queryText, ex.Message), ex); // SQL for extractor
					}
					else
					{
						var message = Res.GetString("3aa239eb-bf3f-4388-a428-f9b5203bb9bd", "This Query failed with the following error: {0}", ex.Message);
						var caption = Res.GetString("b887d4c9-4511-4286-aa45-6e76be14e1fb", "Error with your Query");
						Globals.Message.Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Error); // This is a form of viewModel
						var note = extraction.RelatedQuery.Notes.AddNew();
						note.ST_Description = Res.GetString("bd2e6c22-fd32-47d2-836b-f51e289973ed", "Error With Query");
						note.ST_NoteDataAsText = ex.Message;
					}
				}
				finally
				{
					if (extraction.IsInstantaneous)
					{
						((IDataCollectionStrategy)extraction.RelatedQuery.GetQueryable().CollectionStrategy).PerformPostQueryOperation(readerConnection);
					}
				}
			}

			return dataSet;
		}

		List<Tuple<string, object>> GetColumnPairsForErrorReporting(SQLColumnSpecification[] columns, IDataReader result)
		{
			var columnPairs = new List<Tuple<string, object>>();
			object[] columnValues = new object[columns.Length];
			result.GetValues(columnValues);
			int i = 0;
			foreach (var col in columns)
			{
				if (!col.Description.IsEmpty)
				{
					columnPairs.Add(col.Description, !(columnValues[i] as string).IsNullOrEmpty() ? columnValues[i] : "NULL"); // This is a form of viewModel
				}
				i++;
			}
			return columnPairs;
		}

		bool AnyResults(IDataReader result, SQLColumnSpecification[] allColumns)
		{
			// We know there are actual results if the Y column is not a null value.
			var yColumn = result.GetValue(allColumns.Length - 1);
			return (yColumn as decimal? != null) || !(yColumn is DBNull);
		}

		SQLColumnSpecification[] GetRelevantColumnsForExtraction(ExtractionColumnCollection collection)
		{
			var columnsSelected = collection.Cast<SQLColumnSpecification>().Where(c => c.Selected).OrderBy(c => c.Sequence).ToArray();

			if (!columnsSelected.Any())
			{
				columnsSelected = new SQLColumnSpecification[] { new SQLColumnSpecification(MENTConstants.NullColumn, MENTConstants.NullColumn, MENTConstants.NullColumn, false) };
			}

			return columnsSelected.ToArray();
		}

		MENTDataRow TranslateColumnValuesIntoRow(IDataReader result, SQLColumnSpecification[] allColumns, SQLColumnSpecification[] seriesColumns, SQLColumnSpecification[] categoryColumns)
		{
			var columnValues = new List<IZType>();
			for (var i = 0; i < allColumns.Length; i++)
			{
				columnValues.Add(allColumns[i].TranslateObjectToCompatibleType(result.GetValue(i), position: i, totalItemCount: allColumns.Length));
			}

			var seriesColumnValues = columnValues.Take(seriesColumns.Length).Select((item, index) => new RowSegmentDefinitionAndData(index, item)).ToArray();
			var categoryColumnValues = columnValues.Skip(seriesColumns.Length).Take(categoryColumns.Length).Select((item, index) => new RowSegmentDefinitionAndData(index, item)).ToArray();
			var yColumnValues = columnValues.Last();

			return new MENTDataRow(seriesColumnValues, categoryColumnValues, (ZDecimal)yColumnValues);
		}
	}
}
