using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Excel
{
	public interface IExcelExportCellComment : IExcelExportCustomFunction
	{
		ZString GetComment(BusinessObject bizObj);
	}
}
