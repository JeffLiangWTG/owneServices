namespace Enterprise.DataTransfer.Native.DB.Keys
{
	public class Key : ColumnDef
	{
		public Key(ColumnDef columnDef)
			: base(columnDef.Table)
		{
			Name = columnDef.Name;
			DataType = columnDef.DataType;
			DefaultValue = columnDef.DefaultValue;
			DoesNotRequireAValue = columnDef.DoesNotRequireAValue;
			Nullable = columnDef.Nullable;
		}

		public virtual Table ReferenceTable { set; get; }

		public virtual ColumnDef ReferenceColumnDef
		{
			get
			{
				return referenceColumnDef ?? ReferenceTable.Columns.PrimaryKey;
			}
			set { referenceColumnDef = value; }
		}
		ColumnDef referenceColumnDef;

		public virtual ColumnDef Discriminator
		{
			get
			{
				return null;
			}
		}
	}
}