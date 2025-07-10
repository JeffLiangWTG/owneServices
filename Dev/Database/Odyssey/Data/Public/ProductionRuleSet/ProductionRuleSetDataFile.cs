using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class ProductionRuleSetDataFile : EmbeddedDataFile
	{
		public ProductionRuleSetDataFile() : base(DataFileRelativePath, new string[2] { ProductionRuleSetSchema.Constants.TableName, ProductionRuleSchema.Constants.TableName })
		{
		}

		internal ProductionRuleSetDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, new string[2] { ProductionRuleSetSchema.Constants.TableName, ProductionRuleSchema.Constants.TableName })
		{
		}

		protected override string SelectQuery
		{
			get
			{
				return @"
SELECT
	*
FROM
	dbo.ProductionRuleSet
WHERE
	PRS_IsSystem = 1
ORDER BY
	PRS_PK;

SELECT
	ProductionRule.*
FROM
	dbo.ProductionRule
	JOIN dbo.ProductionRuleSet ON PRL_PRS_RuleSet = PRS_PK
WHERE
	PRS_IsSystem = 1
ORDER BY
	PRL_PK;"; // SQL Query for system use only
			}
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\ProductionRuleSet\ProductionRuleSet.xml";
		public override string ResourceRelativeName => "ProductionRuleSet.ProductionRuleSet.xml";

		#endregion
	}
}
