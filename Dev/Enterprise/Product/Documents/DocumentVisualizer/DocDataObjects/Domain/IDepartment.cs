using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IDepartment
	{
		ZString Code { get; }
		ZString Description { get; }
		ZGuid PK { get; }
	}
}
