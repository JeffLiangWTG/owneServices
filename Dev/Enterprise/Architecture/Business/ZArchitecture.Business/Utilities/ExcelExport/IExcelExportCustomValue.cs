using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Excel
{
	public interface IExcelExportCustomValue : IExcelExportCustomFunction
	{
		IZType GetCustomValue(BusinessObject bizObj);
		ZString GetValueFormat(IZType value);
		ZString GetDescription();
	}
}
