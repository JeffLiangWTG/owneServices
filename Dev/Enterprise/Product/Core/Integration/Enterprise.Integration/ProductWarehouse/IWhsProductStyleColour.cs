using CargoWise.Types;

namespace Enterprise.Integration.Warehouse
{
	public interface IWhsProductStyleColour
	{
		ZGuid PK { get; }
		ZString WSC_Code { get; set; }
		ZGuid WSC_WST_ProductStyle { get; set; }
	}
}
