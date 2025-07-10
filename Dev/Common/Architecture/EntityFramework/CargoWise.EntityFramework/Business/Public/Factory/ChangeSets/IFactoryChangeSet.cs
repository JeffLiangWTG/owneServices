namespace CargoWise.EntityFramework
{
	public interface IFactoryChangeSet
	{
		/// <summary>
		/// Gets change sets for all modified persistent objects in a factory.
		/// </summary>
		/// <returns>An array of <see cref="IObjectChangeSet"/> instances</returns>
		IObjectChangeSet[] GetChangedObjects();
		BusinessObject[] GetAddedObjects();
	}
}
