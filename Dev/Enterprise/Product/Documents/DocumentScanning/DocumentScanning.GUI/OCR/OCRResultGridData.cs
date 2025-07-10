using System;
using System.Collections.Specialized;
using System.Data;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.OCR
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class OCRResultTable
	{
		public const string Name = "OCRResult";
		public const string PK = "ZZ_PK";
		public const string LineNum = "ZZ_Line";
		public const string ColumnPrefix = "ZZ_Column";
	}

	public class OCRResultGridData
	{
		public OCRResultGridData(int aNumColumns)
		{
			this.NumColumns = aNumColumns;
			CreateDataStructures();
		}

		public int NumColumns { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public DataSet TheDataSet { get; set; }
		public DataTable ResultTable { get; set; }
		public const string DataSetName = "OCRResult";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected void CreateDataStructures()
		{
			this.TheDataSet = new DataSet(DataSetName);
			this.ResultTable = CreateResultTableStructure();
			this.TheDataSet.Tables.Add(this.ResultTable);
			this.ResultTable.RowChanging += new DataRowChangeEventHandler(RowChanging);
		}

		void RowChanging(object sender, DataRowChangeEventArgs e)
		{
			if (e.Action == DataRowAction.Add)
			{
				if (!Utilities.IsValidGuid(e.Row[OCRResultTable.PK]))
				{
					e.Row[OCRResultTable.PK] = Guid.NewGuid();
				}
			}
		}

		public static string GetColumnName(int zeroBasedColumnIndex)
		{
			return OCRResultTable.ColumnPrefix + (zeroBasedColumnIndex + 1).ToString();
		}

		protected DataTable CreateResultTableStructure()
		{
			DataTable curTable = new DataTable(OCRResultTable.Name);
			DataColumn primaryColumn = curTable.Columns.Add(OCRResultTable.PK, typeof(Guid));
			curTable.PrimaryKey = new DataColumn[] { primaryColumn };

			curTable.Columns.Add(OCRResultTable.LineNum, typeof(int));

			for (int ii = 0; ii < this.NumColumns; ii++)
			{
				string curColumnName = GetColumnName(ii);
				DataColumn curColumn = curTable.Columns.Add(curColumnName, typeof(string));
				curColumn.DefaultValue = "";
			}

			return curTable;
		}

		protected DataRow CreateNewResultTableRow(int aLineNum)
		{
			DataRow curRow = ResultTable.NewRow();
			curRow[OCRResultTable.PK] = Guid.NewGuid();
			curRow[OCRResultTable.LineNum] = aLineNum;

			return curRow;
		}

		protected void EnsureResultTableHasAtLeastXRows(int requiredRows)
		{
			int savedRowCount = ResultTable.Rows.Count;
			int needToCreate = requiredRows - savedRowCount;

			if (needToCreate > 0)
			{
				for (int i = 0; i < needToCreate; i++)
				{
					int curLineNum = savedRowCount + i + 1;
					ResultTable.Rows.Add(CreateNewResultTableRow(curLineNum));
				}

				ResultTable.AcceptChanges();
			}
		}

		public void SetColumnData(StringCollection aColumnData, int aColumnIndex)
		{
			string curColumnName = GetColumnName(aColumnIndex);

			EnsureResultTableHasAtLeastXRows(aColumnData.Count);
			for (int i = 0; i < aColumnData.Count; i++)
			{
				ResultTable.Rows[i][curColumnName] = aColumnData[i];
			}

			ResultTable.AcceptChanges();
		}

		public static StringCollection ConvertStringToCollection(string aLines)
		{
			StringCollection result = new StringCollection();

			if (aLines != null)
			{
				string[] parts = aLines.Split('\n');
				result.AddRange(parts);
			}
			return result;
		}
	}
}
