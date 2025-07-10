namespace Enterprise.ZArchitecture.GUI
{
	public interface ICustomizableColumn
	{
		string ColumnName { get; }
		bool IsVisible { get; set; }
		bool IsMandatory { get; }
		bool IsCustomColumn { get; set; }
	}

	interface ISubmissiveColumn
	{
		bool IsSubmissive { get; }
	}
}
