using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.Main.Navigation;

public interface IPublicHolidaysRepository
{
	IEnumerable<PublicHoliday> GetPublicHolidays(DateTime startDate, DateTime endDate);
	Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync(DateTime startDate, DateTime endDate);
}
