namespace Enterprise.ZArchitecture.GUI.Grid.Internal
{
	/// <summary>
	/// Marker interface idicating that column style info 
	/// can hold values of different types like GUID/String.
	/// </summary>
	/// <remarks>
	/// Implementing class should return path to list through <see cref="BindToList"/>
	/// which can be used to match values.
	/// </remarks>
	interface IMultiTypeColumnStyleInfo
	{
		string ColumnName	{ get; }
		string BindToList	{ get; }
	}
}
