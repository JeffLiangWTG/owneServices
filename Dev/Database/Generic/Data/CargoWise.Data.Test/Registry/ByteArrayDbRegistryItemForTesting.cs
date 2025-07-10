using System;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Data.Registry.Testing
{
	sealed class ByteArrayDbRegistryItemForTesting : BaseDbRegistryItem<byte[]>
	{
		public ByteArrayDbRegistryItemForTesting(bool preserveTestValue) : base(preserveTestValue)
		{
		}

		public override string ItemName
		{
			get { return "ByteArrayDbRegistryItemForTesting"; }
		}

		protected override byte[] DefaultValue
		{
			get { return DefaultValueConst; }
		}

		protected override string TypeCode
		{
			get { return "BIN"; }
		}

		protected override byte[] GetValueFromBytes(byte[] binaryValue)
		{
			return binaryValue;
		}

		protected override byte[] GetBytesFromValue(byte[] value)
		{
			return value;
		}

		public static readonly byte[] DefaultValueConst = new byte[] { 0, 1, 2 };

		public bool GetPreserveTestValue(DbConnection connection)
		{
			var sql = FormattableString.Invariant($@"
select top(1) SD_PreserveTestValue
	from dbo.StmData 
	where SD_Name = @Name");
			return Convert.ToBoolean(connection.ExecuteScalar(sql, cmd => cmd.AddParameterBasedOnDbColumn("@Name", ItemName, StmDataSchema.SD_Name)));
		}
	}
}
