using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Integration.Billing
{
	public interface IStlItem
	{
		string Code { get; }
		string Role { get; }
		string Module { get; }
		string Function { get; }
		string Feature { get; }
		StlDataGrain StlGrain { get; }
		bool IsSystemLevel { get; }
		bool IsMandatoryForMilestones { get; }
		bool IsActive { get; }
		Exception CollectionException { get; set; }
		bool CollectionOccurred { get; set; }
		StlDateType DateType { get; }
		StlCollectorType CollectorType { get; }
		DateTime CollectionStartDateUtc { get; }

		IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange);
		IEnumerable<IStlTransaction> Run(IDateTimeRange dateTimeRange);
	}

	public enum StlDataGrain
	{
		Transactional = 0,
		MonthlyAllowHistoricalData = 1,
		MonthlyCurrentDataOnly = 2,
		Daily = 3,
		Snapshot = 4
	}

	public enum StlDateType
	{
		DateTime = 0,
		SmallDateTime = 1,
		DateTimeOffset = 2
	}

	public enum StlCollectorType
	{
		Dynamic,
		Custom
	}
}
