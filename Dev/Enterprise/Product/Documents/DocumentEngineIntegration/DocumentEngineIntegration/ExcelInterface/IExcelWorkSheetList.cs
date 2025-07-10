using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IExcelWorkSheetList
	{
		IExcelWorkSheet this[int index] { get; set; }
		IExcelWorkSheet this[string workSheetName] { get; set; }
		IEnumerable<IExcelWorkSheet> this[Regex sheetNameRegex] { get; }

		int IndexOf(IExcelWorkSheet value);
		void Insert(int index, IExcelWorkSheet value);
		int Add(IExcelWorkSheet value);
		void Clear();
		int Count { get; }
	}
}