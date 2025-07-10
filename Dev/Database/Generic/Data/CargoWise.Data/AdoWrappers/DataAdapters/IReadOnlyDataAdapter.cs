using System;
using System.Data;

namespace CargoWise.Data
{
	public interface IReadOnlyDataAdapter : IDisposable
	{
		int Fill(DataSet dataSet);
		int Fill(DataTable dataTable);
	}
}
