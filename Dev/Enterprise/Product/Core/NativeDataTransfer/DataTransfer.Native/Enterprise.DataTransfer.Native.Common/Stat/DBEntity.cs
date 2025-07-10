using System;

namespace Enterprise.DataTransfer.Native.Common.Stat
{
	public class DBEntity
	{
		public string Name { get; private set; }
		public Guid PK { get; private set; }
		public DbAction Action { get; private set; }

		public DBEntity(string name, Guid pk, DbAction action)
		{
			Name = name;
			PK = pk;
			Action = action;
		}

		public enum DbAction
		{
			Insert,
			Update,
			Delete,
			Unchanged
		}
	}
}
