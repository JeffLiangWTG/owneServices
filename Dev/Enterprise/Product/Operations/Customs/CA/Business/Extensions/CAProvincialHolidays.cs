using CargoWise.Types;

namespace Enterprise.Customs.CA.Business;

internal class CAProvincialHolidays
{
	internal CAProvincialHolidays(ZString province, ZDateTime holidayDate)
	{
		this.Province = province;
		this.HolidayDate = holidayDate;
	}

	internal readonly ZString Province;
	internal readonly ZDateTime HolidayDate;
}
