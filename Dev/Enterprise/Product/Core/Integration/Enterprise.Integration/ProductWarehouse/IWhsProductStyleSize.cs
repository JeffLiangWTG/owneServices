using CargoWise.Types;

namespace Enterprise.Integration.Warehouse
{
	public interface IWhsProductStyleSize
	{
		ZGuid PK { get; }
		ZString WSZ_Size { get; set; }
		ZByte WSZ_Sequence { get; set; }
		ZGuid WSZ_WST_ProductStyle { get; set; }
	}
}
