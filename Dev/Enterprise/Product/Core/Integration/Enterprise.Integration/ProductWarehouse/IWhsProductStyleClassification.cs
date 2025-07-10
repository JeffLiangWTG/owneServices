using CargoWise.Types;

namespace Enterprise.Integration.Warehouse
{
	public interface IWhsProductStyleClassification
	{
		ZGuid PK { get; }
		ZString WSS_Code { get; set; }
		ZGuid WSS_WST_ProductStyle { get; set; }
	}
}
