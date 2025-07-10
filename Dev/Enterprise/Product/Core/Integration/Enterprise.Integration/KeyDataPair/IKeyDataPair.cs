using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IKeyDataPair
	{
		ZString Key { get; set; }
		ZString Data { get; set; }
	}
}
