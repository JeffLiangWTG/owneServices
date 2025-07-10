namespace Enterprise.DbUpgrader.Shared
{
	public sealed class InsertStatement
	{
		public InsertStatement(string insertHeader, string insertValuesBlock)
		{
			InsertHeader = insertHeader;
			InsertValuesBlock = insertValuesBlock;
		}

		public string InsertHeader { get; }
		public string InsertValuesBlock { get; }
		public string FullStatement => InsertHeader + InsertValuesBlock;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static implicit operator string(InsertStatement statement) => statement.ToString();

		public override string ToString() => FullStatement;
	}
}
