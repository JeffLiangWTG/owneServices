using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IStmDeliveryGroup
	{
		ZGuid PK { get; }

		ZBool SB_IsProcessed { get; set; }
	}
}
