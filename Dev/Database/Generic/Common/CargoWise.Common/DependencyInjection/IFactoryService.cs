namespace CargoWise.Common
{
	/// <summary>
	/// Interface for a threadsafe service that provides factories
	/// </summary>
	/// <remarks>
	/// For use to ease unit testing and IOC design
	/// </remarks>
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public interface IFactoryService
	{
		/// <summary>
		/// Register a factory of type T
		/// </summary>
		/// <typeparam name="T">Type of factory to register, must be unique</typeparam>
		/// <param name="factory">Factory to register</param>
		/// <remarks>
		/// If a factory of the same type has already been registered, this will do nothing.
		/// </remarks>
		void RegisterFactory<T>(T factory);

		/// <summary>
		/// Retrieve a factory of a specified type
		/// </summary>
		/// <typeparam name="T">The type of the factory</typeparam>
		/// <returns>The factory requested or throws "InvalidOperationException" if no such factory has been registered</returns>
		T GetFactory<T>();
	}
}