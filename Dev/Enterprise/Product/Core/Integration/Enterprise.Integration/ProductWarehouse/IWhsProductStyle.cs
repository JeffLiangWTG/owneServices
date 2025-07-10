using CargoWise.Types;

namespace Enterprise.Integration.Warehouse
{
	public interface IWhsProductStyle
	{
		ZGuid PK { get; }
		ZString WST_Code { get; set; }
	}
}
