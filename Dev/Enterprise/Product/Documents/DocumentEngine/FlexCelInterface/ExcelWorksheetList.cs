using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class ExcelWorkSheetList : List<ExcelWorkSheet>, IExcelWorkSheetList
	{
		public ExcelWorkSheetList()
		{
		}

		public ExcelWorkSheetList(IEnumerable<ExcelWorkSheet> workSheets)
		{
			AddRange(workSheets);
		}

		#region IExcelWorkSheetList Members

		int IExcelWorkSheetList.Add(IExcelWorkSheet value)
		{
			return ((IList)this).Add((ExcelWorkSheet)value);
		}

		int IExcelWorkSheetList.IndexOf(IExcelWorkSheet value)
		{
			return IndexOf((ExcelWorkSheet)value);
		}

		void IExcelWorkSheetList.Insert(int index, IExcelWorkSheet value)
		{
			Insert(index, (ExcelWorkSheet)value);
		}

		IExcelWorkSheet IExcelWorkSheetList.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (ExcelWorkSheet)value; }
		}

		public IEnumerable<IExcelWorkSheet> this[System.Text.RegularExpressions.Regex sheetNameRegex]
		{
			get
			{
				return this.Where(workSheet => sheetNameRegex.IsMatch(workSheet.SheetName));
			}
		}

		public IExcelWorkSheet this[string workSheetName]
		{
			get
			{
				return Find(workSheet => workSheet.SheetName.Equals(workSheetName));
			}
			set
			{
				this[this.IndexOf((ExcelWorkSheet)this[workSheetName])] = (ExcelWorkSheet)value;
			}
		}

		#endregion

		internal ExcelWorkSheet Find(string workSheetName)
		{
			return (ExcelWorkSheet)this[workSheetName];
		}
	}
}
