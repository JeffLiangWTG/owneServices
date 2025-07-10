using System;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	sealed class TableNameAttribute : Attribute
	{
		public TableNameAttribute(string tableName)
		{
			this.TableName = tableName;
		}

		public readonly string TableName;

		internal static string GetTableName<T>() where T : Wrapper
		{
			var tableNameAttributes = typeof(T).GetCustomAttributes(typeof(TableNameAttribute), false);
			if (tableNameAttributes.Length != 1)
			{
				throw new InvalidOperationException("You must have one and only one TableNameAttribute on each Wrapper type subclass.");
			}
			return ((TableNameAttribute)tableNameAttributes[0]).TableName;
		}
	}
}
