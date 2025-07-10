using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IGlbPerson : IBusiness
	{
		ZGuid PK { get; }
	}
}
