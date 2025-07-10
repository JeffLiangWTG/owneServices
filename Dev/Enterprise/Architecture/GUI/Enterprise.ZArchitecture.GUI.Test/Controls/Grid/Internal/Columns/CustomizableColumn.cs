namespace Enterprise.ZArchitecture.GUI.Testing
{
	class CustomizableColumn : ICustomizableColumn
	{
		public string Caption { get; set; }

		public string ColumnName { get; set; }

		public bool IsVisible { get; set; }

		public bool IsMandatory { get; set; }
		public bool IsCustomColumn { get; set; }

		public override string ToString()
		{
			return Caption;
		}
	}
}
