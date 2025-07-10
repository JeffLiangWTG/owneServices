using System.Collections.Generic;
using System.Text;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintCSD_ParentTableCode))]
	sealed class ConstraintCSD_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintCSD_ParentTableCode>
	{
		protected override string TableName => "CusStorageDocPivot";

		protected override string TablePrefix => "CSD";

		protected override string[] SupportedParentPrefixes => new[] { "ABL", "AMA", "BH", "C4", "CA", "CB", "CEI", "CH", "CM", "CS", "CSI", "EUS", "JI", "JZ", "QCH" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[CSD_SystemCreateTimeUtc]", "[CSD_SystemLastEditTimeUtc]" };

		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			columnValues[CusStorageDocPivotSchema.Constants.CSD_StorageDocReference] = "NEWID()";
			columnValues[CusStorageDocPivotSchema.Constants.CSD_DocType] = "'XXX'";
			base.AppendInsertScript(sqlText, columnValues);
		}
	}
}
