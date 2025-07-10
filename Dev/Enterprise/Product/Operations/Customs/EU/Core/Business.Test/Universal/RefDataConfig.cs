using System.Collections.Generic;

namespace Enterprise.Customs.EU.Business.Testing;

public class RefDataConfig(
	IReadOnlyCollection<DataGrouping> dataGroupings = null,
	IReadOnlyCollection<CusCodeType> cusCodeTypes = null,
	IReadOnlyCollection<MapType> mapTypes = null,
	IReadOnlyCollection<Map> maps = null)
{
	public IReadOnlyCollection<DataGrouping> DataGroupings { get; } = dataGroupings ?? [];
	public IReadOnlyCollection<CusCodeType> CusCodeTypes { get; } = cusCodeTypes ?? [];
	public IReadOnlyCollection<MapType> MapTypes { get; } = mapTypes ?? [];
	public IReadOnlyCollection<Map> Maps { get; } = maps ?? [];
}
