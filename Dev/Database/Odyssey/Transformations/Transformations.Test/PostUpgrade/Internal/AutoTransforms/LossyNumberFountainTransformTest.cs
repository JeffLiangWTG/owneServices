using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Internal.AutoTransforms;

public abstract class LossyNumberFountainTransformTest : DataTransformationTestCase
{
	protected abstract LossyNumberFountainTransform GetLossyTransformationInstance();

	const long DefaultSequence = 1337;

	public void TestWhatIfNoStmNum()
	{
		DropSequenceIfNeeded();
		var instance = GetLossyTransformationInstance();
		Db.Connection.ExecuteNonQuery($"delete from StmNums where SN_Name = '{instance.FountainName}'");
		instance.Run();
		AssertEquals("If the StmNum doesn't exist, we don't need to run this transform.", 0, GetSequence(instance));
	}

	protected override void PrepareTestData()
	{
		DropSequenceIfNeeded();
		EnsureStmNum();
	}

	protected sealed override DataTransformation GetNewTestTransformationInstance()
	{
		return GetLossyTransformationInstance();
	}

	protected override void AssertTransformationResults()
	{
		var instance = GetLossyTransformationInstance();
		var idSql = $"select current_value from sys.sequences where name = '{instance.SequenceName}'";
		AssertEquals(1, GetSequence(instance));
		AssertEquals(DefaultSequence, Db.Connection.ExecuteScalar<long>(idSql));
	}

	int GetSequence(LossyNumberFountainTransform instance)
	{
		var sql = $"select count(*) from (select OBJECT_ID('{instance.SequenceName}', N'SO') a) b where a is not null";
		return Db.Connection.ExecuteScalar<int>(sql);
	}

	void DropSequenceIfNeeded()
	{
		var instance = GetLossyTransformationInstance();
		var sqlText = $@"
if (OBJECT_ID('{instance.SequenceName}', N'SO') is not null)
begin
	drop sequence [{instance.SequenceName}]
end";
		Db.Connection.ExecuteNonQuery(sqlText);
	}

	void EnsureStmNum()
	{
		var instance = GetLossyTransformationInstance();
		var sqlText = $@"
delete from StmNums where SN_Name = '{instance.FountainName}';

insert into StmNums (SN_Name, SN_Value, SN_Owner, SN_MinimumValue, SN_MaximumValue, SN_SystemCreateTimeUtc, SN_CanRollover, SN_Sequence)
values ('{instance.FountainName}', {DefaultSequence}, '{instance.OwnerId}', 1000, {instance.MaxValue}, GetUtcDate(), 0, 0)";
		Db.Connection.ExecuteNonQuery(sqlText);
	}
}
