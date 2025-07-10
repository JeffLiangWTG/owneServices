using System;

namespace CargoWise.EntityFramework
{
	public interface ITopLevelDataSourceType
	{
		Type DataSourceType { get; }
	}
}
