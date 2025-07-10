using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Main.Data;

public class PublicHolidaysRepository : IPublicHolidaysRepository
{
	const string ParentCodeOfCountry = "RN";
	const string ParentCodeOfState = "RW";

	BusinessObjectFactory factory;
	BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();

	[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Faster to query DB directly and we there is no need for bizo type reference.")]
	public IEnumerable<PublicHoliday> GetPublicHolidays(DateTime startDate, DateTime endDate)
	{
		var parentCodeQuery = new ZQuery();
		parentCodeQuery.AddToFilter(JoinCondition.Or, GlbHolidaySchema.GH_ParentTableCode, ParentCodeOfState);
		parentCodeQuery.AddToFilter(JoinCondition.Or, GlbHolidaySchema.GH_ParentTableCode, ParentCodeOfCountry);

		var query = new ZQuery();
		query.AddToFilter(JoinCondition.And, GlbHolidaySchema.GH_IsActive, true);
		query.AddToFilter(JoinCondition.And, GlbHolidaySchema.GH_Date, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
		query.AddToFilter(JoinCondition.And, GlbHolidaySchema.GH_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, endDate);
		query.AddToFilter(parentCodeQuery, JoinCondition.And);

		var glbHolidayArray = Factory.Load<GlbHoliday>(query);
		var publicHolidayCollection = new List<PublicHoliday>();
		foreach (var glbHoliday in glbHolidayArray)
		{
			var date = glbHoliday.GH_Date.ToDateTime();
			publicHolidayCollection.Add(new PublicHoliday()
			{
				Date = date,
				RegionCode = glbHoliday.Country?.Code,
				RegionName = glbHoliday.Country?.GetMultilingualDescription(),
				Description = glbHoliday.GH_HolidayNameMultilingual,
				Month = glbHoliday.Months[date.Month - 1].GetMultilingualCode(),
				Weekday = glbHoliday.WeekDays[(int)date.DayOfWeek].GetMultilingualDescription()
			});
		}

		// ZQuery does not support cross-table order by.
		// So here we use linq's orderby.
		// Why didn't we use a sql query?
		// Because GlbHoliday encapsulates some multi-language text queries,
		// e.g. country name, holiday name. Using sql query we will lose these multilingual features.
		return publicHolidayCollection.OrderBy(h => h.Date).ThenBy(h => h.RegionName);
	}

	public Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync(DateTime startDate, DateTime endDate) => Task.Run(() => GetPublicHolidays(startDate, endDate));
}
