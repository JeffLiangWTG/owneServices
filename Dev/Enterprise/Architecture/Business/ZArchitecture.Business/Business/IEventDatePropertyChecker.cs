namespace Enterprise.ZArchitecture.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.Integration;

	/// <summary>
	///		Defines methods which should decide whether the specific property marked with an <see cref="EventDatePropertyAttribute"/> can be updated with specific event.
	/// </summary>
	public interface IEventDatePropertyChecker
	{
		/// <summary>
		///		Determines whether the property marked with an <see cref="EventDatePropertyAttribute"/> can be updated by <paramref name="log"/>.
		/// </summary>
		/// <param name="log">
		///		An event which supposed update the property.
		/// </param>
		/// <param name="property">
		///		The property that will be updated if can be updated is true. When events are used for multiple properties, the result may vary depending upon which property is being checked.
		/// </param>
		/// <returns>
		///		<c>true</c>, if the <paramref name="property"/> can be updated; otherwise, <c>false</c>.
		/// </returns>
		bool CanUpdateProperty(IStmALog log, ZPropertyInfo property = null);
	}
}
