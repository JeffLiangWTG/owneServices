namespace Enterprise.DataPurge.Utility
{
	public class ForeignKeyInfo
	{
		public ForeignKeyInfo(string fkName, string fkTableName)
		{
			FKName = fkName;
			FKTableName = fkTableName;
		}

		public string FKName { get; private set; }
		public string FKTableName { get; private set; }
	}
}
