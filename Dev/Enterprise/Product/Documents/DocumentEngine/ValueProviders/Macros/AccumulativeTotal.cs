using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using static Enterprise.DocumentEngine.FormulaProvider;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class AccumulativeTotal : ValueProvider, ITFormulaProvider
	{
		readonly Dictionary<FieldKey, List<Area>> areasByFieldName = new Dictionary<FieldKey, List<Area>>();
		readonly Dictionary<FieldKey, Area> lastProcessedArea = new Dictionary<FieldKey, Area>();
		readonly Dictionary<FieldKey, ZDecimal> accumulativeTotalData = new Dictionary<FieldKey, ZDecimal>();

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<AccumulativeTotal {fieldname}> or <AccumulativeTotalWithReset {fieldname}>",
				ResString.GetMultilingualString("8f29e30b-0acb-4ac5-8b67-60af23ccf5a9", @"{0} will return an accumulative total of all prior occurrences of the field specified by {1}. {2} will also reset the total so that all subsequent {0} for the specified fields won't include fields already covered.",
				"AccumulativeTotal", "{fieldname}", "AccumulativeTotalWithReset"),
				new List<(string example, object expectedResult)> { ((NoResString)"<AccumulativeTotal InvoiceLines.Amount>", string.Empty), ((NoResString)"<AccumulativeTotalWithReset InvoiceLines.Amount>", string.Empty) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			try
			{
				return DoReplacement(macro, report);
			}
			catch (FormulaProviderException ex)
			{
				ReportMacroError(report, Res.GetString("61256c0b-8b5a-4488-ba47-5b5d195fa838", "Formula Provider Error :- {0}", ex.Message));
			}
			catch (FormulaProviderNotReadyException ex)
			{
				ReportMacroError(report, Res.GetString("c4b020f3-2c1b-457d-9bd7-f8c872c0e2d1", "Formula Provider Not Ready Error :- {0}", ex.Message));
			}

			return string.Empty;
		}

		protected virtual object DoReplacement(string macro, Report report)
		{
			if (report.Analyser != null)
			{
				var match = Regex.Match(macro);
				var isResetting = match.Groups["WithReset"].Success;
				var fieldName = match.Groups["FieldName"].Value;

				if (!string.IsNullOrEmpty(fieldName))
				{
					var fieldKey = new FieldKey(fieldName.ToUpper().Trim());
					var result = CalculateAccumulativeTotalFromReportAreas(fieldKey, report);

					AddLastProcessedArea(fieldKey, report.Renderer.CurrentAreaToProcess);

					if (isResetting)
					{
						var key = accumulativeTotalData.Keys.FirstOrDefault(k => k.IsSameKey(fieldKey));
						if (key != null)
						{
							key.IsResetting = true;
						}
					}

					if (result == 0 && !areasByFieldName.Any(x => x.Key.IsSameKey(fieldKey)))
					{
						return null;
					}

					return result;
				}
			}

			return null;
		}

		#region Calculate

		ZDecimal CalculateAccumulativeTotalFromReportAreas(FieldKey fieldKey, Report report)
		{
			var resultValue = new ZDecimal(0);
			var currentAreaIsBodyArea = false;

			if (report.Renderer.CurrentAreaToProcess is SectionBodyArea currentBodyArea)
			{
				currentAreaIsBodyArea = true;
				var realKey = currentBodyArea.FormulaProvider?.GetFieldKey(fieldKey.FieldName);
				if (realKey != null)
				{
					var resultObject = currentBodyArea.GetColumnValue(report.Renderer.CurrentDataRow, realKey.FieldFullName);
					if (IsNumber(resultObject))
					{
						AccumulativeTotalDataAdd(realKey, new ZDecimal(resultObject), true);
						AreasByFieldNameAdd(realKey, currentBodyArea);
					}
				}
			}

			var start = report.Analyser.Areas.IndexOf(report.Renderer.CurrentAreaToProcess) - 1;
			var shouldAccumulativeTotalData = accumulativeTotalData.Any(x => x.Key.IsSameKey(fieldKey));
			for (var index = start; index >= 0; index--)
			{
				var area = report.Analyser.Areas[index];

				if (IsCalculatedBeforeLastProcessedArea(fieldKey, area))
				{
					break;
				}

				if (area is SectionBodyArea bodyArea
					&& !area.ShouldDelete
					&& area.FormulaProvider != null)
				{
					var realKey = area.FormulaProvider.GetFieldKey(fieldKey.FieldName);
					if (realKey != null)
					{
						if (areasByFieldName.Any(x => x.Key.IsSameKey(fieldKey) && x.Value.Contains(area)))
						{
							if (currentAreaIsBodyArea)
							{
								break;
							}

							if (accumulativeTotalData.TryGetValue(realKey, out var previousValue))
							{
								resultValue += new ZDecimal(previousValue);
								continue;
							}
						}

						resultValue = CalculateAccumulativeDataInSectionBodyArea(realKey.FieldFullName, bodyArea, resultValue);

						AreasByFieldNameAdd(realKey, area);
					}
				}
			}

			resultValue = AccumulativeTotalDataAdd(fieldKey, resultValue, shouldAccumulativeTotalData);
			return resultValue;
		}

		bool IsNumber(object resultObject)
		{
			return resultObject is INumericZType || IsNumericType(resultObject);
		}

		protected bool IsNumericType(object resultObject)
		{
			switch (Type.GetTypeCode(resultObject?.GetType()))
			{
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		ZDecimal CalculateAccumulativeDataInSectionBodyArea(string fieldFullName, SectionBodyArea bodyArea, ZDecimal resultValue)
		{
			var dataRows = bodyArea.DataRowIndexWithRowRanges
				.Where(d => d.Value.Start >= bodyArea.StartingRow && d.Value.Start <= bodyArea.End)
				.ToArray();
			foreach (var row in dataRows)
			{
				var resultObject = bodyArea.GetColumnValue(row.Key, fieldFullName);
				if (IsNumber(resultObject))
				{
					resultValue += new ZDecimal(resultObject);
				}
			}
			return resultValue;
		}

		void AreasByFieldNameAdd(FieldKey fieldKey, Area area)
		{
			var key = areasByFieldName.Keys.FirstOrDefault(k => k.IsSameKey(fieldKey));
			if (key == null)
			{
				areasByFieldName.Add(fieldKey, new List<Area>() { area });
			}
			else
			{
				if (!areasByFieldName[key].Contains(area))
				{
					areasByFieldName[key].Add(area);
				}
			}
		}

		ZDecimal AccumulativeTotalDataAdd(FieldKey fieldKey, ZDecimal resultValue, bool shouldAccumulativeTotalData)
		{
			var key = accumulativeTotalData.Keys.FirstOrDefault(k => k.IsSameKey(fieldKey));
			if (key != null)
			{
				var previousValue = accumulativeTotalData[key];
				if (key.IsResetting)
				{
					previousValue = new ZDecimal(0);
					key.IsResetting = false;
				}

				if (shouldAccumulativeTotalData)
				{
					resultValue += previousValue;
				}
				accumulativeTotalData[key] = resultValue;
			}
			else
			{
				accumulativeTotalData.Add(fieldKey, resultValue);
			}

			return resultValue;
		}

		#endregion

		#region lastProcessedArea

		void AddLastProcessedArea(FieldKey fieldKey, Area area)
		{
			var realKey = lastProcessedArea.Keys.FirstOrDefault(k => k.IsSameKey(fieldKey));
			if (realKey != null)
			{
				lastProcessedArea[realKey] = area;
			}
			else
			{
				lastProcessedArea.Add(fieldKey, area);
			}
		}

		bool IsCalculatedBeforeLastProcessedArea(FieldKey fieldKey, Area area)
		{
			return lastProcessedArea.Any(a => a.Key.IsSameKey(fieldKey) && a.Value == area);
		}

		#endregion

		protected override void ResetCore()
		{
			areasByFieldName.Clear();
			lastProcessedArea.Clear();
			accumulativeTotalData.Clear();
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex => RegexProvider.AccumulativeTotalMacroRegex;
	}
}
