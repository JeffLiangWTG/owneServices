namespace CargoWise.ComponentModel
{
	public interface INullable
	{
		/// <summary>
		/// Get whether the entity or collection is the special 'null' entity or
		/// collection used when an entity is not available.
		/// </summary>
		bool IsNull { get; }
	}
}
