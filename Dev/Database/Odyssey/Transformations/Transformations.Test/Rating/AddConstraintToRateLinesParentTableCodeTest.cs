using System.Collections.Generic;
using System.Text;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Rating
{
	[TestedType(typeof(AddConstraintToRateLinesParentTableCode))]
	sealed class AddConstraintToRateLinesParentTableCodeTest : ConstraintBase_ParentTableCodeTest<AddConstraintToRateLinesParentTableCode>
	{
		protected override string TableName => RateLinesSchema.Constants.TableName;

		protected override string TablePrefix => RateLinesSchema.Constants.Prefix;

		protected override string[] SupportedParentPrefixes => new[] { "OP" };

		protected override bool UseNoCheck => true;

		protected override bool AllowEmptyParentTableCode => true;

		protected override string[] ExpectedIndexIncludeColumns => [];

		protected override (string column, string tablePrefix)[] ForeignKeyColumns => new[]
		{
			("TL_TI", "TI"),
			("TL_AC", "AC"),
		};

		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			columnValues.Add("TL_RateCalculator", "'FLT'");
			columnValues.Add("TL_RX_NKCurrency", "'UAH'");
			if (columnValues["TL_ParentTableCode"] == "''")
			{
				columnValues.Remove("TL_ParentID");
			}
			base.AppendInsertScript(sqlText, columnValues);
		}
	}
}
