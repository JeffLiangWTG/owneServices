using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine
{
	sealed class FormulaProvider
	{
		public FormulaProvider(ExcelWorkSheet workSheet)
			: this(workSheet, new Dictionary<string, CellPosition>())
		{
		}

		FormulaProvider(ExcelWorkSheet workSheet, Dictionary<string, CellPosition> fieldNames)
		{
			this.fieldNames = fieldNames;
			this.workSheet = workSheet;
		}

		readonly ExcelWorkSheet workSheet;
		readonly Dictionary<string, CellPosition> fieldNames;

		//Excel puts some constraints on the max text length for formulae.
		//AccumulativeTotal or similar macros might be used with other macros like <Currency> which will use the ROUND excel function.
		//So we reduce the formula size available to FormulaProvider by a comfortable margin.
		const int FormulaLengthAllocatedToOtherMacros = 64;

		public void AddColumn(int columnNumber, string fieldName, int indexInArea)
		{
			fieldName = fieldName.ToUpperInvariant().Trim();

			if (!fieldNames.ContainsKey(fieldName))
			{
				fieldNames.Add(fieldName, new CellPosition(columnNumber, indexInArea));
			}
		}

		public FormulaProvider Clone()
		{
			return new FormulaProvider(workSheet, fieldNames);
		}

		public TFormula GetFormula(string fieldName, List<Area> areasToProcess, TFileFormats fileFormat)
		{
			return GetFormula(fieldName, areasToProcess, null, fileFormat);
		}

		public TFormula GetFormula(string fieldName, List<Area> areasToProcess, int maxRowsToProcessInLastArea, TFileFormats fileFormat)
		{
			return GetFormula(fieldName, areasToProcess, new int?(maxRowsToProcessInLastArea), fileFormat);
		}

		TFormula GetFormula(string fieldName, List<Area> areasToProcess, int? maxRowsToProcessInLastArea, TFileFormats fileFormat)
		{
			RowRangeList rowRanges = new RowRangeList();

			for (int i = 0; i < areasToProcess.Count; i++)
			{
				Area area = areasToProcess[i];

				if (ThisOrParentAreaContainsField(area, fieldName))
				{
					if (maxRowsToProcessInLastArea.HasValue && i == areasToProcess.Count - 1)
					{
						rowRanges.AddRange(area.GetRowRanges(GetRowIndex(fieldName), fieldName, maxRowsToProcessInLastArea.Value));
					}
					else
					{
						rowRanges.AddRange(area.GetRowRanges(GetRowIndex(fieldName), fieldName));
					}
				}
			}

			return GetFormula(fieldName, rowRanges, fileFormat);
		}

		bool ThisOrParentAreaContainsField(Area area, string fieldName)
		{
			while (area != null)
			{
				if (area.FormulaProvider != null && area.FormulaProvider.ContainsColumn(fieldName))
				{
					return true;
				}

				area = area.DataParent;
			}

			return false;
		}

		#region Implementation

		#region FieldKey

		internal class FieldKey
		{
			public FieldKey(string fieldName)
				: this(fieldName, string.Empty)
			{
			}

			public FieldKey(string fieldName, string fieldFullName)
			{
				FieldName = fieldName;
				FieldFullName = fieldFullName;
			}

			public string FieldName { get; }
			public string FieldFullName { get; }
			public bool IsResetting { get; set; }

			public bool IsSameKey(FieldKey fieldKey)
			{
				return FieldName == fieldKey.FieldName
					|| FieldFullName == fieldKey.FieldName
					|| FieldFullName.EndsWith($".{fieldKey.FieldName}");
			}
		}

		public FieldKey GetFieldKey(string fieldName)
		{
			fieldName = fieldName.ToUpperInvariant().Trim();
			if (!fieldNames.TryGetValue(fieldName, out _))
			{
				var fullName = fieldNames.Keys.FirstOrDefault(x => x.EndsWith($".{fieldName}"));
				return fullName != null ? new FieldKey(fieldName, fullName) : null;
			}

			return new FieldKey(fieldName, fieldName);
		}

		#endregion

		class CellPosition
		{
			public CellPosition(int columnNumber, int rowIndex)
			{
				this.ColumnNumber = columnNumber;
				this.RowIndex = rowIndex;
			}
			public readonly int ColumnNumber;
			public readonly int RowIndex;
		}

		public bool ContainsColumn(string fieldName)
		{
			return GetCell(fieldName) != null;
		}

		CellPosition GetCell(string fieldName)
		{
			fieldName = fieldName.ToUpperInvariant().Trim();
			CellPosition result = null;

			if (!fieldNames.TryGetValue(fieldName, out result))
			{
				foreach (string fullFieldName in fieldNames.Keys)
				{
					if (fullFieldName.EndsWith("." + fieldName))
					{
						result = fieldNames[fullFieldName];
						break;
					}
				}
			}

			return result;
		}

		int GetRowIndex(string fieldName)
		{
			CellPosition position = GetCell(fieldName);

			if (position != null)
			{
				return position.RowIndex;
			}

			throw new FormulaProviderException("Field " + fieldName + " is not found in Area!");
		}

		int GetColumnNumber(string fieldName)
		{
			CellPosition position = GetCell(fieldName);

			if (position != null)
			{
				return position.ColumnNumber;
			}

			throw new FormulaProviderException("Field " + fieldName + " is not found in Area!");
		}

		TFormula GetFormula(string fieldName, RowRangeList rowRanges, TFileFormats fileFormat)
		{
			using (Culture.SetTemporarily(Culture.Default))
			{
				if (fieldNames.Count == 0)
				{
					throw new FormulaProviderNotReadyException("No columns Added yet!");
				}

				if (rowRanges.Count == 0)
				{
					return new TFormula("=0", 0);
				}

				var rangeList = new List<string>();
				var column = GetColumnNumber(fieldName);
				bool needSum = false;

				foreach (var range in rowRanges)
				{
					if (range.End == range.Start)
					{
						rangeList.Add(workSheet.ParentExcelInterface.GetCellReference(range.Start - 1, column));
					}
					else
					{
						needSum = true;
						rangeList.Add(workSheet.ParentExcelInterface.GetCellReference(range.Start - 1, column) + ":" + workSheet.ParentExcelInterface.GetCellReference(range.End - 1, column));
					}
				}

				var maxFormulaArgs = workSheet.ParentExcelInterface.GetMaxFormulaArgumentsByCurrentExcelFile(fileFormat);
				string result = string.Empty;

				if (needSum)
				{
					result = "=SUM(" + string.Join(")+SUM(", rangeList.Chunk(maxFormulaArgs).Select(range => string.Join(",", range))) + ")"; // Excel formula should NOT be translated
				}
				else
				{
					result = "=" + string.Join("+", rangeList);
				}

				var remainingFormulaTextLength = workSheet.ParentExcelInterface.GetMaxFormulaTextSizeSupportedByCurrentExcelFile(fileFormat) - FormulaLengthAllocatedToOtherMacros;

				if (result.Length > remainingFormulaTextLength)
				{
					var totalValue = 0.0;

					try
					{
						workSheet.ParentExcelInterface.Xls.StartBatchRecalcCells();
						foreach (var range in rowRanges)
						{
							for (int row = range.Start; row <= range.End; row++)
							{
								var rowIndex = row - 1;
								if (workSheet[rowIndex, column] is TFormula)
								{
									workSheet.RecalcCell(rowIndex, column, false);
								}

								var cellValue = workSheet[rowIndex, column].ToString();
								var value = 0.0;

								if (double.TryParse(cellValue, out value))
								{
									totalValue += value;
								}
							}
						}
					}
					finally
					{
						workSheet.ParentExcelInterface.Xls.EndBatchRecalcCells();
					}

					result = "=" + totalValue.ToString(CultureInfo.InvariantCulture);
				}

				return new TFormula(result, 0);
			}
		}

		#endregion

		#region For Testing
		internal TFormula GetFormulaForTesting(string fieldName, RowRangeList rowRanges, TFileFormats fileFormat)
		{
			return GetFormula(fieldName, rowRanges, fileFormat);
		}
		#endregion
	}
}
