namespace CargoWise.EntityFramework
{
	public interface ITraversibleNode
	{
		IFamilyMember[] GetHierarchy();
	}
}
