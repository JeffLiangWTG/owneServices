using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICYDYardUnitState
	{
		ZGuid PK { get; }

		ZString YUS_UnitID { get; }
	}
}
