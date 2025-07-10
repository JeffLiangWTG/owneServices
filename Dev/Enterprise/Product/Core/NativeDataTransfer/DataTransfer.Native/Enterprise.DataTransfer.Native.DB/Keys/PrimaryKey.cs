namespace Enterprise.DataTransfer.Native.DB.Keys
{
	public class PrimaryKey : Key
	{
		public PrimaryKey(ColumnDef columnDef)
			: base(columnDef)
		{
		}

		public override Table ReferenceTable
		{
			get { return Table; }
		}
	}
}