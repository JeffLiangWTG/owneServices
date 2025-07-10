using System.Data;

namespace CargoWise.EntityFramework
{
	// DO NOT USE THESE UNLESS YOU ARE WRITING ARCHITECTURE STUFF or are a gumby

	public interface INeedRow
	{
		DataRow Row { get; }
	}

	public interface INeedDataSet
	{
		DataSet Data { get; }
	}

	public interface INeedTable
	{
		ZDataTable Table { get; }
	}
}
