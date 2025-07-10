using System;
using System.Drawing;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IExcelWorkSheet : IDisposable
	{
		object this[int row, int col] { get; set; }
		void ClearRange(int startRow, int startCol, int endRow, int endCol);
		void InsertRows(int rowToInsertBefore, int rowCount);
		void RemoveRows(int startRow, int endRow);
		string GetCellFormula(int row, int col);
		void DuplicateRows(int firstRow, int lastRow, int destinationRow, int count);
		void SetCellFormula(int row, int col, string formula, double formulaResult);
		int GetRowHeight(int rowNumber);
		void SetRowHeight(int rowNumber, int height);
		int GetCellWidth(int row, int col);
		int GetCellFontSize(int row, int col);
		int GetCharCountFittingInCell(int row, int col);
		void InsertHPageBreak(int row);
		void HideColumn(int column);
		void DeleteColumn(int column);
		int GetRowRangeHeight(int startRow, int endRow);
		string SheetName { get; }
		double GetTopMargin();
		double GetBottomMargin();
		int GetColWidth(int col);
		void SetColWidth(int columnNumber, int width);
		void Hide();
		bool DoesRowContainAMergedCellToNextRow(int row);
		void MergeCells(int firstRow, int firstCol, int lastRow, int lastCol);
		void UnMergeCells(int firstRow, int firstCol, int lastRow, int lastCol);
		void ClearBottomLine(int row);
		void CopyBorderSettings(int from, int to);
		void CopyAndInsertRows(IExcelWorkSheet source, int sourceRowIndex, int sourceRowCount, int destinationRowIndex);
		int ColumnCount { get; }
		int RowCount { get; }
		bool HasHPageBreak(int rowNumber);
		CellFormat GetCellFormat(int row, int col);
		void SetCellFormat(int row, int col, CellFormat format);
		Font GetCellFont(int row, int col);
		bool ObjectExists(string objectName);
		void RemoveAllObjectsByName(string objectName);
		string[] GetObjectNames();
		void MoveColumns(int fromColumn, int toColumn);
		bool IsColumnHidden(int column);
		void SetComment(int row, int col, string comment);
#if DEBUG
		string GetComment(int row, int col);
#endif
	}
}
