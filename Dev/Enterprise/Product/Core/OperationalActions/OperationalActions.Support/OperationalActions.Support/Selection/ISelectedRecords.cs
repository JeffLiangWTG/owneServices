using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Support
{
	public interface ISelectedRecords
	{
		ZGuid[] PrimaryKeys { get; }
		bool AutoSelectedAllKeys { get; }
	}
}
