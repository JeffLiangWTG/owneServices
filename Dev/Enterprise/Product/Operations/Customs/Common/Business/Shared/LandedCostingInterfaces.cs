using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public delegate void LCProgressEventHandler(int percentage);

	public interface ILandedCostBulkCreator
	{
		/// <summary>
		/// Create LandedCostHeader for each host if it supports and LandedCostHeader does not already exist.
		/// It runs LandedCost Distribution
		/// </summary>
		/// <param name="hosts"></param>
		/// <returns>A list of job numbers</returns>
		IEnumerable<string> CreateAndRunLC(IEnumerable<ILandedCostHeader> hosts);

		event LCProgressEventHandler OnLCProgressChanged;
	}
}
