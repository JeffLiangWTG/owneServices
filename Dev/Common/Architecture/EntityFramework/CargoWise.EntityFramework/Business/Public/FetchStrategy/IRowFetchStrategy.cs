using System.Data;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Defines a strategy that is executed on loaded data rows, before BusinessObject instances are created
	/// </summary>
	public interface IRowFetchStrategy
	{
		/// <summary>
		/// Define your strategy for load here - useful for preloading objects that will be used in TypeDecider instances
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="rows"></param>
		void FetchForLoad(BusinessObjectFactory factory, DataRow[] rows);
	}
}
