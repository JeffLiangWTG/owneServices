using System.Data;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class SqlParameter
	{
		public SqlParameter(string name, string value, SqlDbType type)
		{
			this.Name = name;
			this.Value = value;
			this.Type = type;
		}

		public string Name { get; private set; }
		public string Value { get; private set; }
		public SqlDbType Type { get; private set; }

		public string keyForDuplicateCheck
		{
			get
			{
				return Type.ToString() + Value;
			}
		}
	}
}
