using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common;

public abstract class LossyNumberFountainTransform : DataTransformation
{
	public override string UserDescription => FormattableString.Invariant($"Migrating number fountain to lossy fountain for {Table}.{Column}.");

	public abstract string Table { get; }
	public abstract string Column { get; }
	public abstract string FountainName { get; }
	public virtual long MaxValue => 9220000000000000000;
	public virtual Guid OwnerId => Guid.Empty;
	public string SequenceName => $"{FountainName}-{OwnerId}";

	protected override void OfflinePostUpgradeTransform()
	{
		var getStartingNumberSql = $"select IsNull((select SN_Value from StmNums where SN_Name = '{FountainName}'), -1)";
		var start = Db.Connection.ExecuteScalar<long>(getStartingNumberSql);

		if (start > 0)
		{
			var createSequenceSql = $@"
if (OBJECT_ID('{SequenceName}', N'SO') is null)
begin
	create sequence [{SequenceName}]
		as bigint
		start with {start}
		increment by 1
		maxvalue {MaxValue}
end";
			Db.Connection.ExecuteNonQuery(createSequenceSql);
		}
	}
}
