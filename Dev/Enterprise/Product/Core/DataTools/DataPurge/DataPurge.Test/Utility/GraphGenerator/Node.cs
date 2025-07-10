namespace Enterprise.DataPurge.Utility
{
	public class Node
	{
		public Node(string pkTableName, string fkTableName, string pkName, string fkName)
		{
			PKTableName = pkTableName;
			FKTableName = fkTableName;
			PKName = pkName;
			FKName = fkName;
		}

		public string PKTableName { get; private set; }
		public string FKTableName { get; private set; }
		public string PKName { get; private set; }
		public string FKName { get; private set; }
	}
}
