using System;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	[Serializable]
	public struct CellReference : IJsonSerializable
	{
		public CellReference(string sheetName, string cell)
		{
			this.sheetName = sheetName;
			this.cell = cell;
		}

		public CellReference(ExcelInterface xlInterface, int sheetNo, string cell)
			: this(xlInterface.GetSheetName(sheetNo), cell)
		{
		}

		public CellReference(string sheetName, int row, int col)
			: this(sheetName, GetCellRef(row, col))
		{
		}

		public CellReference(ExcelInterface xlInterface, int sheetNo, int row, int col)
			: this(xlInterface, sheetNo, GetCellRef(row, col))
		{
		}

		#region Constructor For IJsonSerializable

		internal CellReference(CellReferenceJsonData data)
		{
			sheetName = data.SheetName;
			cell = data.Cell;
		}

		#endregion

		public static string GetCellRef(int rowNumber, int columnNumber)
		{
			string columnName = string.Empty;
			int excelColumnNumber = columnNumber + 1;
			int columnRemain = excelColumnNumber;

			while (columnRemain > 0)
			{
				int currentPart = columnRemain % 26;
				if (currentPart == 0) { currentPart = 26; }
				columnRemain = (columnRemain - currentPart) / 26;
				char columnChar = (char)('A' - 1 + currentPart);
				columnName = columnChar + columnName;
			}

			return columnName + (rowNumber + 1).ToString();
		}

		string sheetName;
		string cell;

		public string SheetName
		{
			get => sheetName ?? UnKnownSheetName;
			set => sheetName = value;
		}

		public string Cell
		{
			get => cell ?? UnKnownCellName;
			set => cell = value;
		}

		public static CellReference UnKnown
		{
			get { return new CellReference(null, null); }
		}

		static string UnKnownSheetName
		{
			get { return Res.GetString("3be0fcd3-5e26-4bb8-9f51-dd9cb872a668", "(unknown)"); }
		}

		static string UnKnownCellName
		{
			get { return Res.GetString("00479629-9f11-4dea-9dee-97f75dd80c13", "N/A"); }
		}

		public bool IsEmpty
		{
			get
			{
				return (string.IsNullOrEmpty(sheetName) || sheetName == UnKnownSheetName)
						&& (string.IsNullOrEmpty(cell) || cell == UnKnownCellName);
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new CellReferenceJsonData
			{
				SheetName = SheetName,
				Cell = Cell
			};

		#endregion
	}
}
