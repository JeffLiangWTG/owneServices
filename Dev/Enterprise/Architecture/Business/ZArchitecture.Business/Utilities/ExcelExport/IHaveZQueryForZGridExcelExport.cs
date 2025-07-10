using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Excel
{
	public interface IHaveZQueryForZGridExcelExport
	{
		ZQuery Query { get; }
	}
}
