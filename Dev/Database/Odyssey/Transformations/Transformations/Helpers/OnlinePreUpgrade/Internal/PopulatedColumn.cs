namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	public class PopulatedColumn
	{
		public int ColumnId;
		public string ColumnName;
		public string ColumnType;

		/// <summary>
		/// Determine if the column to populate is sparse or not
		/// </summary>
		public bool? IsSparse;

		public string Sparse => IsSparse.Value ? "SPARSE" : string.Empty;

		public string ColumnNullable;
		public string ColumnDefaultConstraint;

		public string PopulateSource;
		public string PopulateExpression;

		public string TriggerExpression;
		public string TriggerSource;

		public string PopulatePreAddWhereClause;
		public string PopulateTargetWhereClause;
	}
}
