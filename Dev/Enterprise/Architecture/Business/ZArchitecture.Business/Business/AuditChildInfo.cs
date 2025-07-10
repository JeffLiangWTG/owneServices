using CargoWise.Common;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Business.Business
{
	public class AuditChildInfo
	{
		public AuditChildInfo(SchemaGuidColumn keyColumn, SchemaStringColumn infoColumn)
		{
			Argument.NotNull(keyColumn, nameof(keyColumn));
			KeyColumn = keyColumn;
			InfoColumn = infoColumn;
		}

		public AuditChildInfo(SchemaIntColumn keyColumn, SchemaStringColumn infoColumn)
		{
			Argument.NotNull(keyColumn, nameof(keyColumn));
			KeyColumn = keyColumn;
			InfoColumn = infoColumn;
		}

		public SchemaColumn KeyColumn { get; }
		public SchemaStringColumn InfoColumn { get; }

		public override bool Equals(object obj) => obj is AuditChildInfo auditChildInfo && KeyColumn == auditChildInfo.KeyColumn && InfoColumn == auditChildInfo.InfoColumn;

		public override int GetHashCode() => KeyColumn.GetHashCode() ^ InfoColumn.GetHashCode();

		public override string ToString() => $"{KeyColumn.Name} - {InfoColumn?.Name}";
	}
}
