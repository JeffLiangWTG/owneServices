using System;
using System.Collections.Generic;
namespace Enterprise.Services.OperationalActions.Support
{
	public interface IFilterRecordsSelection : ITargetRecordSelection
	{
		IEnumerable<ISelectedRecords> GetAllFilterRecords(Type bizObjType);
	}

	public interface ITargetRecordSelection
	{
		ISelectedRecords GetSelectedRecords();
		IList<string> ExclusionReasons { get; }
		int FilterRowCount { get; }
	}
}
