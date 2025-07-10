using System.Drawing;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Excel
{
	public interface IExcelExportCellColor : IExcelExportCustomFunction
	{
		Color? GetCustomColor(BusinessObject bizObj);
	}
}
