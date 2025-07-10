using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Identifies in a non-instance way a collection or business object
	/// </summary>
	public interface IIdentified
	{
		ZGuid Identifier { get; }
	}
}
