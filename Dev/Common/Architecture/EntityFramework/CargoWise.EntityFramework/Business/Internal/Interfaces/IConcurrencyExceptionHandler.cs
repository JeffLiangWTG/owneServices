using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IConcurrencyExceptionHandler
	{
		string Info { get; }

		Dictionary<string, object> ColumnsDBChanged { get; }
	}
}
